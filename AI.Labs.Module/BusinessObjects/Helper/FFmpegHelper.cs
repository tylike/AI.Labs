using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.Charts.Native;
using DevExpress.Spreadsheet;
using DevExpress.XtraSpreadsheet.Import.Xls;
using FFMpegCore.Enums;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using YoutubeExplode.Videos;
using System.Drawing;
using edu.stanford.nlp.trees.tregex.gui;
using static com.sun.imageio.plugins.common.PaletteBuilder;
using static DevExpress.XtraPrinting.Native.ExportOptionsPropertiesNames;
using NAudio.Wave.SampleProviders;
using NAudio.Wave;

namespace AI.Labs.Module.BusinessObjects
{
    public enum SimpleMediaType
    {
        Video,
        Audio
    }

    public class SimpleFFmpegCommand
    {
        public SimpleFFmpegCommand(SimpleFFmpegScript script)
        {
            this.Script = script;
        }

        public int Index { get; set; }
        public string Command { get; set; }

        string outputLabel;
        public string OutputLable
        {
            get
            {
                if (outputLabel == null)
                {
                    outputLabel = $"[{SimpleMediaType.ToString().First()}{Index}]";
                }
                return outputLabel;
            }
            set
            {
                outputLabel = value;
            }
        }

        public SimpleMediaType SimpleMediaType { get; set; }

        public SimpleFFmpegScript Script { get; }

    }
    public class SimpleFFmpegInput
    {
        public string FileName { get; set; }
        public int Duration { get; set; }
        public SimpleMediaType Type { get; set; }
    }
    public class SimpleFFmpegScript
    {
        public List<SimpleFFmpegCommand> Commands { get; set; } = new List<SimpleFFmpegCommand>();

        public SimpleFFmpegCommand CreateEmptyVideo(Color color, int durationMS, int w = 1280, int h = 720)
        {
            var cmd = new SimpleFFmpegCommand(this) { Index = Commands.Count };
            var filterComplex = $"color=c={color.Name.ToString().ToLower()}:s={w}x{h}:d={durationMS / 1000d:0.0000},format=yuv420p{cmd.OutputLable}";
            cmd.Command = filterComplex;
            Commands.Add(cmd);
            return cmd;
        }

        public SimpleFFmpegCommand CreateEmptyAudio(int durationMS)
        {
            var cmd = new SimpleFFmpegCommand(this) { Index = Commands.Count, SimpleMediaType = SimpleMediaType.Audio };
            var filterComplex = $"anullsrc=r=44100:cl=stereo,atrim=duration={durationMS / 1000d}{cmd.OutputLable}";
            cmd.Command = filterComplex;
            Commands.Add(cmd);
            return cmd;
            //ffmpeg -i input_video.mp4 -f lavfi -i anullsrc=channel_layout=stereo:sample_rate=44100 -filter_complex "[1:a]atrim=duration=10[audiosilence];[0:a][audiosilence]amerge[audio]" -map 0:v -map "[audio]" -c:v copy -c:a aac output.mp4
        }



        #region inputs
        public List<SimpleFFmpegInput> Inputs = new();
        public List<SimpleFFmpegInput> InputVideos = new List<SimpleFFmpegInput>();

        public List<SimpleFFmpegInput> InputAudios = new List<SimpleFFmpegInput>();
        public List<SimpleFFmpegCommand> InputVideoCommands = new List<SimpleFFmpegCommand>();
        public List<SimpleFFmpegCommand> InputAudioCommands = new List<SimpleFFmpegCommand>();
        public SimpleFFmpegCommand InputVideo(string filename)
        {
            return InputFile(filename, SimpleMediaType.Video);
        }
        public SimpleFFmpegCommand InputAudio(string filename)
        {
            return InputFile(filename, SimpleMediaType.Audio);
        }

        public SimpleFFmpegCommand InputFile(string filename, SimpleMediaType type, int? duration = null)
        {
            var commands = Commands;

            var inputFile = new SimpleFFmpegInput { FileName = filename };
            if (!duration.HasValue)
            {
                duration = (int)FFmpegHelper.GetDuration(filename);
            }
            inputFile.Duration = duration.Value;


            Inputs.Add(inputFile);
            var cmd = new SimpleFFmpegCommand(this) { Index = commands.Count, SimpleMediaType = type };
            switch (type)
            {
                case SimpleMediaType.Video:
                    cmd.OutputLable = $"[{commands.Count}:v]";
                    InputVideos.Add(inputFile);
                    InputVideoCommands.Add(cmd);
                    break;
                case SimpleMediaType.Audio:
                    cmd.OutputLable = $"[{commands.Count}:a]";
                    InputAudios.Add(inputFile);
                    InputAudioCommands.Add(cmd);
                    break;
                default:
                    break;
            }
            commands.Add(cmd);
            return cmd;
        }

        #endregion

        public string GetComplexScript()
        {
            return Commands.Where(t => !string.IsNullOrEmpty(t.Command)).Select(t => t.Command).Join(";");
        }
    }
    public static class FFmpegHelper
    {
        public static SimpleFFmpegCommand Select(this SimpleFFmpegCommand input, int startMS, int endMS)
        {
            var cmd = new SimpleFFmpegCommand(input.Script)
            {
                Index = input.Script.Commands.Count,
            };
            var commandName = input.SimpleMediaType == SimpleMediaType.Video ? "trim" : "atrim";
            cmd.Command = $"{input.OutputLable}{commandName}=start={startMS / 1000d}:end={endMS / 1000d},setpts=PTS-STARTPTS{cmd.OutputLable}";
            input.Script.Commands.Add(cmd);
            return cmd;
        }

        public static SimpleFFmpegCommand PutVideo(this SimpleFFmpegCommand backgroundVideo, SimpleFFmpegCommand overlayVideo, int startMS, int endMS, int x = 0, int y = 0)
        {
            /*
             ffmpeg -i videoA.mp4 -i videoB.mp4 -filter_complex \
                "[1:v]trim=start=1:end=3,setpts=PTS-STARTPTS[vb]; \
                [0:v][vb]overlay=enable='between(t,3.5,6.5)':x=0:y=0[out]" \
                -map "[out]" -map 0:a -c:v libx264 -c:a copy -y output.mp4
             */
            /*
             ffmpeg -i videoA.mp4 -i videoB.mp4 -i videoC.mp4 -filter_complex \
                "[1:v]trim=start=1:end=3,setpts=PTS-STARTPTS[vb]; \
                 [2:v]trim=start=2:end=3,setpts=PTS-STARTPTS[vc]; \
                 [0:v][vb]overlay=enable='between(t,3.5,6.5)':x=0:y=0[va]; \
                 [va][vc]overlay=enable='between(t,7,8)':x=0:y=0[out]" \
                -map "[out]" -map 0:a -c:v libx264 -c:a copy -y output.mp4
             */
            var cmd = new SimpleFFmpegCommand(backgroundVideo.Script) { Index = backgroundVideo.Script.Commands.Count, SimpleMediaType = backgroundVideo.SimpleMediaType };
            var commandName = "overlay";
            var loc = $":x={x}:y={y}";
            cmd.Command = $"{backgroundVideo.OutputLable}{overlayVideo.OutputLable}{commandName}=enable='between(t,{startMS / 1000d},{endMS / 1000d}){loc}'{cmd.OutputLable}";
            backgroundVideo.Script.Commands.Add(cmd);
            return cmd;
        }
        public static SimpleFFmpegCommand AMix(this List<SimpleFFmpegCommand> audios)
        {
            //[a1][a2]amix=inputs=2:duration=longest
            var audio = audios.First();
            var cmd = new SimpleFFmpegCommand(audio.Script) { Index = audio.Script.Commands.Count, SimpleMediaType = audio.SimpleMediaType };

            var commandName = $"amix=inputs={audios.Count}:duration=longest";
            cmd.Command = $"{audios.Select(t=>t.OutputLable).Join()}{commandName}{cmd.OutputLable}";
            audio.Script.Commands.Add(cmd);
            return cmd;
        }

        public static SimpleFFmpegCommand PutAudio(this SimpleFFmpegCommand audio, int startMS, int? clipStart =0,int? clipEnd =0)
        {
            /*
             ffmpeg -i videoA.mp4 -i videoB.mp4 -filter_complex \
                "[1:v]trim=start=1:end=3,setpts=PTS-STARTPTS[vb]; \
                [0:v][vb]overlay=enable='between(t,3.5,6.5)':x=0:y=0[out]" \
                -map "[out]" -map 0:a -c:v libx264 -c:a copy -y output.mp4
             */
            /*
             ffmpeg -i videoA.mp4 -i videoB.mp4 -i videoC.mp4 -filter_complex \
                "[1:v]trim=start=1:end=3,setpts=PTS-STARTPTS[vb]; \
                 [2:v]trim=start=2:end=3,setpts=PTS-STARTPTS[vc]; \
                 [0:v][vb]overlay=enable='between(t,3.5,6.5)':x=0:y=0[va]; \
                 [va][vc]overlay=enable='between(t,7,8)':x=0:y=0[out]" \
                -map "[out]" -map 0:a -c:v libx264 -c:a copy -y output.mp4
             */
            var cmd = new SimpleFFmpegCommand(audio.Script) { Index = audio.Script.Commands.Count, SimpleMediaType = audio.SimpleMediaType };
            var atrim = "";
            if(clipEnd.HasValue && clipEnd.HasValue)
            {
                atrim = $"atrim=start={clipStart.Value}:end={clipEnd.Value},asetpts=PTS-STARTPTS,";
            }

            var commandName = $"{atrim}adelay={startMS}|{startMS}";

            cmd.Command = $"{audio.OutputLable}{commandName}{cmd.OutputLable}";            
            audio.Script.Commands.Add(cmd);
            return cmd;
        }



        public static void CreateEmptyVideo(string outputFile, int durationMS, string image = null, Color? color = null)
        {
            var colorName = "black";
            if (color.HasValue && color.Value.IsNamedColor)
            {
                colorName = color.Value.Name.ToLower();
            }

            var filterComplex = $"color=c={colorName}:s=1280x720:d={durationMS / 1000d:0.0000},format=yuv420p";
            ExecuteFFmpegCommand(
                outputFiles: outputFile,
                filterComplex: filterComplex,
                outputOptions: "-c:v libx264 -crf 18 -y"
                );
        }

        public static void ExecuteFFmpegCommand(string inputOptions = "", string inputFiles = "", string filterComplex = "", string outputOptions = "", string outputFiles = "")
        {
            if (!string.IsNullOrEmpty(filterComplex))
            {
                filterComplex = $"-filter_complex \"{filterComplex}\"";
            }
            ExecuteFFmpegCommand($"{inputOptions} {inputFiles} {filterComplex} {outputOptions} {outputFiles}");
        }
        /// <summary>
        /// 执行ffmpeg        
        /// </summary>
        /// <param name="command">参数部分</param>        
        public static void ExecuteFFmpegCommand(string command)
        {
            var p = new Process();
            var info = new ProcessStartInfo();
            p.StartInfo = info;
            p.StartInfo.FileName = ffmpegFile;
            p.StartInfo.Arguments = command;
            //p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            Debug.WriteLine($"{ffmpegFile} {command}");
            //p.StartInfo.RedirectStandardOutput = true;
            //p.StartInfo.RedirectStandardError = true;
            p.Start();
            //p.BeginOutputReadLine();
            //p.BeginErrorReadLine();
            p.WaitForExit();
        }

        /// <summary>
        /// 保留指定的小数点位数,最（后一位+1）大于0则进一
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimalPlaces">取小数点位数</param>
        /// <returns></returns>
        public static double RoundUp(this double value, int decimalPlaces)
        {
            double factor = Math.Pow(10, decimalPlaces);
            return Math.Ceiling(value * factor) / factor;
        }


        const string ffprobe = @"D:\ffmpeg.gui\last\ffprobe.exe";
        public const string ffmpegFile = @"D:\ffmpeg.gui\last\ffmpeg.exe";
        public static StreamWriter Log { get; set; }
        #region execute command
        public static string LastCommand = "";
        public static string ExecuteCommand(string taskMemo, double? targetDuration, string command,
            string outputFile,
            bool useShellExecute = false,
            bool ffmpeg = true,
            bool readOutput = false,
            bool noWindow = false,
            bool overWriteExist = true,
            string addationLogs = null,
            string inputFiles = null,
            string inputOptions = null,
            string outputOptions = null,
            string logFilePath = null
            )
        {
            if (!string.IsNullOrEmpty(inputFiles))
            {
                command = $" {inputOptions + ""} -i {inputFiles} {command}";
            }

            if (!string.IsNullOrEmpty(outputFile))
            {
                command += $" {outputOptions + ""} {outputFile} ";
                if (overWriteExist)
                {
                    command += " -y ";
                }
            }
            var sw = Stopwatch.StartNew();
            //如果targetDuration为空，则不需要去验证目标时长
            var pi = new ProcessStartInfo();
            pi.FileName = ffmpeg ? ffmpegFile : ffprobe;
            pi.Arguments = command;
            pi.UseShellExecute = useShellExecute;
            pi.RedirectStandardError = readOutput;
            pi.RedirectStandardOutput = readOutput;
            pi.CreateNoWindow = noWindow;
            var inf = Process.Start(pi);
            var rst = "";
            LastCommand = command;
            if (readOutput)
            {
                rst = inf.StandardOutput.ReadToEnd();
            }
            inf.WaitForExit();
            sw.Stop();
            if (taskMemo != null)
                Console.WriteLine(taskMemo);

            Debug.WriteLine($"{pi.FileName} {pi.Arguments}");
            if (Log != null && targetDuration.HasValue && !string.IsNullOrEmpty(outputFile))
            {
                var dur = GetDuration(outputFile);
                var diff = dur.Value - targetDuration.Value;
                if (diff > 0)
                {
                    var logs = @$"..........
{pi.FileName} {command}
输出文件时长:{dur.Value} 目标时长:{targetDuration.Value} 精确差异:{diff} {(diff > 100 ? "***ERROR***" : "")}
附加信息:
{addationLogs + ""}
..........
";
                    Log.WriteLine(logs);
                    Console.Write(logs);
                }
                //if(dur.HasValue && dur.Value != targetDuration.Value)
                //{
                //    throw new Exception($"目标时长:{targetDuration.Value},实际时长:{dur.Value}");
                //}
            }
            return rst;
        }

        /// <summary>
        /// 执行的命令共有两部分,一是如
        /// -i a.mp4 -i b.mp4 -/filter_complex {fileName.txt}  
        /// 二是输出参数
        /// filterName.txt中的内容是filterComplex的内容
        /// </summary>
        /// <param name="mainParameter"></param>
        /// <param name="filterComplex"></param>
        /// <param name="basePath"></param>
        /// <param name="pause">ffmpeg执行完成后,是否暂停</param>
        public static string ExecuteCommand(
            string inputFiles,
            string outputFiles,
            string mainParameter, string filterComplex,
            bool overWriteExist,
            double duration,
            string basePath = null,
            bool pause = false,
            Action<string> outputLog = null
            )
        {
            if (string.IsNullOrEmpty(filterComplex))
            {
                throw new ArgumentException("滤镜脚本为空!退出！", nameof(filterComplex));
            }

            if (duration == 0)
                throw new ArgumentException("视频时长不能为0", nameof(duration));

            if (basePath == null)
            {
                basePath = Environment.GetEnvironmentVariable("TEMP");
            }

            var filterComplexScript = Path.Combine(basePath, "FilterComplexScript.txt");
            if (File.Exists(filterComplexScript))
            {
                File.Delete(filterComplexScript);
            }
            File.WriteAllText(filterComplexScript, filterComplex);

            //Output("开始ffmpeg");
            //var error = Path.Combine(basePath, "error.txt");


            var overrideOptions = "";
            if (overWriteExist)
            {
                overrideOptions = " -y";
            }

            var bat = Path.Combine(basePath, "bat.bat");
            var masterCommand = $@"{FFmpegHelper.ffmpegFile} {inputFiles} -/filter_complex {filterComplexScript} {mainParameter} -t {duration.ToFFmpegString()} {overrideOptions} {outputFiles}";
            var batContent = @$"{masterCommand}
";
            File.WriteAllText(bat, batContent);

            var output = new StringBuilder();

            var p = new Process();
            var info = new ProcessStartInfo();
            p.StartInfo = info;
            p.StartInfo.FileName = bat;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;

            p.OutputDataReceived += OutputDataReceived;
            p.ErrorDataReceived += OutputDataReceived;

            void OutputDataReceived(object sender, DataReceivedEventArgs e)
            {

                if (e.Data != null)
                {
                    outputLog?.Invoke(e.Data);
                    Debug.WriteLine("dbg:" + e.Data);
                    Console.WriteLine(e.Data);
                    output.AppendLine(e.Data);
                }
            }
            try
            {
                p.Start();
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
                //Output($"使用命令:{masterCommand}");
                //Output($"{filterComplexScript}内容:{video.VideoScript.FilterComplexText}");
                //Output("退出ffmpeg");
                return output.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="speed">2.0为2倍，0.5为慢放2倍</param>
        /// <param name="outputFile"></param>

        public static void ChangeAudioSpeed(
            string taskMemo,
            double targetDuration,
            string inputFileName,
            double speed,
            string outputFile,
            double planSpeed
            )
        {
            if (!File.Exists(outputFile))
            {
                var forceLength = planSpeed <= 1.3 ? $"-t {targetDuration}" : "";
                ExecuteCommand($"音频调速 {taskMemo}", targetDuration, $" -loglevel error  -i {inputFileName}  -filter:a:0 \"atempo={speed:0.0########}\" {forceLength}  -async 1 -fflags +genpts", outputFile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="targetSpeed">2.0为2倍，0.5为慢放2倍</param>
        /// <param name="outputFile"></param>
        public static void ChangeVideoSpeed(string taskMemo, double targetDuration, string inputFileName, double targetSpeed, string outputFile)
        {
            ArgumentNullException.ThrowIfNull(inputFileName, nameof(inputFileName));
            ArgumentNullException.ThrowIfNull(outputFile, nameof(outputFile));

            //ffmpeg -i input.mp4 -filter_complex "[0:v]setpts=PTS/2[v];[0:a]atempo=2[a]" -map "[v]" -map "[a]" output_fast.mp4
            ExecuteCommand($"视频调速 {taskMemo}", targetDuration, $"-i {inputFileName} -filter_complex \"[0:v]setpts=PTS/{targetSpeed.ToFFmpegString()}[v];\" -map \"[v]\" ", outputFile);
        }
        public static void DelayAudio(string taskMemo, double targetDuration, string inputFileName, double delayMS, string newOutputFile)
        {
            if (!File.Exists(newOutputFile))
            {
                //-i input.mp3 -af apad=pad_dur=3000 output.mp3
                ExecuteCommand($"音频延时 {taskMemo} ", targetDuration, $" -loglevel error  -i {inputFileName} -af apad=pad_dur={(delayMS / 1000).ToFFmpegString()} ", newOutputFile);
            }
        }

        /// <summary>
        /// 延时视频，使用视频的最后一帧复制
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="delayMS"></param>
        /// <param name="newOutputFile"></param>
        public static void DelayVideoCopyLast(string taskMemo, double targetDuration, string inputFileName, double delayMS, string newOutputFile)
        {
            //-i input.mp4 -vf "setpts=PTS+3/TB" output.mp4
            //开头加: -filter_complex "[0:v]tpad=start_mode=clone:start_duration=3[v];[0:a]apad=pad_dur=3:pad_plac=0[a]" -map "[v]" -map "[a]" output.mp4
            //末尾加: -filter_complex "[0:v]tpad=stop_mode=clone:stop_duration=3[v];[0:a]apad=pad_dur=3[a]" -map "[v]" -map "[a]" output.mp4
            var delayTime = (delayMS / 1000).ToFFmpegString();
            ExecuteCommand($"视频延时 {taskMemo}", targetDuration, $" -loglevel error  -i {inputFileName} -filter_complex \"[0:v]tpad=stop_mode=clone:stop_duration={delayTime}[v];\" -map \"[v]\" ", newOutputFile);
        }

        /// <summary>
        /// 延时视频，使用视频的最后一帧复制
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="delayMS"></param>
        /// <param name="newOutputFile"></param>
        public static void DelayVideoCopyRepeat(string taskMemo, double targetDuration, double originalDuration, string inputFileName, string newOutputFile)
        {
            int loopCount = (int)Math.Ceiling(targetDuration / originalDuration);
            string arguments = " -loglevel error ";
            if (!File.Exists(newOutputFile))
            {
                ExecuteCommand(
                    $"{taskMemo} 视频延时", targetDuration,
                    arguments,
                    newOutputFile,
                    inputFiles: inputFileName,
                    inputOptions: $" -stream_loop {loopCount} ",
                    outputOptions: $" -t {targetDuration / 1000} "
                    );
            }

            //ExecuteCommand($"{taskMemo} 视频延时", targetDuration, $"-i {inputFileName} -filter_complex \"[0:v]tpad=stop_mode=clone:stop_duration={delayTime}[v];\" -map \"[v]\" ", newOutputFile);
        }

        public class ClipParameter
        {
            public TimeSpan Start;
            public TimeSpan End;
            public double Duration => (End - Start).TotalSeconds;
            public int Index;
        }
        /// <summary>
        /// D:\videoInfo\2\v\video_%04d.mp4
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="times"></param>
        /// <param name="outputPath"></param>
        public static string[] SplitVideo(string inputFileName, IEnumerable<SubtitleItem> times, string outputPath)
        {
            //第一步,设置关键帧
            //var source = inputFileName;
            //var keysource = Path.Combine(Path.GetDirectoryName(inputFileName), "keysource.mkv");
            ////D:\ffmpeg.gui\last\ffmpeg.exe -i d:\VideoInfo\2\source.mp4 -c:v libx264 -crf 23 -force_key_frames "9.61,16.84" -x264-params keyint=25:scenecut=0 d:\VideoInfo\2\intermediate.mp4 -y
            //ExecuteCommand(
            //    null,null,
            //    $" -c:v libx264  -crf 0   -g 1 -keyint_min 1 -x264-params \"scenecut=0:open_gop=0:min-keyint=1:keyint=1\" -c:a copy -r 50 ",
            //    //$"-i {source} -c:v ffv1 -level 3 {keysource}", 
            //    keysource,
            //    inputFiles:source
            //    );
            var sw = Stopwatch.StartNew();

            var dur = GetDuration(inputFileName);

            //ffmpeg -i input.mp4 -f segment -segment_times 10.500,22.712,35.145,48.376 -c copy output_%03d.mp4
            var items = times.Select(t => new ClipParameter { Start = t.StartTime, End = t.EndTime, Index = t.Index }).ToArray();
            items.First().Start = TimeSpan.FromSeconds(0);
            items.Last().End = items.Last().Start.AddMilliseconds(dur.Value);

            //var rst = new List<string>();
            //foreach (var item in items)
            //{
            //    var outputFile = Path.Combine(outputPath, $"{item.Index}.mp4");
            //    GetVideoClip(inputFileName, outputFile, item.Start, item.End, item.Duration, $"subtitle_{item.Index} 分割视频");
            //    rst.Add(outputFile);
            //}
            //return rst.ToArray();

            ConcurrentBag<string> rst = new ConcurrentBag<string>();

            Parallel.ForEach(items, new ParallelOptions { MaxDegreeOfParallelism = 8 }, item =>
            {
                var outputFile = Path.Combine(outputPath, $"{item.Index.ToString("00000")}.mp4");
#warning 不会重新生成文件
                if (!File.Exists(outputFile))
                {
                    GetVideoClip(inputFileName, outputFile, item.Start, item.End, item.Duration, $"subtitle_{item.Index} 分割视频");
                }
                rst.Add(outputFile);
            });
            sw.Stop();
            Debug.WriteLine($"分割用时:{sw.Elapsed}");
            return rst.ToArray();


        }

        public static void TestClips(string inputFileName, double clipLength)
        {
            var dur = GetDuration(inputFileName);
            var ps = GetParameters(dur.Value, clipLength).Take(100);

            foreach (var item in ps)
            {
                var outputFile = Path.Combine("d:\\temp", $"{item.Index}.mp4");

                GetVideoClip(inputFileName, outputFile, item.Start.TotalSeconds, item.End.TotalSeconds, $"{item.Index}ClipTest", clipLength, "d:\\temp");
            }
        }

        public static ClipParameter[] GetParameters(double inputFileDurationSecond, double clipLengthSecond)
        {
            int totalClips = (int)Math.Ceiling(inputFileDurationSecond / clipLengthSecond);

            return Enumerable.Range(0, totalClips).Select(index =>
            {
                double start = index * clipLengthSecond;
                double end = Math.Min((index + 1) * clipLengthSecond, inputFileDurationSecond);

                return new ClipParameter
                {
                    Start = TimeSpan.FromSeconds(start),
                    End = TimeSpan.FromSeconds(end),
                    Index = index
                };
            }).ToArray();
        }

        private static void GetVideoClip(string inputFileName, string outputFile, TimeSpan start, TimeSpan end, double targetDuration, string taskMemo)
        {
            GetVideoClip(inputFileName, outputFile, start.TotalSeconds, end.TotalSeconds, taskMemo, targetDuration);
        }

        private static void GetVideoClip(string inputFileName, string outputFile, double start, double end, string taskMemo, double targetDuration, string logPath = null)
        {
            var keyFrames = $"{start},{end}";
            ExecuteCommand(
                                taskMemo, targetDuration,
                                $" -loglevel error -ss {start.ToFFmpegString()} -to {end.ToFFmpegString()} -i {inputFileName} -preset ultrafast   -c:v libx264 -crf 23 -map 0 -force_key_frames {keyFrames} -x264-params keyint=25:scenecut=0 ",
                                //$"-hwaccel cuda -loglevel error -ss {start.ToFFmpegString()} -to {end.ToFFmpegString()} -i {inputFileName} -c:v h264_nvenc -preset hq -rc vbr -cq 23 -g 25 ",
                                outputFile, overWriteExist: true,
                                addationLogs: $"start:{start},end:{end}",
                                logFilePath: logPath
                                );
        }

        public static double? Concat(double targetDuration, IEnumerable<string> files, string outputFile, bool isVideo, string tempFileName, bool overWriteExist = true)
        {
            var fileListFullName = Path.Combine(Path.GetDirectoryName(outputFile), tempFileName);
            WriteToFile(fileListFullName, files);

            WriteToFile(fileListFullName, files);
            var ap = isVideo ? " -c:v copy " : " -c:a copy ";
            var an = isVideo ? "-an" : "";
            //ffmpeg
            var arguments = $" -loglevel error  -f concat -safe 0 -i {fileListFullName} {ap} {an} ";

            try
            {
                ExecuteCommand($"{(isVideo ? "V" : "A")}_连接文件", targetDuration, arguments, outputFile, overWriteExist: overWriteExist);
                return GetDuration(outputFile);
            }
            finally
            {
                //File.Delete(tempFile);
            }
        }

        public static void Concat(IEnumerable<MediaClip> clips, string outputFile, bool overWriteExist = true, StreamWriter logWriter = null)
        {
            var videos = clips.Select(t => t.VideoClip);
            var audios = clips.Select(t => t.AudioClip);

            var n = videos.Count();
            var an = audios.Count();
            if (n != an)
                throw new ArgumentException($"videos:{n},audios:{an}", "视频和音频数量不一致！");

            var videoOnly = Path.Combine(Path.GetDirectoryName(outputFile), "VideoOnly.mp4");
            var audioOnly = Path.Combine(Path.GetDirectoryName(outputFile), "AudioOnly.mp3");
            var v = Concat(clips.Last().End.TotalMilliseconds, videos.Select(t => t.OutputFile), videoOnly, true, "videolist.txt");
            var a = Concat(clips.Last().End.TotalMilliseconds, audios.Select(t => t.OutputFile), audioOnly, false, "audiolist.txt");

            logWriter?.WriteLine($"VideoOnly 时长:{v},AudioOnly 时长:{a},差异:{v - a}");
            //ffmpeg
            var arguments = $" -loglevel error -i {videoOnly} -i {audioOnly} -c:v copy -c:a aac -strict experimental ";

            try
            {
                ExecuteCommand("合并视频", audios.Last().EndTime.TotalSeconds, arguments, outputFile, overWriteExist: overWriteExist);
            }
            finally
            {
                //File.Delete(tempFile);
            }
        }

        private static void WriteToFile(string fileName, IEnumerable<string> fileLines)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                foreach (var item in fileLines)
                {
                    writer.WriteLine($"file '{item}'");
                }
            }
        }

        public static double? GetDuration(string inputAudioFile)
        {
            var rst = FFProbe($"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 {inputAudioFile}");
            //ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 input_audio_file
            if (double.TryParse(rst, out double duration))
            {
                return duration * 1000;
            }
            return null;
        }
        static string FFProbe(string command, bool useShellExecute = false)
        {
            var sw = Stopwatch.StartNew();
            //如果targetDuration为空，则不需要去验证目标时长
            var pi = new ProcessStartInfo();
            pi.FileName = ffprobe;
            pi.Arguments = command;
            pi.UseShellExecute = useShellExecute;
            pi.RedirectStandardError = true;
            pi.RedirectStandardOutput = true;
            pi.CreateNoWindow = true;
            var inf = Process.Start(pi);
            var rst = "";

            rst = inf.StandardOutput.ReadToEnd();
            inf.WaitForExit();
            sw.Stop();
            return rst;
        }
        public static void ConvertFPS(string inputFile,int fps,string outputFile)
        {
            //ffmpeg -i input.mp4 -filter:v "minterpolate='fps=100'" output.mp4
            ExecuteFFmpegCommand(inputFiles:inputFile,outputOptions: $"-filter:v \"minterpolate=fps={fps}\" -preset ultrafast -t 10 -y", outputFiles:outputFile);
        }
        public static void GetClip(string inputFile,int start,int end,string outputFile)
        {
            ExecuteFFmpegCommand(inputFiles: inputFile, outputOptions: $"-ss {start/1000d} -to {end/1000d} -y", outputFiles: outputFile);
        }

        public static void NAudioGetClip(string inputFile, int start, int end, string outputFile)
        {
            using (var reader = new AudioFileReader(inputFile))
            {
                var offsetSampleProvider = new OffsetSampleProvider(reader);
                offsetSampleProvider.SkipOver = TimeSpan.FromMilliseconds(start);  // 开始时间
                offsetSampleProvider.Take = TimeSpan.FromMilliseconds(end-start);   // 结束时间
                using var ofs = File.OpenWrite(outputFile);
                WaveFileWriter.WriteWavFileToStream(ofs, offsetSampleProvider.ToWaveProvider());
            }
        }
        public static void ConcatAudios(string[] audioFilePaths)
        {
            // 创建一个列表用于存储音频文件的ISampleProvider
            List<ISampleProvider> sampleProviders = new List<ISampleProvider>();

            // 循环加载每个音频文件，指定开始时间和结束时间
            foreach (var audioFilePath in audioFilePaths)
            {
                var audioFileReader = new AudioFileReader(audioFilePath);

                // 创建一个OffsetSampleProvider，并设置开始时间和结束时间
                var offsetSampleProvider = new OffsetSampleProvider(audioFileReader);
                offsetSampleProvider.SkipOver = TimeSpan.FromMilliseconds(10);  // 开始时间
                offsetSampleProvider.Take = TimeSpan.FromMilliseconds(1021);   // 结束时间

                // 添加到列表中
                sampleProviders.Add(offsetSampleProvider);
            }

            // 创建一个ConcatenatingSampleProvider，将所有音频文件连接在一起
            var concatenatingSampleProvider = new ConcatenatingSampleProvider(sampleProviders);

            // 写入到输出文件
            WaveFileWriter.CreateWaveFile16("output.wav", concatenatingSampleProvider);
        }

        public static void Mp32Wav(string inputFile, string outputFile,int ar = 44100)
        {
            ExecuteFFmpegCommand(inputFiles: inputFile, outputOptions: $"-acodec pcm_s16le -ar {ar} -y", outputFiles: outputFile);
        }
        public static void Mp32Flac(string inputFile, string outputFile,int ar = 44100)
        {
            ExecuteFFmpegCommand(inputFiles: inputFile, outputOptions: $"-ar {ar} -y", outputFiles: outputFile);
        }
        public static void ConvertFPS2XXX(string inputFile,int fps,string outputFile)
        {
            //ffmpeg -i input.mp4 -filter:v "minterpolate='fps=100'" output.mp4
            ExecuteFFmpegCommand(inputFiles:inputFile,outputOptions: $" -c:v libx264 -keyint_min {fps} -g {fps} -preset ultrafast -t 10 -y", outputFiles:outputFile);
        }
        public static void SetKeyFrames(string inputFile,int[] keyFrames,string outputFile)
        {
            ExecuteFFmpegCommand(inputFiles:inputFile,outputOptions: $"-force_key_frames \"{keyFrames.Select(t=>TimeSpan.FromMilliseconds(t).ToString()).Join(",")}\" -codec:v libx264 -preset veryfast -crf 23 -t 10", outputFiles:outputFile);

        }
        public static string ShowKeyFrames(string inputFile)
        {
            //       ffprobe -select_streams v:0 -show_frames -show_entries frame=pict_type,best_effort_timestamp_time -of csv input.mp4 | grep -n I
            return FFProbe($"-select_streams v:0 -show_frames -show_entries frame=pict_type,best_effort_timestamp_time -of csv {inputFile} ");
        }

    }
}
