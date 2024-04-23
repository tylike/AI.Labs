using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.DashboardCommon;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Xpo;
using edu.stanford.nlp.trees.tregex.gui;
using sun.tools.tree;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Security.AccessControl;
using System.Text;
using VisioForge.Core.Types.MediaPlayer;
using static com.sun.net.httpserver.Authenticator;

namespace AI.Labs.Module.BusinessObjects;

//
public class AudioClip : ClipBase<AudioClip>
{

    public AudioClip(Session s) : base(s)
    {

    }

    public override string GetClipType()
    {
        return "音频";
    }

    #region 调速
    public string 计算调速()
    {

        var target = Parent.AudioInfo.Subtitle;
        IClip waitAdjust = this;
        ClipBase waitAdjustObject = this;
        //第一步:检查当前(中文音频)的时长 大于 字幕时长的,将快放中文音频,取最大1.3倍,与 “完全匹配倍速”
        if (!ChangeSpeed.HasValue && waitAdjust.Duration > target.Duration)
        {
            log.WriteLine($"原音频时长:{waitAdjust.Duration} > 原字幕时长:{target.Duration} = 差异:{waitAdjust.Duration - target.Duration}ms");
            //计算如果播放完整,应该用多快的速度
            var planSource = ((double)waitAdjust.Duration / target.Duration);
            var 计划倍速 = planSource.RoundUp(3);
            log.WriteLine($"计划倍速:{planSource } {计划倍速} ");
            var 实际倍速 = 计划倍速;
            var 调整成功 = false;
            if (计划倍速 > 1.3)
            {

                实际倍速 = 1.3;
            }
            else
            {
                实际倍速 = 计划倍速;
                调整成功 = true;
            }
            log.WriteLine($"实际倍速:{实际倍速}");
            log.WriteLine($"调整成功:{调整成功}-没有使用最大倍数,所以认为快速播放后一定与字幕是匹配的");

            //*****************************************************************
            //完全按最快播放也不行,这样听着不舒服            

            var 原时长 = waitAdjust.Duration;

            var newOutputFile = GetFilePath(FileType.Audio_ChangeSpeed, 实际倍速);
            FFmpegHelper.ChangeAudioSpeed(waitAdjustObject.Index.ToString(), target.Duration, waitAdjustObject.OutputFile, 实际倍速, newOutputFile, 计划倍速);
                            

            //log.WriteLine($"命令:{useCommand}");

            waitAdjustObject.使用文件时长更新结束时间(newOutputFile);

            //Parent.Duration = (int)waitAdjustObject.FileDuration.Value;

            waitAdjustObject.ChangeSpeed = 实际倍速;
            log.WriteLine($"调整后音频时长:{waitAdjust.Duration} - 字幕时长:{target.Duration} = 差异:{waitAdjust.Duration - target.Duration}ms");
            ChangeSpeedLog(waitAdjust, target, waitAdjustObject, 计划倍速, 原时长);
            //字幕为标准:
            //调整后，音频还是太长，那么就停止调整了，将去调整视频:
            //1.1 音频时长 >  字幕时长:调整字幕、视频
            if (!调整成功)
            {
                //Subtitle.FixedStartTime = this.StartTime;
                //Subtitle.SetFixedEndTime(this.EndTime, Parent);
                //Parent.VideoClip.StartTime = StartTime;
                //Parent.VideoClip.EndTime = EndTime;
                //后推时间(waitAdjust.Duration - target.Duration);
                log.WriteLine($"clip.End {Parent.Duration} => {this.FileDuration} 差异:{this.FileDuration - Parent.Duration}");
                log.WriteLine("使用音频时长做为字幕的时长.");
                //Parent.Duration = (int)waitAdjustObject.FileDuration.Value;
                //Parent.End = this.EndTime;
            }
            else
            {
                log.WriteLine($"clip.End {Parent.Duration} => {Subtitle.Duration} 差异:{Subtitle.Duration - Parent.Duration}");                
                log.WriteLine("使用字幕时长做为音频的时长.");
                //Parent.Duration = Subtitle.Duration;
            }
            return log.ToString();
        }
        return null;
    }

    /// <summary>
    /// 将当前片断后面的所有片断后推
    /// </summary>
    /// <param name="后推时间ms"></param>
    public void 后推时间(double 后推时间ms)
    {
        if (后推时间ms > 0)
        {
            var next = this.Parent.VideoClip.Next;
            while (next != null)
            {
                next.Subtitle.FixedStartTime = next.Subtitle.FixedStartTime.AddMilliseconds(后推时间ms);
                next.Subtitle.SetFixedEndTime(next.Subtitle.FixedEndTime.AddMilliseconds(后推时间ms), this.Parent);


                next.Parent.AudioClip.StartTime = next.Parent.AudioClip.StartTime.AddMilliseconds(后推时间ms);
                next.Parent.AudioClip.EndTime = next.Parent.AudioClip.EndTime.AddMilliseconds(后推时间ms);

                next.StartTime = next.StartTime.AddMilliseconds(后推时间ms);
                next.EndTime = next.EndTime.AddMilliseconds(后推时间ms);

                next.TextLogs += $"S+{后推时间ms};";
                next.Parent.Commands += "音频后移" + 后推时间ms + ";";
                next = next.Next;
            }
        }
    }

    #endregion
    #region 延时
    public void 计算延时()
    {
        //return 计算延时(this, Parent.VideoClip, this);
        //var waitAdjustType = waitAdjust.GetClipType();
        //var targetType = "Clip";
        if (Subtitle.Duration > this.FileDuration)
        {
            log.WriteLine($"原字幕时长:{Subtitle.Duration} > 原音频时长:{FileDuration} = 差异:{Subtitle.Duration - FileDuration}ms");
            //var oldDuration = waitAdjust.Duration;
            //var oldEnd = waitAdjust.EndTime;
            //waitAdjustObject.Delay = Parent.Duration - oldDuration;
            this.Delay = (int)(Subtitle.Duration - FileDuration.Value);
            RunDelay(Delay.Value, Subtitle.Duration);
        }
    }

    public override string RunDelay(int delay, double targetDuration)
    {
        var oldDuration = Parent.Duration;
        var newOutputFile = GetFilePath(FileType.Audio_Delay, delay);
        FFmpegHelper.DelayAudio(this.Index.ToString(), targetDuration, this.OutputFile, delay, newOutputFile);
        使用文件时长更新结束时间(newOutputFile);
        
        Parent.Duration = (int)FileDuration.Value;

        //log.WriteLine($"音频延时{delay}:{oldDuration}=>{Parent.Duration} = 差异 {Parent.Duration - oldDuration}");

        //使用音频时长 更新字幕时长
        //因为音频时长可能是根据ffmpeg的结果而来，可能不是准确的期望
        //Subtitle.SetFixedEndTime(EndTime, Parent);
        return $"音频延时{delay}ms";
    }
    #endregion


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
        if (ChangeSpeed.HasValue && ChangeSpeed.Value > 0)
        {
            speed = ChangeSpeed.Value;
        }

        var delay = "";
        if (Delay > 0)
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

