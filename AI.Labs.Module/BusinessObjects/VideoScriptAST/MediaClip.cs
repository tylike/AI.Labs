using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

public class MediaClip : BaseObject
{
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

    public VideoClip CreateVideoClip()
    {
#pragma warning disable CS0618 // 类型或成员已过时
        this.VideoClip = new VideoClip(Session)
        {
            Start = this.AudioInfo.Subtitle.StartTime,
            End = this.AudioInfo.Subtitle.EndTime,
            Index = this.AudioInfo.Subtitle.Index,
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
            Start = this.AudioInfo.Subtitle.StartTime,
            End = TimeSpan.FromMilliseconds(this.AudioInfo.Subtitle.StartTime.TotalMilliseconds + this.AudioInfo.Duration)
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
}

