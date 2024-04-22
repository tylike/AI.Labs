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

namespace AI.Labs.Module.BusinessObjects
{
    public static class FFmpegHelper
    {
        /// <summary>
        /// 
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
            if (targetDuration.HasValue && !string.IsNullOrEmpty(outputFile))
            {
                var dur = GetDuration(outputFile);
                var diff = dur.Value - targetDuration.Value;


                if (diff > 0.5)
                {

                    var logPath = Path.Combine(logFilePath ?? "d:\\temp", "logs", $"{taskMemo}-{(dur.Value == targetDuration.Value).ToString()}_log.txt");
                    var logs = @$"{pi.FileName} {command}
输出文件时长:{dur.Value}
目标时长:{targetDuration.Value}
附加信息:
{addationLogs + ""}
精确差异:{dur.Value - targetDuration.Value}
";
                    File.WriteAllText(logPath, logs);
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

        public static string ChangeAudioSpeed(
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
                return LastCommand;
            }
            else
            {
                return $"文件已存在:{outputFile}";
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

        //public static void ChangeAudioSpeed(this StringBuilder sb, double targetSpeed, string inputLables = null, string outputLables = null)
        //{
        //    //[0:v]trim=0.11:7,setpts=PTS-STARTPTS[v{idx}]
        //    sb.AppendNotEmptyOrNull(inputLables);
        //    sb.Append($"asetpts=PTS*{targetSpeed.ToFFmpegString()}");
        //    sb.AppendNotEmptyOrNull(outputLables);
        //}



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
                    outputOptions: $" -t {targetDuration} "
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

        public static void Concat(IEnumerable<MediaClip> clips, string outputFile, bool overWriteExist = true,StreamWriter logWriter = null)
        {
            var videos = clips.Select(t => t.VideoClip);
            var audios = clips.Select(t => t.AudioClip);

            var n = videos.Count();
            var an = audios.Count();
            if (n != an)
                throw new ArgumentException($"videos:{n},audios:{an}", "视频和音频数量不一致！");

            var videoOnly = Path.Combine(Path.GetDirectoryName(outputFile), "VideoOnly.mp4");
            var audioOnly = Path.Combine(Path.GetDirectoryName(outputFile), "AudioOnly.mp3");
            var v = Concat(videos.Last().EndTime.TotalSeconds, videos.Select(t => t.OutputFile), videoOnly, true, "videolist.txt");
            var a= Concat(audios.Last().EndTime.TotalSeconds, audios.Select(t => t.OutputFile), audioOnly, false, "audiolist.txt");

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
                return duration;
            }
            return null;
        }
        static string FFProbe(string command,bool useShellExecute = false)
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
    }
}
