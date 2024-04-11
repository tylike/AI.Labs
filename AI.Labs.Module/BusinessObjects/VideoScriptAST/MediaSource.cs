using AI.Labs.Module.BusinessObjects.VideoScriptAST.V3;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using System.Drawing;

namespace AI.Labs.Module.BusinessObjects;

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
