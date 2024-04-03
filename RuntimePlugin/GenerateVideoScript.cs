using AI.Labs.Module;
using AI.Labs.Module.BusinessObjects;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.ExpressApp;
using IPlugins;
using System.Diagnostics;
using System.Text;
using static edu.stanford.nlp.io.EncodingPrintWriter;

namespace RuntimePlugin
{
    public class GenerateVideoScript : IPlugin<VideoInfo>, IDisposable
    {
        void GetScript()
        {
            //写一个c#程序，帮助拼接ffmpeg命令的参数：
            //整体的任务目标是将英文视频翻译为中文。
            //其中，英文字幕已经有了。中文也已经翻译好了。中文语音也生成好了。
            //现在要做的是，使用视频内容和中文语音尽量对齐。
            //1.仍然将参数生成到filterComplexScript.txt中去。
            //2.我有一个数组，是 public class SRT
            //{
            //开始时间;结束时间;  //这两项指英文字幕的
            //中文音频文件;中文音频时长;  //这两项是根据英文翻译为中文后，TTS生成的。
            //}
            //现有List<SRT> srts = .....;
            //3.对一这些SRT对象，
            //如果中文音频长度超过了英文字幕时间长度，则尝试快放中文语音，最快放速度为1.3倍。
            //如果快放后仍然中文太长，则慢放视频速度，最慢为0.7倍速。
            //如果前两步处理完成后，然后不能对齐，则使用视频最后一帧做为静止画面，显示不够的时间部分。

            //如果中文音频短于英文时间，则在中文后面加静音占位，使用时间都是合适的、中英文对齐的。
            //在对于慢放或快放时，应该在开始快放或慢放处在视频画面中绘制出文字提示。
            //将这个任务整体生成一个filter_complex并放在文件中。
            //可以使用
            //# 说明文字来做说明，我可以后期程序中删除注释。供我自己理解使用。
            //4.现有中文cn.srt和en.srt，一并加上烧录到视频中去。
            //字幕样式、提示文字样式等都应写在filtercomplexScript.txt中。
            //程序应该尽量尽读。

            //.尽量按功能拆分成更小的方法，更易读
            //.如果能面向对象的方式理解滤镜，是最好的


            List<SRT> srts = new List<SRT>();
            // 假设srts已经被正确填充...

            StringBuilder filterComplex = new StringBuilder();

            // 初始化视频和音频流
            filterComplex.AppendLine("# 初始化视频和音频流");
            filterComplex.Append("[0:v]split=" + srts.Count + " ");
            
            for (int i = 0; i < srts.Count; i++)
            {
                filterComplex.Append($"[v{i}] ");
            }

            filterComplex.AppendLine(";");

            filterComplex.Append("[0:a]asplit=" + srts.Count + " ");
            for (int i = 0; i < srts.Count; i++)
            {
                filterComplex.Append($"[a{i}] ");
            }
            filterComplex.AppendLine(";");

            // 处理每个SRT对象
            for (int i = 0; i < srts.Count; i++)
            {
                var srt = srts[i];
                double videoDuration = (srt.EndTime - srt.StartTime).TotalSeconds;
                double audioDuration = srt.ChineseAudioDuration.TotalSeconds;
                double speedChange = 1.0;

                // 判断是否需要调整音频速度或视频帧速度
                if (audioDuration > videoDuration)
                {
                    // 尝试快放中文音频
                    speedChange = Math.Min(audioDuration / videoDuration, 1.3);
                    filterComplex.AppendLine($"[a{i}]atempo={speedChange}[a{i}f];");
                }
                else if (audioDuration < videoDuration)
                {
                    // 在中文音频后添加静音
                    double silenceDuration = videoDuration - audioDuration;
                    filterComplex.AppendLine($"[a{i}]aevalsrc=0:d={silenceDuration}[silence{i}]; [a{i}][silence{i}]concat=n=2:v=0:a=1[a{i}f];");
                }

                // 对视频进行处理（如果需要）
                // 此处省略视频处理逻辑，如调整视频速度或添加静止画面，因为FFmpeg的atempo滤镜有限制，可能需要其他方法来处理

                // 添加字幕
                filterComplex.AppendLine($"[v{i}]subtitles=cn.srt:si={i}[v{i}s];");
                filterComplex.AppendLine($"[v{i}s]subtitles=en.srt:si={i}[v{i}f];");

                // 添加提示文字（如果有速度调整）
                if (speedChange != 1.0)
                {
                    filterComplex.AppendLine($"[v{i}f]drawtext=text='速度调整:{speedChange}x':x=(w-text_w)/2:y=(h-text_h)/2:fontsize=24:fontcolor=white:box=1:boxcolor=black@0.5[v{i}t];");
                }
                else
                {
                    filterComplex.AppendLine($"[v{i}f]null[v{i}t];");
                }
            }

            // 合并处理过的视频和音频流
            filterComplex.Append("amix=inputs=" + srts.Count + ":duration=first:dropout_transition=3[aout]; ");
            for (int i = 0; i < srts.Count; i++)
            {
                filterComplex.Append($"[v{i}tt] ");
            }
            filterComplex.AppendLine("concat=n=" + srts.Count + ":v=1:a=0 [vout]");

            // 输出处理
            filterComplex.AppendLine("# 输出处理");
            filterComplex.AppendLine("[vout]format=yuv420p[v]; [aout]anull[a]");

            // 将生成的 filter_complex 脚本写入文件
            File.WriteAllText("filterComplexScript.txt", filterComplex.ToString());

            Console.WriteLine("filterComplexScript.txt has been generated.");

        }


        VideoInfo video;
        Controller controller;
        void IPlugin<VideoInfo>.Invoke(VideoInfo video, Controller controller)
        {
            this.video = video;
            this.controller = controller;
            //Debugger.Break();
            //throw new NotImplementedException();
            video.Output("插件输出:你好102!" + Environment.NewLine);

            //ffmpeg - i input.mp4 - filter_complex \
            //"[0:v]trim=0:5,setpts=PTS-STARTPTS[v1]; \
            //[0:v]trim = 5:8,setpts = (PTS - STARTPTS) / 0.6[v2]; \
            //[0:v]trim = 8,setpts = PTS - STARTPTS[v3]; \
            //[v1][v2][v3] concat = n = 3:v = 1[outv]; \
            //[0:a]atrim = 0:5,asetpts = PTS - STARTPTS[a1]; \
            //[0:a]atrim = 5:8,asetpts = (PTS - STARTPTS) * 1.6667[a2]; \
            //[0:a]atrim = 8,asetpts = PTS - STARTPTS[a3]; \
            //[a1][a2][a3] concat = n = 3:v = 0:a = 1[outa]" \
            //- map "[outv]" - map "[outa]" output.mp4
            var filterComplexScript = Path.Combine(video.ProjectPath, "FilterComplexScript.txt");

            if (File.Exists(filterComplexScript))
            {
                File.Delete(filterComplexScript);
            }
            if (string.IsNullOrEmpty(video.VideoScript.FilterComplexText))
            {
                video.Output("滤镜脚本为空!退出！");
                return;
            }
            File.WriteAllText(filterComplexScript, video.VideoScript.FilterComplexText);
            video.Output("开始ffmpeg");
            var error = Path.Combine(video.ProjectPath, "error.txt");
            var bat = Path.Combine(video.ProjectPath, "bat.bat");
            var masterCommand = $@"{AudioHelper.ffmpegFile} -/filter_complex {filterComplexScript} {video.VideoScript.StartCommand}";
            var batContent = @$"{masterCommand}
";
            File.WriteAllText(
                bat, batContent
                );

            var p = new Process();
            var info = new ProcessStartInfo();
            p.StartInfo = info;
            p.StartInfo.FileName = bat;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += P_OutputDataReceived;
            p.ErrorDataReceived += P_OutputDataReceived;
            try
            {
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                video.Output($"使用命令:{masterCommand}");
                video.Output($"{filterComplexScript}内容:{video.VideoScript.FilterComplexText}");

                video.Output("退出ffmpeg");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Dispose()
        {
            this.video = null;
            this.controller = null;
        }

        private void P_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                if (video != null)
                {
                    Debug.WriteLine(e.Data);

                    controller.Application.UIThreadInvoke(() =>
                    {
                        video.Output(e.Data);
                    });
                }
            }
        }
    }
}
