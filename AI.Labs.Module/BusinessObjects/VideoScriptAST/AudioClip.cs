using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.DashboardCommon;
using DevExpress.Xpo;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using VisioForge.Core.Types.MediaPlayer;

namespace AI.Labs.Module.BusinessObjects;

public class AudioClip : ClipBase<AudioClip>
{
    public AudioClip(Session s) : base(s)
    {

    }


    /// <summary>
    /// 根据字幕时长,计算音频的调整
    /// </summary>
    /// <returns></returns>
    public double 计算调速()
    {
        var enSRt = Parent.AudioInfo.Subtitle;
        //第一步:检查当前(中文音频)的时长 大于 字幕时长的,将快放中文音频,取最大1.3倍,与 “完全匹配倍速”
        if (Duration.TotalMilliseconds > Subtitle.Duration)
        {
            //计算如果播放完整,应该用多快的速度
            var 计划倍速 = (double)Duration.TotalMilliseconds / Subtitle.Duration;
            //*****************************************************************
            //完全按最快播放也不行,这样听着不舒服
            
            var 实际倍速 = Math.Min(计划倍速, 1.3d);
            var oldDuration = (int)this.Duration.TotalMilliseconds;

            this.End = this.Start.AddMilliseconds(this.Duration.TotalMilliseconds / 实际倍速);
            
            this.ChangeSpeed = 实际倍速;

            Project.DrawText(10, 10, $"音频变速:{ChangeSpeed.Value:0.0#####} 计划:{计划倍速:0.0#####} 实际:{实际倍速:0.0#####} {(计划倍速>实际倍速 ? "XXXX":"VVVV")}", 24,
                enSRt.StartTime,enSRt.EndTime);

            //字幕标准:
            ChangeLog(
                "根据字幕时间加速音频", 
                "字幕", 
                $"{Subtitle.StartTime}-{Subtitle.EndTime}:{Subtitle.Duration}",
                oldDuration, 
                (int)Duration.TotalMilliseconds, 
                计划倍速, 
                实际倍速, 
                (int)Duration.TotalMilliseconds - Subtitle.Duration);

            return 实际倍速;
        }
        else
        {
            this.ChangeSpeed = null;
            //Parent.Project.DrawText(10, 10, "音频无变速", 24, enSRt.StartTime, enSRt.EndTime);
            //Project.Log($"音频:[{Index}]-[{Start}-{this.End}],无变速");
        }
        return 1;
    }

    public double 计算延时()
    {
        //如果当前音频的时长 小于 字幕时长
        if (this.Duration.TotalMilliseconds < Subtitle.Duration)
        {
            var oldDuration = this.Duration.TotalMilliseconds;

            this.End = Subtitle.EndTime;
            this.Delay = Subtitle.Duration - (int)this.Duration.TotalMilliseconds;

            var text = $"延时{Delay}";
            Parent.Project.DrawText(10, 60, text, 24,
                TimeSpan.FromMilliseconds(Subtitle.StartTime.TotalMilliseconds + Subtitle.Duration),
                Subtitle.EndTime
                );

            ChangeLog("延时",
                "字幕",
                $"{Subtitle.StartTime}-{Subtitle.EndTime}:{Subtitle.Duration}",
                (int)oldDuration, 
                (int)Duration.TotalMilliseconds,
                0, 
                this.Delay.Value, 
                (int)Duration.TotalMilliseconds - Subtitle.Duration
                );


            return this.Delay.Value;
        }
        return 0;
    }
    public void LogTitle()
    {
        Project.Log("操作,目标类型,目标内容,序号,调整前,调整后,调整后差异,计划变速,实际变速");
    }
    public void ChangeLog(string 操作,string 目标类型,string 目标内容,int 调整前,int 调整后,double 计划,double 实际,int 调整后差异)
    {
        Project.Log($"{操作},{目标类型},{目标内容},{this.Index},{调整前},{调整后},{调整后差异},{计划:0.0####},{实际:0.0####}");
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
            delay = $",apad=pad_dur={(Delay.Value/1000d).ToString("0.0#####")}";
        }
        return $"{inputLables}asetpts=PTS*{speed.ToString("0.0#####")}{delay}{outputLables}";
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

