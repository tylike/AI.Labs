using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.DashboardCommon;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.Xpo;
using edu.stanford.nlp.trees.tregex.gui;
using sun.tools.tree;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Security.AccessControl;
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
    public double 计算调速()
    {
        return 计算调速(Parent.AudioInfo.Subtitle);
    }
    /// <summary>
    /// 根据target时长,计算waitAdjust的调整
    /// </summary>
    /// <returns></returns>
    public double 计算调速(IClip target)
    {
        IClip waitAdjust = this;
        ClipBase waitAdjustObject = this;
        //第一步:检查当前(中文音频)的时长 大于 字幕时长的,将快放中文音频,取最大1.3倍,与 “完全匹配倍速”
        if (!ChangeSpeed.HasValue && waitAdjust.Duration > target.Duration)
        {
            //计算如果播放完整,应该用多快的速度
            var 计划倍速 = (double)waitAdjust.Duration / target.Duration;
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

            //*****************************************************************
            //完全按最快播放也不行,这样听着不舒服            

            var 原时长 = waitAdjust.Duration;

            var newOutputFile = GetFilePath(FileType.Audio_ChangeSpeed, 实际倍速);
            FFmpegHelper.ChangeAudioSpeed(waitAdjustObject.Index.ToString(), target.Duration / 1000d, waitAdjustObject.OutputFile, 实际倍速, newOutputFile, 计划倍速);
            waitAdjustObject.使用文件时长更新结束时间(newOutputFile);

            waitAdjustObject.ChangeSpeed = 实际倍速;

            ChangeSpeedLog(waitAdjust, target, waitAdjustObject, 计划倍速, 原时长);
            //字幕为标准:
            //调整后，音频还是太长，那么就停止调整了，将去调整视频:
            //1.1 音频时长 >  字幕时长:调整字幕、视频
            if (!调整成功)
            {
                Subtitle.FixedStartTime = this.StartTime;

                Subtitle.SetFixedEndTime(this.EndTime, Parent);

                Parent.VideoClip.StartTime = StartTime;
                Parent.VideoClip.EndTime = EndTime;
                后推时间(waitAdjust.Duration - target.Duration);
            }
            else
            {
                this.StartTime = Subtitle.FixedStartTime;
                this.EndTime = Subtitle.FixedEndTime;
            }
            
            //1.2 音频时长 <= 字幕时长:调整音频、视频 为 字幕 时长
            //1.2.1 音频时长 <  字幕时长:调整音频、视频 为 字幕 时长
            //1.2.2 音频时长 =  字幕时长:调整音频、视频 为 字幕 时长

            return 实际倍速;
        }
        return 1;
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
                next.Subtitle.SetFixedEndTime(next.Subtitle.FixedEndTime.AddMilliseconds(后推时间ms),this.Parent);


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
    public double 计算延时()
    {
        return 计算延时(this, Parent.VideoClip, this);
    }

    public override void RunDelay(int delay, double targetDuration)
    {
        var newOutputFile = GetFilePath(FileType.Audio_Delay, delay);
        FFmpegHelper.DelayAudio(this.Index.ToString(), targetDuration, this.OutputFile, delay, newOutputFile);
        使用文件时长更新结束时间(newOutputFile);
        //使用音频时长 更新字幕时长
        //因为音频时长可能是根据ffmpeg的结果而来，可能不是准确的期望
        Subtitle.SetFixedEndTime(EndTime, Parent);
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

