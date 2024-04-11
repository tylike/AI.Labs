using AI.Labs.Module.BusinessObjects.VideoTranslate;
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

    public void LogTitle()
    {
        Project.Log("Clip序号,字幕开始时间,结束时间,时长,音频超时时长,变速/延时,调整后差异,操作");
    }


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
            this.End = this.Start.AddMilliseconds(this.Duration.TotalMilliseconds / 实际倍速);
            this.ChangeSpeed = 实际倍速;

            Project.DrawText(10, 10, $"音频变速:{ChangeSpeed.Value:0.0#####} 计划:{计划倍速:0.0#####} 实际:{实际倍速:0.0#####} {(计划倍速>实际倍速 ? "XXXX":"VVVV")}", 24,
                enSRt.StartTime,enSRt.EndTime);
            return 实际倍速;
        }
        else
        {
            this.ChangeSpeed = null;
            Parent.Project.DrawText(10, 10, "音频无变速", 24, enSRt.StartTime, enSRt.EndTime);
            //Project.Log($"音频:[{Index}]-[{Start}-{this.End}],无变速");
        }
        return 1;
    }

    public double 计算延时()
    {
        //如果当前音频的时长 小于 字幕时长
        if (this.Duration.TotalMilliseconds < Subtitle.Duration)
        {
            this.End = Subtitle.EndTime;
            this.Delay = Subtitle.Duration - (int)this.Duration.TotalMilliseconds;

            var text = $"延时{Delay}"; 
            Parent.Project.DrawText(10, 60, text, 24,
                TimeSpan.FromMilliseconds(Subtitle.StartTime.TotalMilliseconds + Subtitle.Duration),
                Subtitle.EndTime
                );
            return this.Delay.Value;
        }
        return 0;
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

