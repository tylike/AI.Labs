using AI.Labs.Module.BusinessObjects.VideoTranslate;
using DevExpress.Charts.Native;
using DevExpress.Spreadsheet;
using FFMpegCore.Enums;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using YoutubeExplode.Videos;

namespace AI.Labs.Module.BusinessObjects
{
    public static class FFmpegHelper
    {

        const string ffprobe = @"D:\ffmpeg.gui\last\ffprobe.exe";
        public const string ffmpegFile = @"D:\ffmpeg.gui\last\ffmpeg.exe";

        #region execute command
        public static string ExecuteCommand(string command, bool useShellExecute = false,bool ffmpeg = true,bool readOutput = false,bool noWindow = false)
        {
            var pi = new ProcessStartInfo();
            pi.FileName = ffmpeg ? ffmpegFile : ffprobe;
            pi.Arguments = command;
            pi.UseShellExecute = useShellExecute;
            pi.RedirectStandardError = readOutput;
            pi.RedirectStandardOutput = readOutput;
            pi.CreateNoWindow = noWindow;
            var inf = Process.Start(pi);
            var rst = "";
            if(readOutput)
            {
                rst = inf.StandardOutput.ReadToEnd();
            }
            inf.WaitForExit();
            Debug.WriteLine($"{pi.FileName} {pi.Arguments}");
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
        public static void ChangeAudioSpeed(string inputFileName, double speed, string outputFile)
        {
            ExecuteCommand($"-i {inputFileName} -filter:a:0 \"atempo={speed:0.0########}\" {outputFile} -y");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="targetSpeed">2.0为2倍，0.5为慢放2倍</param>
        /// <param name="outputFile"></param>
        public static void ChangeVideoSpeed(string inputFileName, double targetSpeed, string outputFile)
        {
            ArgumentNullException.ThrowIfNull(inputFileName, nameof(inputFileName));
            ArgumentNullException.ThrowIfNull(outputFile, nameof(outputFile));

            //ffmpeg -i input.mp4 -filter_complex "[0:v]setpts=PTS/2[v];[0:a]atempo=2[a]" -map "[v]" -map "[a]" output_fast.mp4
            ExecuteCommand($"-i {inputFileName} -filter_complex \"[0:v]setpts=PTS/{targetSpeed.ToFFmpegString()}[v];\" -map \"[v]\" {outputFile} -y");
        }

        public static void ChangeAudioSpeed(this StringBuilder sb, double targetSpeed, string inputLables = null, string outputLables = null)
        {
            //[0:v]trim=0.11:7,setpts=PTS-STARTPTS[v{idx}]
            sb.AppendNotEmptyOrNull(inputLables);
            sb.Append($"asetpts=PTS*{targetSpeed.ToFFmpegString()}");
            sb.AppendNotEmptyOrNull(outputLables);
        }



        public static void DelayAudio(string inputFileName, double delayMS, string newOutputFile)
        {
            //-i input.mp3 -af apad=pad_dur=3000 output.mp3
            ExecuteCommand($"-i {inputFileName} -af apad=pad_dur={(delayMS / 1000).ToFFmpegString()} {newOutputFile} -y");
        }

        /// <summary>
        /// 延时视频，使用视频的最后一帧复制
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="delayMS"></param>
        /// <param name="newOutputFile"></param>
        public static void DelayVideoCopyLast(string inputFileName, double delayMS, string newOutputFile)
        {
            //-i input.mp4 -vf "setpts=PTS+3/TB" output.mp4
            //开头加: -filter_complex "[0:v]tpad=start_mode=clone:start_duration=3[v];[0:a]apad=pad_dur=3:pad_plac=0[a]" -map "[v]" -map "[a]" output.mp4
            //末尾加: -filter_complex "[0:v]tpad=stop_mode=clone:stop_duration=3[v];[0:a]apad=pad_dur=3[a]" -map "[v]" -map "[a]" output.mp4
            var delayTime = (delayMS / 1000).ToFFmpegString();
            ExecuteCommand($"-i {inputFileName} -filter_complex \"[0:v]tpad=stop_mode=clone:stop_duration={delayTime}[v];\" -map \"[v]\" {newOutputFile} -y");
        }
        class temp
        {
            public TimeSpan start;
            public TimeSpan? end;
            public int index;
        }
        /// <summary>
        /// D:\videoInfo\2\v\video_%04d.mp4
        /// </summary>
        /// <param name="inputFileName"></param>
        /// <param name="times"></param>
        /// <param name="outputPath"></param>
        public static string[] SplitVideo(string inputFileName, IEnumerable<SubtitleItem> times, string outputPath)
        {
            //ffmpeg -i input.mp4 -f segment -segment_times 10.500,22.712,35.145,48.376 -c copy output_%03d.mp4
            var items = times.Select(t => new temp { start = t.StartTime,end = t.EndTime,index = t.Index }).ToArray();
            items.First().start = TimeSpan.FromSeconds(0);
            items.Last().end = null;

            var rst = new List<string>();
            foreach (var item in items)
            {
                var outputFile = Path.Combine(outputPath, $"{item.index}.mp4");
                var ss = item.start.TotalSeconds.ToFFmpegString();
                var key = ss;
                var to = "";
                if (item.end.HasValue)
                {
                    to += $"-to {item.end.Value.TotalSeconds.ToFFmpegFormat()}";
                    key+=","+item.end.Value.TotalSeconds.ToFFmpegFormat(); 
                }

                ExecuteCommand($"-ss {ss} {to} -i {inputFileName}   -c:v libx264 -crf 23 -map 0 -force_key_frames {key} -x264-params keyint=25:scenecut=0   {outputFile} -y");
                rst.Add(outputFile);
            }
            return rst.ToArray();
        }

        public static void Concat(IEnumerable<string> files, string outputFile,bool isVideo,string tempFileName)
        {


            var fileListFullName = Path.Combine(Path.GetDirectoryName(outputFile), tempFileName);
            WriteToFile(fileListFullName, files);

            WriteToFile(fileListFullName, files);
            var ap = isVideo ? " -c:v copy " : " -c:a copy ";
            var an = isVideo ? "-an":"";
            //ffmpeg
            var arguments = $"-report -f concat -safe 0 -i {fileListFullName} {ap} {an} {outputFile} -y";

            try
            {
                ExecuteCommand(arguments);
            }
            finally
            {
                //File.Delete(tempFile);
            }
        }


        public static void Concat(IEnumerable<string> videos, IEnumerable<string> audios, string outputFile)
        {
            var n = videos.Count();
            var an = audios.Count();
            if (n != an)
                throw new ArgumentException($"videos:{n},audios:{an}", "视频和音频数量不一致！");
            
            var videoOnly = Path.Combine(Path.GetDirectoryName(outputFile), "videoOnly.mp4");
            var audioOnly = Path.Combine(Path.GetDirectoryName(outputFile), "AudioOnly.mp3");
            Concat(videos, videoOnly, true, "videolist.txt");
            Concat(audios, audioOnly, false, "audiolist.txt");

            //ffmpeg
            var arguments = $" -i {videoOnly} -i {audioOnly} -c:v copy -c:a aac -strict experimental {outputFile} -y";

            try
            {
                ExecuteCommand(arguments);
            }
            finally
            {
                //File.Delete(tempFile);
            }
        }

        private static void WriteToFile(string fileName,IEnumerable<string> fileLines)
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
            var rst = ExecuteCommand(
                $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 {inputAudioFile}", ffmpeg: false,readOutput:true,noWindow:true);
            //ffprobe -v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 input_audio_file
            if(double.TryParse(rst,  out double duration))
            {
                return duration;
            }
            return null;
        }
    }
}
