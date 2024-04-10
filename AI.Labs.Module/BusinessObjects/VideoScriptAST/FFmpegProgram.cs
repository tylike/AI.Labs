using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using com.sun.tools.javadoc;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using FFMpegCore.Enums;
using Microsoft.CodeAnalysis.Scripting;
using System.Drawing;
using System.Windows.Forms;

namespace AI.Labs.Module.BusinessObjects;

/// <summary>
/// 认为最终要执行的ffmpeg命令中的filter_complex是一段程序
/// </summary>
public class VideoScript : BaseObject
{
    public VideoScript(Session s) : base(s)
    {

    }
    public override void AfterConstruction()
    {
        base.AfterConstruction();
        OverWriteExist = true;
    }

    public string Name
    {
        get { return GetPropertyValue<string>(nameof(Name)); }
        set { SetPropertyValue(nameof(Name), value); }
    }

    [Association, Aggregated]
    public XPCollection<VideoSource> VideoSources => GetCollection<VideoSource>(nameof(VideoSources));
    [Association, Aggregated]
    public XPCollection<AudioSource> AudioSources => GetCollection<AudioSource>(nameof(AudioSources));

    /// <summary>
    /// 导入全部完成,重新生成序号、label
    /// </summary>
    public void ImportCompleted()
    {

    }

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

    /// <summary>
    /// 程序由语句组成
    /// </summary>
    [Association, DevExpress.Xpo.Aggregated]
    public XPCollection<Statement> Statements
    {
        get
        {
            return GetCollection<Statement>(nameof(Statements));
        }
    }

    public Statement CreateStatement(string commandJoinSpliter =",")
    {
        var rst = new Statement(Session);
        rst.CommandJoinSpliter = commandJoinSpliter;
        rst.Index = Statements.Count;
        Statements.Add(rst);
        return rst;
    }


    public bool OverWriteExist
    {
        get { return GetPropertyValue<bool>(nameof(OverWriteExist)); }
        set { SetPropertyValue(nameof(OverWriteExist), value); }
    }


    public VideoInfo VideoInfo
    {
        get { return GetPropertyValue<VideoInfo>(nameof(VideoInfo)); }
        set { SetPropertyValue(nameof(VideoInfo), value); }
    }

    string GetScript()
    {
        return string.Join(";\n", Statements.Select(t => t.GetScript()));
    }
    public void Export2()
    {
        //1.准备所有音频片断
        //2.准备所有视频片断
        //3.将视频片断、音频片断、字幕片断都做对应关系。
        //4.处理对齐逻辑
        //5.合并所有片断
    }
    public void Export()
    {
        var vi = VideoInfo;
        var mainVideo = ImportVideo(vi.VideoFile);

        foreach (var item in vi.Audios)
        {
            ImportAudio(item.OutputFileName, item);
        }
        //第一步:检查中文音频的时长,大于字幕时长的,将快放中文音频,取最大1.3倍,与 “完全匹配倍速”
        var timeoutRecords1 = AudioSources.Where(t => t.SourceInfo.Duration > t.SourceInfo.Subtitle.Duration).ToList();

        #region 第一步
        foreach (var item in timeoutRecords1)
        {
            //计算如果播放完整,应该用多快的速度
            var 计划速度 = item.SourceInfo.Duration / item.SourceInfo.Subtitle.Duration;
            //*****************************************************************
            //完全按最快播放也不行,这样听着不舒服
            var 实际速度 = (decimal)Math.Min(计划速度, 1.3);

            var statement = CreateStatement();
            statement.InputLables = item.GetLabel();
            statement.OutputLabels = $"[a{item.Index}]";

            var changeSpeed = statement.CreateCommand(new ChangeSpeed(Session) { MediaType = MediaType.Audio });
            changeSpeed.TargetSpeed = 实际速度;
            item.CustomLabel = statement.OutputLabels;
            //加文字说明
            item.ChangeLogs = @$"原字幕时间:{item.SourceInfo.Subtitle.StartTime}-{item.SourceInfo.Subtitle.EndTime}
音频时长:{item.SourceInfo.Duration}
字幕时长:{item.SourceInfo.Subtitle.Duration}
调整速度:{实际速度.ToString("0.0000")}";

            item.SourceInfo.Duration = (int)实际速度;
        } 
        #endregion

        //第二步:检查中文音频的时长，仍然大于字幕的，将慢放视频，取最小0.7倍速
        //var 准备调速 = Math.Max(准备调速, 0.7);
        //var 计划视频调速 = (double)当前1条英文字幕.Duration / (double)加速1点3倍后中文音频.Duration;
        //var 实际视频倍速 = Math.Min(0.7, 计划视频调速);
        //加文字说明
        //var timeoutRecords2 = AudioSources.Where(t => t.SourceInfo.Duration > t.SourceInfo.Subtitle.Duration).ToList();
        var mainVideoLabel = mainVideo.GetLabel();
        var videoClips = new List<Statement>();
        foreach (var item in AudioSources)
        {
            //计算如果播放完整,应该用多快的速度
            var 计划速度 = (decimal)item.SourceInfo.Subtitle.Duration/item.SourceInfo.Duration;
            //*****************************************************************
            //完全按最快播放也不行,这样听着不舒服
            var 实际速度 = 0m;
            if (item.SourceInfo.Duration > item.SourceInfo.Subtitle.Duration)
            {
                实际速度 = Math.Max(计划速度, 0.7m);
            }

            var createClip = CreateStatement();
            videoClips.Add(createClip);
            createClip.InputLables = mainVideoLabel;
            createClip.OutputLabels = $"[v{item.Index}]";
            createClip.CreateCommand(new SelectMediaCommand(Session)
            {
                Start = item.SourceInfo.Subtitle.StartTime,
                End = item.SourceInfo.Subtitle.EndTime,
                TargetSpeed = 实际速度
            });
            //加文字说明
            item.ChangeLogs = @$"
音频时长:{item.SourceInfo.Duration}
字幕时长:{item.SourceInfo.Subtitle.Duration}
视频调整速度:{实际速度.ToString("0.0000")}";
        }

        //第三步:检查中文音频的时长，仍然大于字幕的，将延长视频时长
        //也可以选择为重复播放视频一个片断
        //var delayTime = (float)(当前中文音频信息.Duration - 慢放视频0点7倍.Duration);
        //加文字说明

        //加英文字幕
        //加中文字幕
        //加片头等动作

        //合并音频时需要考虑视频不够长时在中间加静音，使音频与视频是对齐的。
        //objectSpace.CommitChanges();

        for(int i = 0; i < AudioSources.Count; i++)
        {
            var cur = AudioSources[i];
            var next = i > AudioSources.Count - 1 ? null : AudioSources[i + 1];
            if (next == null)
            {
                break;
            }
            if(cur.SourceInfo.Subtitle.EndTime < next.SourceInfo.Subtitle.StartTime)
            {

            }
        }

        var concatAllAudio = CreateStatement();
        concatAllAudio.InputLables = string.Join("", AudioSources.OrderBy(t => t.Index).Select(t => t.GetLabel()));
        concatAllAudio.CreateCommand(new ConcatMedia(Session) { MediaCount = AudioSources.Count, ConcatAudio = true,AddationCommand= ", aformat=sample_fmts=fltp:sample_rates=48000:channel_layouts=stereo" });
        concatAllAudio.OutputLabels = "[aout]";

        var concatAllVideo = CreateStatement();
        concatAllVideo.InputLables = string.Join("", videoClips.OrderBy(t => t.Index).Select(t => t.OutputLabels));
        concatAllVideo.CreateCommand(new ConcatMedia(Session) { MediaCount = videoClips.Count, ConcatVideo = true });
        concatAllVideo.OutputLabels = "[vt]";

        //绘制提示文字
        var drawText = CreateStatement(",\n");
        drawText.InputLables = concatAllVideo.OutputLabels;

        //视频片断的序号是根据音频的数量来的。音频的起始是 视频数量+1
        drawText.OutputLabels = $"[vout]";
        foreach (var item in timeoutRecords1)
        {
            drawText.CreateCommand(new DrawTextOptions(Session)
            {
                X = 10,
                Y = 10,
                FontSize = 24,
                Start = item.SourceInfo.Subtitle.StartTime,
                End = item.SourceInfo.Subtitle.EndTime
            }).SetText(item.ChangeLogs);
        }
        var currentTime = drawText.CreateCommand(new DrawTextOptions(Session)
        {
            X = 10,
            Y = 300,
            FontSize = 24,
            Start = TimeSpan.Zero,
            End = TimeSpan.FromSeconds(90)
        });
        var duration = AudioSources.Max(t => t.SourceInfo.Subtitle.EndTime);

        currentTime.SetDisplayCurrentVideoTime(duration);



        //具体处理过程
        var filterComplex = string.Join(";\n", Statements.Select(t => t.GetScript()));
        filterComplex = filterComplex + $";\n[vout][aout]concat=n=1:v=1:a=1[MediaOut];";

        Console.WriteLine("==============================================================");
        Console.WriteLine(filterComplex);

        var output = Path.Combine(vi.ProjectPath, "product.mp4");
        var args = $" -map \"[MediaOut]\" -progress pipe:1";
        var basePath = Path.GetDirectoryName(output);
        //var task = RunHttp();
        //var all = AudioSources.Select(t => t.SourceInfo.Subtitle).ToArray();

        FFmpegHelper.ExecuteCommand(
            $"{GetInputVideosParameter()} {GetInputAudiosParameter()}",
            output,
            args,
            filterComplex,
            OverWriteExist,
            duration.TotalSeconds,
            basePath: basePath
            );
    }
    string GetInputVideosParameter()
    {
        return string.Join(" ", VideoSources.Select(t => $" -i {t.Path}"));
    }
    string GetInputAudiosParameter()
    {
        return string.Join(" ", AudioSources.Select(t => $" -i {t.Path}"));
    }
}



public class Label : BaseObject
{
    public Label(Session s) : base(s)
    {

    }

    public string Name
    {
        get { return GetPropertyValue<string>(nameof(Name)); }
        set { SetPropertyValue(nameof(Name), value); }
    }
}

public abstract class MediaCommand : BaseObject
{
    [Association]
    public Statement Statement
    {
        get { return GetPropertyValue<Statement>(nameof(Statement)); }
        set { SetPropertyValue(nameof(Statement), value); }
    }

    public MediaCommand(Session s) : base(s)
    {
    }

    public int Index
    {
        get { return GetPropertyValue<int>(nameof(Index)); }
        set { SetPropertyValue(nameof(Index), value); }
    }
    public abstract string GetScript();

}

public class ChangeSpeed : MediaCommand
{
    public ChangeSpeed(Session s) : base(s)
    {

    }

    public MediaType MediaType
    {
        get { return GetPropertyValue<MediaType>(nameof(MediaType)); }
        set { SetPropertyValue(nameof(MediaType), value); }
    }

    public decimal TargetSpeed
    {
        get { return GetPropertyValue<decimal>(nameof(TargetSpeed)); }
        set { SetPropertyValue(nameof(TargetSpeed), value); }
    }
    public override string GetScript()
    {
        switch (MediaType)
        {
            case MediaType.Video:
                return $"setpts=PTS*{TargetSpeed.ToString("0.0000")}";
            case MediaType.Audio:
                return $"asetpts=PTS*{TargetSpeed.ToString("0.0000")}";
            default:
                throw new NotImplementedException();
        }
        //return $"setpts=PTS/{TargetSpeed.ToString("0.0000")}";
    }
}

public abstract class TextOptionBase : MediaCommand
{
    public TextOptionBase(Session s) : base(s)
    {

    }

    //public string FontPath { get; set; } = FFmpegGlobalSettings.DefaultFont;
    //public Color BoxBorderColor { get; set; } = Color.White;

    public Color BoxBorderColor
    {
        get { return GetPropertyValue<Color>(nameof(BoxBorderColor)); }
        set { SetPropertyValue(nameof(BoxBorderColor), value); }
    }

    //public Color FontColor { get; set; } = Color.White;
    public Color FontColor
    {
        get { return GetPropertyValue<Color>(nameof(FontColor)); }
        set { SetPropertyValue(nameof(FontColor), value); }
    }

    //public bool BoxStyle { get; set; } = SubtitleBorderStyle.Box;

    public bool HasBoxBorder
    {
        get { return GetPropertyValue<bool>(nameof(HasBoxBorder)); }
        set { SetPropertyValue(nameof(HasBoxBorder), value); }
    }

    //public int BoxBorderWidth { get; set; } = 5;
    //public int BoxBorderHeight { get; set; } = 1;
    //public double BoxBorderAlpha { get; set; } = 1.0;
    //public int FontSize { get; set; } = 24;

    public int FontSize
    {
        get { return GetPropertyValue<int>(nameof(FontSize)); }
        set { SetPropertyValue(nameof(FontSize), value); }
    }

}

public class VideoSource : MediaSource
{
    public VideoSource(Session s) : base(s)
    {

    }
    public override VideoScript GetVideoScript() => VideoScript;

    [Association]
    public VideoScript VideoScript
    {
        get { return GetPropertyValue<VideoScript>(nameof(VideoScript)); }
        set { SetPropertyValue(nameof(VideoScript), value); }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        MediaType = MediaType.Video;
    }
}
public class AudioSource : MediaSource
{
    public AudioSource(Session s) : base(s)
    {

    }

    public AudioBookTextAudioItem SourceInfo
    {
        get { return GetPropertyValue<AudioBookTextAudioItem>(nameof(SourceInfo)); }
        set { SetPropertyValue(nameof(SourceInfo), value); }
    }

    public override VideoScript GetVideoScript() => VideoScript;
    [Association]
    public VideoScript VideoScript
    {
        get { return GetPropertyValue<VideoScript>(nameof(VideoScript)); }
        set { SetPropertyValue(nameof(VideoScript), value); }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        MediaType = MediaType.Audio;
    }
}
public abstract class MediaSource : BaseObject
{
    public MediaSource(Session s) : base(s)
    {

    }

    public abstract VideoScript GetVideoScript();

    public MediaType MediaType
    {
        get { return GetPropertyValue<MediaType>(nameof(MediaType)); }
        set { SetPropertyValue(nameof(MediaType), value); }
    }

    public string Path
    {
        get { return GetPropertyValue<string>(nameof(Path)); }
        set { SetPropertyValue(nameof(Path), value); }
    }
    public int Index
    {
        get { return GetPropertyValue<int>(nameof(Index)); }
        set { SetPropertyValue(nameof(Index), value); }
    }


    public string CustomLabel
    {
        get { return GetPropertyValue<string>(nameof(CustomLabel)); }
        set { SetPropertyValue(nameof(CustomLabel), value); }
    }


    public virtual string GetLabel()
    {
        if (string.IsNullOrEmpty(CustomLabel))
        {
            switch (MediaType)
            {
                case MediaType.Video:
                    return $"[{Index}:v]";
                case MediaType.Audio:
                    return $"[{Index}:a]";
                default:
                    throw new NotImplementedException();
            }
        }
        return CustomLabel;
    }

    [Size(-1)]
    public string ChangeLogs
    {
        get { return GetPropertyValue<string>(nameof(ChangeLogs)); }
        set { SetPropertyValue(nameof(ChangeLogs), value); }
    }

}

public enum MediaType
{
    Video,
    Audio
}


public class ConcatMedia : MediaCommand
{
    public ConcatMedia(Session s) : base(s)
    {

    }


    public bool ConcatVideo
    {
        get { return GetPropertyValue<bool>(nameof(ConcatVideo)); }
        set { SetPropertyValue(nameof(ConcatVideo), value); }
    }

    public bool ConcatAudio
    {
        get { return GetPropertyValue<bool>(nameof(ConcatAudio)); }
        set { SetPropertyValue(nameof(ConcatAudio), value); }
    }




    public int MediaCount
    {
        get { return GetPropertyValue<int>(nameof(MediaCount)); }
        set { SetPropertyValue(nameof(MediaCount), value); }
    }

    [Size(-1)]
    public string AddationCommand
    {
        get { return GetPropertyValue<string>(nameof(AddationCommand)); }
        set { SetPropertyValue(nameof(AddationCommand), value); }
    }


    public override string GetScript()
    {
        var rst = $"concat=n={MediaCount}:a={(ConcatAudio ? "1" : "0")}:v={(ConcatVideo ? "1" : "0")}";
        if (!string.IsNullOrEmpty(AddationCommand))
        {
            rst += $"{AddationCommand}";
        }
        return rst;
    }
}