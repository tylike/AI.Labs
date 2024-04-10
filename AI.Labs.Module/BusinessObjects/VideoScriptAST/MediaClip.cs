using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

public class VideoScriptProject : BaseObject
{
    public VideoScriptProject(Session s):base(s)
    {
        
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

    public void CreateProject()
    {
        foreach (var item in VideoInfo.Audios)
        {
            var clip = new MediaClip(Session)
            {
                Subtitle = item,
            };
            var audioClip = new AudioClip(Session) { Parent = clip };
            clip.AudioClip = audioClip;
            var videoClip = new VideoClip(Session) { Parent = clip };
            clip.VideoClip = videoClip;
            MediaClips.Add(clip);
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


    public string Commands
    {
        get { return GetPropertyValue<string>(nameof(Commands)); }
        set { SetPropertyValue(nameof(Commands), value); }
    }

    public void Adjust()
    {
        //[input][output]
        AudioClip.快放();
        VideoClip.慢放();
        VideoClip.延时();
        AudioClip.延时();
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
    public AudioBookTextAudioItem Subtitle
    {
        get { return GetPropertyValue<AudioBookTextAudioItem>(nameof(Subtitle)); }
        set { SetPropertyValue(nameof(Subtitle), value); }
    }
}

public class VideoClip:ClipBase<VideoClip>  
{
    public VideoClip(Session s):base(s)
    {
        
    }

    internal void 延时()
    {
        throw new NotImplementedException();
    }

    internal void 慢放()
    {
        throw new NotImplementedException();
    }
}

public class AudioClip : ClipBase<AudioClip>
{
    public AudioClip(Session s) : base(s)
    {

    }

    public void 延时()
    {
        throw new NotImplementedException();
    }

    public void 快放()
    {
        var TargetSpeed = 1m;
        if( Duration.TotalMilliseconds < Parent.Subtitle.Subtitle.Duration)
        {
            TargetSpeed = Math.Min((decimal)Parent.Subtitle.Duration / Parent.Subtitle.Subtitle.Duration, 1.3m);
        }

        var command = $"asetpts=PTS*{TargetSpeed.ToString("0.0000")}";
    }
}

public class ClipBase<T> : BaseObject
{
    public ClipBase(Session s):base(s)
    {
        
    }

    public MediaClip Parent
    {
        get { return GetPropertyValue<MediaClip>(nameof(Parent)); }
        set { SetPropertyValue(nameof(Parent), value); }
    }

    public T Before
    {
        get { return GetPropertyValue<T>(nameof(Before)); }
        set { SetPropertyValue(nameof(Before), value); }
    }

    public T Next
    {
        get { return GetPropertyValue<T>(nameof(Next)); }
        set { SetPropertyValue(nameof(Next), value); }
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
    public TimeSpan Duration => End - Start;

}

