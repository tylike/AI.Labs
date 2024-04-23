using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.Helper;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace AI.Labs.Module.BusinessObjects;
[NavigationItem("视频")]
public class VideoScriptProject : BaseObject
{
    public VideoScriptProject(Session s) : base(s)
    {

    }
    [ModelDefault("DisplayFormat", "yyyy-MM-dd HH:mm:ss.fff")]
    public DateTime CreateDate
    {
        get { return GetPropertyValue<DateTime>(nameof(CreateDate)); }
        set { SetPropertyValue(nameof(CreateDate), value); }
    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        this.CreateDate = DateTime.Now;
    }


    public List<string> OperateLogs = new();

    public void Log(string text)
    {
        OperateLogs.Add(text);
    }

    [Association, DevExpress.Xpo.Aggregated]
    public XPCollection<VideoSource> VideoSources => GetCollection<VideoSource>(nameof(VideoSources));
    [Association, DevExpress.Xpo.Aggregated]
    public XPCollection<AudioSource> AudioSources => GetCollection<AudioSource>(nameof(AudioSources));
    public VideoSource ImportVideo(string path)
    {
        var mediaSource = new VideoSource(Session)
        {
            Path = path,
            Index = VideoSources.Count
        };
        VideoSources.Add(mediaSource);
        return mediaSource;
    }
    public void ImportAudio(string path, AudioBookTextAudioItem sourceInfo)
    {
        var mediaSource = new AudioSource(Session)
        {
            Path = path,
            //VideoScript = this,
            Index = VideoSources.Count + AudioSources.Count,
            SourceInfo = sourceInfo,
        };
        AudioSources.Add(mediaSource);
    }
    public string Name
    {
        get { return GetPropertyValue<string>(nameof(Name)); }
        set { SetPropertyValue(nameof(Name), value); }
    }

    public VideoInfo VideoInfo
    {
        get { return GetPropertyValue<VideoInfo>(nameof(VideoInfo)); }
        set { SetPropertyValue(nameof(VideoInfo), value); }
    }

    List<AudioClip> AudioTrack = new();
    List<VideoClip> VideoTrack = new();
    List<DrawTextClip> TextTrack = new();

    #region 相关的角本
    public string VideoScript
    {
        get
        {
            var videoClips = VideoTrack.Select(t => t.GetScript()).Join(";\n");
            return videoClips;
        }
    }

    public string AudioScript
    {
        get
        {
            var audioClips = AudioTrack.Select(t => t.GetScript()).Join(";\n");
            return audioClips;
        }
    }

    public string ConcatScript
    {
        get
        {
            var videoClips = VideoTrack.Select(t => t.GetOutputLabel()).Join();
            var audioClips = AudioTrack.Select(t => t.GetOutputLabel()).Join();
            var textClips = TextTrack.Select(t => t.GetScript()).Join(",\n");

            return @$"{videoClips}concat=n={VideoTrack.Count}:v=1:a=0[vt];
[vt]{textClips}[v];
{audioClips}concat=n={AudioTrack.Count}:v=0:a=1[a]";
        }
    }


    public string ComplexScript
    {
        get
        {
            return @$"{VideoScript};
{AudioScript};
{ConcatScript};
{ReleaseProductScript}
";
        }
    }
    #endregion

    [Size(-1)]
    public string OutputVideoFile
    {
        get { return GetPropertyValue<string>(nameof(OutputVideoFile)); }
        set { SetPropertyValue(nameof(OutputVideoFile), value); }
    }

    FileStream logFileStream;
    StreamWriter logWriter;

    public void Export()
    {

        var clips = MediaClips.OrderBy(t => t.Index).ToList();
        clips.First().AudioClip.LogTitle();
        SRT pre = null;

        #region 字幕、日志
        var cnSrtFile = new SRTFile() { FileName = Path.Combine(VideoInfo.ProjectPath, "cnsrt.fix.srt"), UseIndex = true };
        var enSrtFile = new SRTFile() { FileName = Path.Combine(VideoInfo.ProjectPath, "ensrt.fix.srt"), UseIndex = true };


        #endregion

        var clipHtml = new StringBuilder();
        var audioHtml = new StringBuilder();
        var videoHtml = new StringBuilder();
        var subtitleHtml = new StringBuilder();

        foreach (var item in clips)
        {

            //item.Start = pre?.End ?? TimeSpan.Zero;
            //item.End = item.Subtitle.EndTime;
            item.Duration = (int)(item.End - item.Start).TotalMilliseconds;

            logWriter.WriteLine("=============================================================================================================");
            logWriter.WriteLine($"Clip.Index = {item.Index}");
            logWriter.WriteLine($"片断.时长:{item.Duration}");
            logWriter.WriteLine($"音频.时长:{item.AudioClip.FileDuration}");
            logWriter.WriteLine($"视频.时长:{item.VideoClip.FileDuration}");
            logWriter.WriteLine($"字幕.时长:{item.Subtitle.Duration}");

            logWriter.WriteLine($"A:音频文件快放");
            if (item.AudioClip.FileDuration > item.Subtitle.Duration)
            {
                item.AudioClip.计算调速();
                item.Duration = (int)item.AudioClip.FileDuration.Value;
            }
            logWriter.WriteLine("--------------------------------------------------------------------------------------------------------------");

            logWriter.WriteLine($"片断.时长:{item.Duration}");
            logWriter.WriteLine($"音频.时长:{item.AudioClip.FileDuration}");
            logWriter.WriteLine($"视频.时长:{item.VideoClip.FileDuration}");
            logWriter.WriteLine($"字幕.时长:{item.Subtitle.Duration}");


            logWriter.WriteLine($"V:视频文件延长,当前时长:{item.VideoClip.FileDuration} ");
            if (item.Duration > item.VideoClip.GetDuration())
            {
                item.VideoClip.计算延时();
            }

            #region 写字幕文件
            var cnText = item.Subtitle.CnText;
            var cnSrtItem = new SRT();

            if (!string.IsNullOrEmpty(cnText))
            {
                cnText = cnText.Replace("\n", "");
                var start = pre?.EndTime ?? TimeSpan.Zero;
                var end = start.AddMilliseconds(item.AudioClip.FileDuration.Value);
                cnSrtItem.Index = item.Index;
                cnSrtItem.StartTime = start;
                cnSrtItem.EndTime = end;
                cnSrtItem.Text = cnText;
                cnSrtFile.Texts.Add(cnSrtItem);

                //cnSrtFile.Texts.Add(new SRT
                //{
                //    Index = item.Index,
                //    StartTime = start,
                //    EndTime = end,
                //    Text = cnText
                //});
            }
            pre = cnSrtItem;

            enSrtFile.Texts.Add(new SRT
            {
                Index = item.Index,
                StartTime = cnSrtItem.StartTime,
                EndTime = cnSrtItem.EndTime,
                Text = item.Subtitle.PlainText
            });
            #endregion

            logWriter.WriteLine("结果:");
            logWriter.WriteLine($"片断.时长:{item.Duration}");
            logWriter.WriteLine($"音频.时长:{item.AudioClip.FileDuration}");
            logWriter.WriteLine($"视频.时长:{item.VideoClip.FileDuration}");
            logWriter.WriteLine($"字幕.时长:{(cnSrtItem.EndTime - cnSrtItem.StartTime).TotalMilliseconds}");

            clipHtml.AppendLine($"<div title='时长:{item.Duration},视频:{item.VideoClip.FileDuration},音频:{item.AudioClip.FileDuration}' class='clip' style='width:{item.Duration / 100}px'>{item.Index}</div>");
            audioHtml.AppendLine($"<div class='clip' style='width:{item.AudioClip.FileDuration / 100 }px'>{item.Index}</div>");
            videoHtml.AppendLine($"<div class='clip' style='width:{item.VideoClip.FileDuration / 100}px'>{item.Index}</div>");
            subtitleHtml.AppendLine($"<div class='clip' style='width:{item.Duration / 100}px'>{item.Index}</div>");
        }
        logWriter.Flush();
        logFileStream.Flush();

        var html = $@"
<html>
<head>
<style>
        .clip {{position: relative;
            display: inline-block;
            border: 1px solid #ccc;
            text-overflow:clip;
            overflow:hidden
        }}
        .rootClips{{width:11000px;
        }}
</style>
</head>
<body>
<div id='clip' class='rootClips'>{clipHtml.ToString()}</div>
<div id='audio' class='rootClips'>{audioHtml.ToString()}</div>
<div id='video' class='rootClips'>{videoHtml.ToString()}</div>
<div id='subtitle' class='rootClips'>{subtitleHtml.ToString()}</div>
</body>
</html>
";

        File.WriteAllText(Path.Combine(VideoInfo.ProjectPath, "result.htm"), html);

        cnSrtFile.Save();
        enSrtFile.Save();




        //clips.ForEach(t => t.AudioClip.计算调速());

        #region 根据字幕加速音频,最快1.3倍.
        //字幕不变,音频减少,但不需要记录变化,因为没有到最终不变的时间
        var sw = Stopwatch.StartNew();
        //Parallel.ForEach(clips.Select(t => t.AudioClip).ToArray(), new ParallelOptions { MaxDegreeOfParallelism = 8 }, item =>
        //{
        //    item.计算调速();
        //});
        //sw.Stop();
        Debug.WriteLine($"音频调整用时:{sw.Elapsed}");
        #endregion

        #region 根据音频延长视频
        //视频变长,音频不变,需要记录变化
        //需要后推
        sw.Restart();
        //clips.ForEach(t => t.VideoClip.计算调速());
        //clips.ForEach(t => t.VideoClip.计算延时());
        sw.Stop();
        #endregion

        #region 音频不够长的,延长音频
        //clips.ForEach(t => t.AudioClip.计算延时());
        #endregion

        #region 绘制文本
        clips.ForEach(t =>
{
    DrawText(10, 10, t.VideoClip.TextLogs, 16, t.VideoClip.StartTime, t.VideoClip.EndTime);
    DrawText(10, 80, t.AudioClip.TextLogs, 16, t.AudioClip.StartTime, t.AudioClip.EndTime);
});

        TextTrack.Add(DrawCurrentTime());
        #endregion

        var filterComplex = ComplexScript;
        var duration = MediaClips.Last().VideoClip.EndTime;
        Console.WriteLine("==============================================================");
        Console.WriteLine(filterComplex);

        var args = $" -map \"[MediaOut]\" -progress pipe:1";

        var basePath = Path.GetDirectoryName(OutputVideoFile);
        //var task = RunHttp();
        //var all = AudioSources.Select(t => t.SourceInfo.Subtitle).ToArray();

        CreateFinalClipForDebug(clips);
        var clipsEndTimeDuration = clips.Last().End.TotalMilliseconds;
        var audioFilesDuration = clips.Sum(t => t.AudioClip.FileDuration);
        var videoFileDuration = clips.Sum(t => t.VideoClip.FileDuration);

        logWriter.WriteLine($"计算时长:{clipsEndTimeDuration},Audio:{audioFilesDuration},Video:{videoFileDuration}");

        FFmpegHelper.Concat(clips, OutputVideoFile, logWriter: logWriter);

        logWriter.Flush();
        logFileStream.Flush();



        //FFmpegHelper.ExecuteCommand(
        //    $"{GetInputVideosParameter()} {GetInputAudiosParameter()}",
        //    OutputVideoFile,
        //    args,
        //    filterComplex,
        //    true,
        //    duration.TotalSeconds + 1,
        //    basePath: basePath
        //    );
    }

    private void CreateFinalClipForDebug(List<MediaClip> clips)
    {
        var videoClipFinal = Path.Combine(VideoInfo.ProjectPath, "VideoClip.Final");
        var audioClipFinal = Path.Combine(VideoInfo.ProjectPath, "AudioClip.Final");
        if (!Directory.Exists(videoClipFinal))
        {
            Directory.CreateDirectory(videoClipFinal);
        }
        if (!Directory.Exists(audioClipFinal))
        {
            Directory.CreateDirectory(audioClipFinal);
        }

        foreach (var item in clips)
        {
            File.Copy(item.VideoClip.OutputFile, Path.Combine(videoClipFinal, $"{item.Index}.mp4"), true);
            File.Copy(item.AudioClip.OutputFile, Path.Combine(audioClipFinal, $"{item.Index}.mp3"), true);
        }
    }

    public string ReleaseProductScript => $"[v][a]concat=n=1:a=1:v=1[MediaOut];";
    public virtual string GetFilePath(FileType fileType, double parameter, int? index)
    {
        var ext = "";
        switch (fileType)
        {
            case FileType.Audio_ChangeSpeed:
            case FileType.Audio_Delay:
                ext = "mp3";
                break;
            case FileType.Video_ChangeSpeed:
            case FileType.Video_Delay:
                ext = "mp4";
                break;
            default:
                break;
        }
        var fileName = Path.Combine(VideoInfo.ProjectPath, fileType.ToString(), $"{(index.HasValue ? index.ToString() + "_" : "")}{parameter.ToFFmpegString()}.{ext}");
        var dir = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        return fileName;
    }

    #region 绘制文本
    public TextOption DefaultTextOption
    {
        get { return GetPropertyValue<TextOption>(nameof(DefaultTextOption)); }
        set { SetPropertyValue(nameof(DefaultTextOption), value); }
    }

    public DrawTextClip DrawText(int x, int y, string text, int fontSize, TimeSpan start, TimeSpan end)
    {
        return DrawText(x.ToString(), y.ToString(), text, fontSize, start, end);
    }

    public DrawTextClip DrawText(string x, string y, string text, int fontSize, TimeSpan start, TimeSpan end)
    {
        var textClip = new DrawTextClip(Session)
        {
            Left = x.ToString(),
            Top = y.ToString(),
            Option = DefaultTextOption,
            StartTime = start,
            EndTime = end
        };
        textClip.SetText(text);
        TextTrack.Add(textClip);
        return textClip;
    }

    public DrawTextClip DrawCurrentTime(int x = 10, int y = 450, int fontSize = 24, TimeSpan? start = null, TimeSpan? end = null)
    {
        var currentTime = new DrawTextClip(Session)
        {
            Left = x.ToString(),
            Top = y.ToString(),
            Option = DefaultTextOption,
            StartTime = TimeSpan.Zero,
            EndTime = TimeSpan.FromSeconds(90)
        };
        var duration = AudioSources.Max(t => t.SourceInfo.Subtitle.EndTime);
        currentTime.SetDisplayCurrentVideoTime(duration);
        return currentTime;
    }
    #endregion

    public void CreateProject(IObjectSpace os)
    {
        logFileStream = new FileStream(Path.Combine(VideoInfo.ProjectPath, "logs.txt"), FileMode.Create);
        logWriter = new StreamWriter(logFileStream, Encoding.UTF8);
        FFmpegHelper.Log = logWriter;
        Console.WriteLine("创建项目" + DateTime.Now.ToString());
        #region 导入音频、视频
        var vi = this.VideoInfo;

        var mainVideo = ImportVideo(vi.VideoFile);

        foreach (var item in vi.Audios)
        {
            ImportAudio(item.OutputFileName, item);
        }
        #endregion

        #region 设置日志目录
        var temp = @"d:\temp\logs";
        if (Directory.Exists(temp))
        {
            Directory.Delete(temp, true);
        }
        if (!Directory.Exists(temp))
        {
            Directory.CreateDirectory(temp);
        }
        #endregion

        #region 设置字幕
        var subtitles = VideoInfo.Subtitles.OrderBy(t => t.Index).ToArray();

        SubtitleItem pre = null;
        //所有的结束时间，但除了最后一条用开始时间
        foreach (var item in subtitles)
        {

            item.StartTime = item.StartTime.AdjustTime(true);
            item.EndTime = item.EndTime.AdjustTime(true);

            if (pre != null)
            {
                pre.EndTime = item.StartTime;
            }
            item.FixedStartTime = item.StartTime;
            item.FixedEndTime = item.EndTime;
            item.Save();
            pre = item;
        }

        subtitles.First().StartTime = TimeSpan.Zero;
        #endregion
        Console.WriteLine("预处理视频" + DateTime.Now.ToString());
        #region 视频片断位置
        var videoClips = Path.Combine(VideoInfo.ProjectPath, "VideoClip");

        if (!Directory.Exists(videoClips))
        {
            Directory.CreateDirectory(videoClips);
        }
        var splitFiles = FFmpegHelper.SplitVideo(VideoInfo.VideoFile, subtitles, videoClips).OrderBy(t => t).ToArray();

        #endregion
        Console.WriteLine("预处理音频" + DateTime.Now.ToString());
        #region 绑定音频与视频片断
        MediaClip last = null;
        foreach (var item in VideoInfo.Audios.OrderBy(t => t.Index))
        {
            Console.WriteLine("预处理音频" + item.Index);

            var clip = new MediaClip(Session)
            {
                AudioInfo = item,
                Index = item.Index,
                Project = this,
                LogWriter = this.logWriter
            };

            AudioTrack.Add(clip.CreateAudioClip());
            VideoTrack.Add(clip.CreateVideoClip(splitFiles[item.Index - 1]));

            if (clip.Subtitle.Duration > clip.AudioClip.FileDuration)
            {
                clip.AudioClip.计算延时();
            }

            if (last != null)
            {
                clip.VideoClip.Before = last.VideoClip;
                clip.AudioClip.Before = last.AudioClip;

                last.VideoClip.Next = clip.VideoClip;
                last.AudioClip.Next = clip.AudioClip;
            }
            last = clip;
        }
        #endregion

        //var audios = MediaClips.Select(t=>t.AudioClip)
        this.OutputVideoFile = Path.Combine(vi.ProjectPath, "product.mp4");
    }

    [Association, DevExpress.Xpo.Aggregated]
    public XPCollection<MediaClip> MediaClips
    {
        get
        {
            return GetCollection<MediaClip>(nameof(MediaClips));
        }
    }
}
public enum FileType
{
    Audio_ChangeSpeed,
    Audio_Delay,
    Video_ChangeSpeed,
    Video_Delay
}
