//using SubtitlesParser.Classes; // 引入SubtitlesParser的命名空间
//using SubtitlesParser.Classes.Parsers;
using AI.Labs.Module.BusinessObjects.AudioBooks;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using com.google.protobuf;
using DevExpress.ExpressApp;
using DevExpress.PivotGrid.Criteria;
using DevExpress.Spreadsheet;
using DevExpress.Utils.Text;
using DevExpress.XtraSpreadsheet.Model;
using FFMpegCore;
using FFMpegCore.Enums;
using Humanizer;
using java.security.cert;
using javax.print.attribute.standard;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;
using Xabe.FFmpeg;
using YoutubeExplode.Videos;
using ffmpeg = Xabe.FFmpeg.FFmpeg;
namespace AI.Labs.Module.BusinessObjects
{
    public class AddVideoSubtitle
    {
        public string SubtitleFileName { get; set; }
        public string Style { get; set; }
    }

    public class VideoHelper
    {
        public static (bool, string) AddText(
            (TimeSpan Time, int Duration)[] addDuration,
            string inputFile,
            string outputFile,
            DrawTextOptions option
            )
        {
            // 使用DrawTextOptions类:
            var options = addDuration.Select(t => new SRT
            {
                Text = $"此处延时:{t.Duration}",
                StartTime = t.Time,
                EndTime = t.Time.Add(TimeSpan.FromMilliseconds(t.Duration))
            });
            var srtFile = new SRTFile();
            srtFile.Texts.AddRange(options);
            srtFile.FileName = Path.Combine(Path.GetDirectoryName(inputFile), "附加显示文字.srt");
            srtFile.Save();


            //-map "[out]" -c:v libx264 
            var cnStyle = "Alignment=1,Fontsize=24,PrimaryColour=&HFFFFFF,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000";
            var args = $"-i {inputFile} -filter_complex \"[0:v]subtitles='{Fix(srtFile.FileName)}':force_style='{Fix(cnStyle)}'[r1];\" -map \"[r1]\"  -c:v libx264 -c:a copy {outputFile} -y ";

            //-/filter_complex 文件名.txt 来自文件的滤镜命令设置

            return RunFFmpeg(args, "填加说明字幕");

            //var opts = string.Join(",", options.Select(o => o.ToFilterString()));

            //var arg = $"-i {inputFile} -vf \"{opts}\" {outputFile} -y";

            //return RunFFmpeg(arg, "填加文字",new Dictionary<string, string>() { { "DefFont", "fontsize=24:fontcolor=white@0.5:borderw=2:bordercolor=white:x=100:y=100:fontFile=C:\\Users\\46035\\Documents\\mindplus-py\\environment\\Python3.8.5-64\\Lib\\site-packages\\pinpong\\base\\msyh.ttf" } });
        }
        static string Fix(string path)
        {
            return path.Replace("\\", "\\\\").Replace(":", "\\:");
        }
        public static (bool, string) 增加时长((string File, int Duration)[] addDuration, string outputFile)
        {
            var inputs = string.Join(" ", addDuration.Select(t => $" -i {t.File} ").ToArray());
            var filters = string.Join("",
                addDuration.Select(
                (t, i) => $"[{i}:v]tpad=stop_duration={t.Duration}ms:stop_mode=clone[v{i}];"
                ).ToArray()
                );

            var mapItems = string.Join("", addDuration.Select((t, i) => $"[v{i}]").ToArray());

            var map = $" -map \"[v]\"";
            return RunFFmpeg($"{inputs} -filter_complex \"{filters}{mapItems}concat=n={addDuration.Length}:v=1[v]\" {map} {outputFile} -y", "增加时长");
            
            //ffmpeg -i input1.mp4 -i input2.mp4 -i input3.mp4 -filter_complex
            //"
            //[0:v]tpad=stop_duration=1000ms:stop_mode=clone[v0];
            //[1:v]tpad=stop_duration=1000ms:stop_mode=clone[v1];
            //[2:v]tpad=stop_duration=1000ms:stop_mode=clone[v2];
            //[v0][0:a][v1][1:a][v2]
            //[2:a]concat=n=3:v=1:a=1[v][a]"
            //-map "[v]"
            //-map "[a]"
            //output.mp4

            //ffmpeg -i input.mp4 -vf "tpad=stop_duration=1000ms:stop_mode=clone" output.mp4
            //    string arguments = $"-i \"{inputFile}\" -vf \"tpad=start_duration=0:stop_duration={milliseconds}ms:stop_mode=clone\" \"{outputFile}\"";
        }

        public class Segment
        {
            public int Index;
            public List<AudioBookTextAudioItem> Audios;
        }

        public static string AddDelayWithText(VideoInfo video, string videoPath,string outputFile = "已加字幕.mp4")
        {
            //如果音频时长超时了,则取到这些音频，并增加视频的时长
            var groups = video.Audios.GroupWhile(
                (pre, current) =>
                {
                    return current.Diffence <= 0;
                }
                );

            var srts = groups.Select(t => t.Last().Duration).ToList();
            //分隔的视频            
            var files = SplitVideo(groups, video, videoPath);

            var fileAddDuration = srts.Zip(files, (d, f) => (File: f, Duration: d)).ToArray();
            //增加时长
            var fixedSubtitle = Path.Combine(video.ProjectPath, "已修正时长.mp4");
            Debug.WriteLine("===============");
            Debug.WriteLine("为每个小段增加时长,与音频时长相同:");
            var logs = 增加时长(fileAddDuration, fixedSubtitle);
            video.AddLog("增加时长完成", logs.Item2);


            var addTexted = Path.Combine(video.ProjectPath, outputFile);

            Debug.WriteLine("===============");
            Debug.WriteLine("填加说明文字:");
            logs = AddText(
                groups.Select(t => (Time: t.Last().Subtitle.EndTime, Duration: t.Last().Diffence)).ToArray(),
                fixedSubtitle, addTexted, new DrawTextOptions { }
                );
            video.AddLog("填加说明文字完成", logs.Item2);
            return addTexted;
        }

        public static string[] SplitVideo(IEnumerable<IEnumerable<AudioBookTextAudioItem>> groups, VideoInfo video, string videoPath)
        {
            var path = Path.Combine(video.ProjectPath, "tempVideo");

            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //// 对于最后一段，需要从splitPoints[i]到视频结尾
            //// 可以通过获取视频信息来计算或者不指定持续时间以直接到结尾
            //var videoInfo = FFMpegArguments
            //    .FromFileInput(videoPath);

            ////var inputFileInfo = new FFMpegCore.Pipes.InputFileInfo(inputVideoPath);
            //var splitPoints = groups.Select(t => t.Last().Subtitle.EndTime).ToArray();

            //// 按照提供的切割点分割视频
            //TimeSpan start = TimeSpan.Zero;

            //for (int i = 0; i < splitPoints.Length; i++)
            //{
            //    var currentPoint = splitPoints[i];
            //    if (currentPoint <= start)
            //    {
            //        throw new Exception("错误,结束时间在开始时间之前!");
            //    }

            //    // 计算当前段的持续时间
            //    TimeSpan duration = currentPoint - start;

            //    var outputPath = Path.Combine(path, $"srt_split_{i + 1}.mp4");

            //    // 使用FFMpeg执行分割
            //    var p =
            //    FFMpegArguments
            //        .FromFileInput(videoPath)
            //        .OutputToFile(outputPath, true, options => options
            //            .Seek(start)
            //            .WithDuration(duration)
            //            .CopyChannel(Channel.Video)
            //            .CopyChannel(Channel.Audio)
            //            );

            //        p.ProcessSynchronously();

            //    Debug.WriteLine(p.Arguments);
            //    start = currentPoint;
            //}



            //使用SegmentMuxerOptions类:
            var options = new SegmentMuxerOptions()
            {
                InputFile = videoPath,
                OutputFile = Path.Combine(video.ProjectPath, "tempVideo", "srt_split_%4d.mp4"),
                SegmentTimes = groups.Select(t => t.Last().Subtitle.EndTime).ToList()
            };
            Debug.WriteLine("===============");
            Debug.WriteLine("分割视频:");
            var logs = RunFFmpeg(options.ToArgumentString(), "分割视频");
            video.AddLog("分割视频完成", logs.Item2);

            return Directory.GetFiles(Path.Combine(video.ProjectPath, "tempVideo"), "srt_split_*.mp4");
            //D:\ffmpeg.gui\ffmpeg\bin\ffmpeg.exe
            //- i d:\VideoInfo\44\video.srt.mp4 -c:v libx264 -crf 23 -map 0
            //-force_key_frames
            //4.15,10.4,15.79,19.63,24.42,30.26,31.92,33.37,54.13,56.31,60.68,61.61,64.4,69.0699999,75.06,81.55,83.99,89.63,95.47,101.27,108.74,122.04,128.46,133.46,137.06,140.36,149.25,151.2,158.3899999,166.7,171.02,174.31,178.97,186.81,192.08,196.6699999,204.02,208.52,218.75,223.46,235.7,240.58,246.11,251.85,257.86,261.52,269.47,271.92,285.87,291.36,297.54,307.91,313.63,318.66,323.12,327.71,341.51,346.12,351.5,358.63,364.74,372.8,376.99,380.67,387.42,403.68
            //-x264-params keyint=25:scenecut=0
            //-f segment -segment_times 4.15,10.4,15.79,19.63,24.42,30.26,31.92,33.37,54.13,56.31,60.68,61.61,64.4,69.0699999,75.06,81.55,83.99,89.63,95.47,101.27,108.74,122.04,128.46,133.46,137.06,140.36,149.25,151.2,158.3899999,166.7,171.02,174.31,178.97,186.81,192.08,196.6699999,204.02,208.52,218.75,223.46,235.7,240.58,246.11,251.85,257.86,261.52,269.47,271.92,285.87,291.36,297.54,307.91,313.63,318.66,323.12,327.71,341.51,346.12,351.5,358.63,364.74,372.8,376.99,380.67,387.42,403.68
            //d:\VideoInfo\44\tempVideo\srt_split_ % 4d.mp4
        }
        public static (bool, string) RunFFmpeg(string argument, string taskTitle, IDictionary<string, string> env = null)
        {
            var logs = new StringBuilder();
            var cmdForLog = $"{taskTitle}\n{FFmpegHelper.ffmpegFile} {argument}";
            logs.Log(cmdForLog, showDebug: true);
            //argument = argument.Replace("\"", "\\\"");
            //var fa = $"/c \"{AudioHelper.ffmpegFile} {argument}\" && echo 按键退出 &&  pause ";
            var fa = argument;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = FFmpegHelper.ffmpegFile,
                    Arguments = fa,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                },
                EnableRaisingEvents = true,
            };
            if (env != null)
            {
                foreach (var item in env)
                {
                    process.StartInfo.Environment.Add(item.Key, item.Value);
                }
            }

            process.ErrorDataReceived += (s, e) => logs.Log(e.Data, true);
            process.OutputDataReceived += (s, e) => logs.Log(e.Data, false);

            //process.Exited += (s, e) => logs.Log("退出", false);
            cmdForLog = $"{FFmpegHelper.ffmpegFile} {argument}";
            Debug.WriteLine(cmdForLog);

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                var error = $"{taskTitle}=>出错了:{cmdForLog}";

                logs.Log(error, true, true);
                Debug.WriteLine(logs.ToString());
            }
            else
            {
                logs.Log($"{taskTitle}成功完成!", false, true);
            }
            return (process.ExitCode != 0, logs.ToString());
        }

        public static void AddFreezeFramesWithText(
    string inputVideoPath,
    string outputVideoPath,
    (TimeSpan FreezePosition, int FreezeDurationMs, string FreezeText)[] freezeFrames,
    string fontFilePath = "C:\\Users\\46035\\Documents\\mindplus-py\\environment\\Python3.8.5-64\\Lib\\site-packages\\pinpong\\base\\msyh.ttf")
        {
            // Build the filter_complex string using the object-oriented approach
            string filterComplex = BuildFilterComplex(freezeFrames, fontFilePath);

            // Set up the process start information for FFmpeg
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = FFmpegHelper.ffmpegFile,
                Arguments = $"-i \"{inputVideoPath}\" -filter_complex \"{filterComplex}\" -map \"[v]\" -c:v libx264 -preset fast -crf 22 -y \"{outputVideoPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process ffmpegProcess = new Process { StartInfo = startInfo })
            {
                // Start the process and wait for it to exit
                ffmpegProcess.Start();

                // Read the standard error stream asynchronously to capture FFmpeg logs
                string stderr = ffmpegProcess.StandardError.ReadToEnd();
                ffmpegProcess.WaitForExit();

                // Log or handle FFmpeg stderr output if needed
                Console.WriteLine(stderr);

                // Check for process exit code (0 is success)
                if (ffmpegProcess.ExitCode != 0)
                {
                    throw new InvalidOperationException($"FFmpeg exited with non-zero exit code: {ffmpegProcess.ExitCode}");
                }
            }
        }


        public static string BuildFilterComplex(
    (TimeSpan FreezePosition, int FreezeDurationMs, string FreezeText)[] freezeFrames,
    string fontFilePath)
        {
            List<FFMpegFilter> filters = new List<FFMpegFilter>();
            string[] freezeLabels = new string[freezeFrames.Length];
            string[] textLabels = new string[freezeFrames.Length];
            string[] videoLabels = new string[freezeFrames.Length * 3]; // Original + Freeze + Text

            for (int i = 0; i < freezeFrames.Length; i++)
            {
                var (FreezePosition, FreezeDurationMs, FreezeText) = freezeFrames[i];
                TimeSpan freezeDuration = TimeSpan.FromMilliseconds(FreezeDurationMs);

                // Create identifiers for each filter output
                freezeLabels[i] = $"freeze{i}";
                textLabels[i] = $"freezeText{i}";
                videoLabels[i * 3] = $"before{i}";
                videoLabels[i * 3 + 1] = $"freezeWithText{i}";
                videoLabels[i * 3 + 2] = $"after{i}";

                // Trim original video until freeze frame
                filters.Add(new TrimFilter(FreezePosition, null) { Label = videoLabels[i * 3] });

                // Create freeze frame
                filters.Add(new FreezeFrameFilter(freezeDuration) { Label = freezeLabels[i] });
                filters.Add(new DrawTextFilter(FreezeText, fontFilePath, 24) { Label = textLabels[i] });

                // Overlay text on freeze frame
                filters.Add(new OverlayFilter(freezeLabels[i], textLabels[i], videoLabels[i * 3 + 1]));

                // Trim original video after freeze frame
                filters.Add(new TrimFilter(FreezePosition, null) { Label = videoLabels[i * 3 + 2] });
            }

            // Concatenate all pieces together
            filters.Add(new ConcatFilter(videoLabels.Length) { Label = "final" });

            // Build the filter_complex string
            return string.Join(";", filters.Select(f => f.ToString()));
        }

        static VideoHelper()
        {
            ffmpeg.SetExecutablesPath(Path.GetDirectoryName(FFmpegHelper.ffmpegFile));
            GlobalFFOptions.Configure(new FFOptions
            {
                BinaryFolder = Path.GetDirectoryName(FFmpegHelper.ffmpegFile),
            });
        }

        public static async Task MakeVideoAsync(VideoInfo video)
        {
            await Task.Run(() =>
            {
                MakeVideo(video);
            });
        }

        public static void MakeVideo(VideoInfo video)
        {
            string videoFile = video.VideoFile;

            //var duration = TimeSpan.Parse(video.Duration);

            //根据字幕增加时长
            var file = AddDelayWithText(video, video.VideoFile);

            //得到中文音频
            var cnAudioFile = Path.Combine(video.ProjectPath, "CnAudio.mp3");
            合并小段音频(videoFile, cnAudioFile, video);

            //加字幕:中+英
            var videoWithSrt = Path.Combine(video.ProjectPath, "video.srt.mp4");
            var log = 填加字幕(file, video.VideoChineseSRT, videoWithSrt, cnAudioFile, video.VideoDefaultSRT);
            video.AddLog("填加字幕", log.Item2);

            //合并音频和视频
            合并音频和视频(video, videoWithSrt, cnAudioFile).Wait();

        }

        static async Task ConvertAudioToWav(string inputFile, string outputWav)
        {
            // 创建一个FFmpeg实例
            IConversion conversion = ffmpeg.Conversions.New();

            var info = await ffmpeg.GetMediaInfo(inputFile);

            // 添加输入文件
            conversion.AddStream(info.AudioStreams);

            // 设置输出文件的路径，更改扩展名为.wav            
            conversion.SetOutput(outputWav);

            // 开始转换
            await conversion.Run();
        }

        static void ConvertWavToMp3(string inputWav, string outputMp3)
        {
            // 创建转换器并设置输入文件
            IConversion conversion = ffmpeg.Conversions.New()
                .AddParameter($"-i \"{inputWav}\"") // 输入文件
                .AddParameter("-ar 24000") // 设置音频采样率为 24000 Hz
                .AddParameter("-ac 1") // 设置音频通道为 mono
                .AddParameter("-ab 48k") // 设置音频比特率为 48 kb/s
                .AddParameter("-f mp3"); // 设置输出格式为 MP3

            conversion.SetOutput(outputMp3);

            // 异步执行转换
            conversion.Run().Wait();

        }
        /// <summary>
        /// 使用的是ffmpeg -f concat
        /// </summary>
        /// <param name="videoFile"></param>
        /// <param name="outputAudioFile"></param>
        /// <param name="video"></param>
        public static void 合并小段音频(string videoFile, string outputAudioFile, VideoInfo video)
        {
            //这里的'duration 5'表示在每个文件之间添加5秒的间隔。

            var last = TimeSpan.Zero;
            var parametersFile = Path.Combine(video.ProjectPath, "cfiles.txt");

            if (File.Exists(parametersFile))
            {
                File.Delete(parametersFile);
            }

            using var stream = File.CreateText(parametersFile);
            foreach (var srt in video.Audios.OrderBy(t => t.Index).Where(t => t.State == TTSState.Generated))
            {
                var audioFile = Path.Combine(video.ProjectPath, "Audio", $"{srt.Oid}_last.mp3");
                if (!File.Exists(audioFile))
                {
                    ConvertWavToMp3(srt.OutputFileName, audioFile);
                }
                stream.WriteLine($"file '{audioFile}'");
                // 添加间隔时间
                if (srt.Subtitle.StartTime > last)
                {
                    var silenceDuration = (srt.Subtitle.StartTime.TotalSeconds - last.TotalSeconds); // 间隔时间，单位为秒
                    stream.WriteLine($"duration {silenceDuration.ToString("####0.###")}");
                }
                last = srt.Subtitle.EndTime;
            }
            stream.Flush();
            stream.Close();

            //if (File.Exists(outputAudioFile))
            //{
            //    File.Delete(outputAudioFile);
            //}

            RunFFmpeg($"-f concat  -safe 0 -i {parametersFile} -c copy {outputAudioFile} -y", "合并小段音频");

        }

        public static void CombineAudioWithVideoAsync(string videoFile, string outputWavFile, VideoInfo video)
        {
            if (File.Exists(outputWavFile))
            {
                File.Delete(outputWavFile);
            }

            //var format = new WaveFormat(24000, 16, 1);
            //var last = TimeSpan.Zero;
            //var currentTime = TimeSpan.Zero;

            //using (var writer = new LameMP3FileWriter(outputWavFile, format, LAMEPreset.VBR_90))
            //{
            //    foreach (var srt in video.Audios.OrderBy(t => t.Index).Where(t => t.State == TTSState.Generated))
            //    {
            //        var audioFile = Path.Combine(video.ProjectPath, "Audio", $"{srt.Oid}_last.mp3");
            //        if (!File.Exists(audioFile))
            //        {
            //            ConvertWavToMp3(srt.OutputFileName, audioFile);
            //        }

            //        // 添加间隔时间
            //        if (srt.Subtitle.StartTime > last)
            //        {
            //            int silenceDuration = (int)(srt.Subtitle.StartTime.TotalMilliseconds - last.TotalMilliseconds); // 间隔时间，单位为秒
            //            byte[] silence = new byte[silenceDuration * format.AverageBytesPerSecond / 1000];
            //            writer.Write(silence, 0, silence.Length);
            //        }

            //        using (var reader = new Mp3FileReader(audioFile))
            //        {
            //            if ((format.SampleRate != reader.WaveFormat.SampleRate) ||
            //                (format.BitsPerSample != reader.WaveFormat.BitsPerSample) ||
            //                (format.Channels != reader.WaveFormat.Channels))
            //            {
            //                throw new InvalidOperationException("The MP3 files have different wave formats and cannot be concatenated directly.");
            //            }

            //            reader.CopyTo(writer);
            //            //实际的音频结束时间，比字幕的结束时间短时，应补充静默时间
            //            var realEnd = srt.Subtitle.StartTime.Add(reader.TotalTime);
            //            if (realEnd < srt.Subtitle.EndTime)
            //            {
            //                int silenceDuration = (int)(srt.Subtitle.EndTime.TotalMilliseconds - realEnd.TotalMilliseconds); // 间隔时间，单位为秒
            //                byte[] silence = new byte[silenceDuration * format.AverageBytesPerSecond / 1000];
            //                writer.Write(silence, 0, silence.Length);
            //            }
            //        }


            //        last = srt.Subtitle.EndTime;
            //    }
            //}

            //using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputWavFile, new WaveFormat(16000,16,1)))
            //{
            //    var last = TimeSpan.Zero;
            //    foreach (var srt in video.Audios.Where(t => t.State == TTSState.Generated))
            //    {

            //        using (WaveFileReader reader = new WaveFileReader(audioFile))
            //        {
            //            // 如果需要，添加静默时间
            //            if (srt.Subtitle.StartTime > last)
            //            {
            //                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;
            //                int silenceLength = (int)(srt.Subtitle.StartTime.TotalMilliseconds * bytesPerMillisecond);
            //                byte[] silence = new byte[silenceLength];
            //                waveFileWriter.Write(silence, 0, silence.Length);
            //            }

            //            // 将音频数据写入输出文件
            //            byte[] buffer = new byte[reader.Length];
            //            int bytesRead;
            //            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            //            {
            //                waveFileWriter.Write(buffer, 0, bytesRead);
            //            }
            //        }

            //        last = srt.Subtitle.EndTime;
            //    }
            //}
        }

        private static async Task 合并音频和视频(VideoInfo video, string outputVideoFile, string audioFileV1)
        {
            var product = Path.Combine(video.ProjectPath, "product.mp4");

            // 将合成的音频文件与视频文件合并
            IConversion mergeAudioVideo = ffmpeg.Conversions.New()
                .AddParameter($"-i {outputVideoFile} -i {audioFileV1} -c:v copy -c:a aac -strict experimental {product}");

            Debug.WriteLine(mergeAudioVideo.Build());
            await mergeAudioVideo.Run();
        }

        ///// <summary>
        ///// 填加多个字幕
        ///// </summary>
        ///// <param name="subtitles"></param>
        ///// <param name="inputVideo"></param>
        ///// <param name="outputVideo"></param>
        ///// <returns></returns>
        //public static (bool,string) AddSubtitle(IEnumerable<AddVideoSubtitle> subtitles,string inputVideo,string outputVideo)
        //{
            
        //    foreach (var item in subtitles)
        //    {

        //    }
        //}

        public static (bool, string) 填加字幕(string inputVideoFile, string srtFile, string outputVideoFile, string audioFile, string enSrt)
        {
            var cnStyle = "Alignment=1,Fontsize=24,PrimaryColour=&HFFFFFF,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000";
            var enStyle = "Alignment=9,Fontsize=24,PrimaryColour=&HFFFF00,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000";
            var args = $"-i {inputVideoFile} -filter_complex \"[0:v]subtitles='{Fix(srtFile)}':force_style='{cnStyle}'[r1];[r1]subtitles='{Fix(enSrt)}':force_style='{enStyle}'[out];\" -map \"[out]\" -c:v libx264 -preset fast -crf 23 -c:a  {outputVideoFile} -y";

            //-/filter_complex 文件名.txt 来自文件的滤镜命令设置

            return RunFFmpeg(args, "填加字幕");
            //var conversion = FFmpeg.Conversions.New()
            //    //.AddParameter("-i", inputVideoFile)
            //    .AddParameter($"-filter_complex \"[0:v]subtitles={srtFile}:force_style={cnStyle},[v1];[v1]subtitles={enSrt}:force_style={enStyle}\"")
            //    .SetOverwriteOutput(true)
            //    .SetOutput(outputVideoFile);
            //await conversion.Start();

            //ffmpeg 


            ////FFmpegDownloader

            ////ffmpeg -i input.mp4 -vf "subtitles=chars1.srt:force_style='Alignment=1,Fontsize=24,PrimaryColour=&HFFFFFF,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000', subtitles=chars2.srt:force_style='Alignment=9,Fontsize=24,PrimaryColour=&HFFFF00,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000'" -c:a copy output.mp4


            ////videoFile = outputVideoFile;
            ////outputVideoFile = Path.Combine(video.ProjectPath, "video_subtitle.mp4");
            ////// 创建SRT文件
            ////string srtFile = video.VideoChineseSRT;//.Replace("\\", "\\\\").Replace(":", "\\:");

            //// 将字幕烧录到视频中
            ////IConversion burnSubtitles = ffmpeg.Conversions.New();
            ////.AddParameter($"-i {videoFile} -vf \"subtitles='{srtFile}'\" -c:a copy {outputVideoFile}");

            //IMediaInfo video = await ffmpeg.GetMediaInfo(inputVideoFile);
            ////IMediaInfo subtitleInfo = await ffmpeg.GetMediaInfo(srtFile);
            ////IVideoStream videoStream = video.VideoStreams.First();//.AddSubtitles(srtFile).AddSubtitles(enSrt);
            //var audio = await ffmpeg.GetMediaInfo(audioFile);

            //var burnSubtitles = ffmpeg.Conversions.New();

            ////    .AddStream(video.VideoStreams)
            ////    .AddStream(audio.AudioStreams)
            ////    .AddParameter($" -vf \"subtitles={srtFile}:force_style='Alignment=1,Fontsize=24,PrimaryColour=&HFFFFFF,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000', subtitles={enSrt}:force_style='Alignment=9,Fontsize=24,PrimaryColour=&HFFFF00,BackgroundColour=&H00000000,BorderColour=&HFFFFFF,OutlineColour=&H00000000'\"")
            ////    .SetOutput(outputVideoFile);
            ////.Run();
            ////ISubtitleStream subtitleStream = subtitleInfo.SubtitleStreams.First()
            ////.SetLanguage(language);
            //// burnSubtitles
            ////    .AddStream(mediaInfo.Streams)
            ////    .AddStream(subtitleStream)
            ////    .SetOutput(outputVideoFile);


            ////ffmpeg - i input_video.mp4 - vf "subtitles=VideoInfo2424.中文.srt" - c:a copy output_video.mp4
            ////var conversion = FFmpeg.Conversions.New()
            ////.AddParameter($"-i {inputVideoFile}")
            ////.SetOutput(outputVideoFile)
            ////.AddParameter($"-vf \"subtitles={Path.GetFileName(srtFile)}\"")
            ////.SetAudioCodec(AudioCodec.aac)
            ////.SetVideoCodec(VideoCodec.libx264);

            ////await conversion.Start();

            ////Debug.WriteLine(burnSubtitles.Build());

            //await burnSubtitles.Run();

        }

        private static async Task 将每个朗读音频插入到主音频(VideoInfo video, string inputAudioFile, string outputAudioFile)
        {
            #region 组织参数:初始化一个 filter_complex 字符串
            var filterComplex = string.Empty;

            // 用于记录音频流的索引
            int audioIndex = 1;

            foreach (var subtitle in video.CnAudioSolution.AudioItems.Where(t => t.State == TTSState.Generated))
            {
                // 为每个音频流创建一个唯一的标签
                string audioLabel = $"aud{audioIndex}";

                // 添加延迟到该音频流
                //filterComplex += $"[{audioIndex}:a]adelay={subtitle.Subtitle.StartTime.TotalMilliseconds}|{subtitle.Subtitle.StartTime.TotalMilliseconds}[{audioLabel}];";
                filterComplex += $"[{audioIndex}:a]adelay={subtitle.Subtitle.StartTime.TotalMilliseconds}|{subtitle.Subtitle.StartTime.TotalMilliseconds},volume=1.0[{audioLabel}];";

                // 准备下一个音频流的索引
                audioIndex++;
            }
            // 最后的音频混合部分，设置 duration 和 dropout_transition 参数
            //filterComplex += $"[{string.Join("][", Enumerable.Range(1, audioIndex - 1).Select(i => $"aud{i}"))}]amix=inputs={audioIndex - 1}:duration=first:dropout_transition=3[a]";
            // 最后的音频混合部分
            filterComplex += $"[{string.Join("][", Enumerable.Range(1, audioIndex - 1).Select(i => $"aud{i}"))}]amix=inputs={audioIndex - 1}[a]";
            #endregion

            // 创建转换实例
            IConversion conversion = ffmpeg.Conversions.New()
                .AddParameter($"-i \"{inputAudioFile}\"") // 主音频文件，确保路径被引用
                .SetOutput($"\"{outputAudioFile}\"") // 输出文件，确保路径被引用
                .SetOverwriteOutput(true);

            // 添加所有音频文件
            foreach (var subtitle in video.CnAudioSolution.AudioItems.Where(t => t.State == TTSState.Generated))
            {
                conversion.AddParameter($"-i \"{subtitle.OutputFileName}\""); // 确保路径被引用
            }

            // 应用 filter_complex 字符串
            conversion.AddParameter($"-filter_complex \"{filterComplex}\"");

            conversion.AddParameter("-map [a]");
            try
            {
                // 开始转换
                await conversion.Start();
            }
            catch (Exception ex)
            {
                throw ex;
                // 处理转换过程中的错误
                //Console.WriteLine($"转换失败: {ex.Message}");
            }
        }

        private static void CreateEmptyAudio(string tempAudioFile, TimeSpan duration)
        {
            var createSilentAudio = ffmpeg.Conversions.New()
                .SetOutput(tempAudioFile)
                .AddParameter($"-f lavfi -i anullsrc=r=44100:cl=mono -t {duration.TotalSeconds}");

            createSilentAudio.Run().Wait();
        }

        public static void GetAudio(string inputPath, string outputPath)
        {
            // 创建一个 WaveFileReader 对象
            using (WaveFileReader reader = new WaveFileReader(inputPath))
            {
                // 创建一个 WaveFileWriter 对象
                using (WaveFileWriter writer = new WaveFileWriter(outputPath, reader.WaveFormat))
                {
                    // 逐帧读取并写入音频数据
                    byte[] buffer = new byte[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels * 2];
                    int bytesRead;
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
    }

}
