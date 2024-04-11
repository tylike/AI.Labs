using DevExpress.Charts.Native;
using Microsoft.CodeAnalysis.Operations;
using System.Diagnostics;
using System.Text;
using YoutubeExplode.Videos;

namespace AI.Labs.Module.BusinessObjects
{
    public static class FFmpegHelper
    {
        const string ffprobe = @"D:\ffmpeg.gui\last\ffprobe.exe";
        public const string ffmpegFile = @"D:\ffmpeg.gui\last\ffmpeg.exe";
        public static void ExecuteCommand(string command)
        {
            var pi = new ProcessStartInfo();
            pi.FileName = ffmpegFile;
            pi.Arguments = command;
            pi.UseShellExecute = true;
            var inf = Process.Start(pi);
            inf.WaitForExit();
            Debug.WriteLine($"{pi.FileName} {pi.Arguments}");
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
            var masterCommand = $@"{FFmpegHelper.ffmpegFile} {inputFiles} -/filter_complex {filterComplexScript} {mainParameter} -t {duration.ToString("0.000")} {overrideOptions} {outputFiles}";
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


        public static string ChangeVideoSpeed(decimal targetSpeed, string inputLables = null, string outputLables = null)
        {
            var sb = new StringBuilder();
            sb.ChangeAudioSpeed(targetSpeed, inputLables, outputLables);
            return sb.ToString();
        }

        public static void ChangeAudioSpeed(this StringBuilder sb,decimal targetSpeed, string inputLables = null, string outputLables = null)
        {
            //[0:v]trim=0.11:7,setpts=PTS-STARTPTS[v{idx}]
            sb.AppendNotEmptyOrNull(inputLables);
            sb.Append($"asetpts=PTS*{targetSpeed.ToString("0.00000")}");
            sb.AppendNotEmptyOrNull(outputLables);
        }

        public static void AppendNotEmptyOrNull(this StringBuilder sb, string text, bool appendLine = false)
        {
            if (!string.IsNullOrEmpty(text))
            {
                sb.Append(text);
                if (appendLine)
                    sb.AppendLine();
            }
        }
        public static string Join(this IEnumerable<string> values, string separator = "")
        {
            return string.Join(separator, values);
        }
        public static string ToFFmpegSeconds(this TimeSpan time)
        {
            return time.TotalSeconds.ToString("0.0#####");
        }
        public static TimeSpan AddMilliseconds(this TimeSpan time, double milliseconds)
        {
            return time.Add(TimeSpan.FromMilliseconds(milliseconds));
        }
        public static TimeSpan Subtract(this TimeSpan time, double substract)
        {
            return time.Subtract(TimeSpan.FromMilliseconds(substract));
        }
    }
}
