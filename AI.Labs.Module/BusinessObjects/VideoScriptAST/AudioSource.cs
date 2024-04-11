using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoScriptAST.V3;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

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
    [Association]
    public VideoScriptProject VideoScriptProject
    {
        get { return GetPropertyValue<VideoScriptProject>(nameof(VideoScriptProject)); }
        set { SetPropertyValue(nameof(VideoScriptProject), value); }
    }

    public override void AfterConstruction()
    {
        base.AfterConstruction();
        MediaType = MediaType.Audio;
    }
}
