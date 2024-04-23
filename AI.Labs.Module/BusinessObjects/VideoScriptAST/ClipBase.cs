using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DiffPlex.DiffBuilder.Model;
using Humanizer;
using System.Text;

namespace AI.Labs.Module.BusinessObjects;

public interface IClip
{
    [ModelDefault("DisplayFormat", @"hh\:mm\:ss\.fff")]
    public TimeSpan StartTime { get; set; }

    [ModelDefault("DisplayFormat", @"hh\:mm\:ss\.fff")]
    public TimeSpan EndTime { get; set; }

   

    /// <summary>
    /// 毫秒
    /// </summary>
    [ModelDefault("DisplayFormat", @"0.0##")]
    public int Duration { get => (int)(EndTime - StartTime).TotalMilliseconds; }

    string GetClipType();
}

public abstract class ClipBase : BaseObject, IClip
{
    public ClipBase(Session s) : base(s)
    {
    }

    [Size(-1)]
    [ModelDefault("RowCount", "0")]
    public string OutputFile
    {
        get { return GetPropertyValue<string>(nameof(OutputFile)); }
        set { SetPropertyValue(nameof(OutputFile), value); }
    }

    public void 使用文件时长更新结束时间(string outputFile)
    {
        this.OutputFile = outputFile;
        FileDuration = FFmpegHelper.GetDuration(outputFile);
        this.EndTime = this.StartTime.AddMilliseconds(FileDuration.Value);
    }

    public double? FileDuration
    {
        get { return GetPropertyValue<double>(nameof(FileDuration)); }
        set { SetPropertyValue(nameof(FileDuration), value); }
    }

    public int GetDuration()
    {
        return (this as IClip).Duration;
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

    [ModelDefault("DisplayFormat", @"hh\:mm\:ss\.fff")]
    public TimeSpan StartTime
    {
        get { return GetPropertyValue<TimeSpan>(nameof(StartTime)); }
        set { SetPropertyValue(nameof(StartTime), value); }
    }

    [ModelDefault("DisplayFormat", @"hh\:mm\:ss\.fff")]
    public TimeSpan EndTime
    {
        get { return GetPropertyValue<TimeSpan>(nameof(EndTime)); }
        set { SetPropertyValue(nameof(EndTime), value); }
    }
        
    //public int Duration => (this as IClip).Duration;


    public abstract string GetOutputLabel();
    public int? Delay
    {
        get { return GetPropertyValue<int?>(nameof(Delay)); }
        set { SetPropertyValue(nameof(Delay), value); }
    }
    [ModelDefault("DisplayFormat", @"0.0##")]
    public double? ChangeSpeed
    {
        get { return GetPropertyValue<double?>(nameof(ChangeSpeed)); }
        set { SetPropertyValue(nameof(ChangeSpeed), value); }
    }
    public SubtitleItem Subtitle => Parent.Subtitle;

    [Association, Aggregated]
    public XPCollection<ClipLog> Logs
    {
        get => GetCollection<ClipLog>(nameof(Logs));
    }

    [Size(-1)]
    public string TextLogs
    {
        get { return GetPropertyValue<string>(nameof(TextLogs)); }
        set { SetPropertyValue(nameof(TextLogs), value); }
    }

    public void LogTitle()
    {
        Project.Log("操作,目标类型,目标内容,序号,调整前,调整后,调整后差异,计划变速,实际变速");
    }
    public void ChangeLog(string 操作, string 目标类型, string 目标内容, string 成功)
    {
        Project.Log($"{操作},{目标类型},{目标内容},{this.Index},{成功}");
    }

    protected static void ChangeSpeedLog(IClip waitAdjust, IClip target, ClipBase waitAdjustObject, double 计划倍速, int 原时长)
    {
        var changeType = 计划倍速 > 1 ? "加速" : "减速";

        //调整后的与目标计算差异
        var diff = 0;
        if (计划倍速 > 1)
        {
            //加速
            diff = waitAdjust.Duration - target.Duration;
        }
        else
        {
            diff = target.Duration - waitAdjust.Duration;
        }


        var success = "成功!";
        if (diff > 0)
        {
            success = "仍需调整";
        }
        string waitAdjustType = waitAdjust.GetClipType();
        string targetType = target.GetClipType();
        var logText = $"{waitAdjustType}{changeType}:{targetType}时间:{target.StartTime.GetTimeString()}-{target.EndTime.GetTimeString()}，" +
            $"时长:{target.Duration}，计划将{waitAdjustType}从{原时长}ms=>{target.Duration}ms({计划倍速:0.###}X)，" +
            $"实际:{(int)waitAdjust.Duration}，" +
            $"|差异:{diff} {success}";
        waitAdjustObject.TextLogs += logText;


        waitAdjustObject.ChangeLog(
            $"根据{targetType}时间{changeType}{waitAdjustType}",
            $"{targetType}",
            logText,
            success
           );
    }

    public StreamWriter log => Parent.LogWriter;

    public void 计算延时(IClip waitAdjust, IClip targetx, ClipBase waitAdjustObject)
    {
        var waitAdjustType = waitAdjust.GetClipType();
        var targetType = "Clip";
        if ( Parent.Duration > this.FileDuration )
        {
            log.WriteLine($"原{waitAdjustType}时长:{waitAdjust.Duration} > 原{targetType}时长:{Parent.Duration} = 差异:{waitAdjust.Duration - Parent.Duration}ms");
            var oldDuration = waitAdjust.Duration;
            var oldEnd = waitAdjust.EndTime;

            waitAdjustObject.Delay = Parent.Duration - oldDuration;
            RunDelay(waitAdjustObject.Delay.Value, Parent.Duration);
        }
    }
    public virtual string RunDelay(int delay,double targetDuration)
    {
        return "";
    }

    public abstract string GetClipType();
    public virtual string GetFilePath(FileType fileType, double parameter)
    {
        return Project.GetFilePath(fileType, parameter, this.Index);
    }
    
}
public abstract class ClipBase<T> : ClipBase
{
    public ClipBase(Session s) : base(s)
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

