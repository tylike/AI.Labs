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
using System.Drawing;
using DevExpress.Charts.Native;

namespace RuntimePlugin
{
    public class FFmpegGlobalSettings
    {
        public static string DefaultFont = "C:\\Windows\\Fonts\\simhei.ttf";
    }
    public class MediaSegment
    {
        public static int GlobalID { get; set; } = 0;

        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }

        public List<MediaSegment> Inputs { get; } = new List<MediaSegment>();
        public List<MediaSegment> Outputs { get; } = new List<MediaSegment>();

        public virtual string GetCommand() => "";
        public string Label { get; protected set; }
        public virtual void CreateLabel() { Label = $"[v{GlobalID++}]"; }
        public string InputLabels => string.Join("", Inputs.Select(t => t.Label));
        public MediaSegment()
        {
            CreateLabel();
        }
    }

    public class VideoObject : MediaSegment
    {
        static int InputVideoGlobalID = 0;
        public VideoObject(string fileName)
        {

        }
        public override void CreateLabel()
        {
            Label = $"[v{InputVideoGlobalID++}]";
        }
        public override string GetCommand()
        {
            return Label;
        }
    }

    public class MediaSelect : MediaSegment
    {
        public MediaSelect()
        {
        }
        //在FFmpeg中，`t`是时间（time）的缩写，它通常用于时间选择或时间基点操作。
        //`select`滤镜基于时间`t`来选择视频流的帧，
        //`between(t, 0, 3)`表示选择0到3秒的时间段。
        //- `pad`滤镜用于在视频帧的尺寸上进行扩展或填充。
        //`iw`和`ih`分别代表输入视频的宽度（width）和高度（height），
        //`pad = iw:s = ih`表示按照原始宽度和高度填充空白，保留1: 1的宽高比。
        //这里的`s`是一个小写的`s`，实际上是`sar`（sample aspect ratio）的误拼，FFmpeg的文档中是指原始的宽高比，实际上应该是`ih`，表示填充高度与宽度相同。
        //- `format = mp4`是将输出的视频流转换为MPEG - 4（H.264编码，MP4文件格式）。
        //这个部分是可选的，如果你希望保留原始格式，可以省略。但如果你的输出目标是MP4文件，或者需要与后续处理兼容，建议保留。
        //注意：在进行输出时，FFmpeg会根据滤镜的输出进行编码，如果省略`format`，FFmpeg会默认选择一个编码器来处理输出。
        //如果你省略了`format`，FFmpeg可能会以不同的格式进行编码，这可能与你的预期不符。因此，根据你的目标输出格式，最好明确指定`format`。

        public override string GetCommand()
        {
            return $"{InputLabels}select='between(t,{Start.TotalSeconds},{Start.TotalSeconds})',pad=iw:s=ih,format=mp4{Label};";
        }
    }

    public class MediaDelay : MediaSegment
    {
        public MediaDelay()
        {
        }
        public float Delay { get; set; }
        public override string GetCommand()
        {
            var inputs = string.Join("", Inputs.Select(t => t.GetCommand()));
            return $"{InputLabels}trim=end={Delay.ToString("0.000")},setpts=PTS-STARTPTS{Label}";
        }
    }

    public class MediaAdjustSpeed:MediaSegment
    {
        public override string GetCommand()
        {

            return base.GetCommand();
        }
    }

    public class MediaConcat : MediaSegment
    {        
        public override string GetCommand()
        {
            //concat=n=2:v=1:a=1[v_out]
            return $"{InputLabels}concat=n={Inputs.Count}{Label};";
        }
    }


    public class AddSubtitles : MediaSegment
    {
        public string SubtitleFile { get; set; }
        public SubtitleBorderStyle BoxStyle { get; set; } = SubtitleBorderStyle.Box;
        public int BoxBorderWidth { get; set; } = 5;
        public Color BoxBorderColor { get; set; } = Color.White;
        public int BoxBorderHeight { get; set; } = 1;
        public double BoxBorderAlpha { get; set; } = 1.0;
        public string FontPath { get; set; } = FFmpegGlobalSettings.DefaultFont;
        public int FontSize { get; set; } = 24;
        public Color FontColor { get; set; } = Color.Black;

        public AddSubtitles(string subtitleFile)
        {
            SubtitleFile = subtitleFile;
        }
        public override string GetCommand()
        {
            var command = $"{InputLabels}subtitles={SubtitleFile}";
            command += $":box={BoxStyle}:boxborderw={BoxBorderWidth}:color={BoxBorderColor}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";
            command += $",fontfile={FontPath}:fontsize={FontSize}:fontcolor={FontColor}{Label}";
            return command;
        }
    }
    public enum SubtitleBorderStyle
    {
        None,
        Box
    }
    public static class VideoSegmentLogic
    {
        public static MediaSelect Select(this MediaSegment segment, MediaSelection mediaSelection)
        {
            var ms = new MediaSelect();
            ms.Start = mediaSelection.Start;
            ms.End = mediaSelection.End;
            ms.Inputs.Add(segment);
            return ms;
        }

        public static MediaSelect Select(this MediaSegment segment, float start, float end)
        {
            return segment.Select(new MediaSelection { Start = TimeSpan.FromSeconds(start), End = TimeSpan.FromSeconds(end) });
        }
        public static List<MediaSelect> SelectMany(this MediaSegment segment, IEnumerable<MediaSelection> selects)
        {
            return selects.Select(item => segment.Select(item)).ToList();
        }
        public static MediaDelay Delay(this MediaSegment segment, float delay)
        {
            var md = new MediaDelay();
            md.Delay = delay;
            md.Inputs.Add(segment);
            return md;
        }
        public static AddSubtitles AddSubtitles(this MediaSegment segment, string srtFile)
        {
            var asb = new AddSubtitles(srtFile);
            asb.Inputs.Add(segment);
            return asb;
        }
    }

    public class MediaSelection
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class DrawTextOptions : MediaSegment
    {
        public string FontPath { get; set; } = FFmpegGlobalSettings.DefaultFont;
        public string Text { get; set; }
        public Point Position { get; set; } = new Point(10, 10);
        public Color Color { get; set; } = Color.White;
        public int FontSize { get; set; } = 24;
        public SubtitleBorderStyle BoxStyle { get; set; } = SubtitleBorderStyle.None;
        public int BoxBorderWidth { get; set; } = 5;
        public int BoxBorderHeight { get; set; } = 1;
        public double BoxBorderAlpha { get; set; } = 1.0;

        public DrawTextOptions(string text,bool commandIncludeInputAndOutputLabes = true)
        {
            this.CommandIncludeInputAndOutputLabels = commandIncludeInputAndOutputLabes;
            Text = text;
        }

        public override void CreateLabel()
        {
            base.CreateLabel();
        }
        public bool CommandIncludeInputAndOutputLabels = true;
        public override string GetCommand()
        {
            var command = $"drawtext=fontfile={FontPath}: text='{Text}': x={Position.X}: y={Position.Y}";
            if (BoxStyle != SubtitleBorderStyle.None)
            {
                command += $", box={BoxStyle}:boxborderw={BoxBorderWidth}:boxborderh={BoxBorderHeight}:boxbordera={BoxBorderAlpha}";
            }
            command += $", color={Color}: fontsize={FontSize}";


            if (CommandIncludeInputAndOutputLabels)
            {
                command = $"{InputLabels}{command}{Label}";
            }

            return command;
        }
    }

    public class DrawManyTextOptions : MediaSegment
    {
        public List<DrawTextOptions> Options { get; } = new List<DrawTextOptions>();
        public override string GetCommand()
        {
            var commands =string.Join(";", Options.Select(t => t.GetCommand()));
            return $"{InputLabels}{commands}{Label}";
        }
    }

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
