using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

[VisibleInDashboards]
public class MediaClip : BaseObject
{
    [Appearance("结束时间.字幕=音频", TargetItems = "AudioClip.EndTime", BackColor = "Red")]
    public bool EndTimeError()
    {
        return Subtitle.FixedEndTime != this.AudioClip.EndTime;
    }

    [Appearance("开始时间.字幕=音频", TargetItems = "AudioClip.StartTime", BackColor = "Red")]
    public bool StartTimeError()
    {
        return Subtitle.FixedStartTime != this.AudioClip.StartTime;
    }


    [Association]
    public VideoScriptProject Project
    {
        get { return GetPropertyValue<VideoScriptProject>(nameof(Project)); }
        set { SetPropertyValue(nameof(Project), value); }
    }

    public MediaClip(Session s) : base(s)
    {

    }


    public int Index
    {
        get { return GetPropertyValue<int>(nameof(Index)); }
        set { SetPropertyValue(nameof(Index), value); }
    }



    public string Commands
    {
        get { return GetPropertyValue<string>(nameof(Commands)); }
        set { SetPropertyValue(nameof(Commands), value); }
    }

    public VideoClip CreateVideoClip(string videoFile)
    {
#pragma warning disable CS0618 // 类型或成员已过时
        this.VideoClip = new VideoClip(Session)
        {
            StartTime = this.AudioInfo.Subtitle.StartTime,
            EndTime = this.AudioInfo.Subtitle.EndTime,
            Index = this.AudioInfo.Subtitle.Index,
            OutputFile = videoFile,
            Parent = this
        };
#pragma warning restore CS0618 // 类型或成员已过时
        
        return this.VideoClip;
    }

    public MediaSource Source
    {
        get { return GetPropertyValue<MediaSource>(nameof(Source)); }
        set { SetPropertyValue(nameof(Source), value); }
    }

    public TimeSpan Start
    {
        get { return GetPropertyValue<TimeSpan>(nameof(Start)); }
        set { SetPropertyValue(nameof(Start), value); }
    }

    public TimeSpan End
    {
        get { return GetPropertyValue<TimeSpan>(nameof(End)); }
        set { SetPropertyValue(nameof(End), value); }
    }

    public string GetLabel()
    {
        return Source.GetLabel();
    }

    public AudioClip CreateAudioClip()
    {
        this.AudioClip = new AudioClip(Session)
        {
            Parent = this,
            Index = this.AudioInfo.Index + Project.VideoSources.Count,
            StartTime = this.AudioInfo.Subtitle.StartTime,
            EndTime = TimeSpan.FromMilliseconds(this.AudioInfo.Subtitle.StartTime.TotalMilliseconds + this.AudioInfo.Duration),
            OutputFile = this.AudioInfo.OutputFileName,
            FileDuration = FFmpegHelper.GetDuration(this.AudioInfo.OutputFileName)
        };

        return AudioClip;
    }

    public VideoClip VideoClip
    {
        get { return GetPropertyValue<VideoClip>(nameof(VideoClip)); }
        set { SetPropertyValue(nameof(VideoClip), value); }
    }
    public AudioClip AudioClip
    {
        get { return GetPropertyValue<AudioClip>(nameof(AudioClip)); }
        set { SetPropertyValue(nameof(AudioClip), value); }
    }
    public AudioBookTextAudioItem AudioInfo
    {
        get { return GetPropertyValue<AudioBookTextAudioItem>(nameof(AudioInfo)); }
        set { SetPropertyValue(nameof(AudioInfo), value); }
    }
    public SubtitleItem Subtitle => AudioInfo.Subtitle;

    /// <summary>
    /// 将当前片断后面的所有片断后推
    /// </summary>
    /// <param name="后推时间ms"></param>
    public void 后推时间(double 后推时间ms)
    {
        if (后推时间ms > 0)
        {
            var next = this.VideoClip.Next;
            while (next != null)
            {
                next.Subtitle.FixedStartTime = next.Subtitle.FixedStartTime.AddMilliseconds(后推时间ms);
                next.Subtitle.SetFixedEndTime(next.Subtitle.FixedEndTime.AddMilliseconds(后推时间ms), this);

                next.Parent.AudioClip.StartTime = next.Parent.AudioClip.StartTime.AddMilliseconds(后推时间ms);
                next.Parent.AudioClip.EndTime = next.Parent.AudioClip.EndTime.AddMilliseconds(后推时间ms);

                next.StartTime = next.StartTime.AddMilliseconds(后推时间ms);
                next.EndTime = next.EndTime.AddMilliseconds(后推时间ms);

                next.TextLogs += $"S+{后推时间ms};";
                next.Parent.Commands += "音频后移" + 后推时间ms + ";";
                next = next.Next;
            }
        }
    }
}

