using DevExpress.Xpo;
using edu.stanford.nlp.trees.tregex.gui;
using sun.security.provider;
using System.Drawing.Drawing2D;
using VisioForge.Core.Types.MediaPlayer;
using static com.sun.net.httpserver.Authenticator;

namespace AI.Labs.Module.BusinessObjects;

public class VideoClip : ClipBase<VideoClip>
{
    [Obsolete("使用CreateVideoClip显示参数根建此对象,防止丢失必要参数!")]
    public VideoClip(Session s) : base(s)
    {

    }

    public string GetScript()
    {
        //加文字说明
        //item.ChangeLogs = @$"
        //音频时长:{item.SourceInfo.Duration}
        //字幕时长:{item.SourceInfo.Subtitle.Duration}
        //视频调整速度:{实际速度.ToString("0.0000")}";

        var inputLables = "[0:v]";
        var outputLables = $"[v{Parent.Index}]";
        var start = StartTime.ToFFmpegSeconds();
        var end = EndTime.ToFFmpegSeconds();
        //setpts=PTS-STARTPTS 是修复视频时间戳，防止静止画面的出现
        var setpts = "setpts=PTS-STARTPTS";
        if (ChangeSpeed.HasValue && ChangeSpeed.Value != 1.0)
        {
            setpts = $"setpts={ChangeSpeed.Value.ToString("0.0#####")}*PTS,setpts=PTS-STARTPTS";
        }
        return $"{inputLables}trim={start}:{end},{setpts}{outputLables}";
    }

    public override string GetOutputLabel()
    {
        return $"[v{Parent.Index}]";
    }
    /// <summary>
    /// 根据音频时长调整视频时长
    /// 因为音频时长可能更长
    /// </summary>
    /// <returns></returns>
    public double 计算延时()
    {
        var rst = 计算延时(this, Parent.AudioClip, this);
        //视频延长后,字幕需要后移
        //由于延长了视频，所以把字幕时间与视频时间同步
        if (rst > 0)
        {
            var next = this.Next;
            while (next != null)
            {
                next.Subtitle.StartTime = next.Subtitle.StartTime.AddMilliseconds(rst);
                next.Subtitle.EndTime = next.Subtitle.EndTime.AddMilliseconds(rst);

                next.Parent.AudioClip.StartTime = next.Parent.AudioClip.StartTime.AddMilliseconds(rst);
                next.Parent.AudioClip.EndTime = next.Parent.AudioClip.EndTime.AddMilliseconds(rst);

                next.Parent.VideoClip.StartTime = next.Parent.VideoClip.StartTime.AddMilliseconds(rst);
                next.Parent.VideoClip.EndTime = next.Parent.VideoClip.EndTime.AddMilliseconds(rst);

                next.TextLogs += $"S+{rst};";
                next.Parent.Commands +="音频后移"+rst+";";
                next = next.Next;
            }
        }
        

        return rst;
        ////如果音频的时长 大于 当前视频的时长
        //var diff = (int)(Parent.AudioClip.Duration - Duration).TotalMilliseconds;
        //if (diff > 10)
        //{
        //    var oldDuration = (int)this.Duration.TotalMilliseconds;
        //    var oldEnd = this.EndTime;
        //    //延时：音频时长-视频时长
        //    this.Delay = diff;

        //    this.EndTime = this.EndTime.AddMilliseconds(Delay.Value);

        //    var text = $"视频延时{Delay}";
        //    //显示调试信息：
        //    //开始时间:原视频结束时间
        //    //结束时间:原视频结束时间+延时
        //    Parent.Project.DrawText(500, 60, text, 24,
        //        EndTime,
        //        EndTime.Add(TimeSpan.FromMilliseconds(Delay.Value))
        //        );
        //    return this.Delay.Value;
        //}
        //return 0;
    }

    public override string GetClipType()
    {
        return "视频";
    }

    /// <summary>
    /// 根据音频时长调整视频时长
    /// 因为音频时长可能更长
    /// </summary>
    /// <returns></returns>
    public double 计算调速()
    {
        //计算如果播放完整,应该用多快的速度
        //*****************************************************************
        IClip waitAdjust = this;
        IClip target = Parent.AudioClip;
        if (target.Duration > waitAdjust.Duration)
        {
            var 计划倍速 = waitAdjust.Duration / (double)target.Duration;
            var 实际倍速 = Math.Max(计划倍速, 0.7d);

            var oldDuration = (int)Duration.TotalMilliseconds;
            var oldEnd = this.EndTime;
            EndTime = waitAdjust.StartTime.AddMilliseconds(waitAdjust.Duration / 实际倍速);
            ChangeSpeed = 实际倍速;
            ChangeSpeedLog(waitAdjust, target, this, 计划倍速, oldDuration);

            return ChangeSpeed.Value;
        }
        else
        {
            //Parent.Project.DrawText("w-tw", "0", $"视频无变速", 24, Parent.AudioInfo.Subtitle.StartTime, Parent.AudioInfo.Subtitle.EndTime);
        }
        return 1;
    }

    //public void LogTitle()
    //{
    //    Project.Log("操作,目标类型,目标内容,序号,调整前,调整后,调整后差异,计划变速,实际变速");
    //}
    //public void ChangeLog(string 操作, string 目标类型, string 目标内容, int 调整前, int 调整后, double 计划, double 实际, int 调整后差异)
    //{
    //    Project.Log($"{操作},{目标类型},{目标内容},{this.Index},{调整前},{调整后},{调整后差异},{计划:0.0####},{实际:0.0####}");
    //}

    //要在视频片段前添加2秒的黑屏,可以使用color滤镜生成一个黑色视频,然后使用concat滤镜将其与原视频连接起来:

    //[0:v] trim=10.0:15.0,setpts=0.7*PTS,setpts=PTS-STARTPTS[v];
    //color=c=black:s=1280x720:d=2[black];
    //[black]
    //    [v]
    //    concat[v3]
    //要在视频片段后延长最后一帧显示3秒, 可以使用tpad滤镜:

    //[0:v] trim=10.0:15.0,setpts=0.7*PTS,setpts=PTS-STARTPTS,tpad=stop_mode=clone:stop_duration=3[v3]
    //    tpad的stop_mode=clone表示复制最后一帧,stop_duration=3表示延长3秒。

    //要复制10秒到15秒之间的内容,可以再次使用trim滤镜,然后用concat连接:

    //[0:v] trim=10.0:15.0,setpts=0.7*PTS,setpts=PTS-STARTPTS[v];
    //[0:v] trim=11.0:15.0,setpts=PTS-STARTPTS[v2];
    //[v]
    //    [v2]
    //    concat[v3]
    //第一个trim滤镜截取10秒到15秒的片段并加速,第二个trim滤镜截取11秒到15秒的片段(即要复制的部分),然后使用concat将两个片段连接起来。

    //完整的示例命令如下:


    //ffmpeg -i input.mp4 -filter_complex "
    //    [0:v] trim= 10.0:15.0, setpts= 0.7 * PTS, setpts= PTS - STARTPTS[v];
    //    color=c=black:s=1280x720:d=2[black];
    //    [black][v] concat[v3];

    //    [0:v] trim=10.0:15.0,setpts=0.7*PTS,setpts=PTS-STARTPTS,tpad=stop_mode=clone:stop_duration=3[v4];

    //    [0:v] trim=10.0:15.0,setpts=0.7*PTS,setpts=PTS-STARTPTS[v5];
    //    [0:v] trim=11.0:15.0,setpts=PTS-STARTPTS[v6];
    //    [v5][v6] concat[v7]
    //" -map "[v3]" output1.mp4 -map "[v4]" output2.mp4 -map "[v7]" output3.mp4
    //这个命令将生成三个输出文件:

    //output1.mp4: 在片段前添加2秒黑屏的视频
    //output2.mp4: 在片段后延长最后一帧3秒的视频
    //output3.mp4: 复制11秒到15秒内容的视频
    //请根据你的实际需求调整输入文件名、时间范围、视频尺寸等参数。
}

