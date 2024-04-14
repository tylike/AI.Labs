using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using Humanizer;

namespace AI.Labs.Module.BusinessObjects;



public abstract class ClipBase : BaseObject
{
    public ClipBase(Session s):base(s)
    {
    }
    /// <summary>
    /// 代表了输入audio的序号
    /// 也将代表输出标签时使用
    /// audio的序号是在视频序号的基础上加上的
    /// 即输入文件时的序号
    /// </summary>
    public int Index
    {
        get { return GetPropertyValue<int>(nameof(Index)); }
        set { SetPropertyValue(nameof(Index), value); }
    }
    public MediaClip Parent
    {
        get { return GetPropertyValue<MediaClip>(nameof(Parent)); }
        set { SetPropertyValue(nameof(Parent), value); }
    }

    public VideoScriptProject Project => Parent?.Project;
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

    public abstract string GetOutputLabel();

    public int? Delay
    {
        get { return GetPropertyValue<int?>(nameof(Delay)); }
        set { SetPropertyValue(nameof(Delay), value); }
    }
    public double? ChangeSpeed
    {
        get { return GetPropertyValue<double?>(nameof(ChangeSpeed)); }
        set { SetPropertyValue(nameof(ChangeSpeed), value); }
    }
    public SubtitleItem Subtitle => Parent.Subtitle;

    [Association,Aggregated]
    public XPCollection<ClipLog> Logs
    {
        get=>GetCollection<ClipLog>(nameof(Logs));
    }

}
public abstract class ClipBase<T> : ClipBase
{
    public ClipBase(Session s):base(s)
    {
        
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
    //protected override void OnChanged(string propertyName, object oldValue, object newValue)
    //{
    //    base.OnChanged(propertyName, oldValue, newValue);
    //    if (!IsLoading)
    //    {
    //        if (nameof(ChangeSpeed) == propertyName)
    //        {
    //            LogChangeSpeed();
    //        }
    //        if (nameof(Delay) == propertyName)
    //        {
    //            LogDelay();
    //        }
    //    }
    //}


}

