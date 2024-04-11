using AI.Labs.Module.BusinessObjects.VideoScriptAST.V3;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects;

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

    [Association]
    public VideoScriptProject VideoScriptProject
    {
        get { return GetPropertyValue<VideoScriptProject>(nameof(VideoScriptProject)); }
        set { SetPropertyValue(nameof(VideoScriptProject), value); }
    }


    public override void AfterConstruction()
    {
        base.AfterConstruction();
        MediaType = MediaType.Video;
    }
}
