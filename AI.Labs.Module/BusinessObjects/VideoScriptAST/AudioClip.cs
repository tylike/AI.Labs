using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.DashboardCommon;
using DevExpress.Xpo;
using edu.stanford.nlp.trees.tregex.gui;
using sun.tools.tree;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using VisioForge.Core.Types.MediaPlayer;
using static com.sun.net.httpserver.Authenticator;

namespace AI.Labs.Module.BusinessObjects;

public class AudioClip : ClipBase<AudioClip>
{
    public AudioClip(Session s) : base(s)
    {

    }

    public override string GetClipType()
    {
        return "音频";
    }

    public double 计算调速()
    {
        return 计算调速(this, Parent.AudioInfo.Subtitle, this, t => Math.Min(t, 1.3d));
    }
    /// <summary>
     /// 根据target时长,计算waitAdjust的调整
     /// </summary>
     /// <returns></returns>
    public static double 计算调速(IClip waitAdjust, IClip target, ClipBase waitAdjustObject, Func<double, double> calc)
    {
        //第一步:检查当前(中文音频)的时长 大于 字幕时长的,将快放中文音频,取最大1.3倍,与 “完全匹配倍速”
        if (waitAdjust.Duration > target.Duration)
        {
            //计算如果播放完整,应该用多快的速度
            var 计划倍速 = (double)waitAdjust.Duration / target.Duration;
            //*****************************************************************
            //完全按最快播放也不行,这样听着不舒服            
            var 实际倍速 = calc(计划倍速);
            var 原时长 = waitAdjust.Duration;
            waitAdjust.EndTime = waitAdjust.StartTime.AddMilliseconds(waitAdjust.Duration / 实际倍速);
            waitAdjustObject.ChangeSpeed = 实际倍速;
            //字幕为标准:
            ChangeSpeedLog(waitAdjust, target, waitAdjustObject, 计划倍速, 原时长);
            return 实际倍速;
        }
        else
        {
            waitAdjustObject.ChangeSpeed = null;
        }
        return 1;
    }

    public double 计算延时()
    {
        return 计算延时(this,Parent.VideoClip,this);        
    }


    public string ChangeLogs
    {
        get { return GetPropertyValue<string>(nameof(ChangeLogs)); }
        set { SetPropertyValue(nameof(ChangeLogs), value); }
    }
    public string GetScript()
    {
        var inputLables = $"[{Index}:a]";
        var outputLables = $"[a{Parent.Index}]";
        //var start = Start.ToFFmpegSeconds();
        //var end = End.ToFFmpegSeconds();
        var speed = 1d;
        if(ChangeSpeed.HasValue && ChangeSpeed.Value>0)
        {
            speed = ChangeSpeed.Value;
        }

        var delay = "";
        if(Delay>0)
        {
            delay = $",apad=pad_dur={Delay.Value / 1000d:0.0#####}";
        }
        return $"{inputLables}asetpts=PTS*{speed:0.0#####}{delay}{outputLables}";
    }
    public override string GetOutputLabel()
    {
        return $"[a{Parent.Index}]";
    }
}

public class ClipLog : XPObject
{
    public ClipLog(Session s) : base(s)
    {

    }

    [Association]
    public ClipBase Clip
    {
        get { return GetPropertyValue<ClipBase>(nameof(Clip)); }
        set { SetPropertyValue(nameof(Clip), value); }
    }

    [Size(-1)]
    public string 片断信息
    {
        get { return GetPropertyValue<string>(nameof(片断信息)); }
        set { SetPropertyValue(nameof(片断信息), value); }
    }

    public string 变更内容
    {
        get { return GetPropertyValue<string>(nameof(变更内容)); }
        set { SetPropertyValue(nameof(变更内容), value); }
    }

    public int 变更后差异
    {
        get { return GetPropertyValue<int>(nameof(变更后差异)); }
        set { SetPropertyValue(nameof(变更后差异), value); }
    }

    //原字幕时长,,变更内容,      变更后时长,变更后差异
    //           如:变速、快放     1000ms ,    100ms
    //字幕(时间)时长:900       不变
    //音频(时间)时长:1000      快放       1000ms->900ms 0ms
    //视频(时间)时长:900       不变

}

