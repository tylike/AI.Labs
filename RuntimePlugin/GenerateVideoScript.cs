using AI.Labs.Module;
using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.CodeParser;
using DevExpress.ExpressApp;
using IPlugins;
using sun.security.provider;
using System.Diagnostics;
using System.Text;
using YoutubeExplode.Search;
using static edu.stanford.nlp.io.EncodingPrintWriter;
using System.Linq;
using DevExpress.Charts.Native;
using DevExpress.Pdf;
using YoutubeExplode.Videos;
using javax.swing.text;
using DevExpress.XtraSpreadsheet.Model;
using NAudio.Dmo;
using jdk.nashorn.@internal.ir;
using AI.Labs.Module.BusinessObjects.AudioBooks;

namespace RuntimePlugin;


public class GenerateVideoScript : IPlugin<VideoInfo>, IDisposable
{
    //ffmpeg 使用 filter_complex,如何实现：
    //1.视频逻辑上分有几个段
    //A:  0至3秒.
    //B: 3-5秒
    //C: 5-7秒
    //D: 7-12秒。
    //需要命令1：
    //将A延长到4秒。新增的1秒使用A段的最后一帧。不处理音频。
    //需要命令2:
    //将B段由原来的2秒慢放到4秒。不处理音频。
    //需要命令3：
    //将C段的音频由原来的2秒快放到1.75秒。
    //如果你想在每条命令中使用`filter_complex`，你可以在单个命令中使用多个`[stream_name]`和`[stream_name]...`，每个部分单独定义你想要处理的视频或音频流。以下是将三个需求分割到三个不同的`filter_complex`中：

    //1. 对A段（0-3秒）延长1秒：
    //```bash
    //ffmpeg -i input_video.mp4 -filter_complex "[0:v]select='between(t,0,3)',pad=iw:s=ih,format=mp4" -t 4 -c:v libx264 -map "[0:v]" output_A.mp4
    //```
    //这里，`[0:v]`表示视频流，只处理视频部分。
    //2. 对B段（3-5秒）慢放到4秒：
    //```bash
    //ffmpeg -i input_video.mp4 -filter_complex "[1:v]select='between(t,3,5)',setpts=PTS-STARTPTS*2,format=mp4" -c:v libx264 -map "[1:v]" -map "[0:a]" output_B.mp4
    //```
    //这里，`[1:v]`处理B段的视频，`[0:a]`保留原始音频，因为音频不需要处理。
    //3. 对C段（5-7秒）的音频快放到1.75秒：
    //```bash
    //ffmpeg -i input_video.mp4 -filter_complex "[2:a]aselect='between(t,5,7)',atrim=end=1.75,aresample=rate=1.75" -c:a aac output_C_audio.mp3
    //```
    //`[2:a]`处理C段的音频。
    //注意，这些命令将视频和音频流分别处理并输出，你需要将`input_video.mp4`替换为实际的输入文件名，输出文件名根据需要调整。


    void GetScript()
    {

        ////写一个c#程序，帮助拼接ffmpeg命令的参数：
        ////整体的任务目标是将英文视频翻译为中文。
        ////其中，英文字幕已经有了。中文也已经翻译好了。中文语音也生成好了。
        ////现在要做的是，使用视频内容和中文语音尽量对齐。
        ////1.仍然将参数生成到filterComplexScript.txt中去。
        ////2.我有一个数组，是 public class SRT
        ////{
        ////开始时间;结束时间;  //这两项指英文字幕的
        ////中文音频文件;中文音频时长;  //这两项是根据英文翻译为中文后，TTS生成的。
        ////}
        ////现有List<SRT> srts = .....;
        ////3.对一这些SRT对象，
        ////如果中文音频长度超过了英文字幕时间长度，则尝试快放中文语音，最快放速度为1.3倍。
        ////如果快放后仍然中文太长，则慢放视频速度，最慢为0.7倍速。
        ////如果前两步处理完成后，然后不能对齐，则使用视频最后一帧做为静止画面，显示不够的时间部分。
        ////如果中文音频短于英文时间，则在中文后面加静音占位，使用时间都是合适的、中英文对齐的。
        ////在对于慢放或快放时，应该在开始快放或慢放处在视频画面中绘制出文字提示。
        ////将这个任务整体生成一个filter_complex并放在文件中。
        ////可以使用
        ////# 说明文字来做说明，我可以后期程序中删除注释。供我自己理解使用。
        ////4.现有中文cn.srt和en.srt，一并加上烧录到视频中去。
        ////字幕样式、提示文字样式等都应写在filtercomplexScript.txt中。
        ////程序应该尽量尽读。
        ////.尽量按功能拆分成更小的方法，更易读
        ////.如果能面向对象的方式理解滤镜，是最好的
        //List<SRT> srts = new List<SRT>();
        //// 假设srts已经被正确填充...
        //StringBuilder filterComplex = new StringBuilder();

        //// 初始化视频和音频流
        //filterComplex.AppendLine("# 初始化视频和音频流");
        //filterComplex.Append("[0:v]split=" + srts.Count + " ");
        //for (int i = 0; i < srts.Count; i++)
        //{
        //    filterComplex.Append($"[v{i}] ");
        //}
        //filterComplex.AppendLine(";");

        //// 处理每个SRT对象
        //for (int i = 0; i < srts.Count; i++)
        //{
        //    var srt = srts[i];
        //    double videoDuration = (srt.EndTime - srt.StartTime).TotalSeconds;
        //    double audioDuration = srt.ChineseAudioDuration.TotalSeconds;
        //    double speedChange = 1.0;

        //    // 判断是否需要调整音频速度或视频帧速度
        //    if (audioDuration > videoDuration)
        //    {
        //        // 尝试快放中文音频
        //        speedChange = Math.Min(audioDuration / videoDuration, 1.3);
        //        filterComplex.AppendLine($"[a{i}]atempo={speedChange}[a{i}f];");
        //    }
        //    else if (audioDuration < videoDuration)
        //    {
        //        // 在中文音频后添加静音
        //        double silenceDuration = videoDuration - audioDuration;
        //        filterComplex.AppendLine($"[a{i}]aevalsrc=0:d={silenceDuration}[silence{i}]; [a{i}][silence{i}]concat=n=2:v=0:a=1[a{i}f];");
        //    }

        //    // 对视频进行处理（如果需要）
        //    // 此处省略视频处理逻辑，如调整视频速度或添加静止画面，因为FFmpeg的atempo滤镜有限制，可能需要其他方法来处理

        //    // 添加字幕
        //    filterComplex.AppendLine($"[v{i}]subtitles=cn.srt:si={i}[v{i}s];");
        //    filterComplex.AppendLine($"[v{i}s]subtitles=en.srt:si={i}[v{i}f];");

        //    // 添加提示文字（如果有速度调整）
        //    if (speedChange != 1.0)
        //    {
        //        filterComplex.AppendLine($"[v{i}f]drawtext=text='速度调整:{speedChange}x':x=(w-text_w)/2:y=(h-text_h)/2:fontsize=24:fontcolor=white:box=1:boxcolor=black@0.5[v{i}t];");
        //    }
        //    else
        //    {
        //        filterComplex.AppendLine($"[v{i}f]null[v{i}t];");
        //    }
        //}

        //// 合并处理过的视频和音频流
        //filterComplex.Append("amix=inputs=" + srts.Count + ":duration=first:dropout_transition=3[aout]; ");
        //for (int i = 0; i < srts.Count; i++)
        //{
        //    filterComplex.Append($"[v{i}tt] ");
        //}
        //filterComplex.AppendLine("concat=n=" + srts.Count + ":v=1:a=0 [vout]");

        //// 输出处理
        //filterComplex.AppendLine("# 输出处理");
        //filterComplex.AppendLine("[vout]format=yuv420p[v]; [aout]anull[a]");

        //// 将生成的 filter_complex 脚本写入文件
        //File.WriteAllText("filterComplexScript.txt", filterComplex.ToString());

        //Console.WriteLine("filterComplexScript.txt has been generated.");

    }

    VideoInfo video;
    Controller controller;
    void IPlugin<VideoInfo>.Invoke(VideoInfo video, Controller controller)
    {
        Debugger.Break();
        this.video = video;
        this.controller = controller;

        //Debugger.Break();
        //throw new NotImplementedException();
        Output("插件输出:你好102!" + Environment.NewLine);
        var project = Core(video);
        project.Export(Path.Combine(video.ProjectPath, "product.mp4"));
    }

    void Output(string msg)
    {
        controller.Application.UIThreadInvoke(() =>
        {
            video.Output(msg);
        });
    }

    VideoProject Core(VideoInfo info)
    {
        if (info.CnAudioSolution == null)
        {
            throw new Exception("视频项目还没有生成中文配音!VideoInfo.CnAudioSolution为空!");
        }
        var videoProject = new VideoProject();
        //*****************************************************************
        var mainVideo = videoProject.AddToMainVideoTrack(info.VideoFile);
        //1.对齐中英文语音
        foreach (var 当前中文音频信息 in info.CnAudioSolution.AudioItems)
        {
            var 当前1条英文字幕 = 当前中文音频信息.Subtitle;
            //应该直接指定目标时间:            
            //*****************************************************************
            var 当前原始中文音频片断 = videoProject.AddToMainAudioTrack(当前中文音频信息.OutputFileName, 当前1条英文字幕.StartTime, 当前中文音频信息.Duration);

            //1.1,如果中文音频长度超过了英文字幕时间长度，则尝试快放中文语音，最快放速度为1.3倍。
            if (当前1条英文字幕.Duration < 当前中文音频信息.Duration)
            {
                var 完全匹配倍速 = 当前1条英文字幕.Duration / 当前中文音频信息.Duration;
                //*****************************************************************
                var 准备调速 = Math.Max(完全匹配倍速, 1.3);
                var 加速1点3倍后中文音频 = 当前原始中文音频片断.ChangeSpeed(准备调速);
                //*****************************************************************
                var 英文字幕对应的视频片断 = mainVideo.CreateSegmentWithSelect(当前1条英文字幕.StartTime, 当前1条英文字幕.EndTime);
                英文字幕对应的视频片断.DrawText($"中文语音加速{准备调速.ToString("0.0000")}", 100, 100, 当前1条英文字幕.StartTime, 当前1条英文字幕.EndTime);
                //1.2如果快放后仍然中文太长，则慢放视频速度，最慢为0.7倍速。
                if (加速1点3倍后中文音频.Duration > 当前1条英文字幕.Duration)
                {
                    //选中一个视频片段
                    //返回选中的片段:可以后续继续填加命令
                    //返回SelectCommand:可以调整选中的片段中的参数

                    var 计划视频调速 = (double)当前1条英文字幕.Duration / (double)加速1点3倍后中文音频.Duration;
                    var 实际视频倍速 = Math.Min(0.7, 计划视频调速);
                    //*****************************************************************
                    var 慢放视频0点7倍 = 英文字幕对应的视频片断.ChangeSpeed(实际视频倍速);
                    慢放视频0点7倍.DrawText($"慢放视频:{实际视频倍速.ToString("0.0000")}", 100, 200, 当前1条英文字幕.StartTime, 当前1条英文字幕.EndTime);

                    //如果慢放后仍然不够，则延长
                    if (慢放视频0点7倍.Duration < 当前中文音频信息.Duration)
                    {
                        var delayTime = (float)(当前中文音频信息.Duration - 慢放视频0点7倍.Duration);
                        //*****************************************************************
                        var delayed = 慢放视频0点7倍.Delay(delayTime);
                        慢放视频0点7倍.DrawText($"延时播放", 100, 100, 当前1条英文字幕.StartTime, 当前1条英文字幕.EndTime);

                        #region 后移后面的字幕时间
                        //总时长加长了:
                        //应该将所有的后面的音频片断推迟
                        指定序号的现有字幕时间向后推(info, 当前中文音频信息.Index, delayTime);
                        #endregion
                    }
                }
            }
        }
        //*****************************************************************
        //2.添加英文字幕:
        videoProject.ImportSubtitle(info.VideoDefaultSRT, false);
        //3.填加中文字幕:
        videoProject.ImportSubtitle(info.VideoChineseSRT, true);

        return videoProject;

    }

    private static void 指定序号的现有字幕时间向后推(VideoInfo info, int Index, float delayTime)
    {
        var afters = info.CnAudioSolution.AudioItems.Where(t => t.Index > Index);
        foreach (var ai in afters)
        {
            ai.Subtitle.StartTime = ai.Subtitle.StartTime.Add(TimeSpan.FromSeconds(delayTime));
            ai.Subtitle.EndTime = ai.Subtitle.EndTime.Add(TimeSpan.FromSeconds(delayTime));
        }
    }

    public void Dispose()
    {
        this.video = null;
        this.controller = null;
    }
}
