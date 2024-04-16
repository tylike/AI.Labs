using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.Charts.Native;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Drawing;

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
            SourceInfo = sourceInfo
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

    string GetInputVideosParameter()
    {
        return string.Join(" ", VideoSources.Select(t => $" -i {t.Path}"));
    }
    string GetInputAudiosParameter()
    {
        return string.Join(" ", AudioSources.Select(t => $" -i {t.Path}"));
    }

    [Size(-1)]
    public string OutputVideoFile
    {
        get { return GetPropertyValue<string>(nameof(OutputVideoFile)); }
        set { SetPropertyValue(nameof(OutputVideoFile), value); }
    }
    public void Export()
    {
        var clips = MediaClips.OrderBy(t => t.Index).ToList();
        clips.First().AudioClip.LogTitle();

        //var r1 = clips.Select(t => t.AudioClip.计算调速());

        clips.ForEach(t => t.AudioClip.计算调速());
        clips.ForEach(t => t.VideoClip.计算调速());
        clips.ForEach(t => t.VideoClip.计算延时());
        clips.ForEach(t => t.AudioClip.计算延时());

        clips.ForEach(t =>
        {
            DrawText(10, 10, t.VideoClip.TextLogs, 16, t.VideoClip.StartTime, t.VideoClip.EndTime);
            DrawText(10, 80, t.AudioClip.TextLogs, 16, t.AudioClip.StartTime, t.AudioClip.EndTime);
        });

        TextTrack.Add(DrawCurrentTime());


        var filterComplex = ComplexScript;
        var duration = MediaClips.Last().VideoClip.EndTime;
        Console.WriteLine("==============================================================");
        Console.WriteLine(filterComplex);

        var args = $" -map \"[MediaOut]\" -progress pipe:1";

        var basePath = Path.GetDirectoryName(OutputVideoFile);
        //var task = RunHttp();
        //var all = AudioSources.Select(t => t.SourceInfo.Subtitle).ToArray();

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

        File.WriteAllText(Path.Combine(basePath, "log.csv"), OperateLogs.Join("\n"));
        FFmpegHelper.Concat(clips.Select(t => t.VideoClip.OutputFile), clips.Select(t => t.AudioClip.OutputFile), OutputVideoFile);

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

    public void CreateProject()
    {
        var subtitles = VideoInfo.Subtitles.OrderBy(t => t.Index).ToArray();

        //所有的结束时间，但除了最后一条用开始时间
        foreach (var item in subtitles)
        {
            item.FixedStartTime = item.StartTime;
            item.FixedEndTime = item.EndTime;
        }

        MediaClip last = null;
        var videoClips = Path.Combine(VideoInfo.ProjectPath, "VideoClip");

        var dir = Path.GetDirectoryName(videoClips);
        if(!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var splitFiles = FFmpegHelper.SplitVideo(VideoInfo.VideoFile, subtitles, videoClips);

        foreach (var item in VideoInfo.Audios.OrderBy(t => t.Index))
        {
            var clip = new MediaClip(Session)
            {
                AudioInfo = item,
                Index = item.Index,
                Project = this
            };
            AudioTrack.Add(clip.CreateAudioClip());
            VideoTrack.Add(clip.CreateVideoClip(splitFiles[item.Index - 1]));
            if (last != null)
            {
                clip.VideoClip.Before = last.VideoClip;
                clip.AudioClip.Before = last.AudioClip;

                last.VideoClip.Next = clip.VideoClip;
                last.AudioClip.Next = clip.AudioClip;
            }
            last = clip;
        }

        var vi = this.VideoInfo;

        this.OutputVideoFile = Path.Combine(vi.ProjectPath, "product.mp4");

        var mainVideo = ImportVideo(vi.VideoFile);

        foreach (var item in vi.Audios)
        {
            ImportAudio(item.OutputFileName, item);
        }
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
