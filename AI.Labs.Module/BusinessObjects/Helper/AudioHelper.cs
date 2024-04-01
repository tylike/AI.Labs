using NAudio.Wave;
using System.Diagnostics;
using System.Text.Json;
using SoundTouch.Net.NAudioSupport;
using NAudio.Wave.SampleProviders;
using Xabe.FFmpeg;
using DevExpress.XtraRichEdit.Layout.Engine;

namespace AI.Labs.Module.BusinessObjects
{
    //public class AudioFileInfo
    //{
    //    public string Format { get; set; }
    //    public string Codec { get; set; }
    //}
    public class AudioHelper
    {
        static AudioHelper()
        {
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(ffmpegFile));
        }

        public static (bool 已调整, int 调整后时长, string 输出文件) FixAudio(string wavFileName, int subtitleDurationMs, int? audioDuration = null)
        {
            // 获取WAV文件的时长
            int wavDuration = audioDuration ?? GetAudioDuration(wavFileName);

            // 计算音频是否需要加速
            if (wavDuration - subtitleDurationMs > 0)
            {
                // 计算加速的速率
                float speed = (float)wavDuration / (float)subtitleDurationMs;
                speed = Math.Min(speed, 1.3f);
                int adjustedTargetDurationMs = (int)(wavDuration / speed);

                // 如果加速速率小于或等于1.3，则进行加速处理
                // 加速音频并保存到新文件

                string outputFileName = Path.Combine(Path.GetDirectoryName(wavFileName), Path.GetFileNameWithoutExtension(wavFileName) + "_fixed.wav");
                AdjustPlaybackSpeed(wavFileName, outputFileName, speed);
                return (true, adjustedTargetDurationMs, outputFileName); // 加速处理成功                
            }
            return (false, wavDuration, null); // 无需加速处理或加速比率超出范围
        }

        // 获取音频文件的时长（毫秒）
        private static int GetAudioDuration(string fileName)
        {
            using (var reader = new AudioFileReader(fileName))
            {
                return (int)reader.TotalTime.TotalMilliseconds;
            }
        }

        // 加速播放音频（保持原音调）
        private static void AdjustPlaybackSpeed(string inputFilePath, string outputFilePath, float newSpeed)
        {
            // 构建 ffmpeg 命令
            string ffmpegCommand = $" -i \"{inputFilePath}\" -filter:a \"atempo={newSpeed}\" -vn \"{outputFilePath}\"";

            // 执行 ffmpeg 命令
            ExecuteFfmpegCommand(ffmpegCommand);
        }

        private static void ExecuteFfmpegCommand(string command)
        {
            // 这里添加执行 ffmpeg 命令的代码
            // 例如，使用 System.Diagnostics.Process.Start()
            var pi = new ProcessStartInfo();
            pi.FileName = $@"D:\ffmpeg.gui\ffmpeg\bin\ffmpeg.exe";
            pi.Arguments = command;
            pi.UseShellExecute = true;
            var inf = Process.Start(pi);
            inf.WaitForExit();
            Debug.WriteLine($"{pi.FileName} {pi.Arguments}");
        }


        const string ffprobe = @"D:\ffmpeg.gui\ffprobe\bin\ffprobe.exe";
        public const string ffmpegFile = @"D:\ffmpeg.gui\ffmpeg\bin\ffmpeg.exe";
        public static async Task<IMediaInfo> GetAudioFileInfo(string filePath)
        {
            return await FFmpeg.GetMediaInfo(filePath);
            //ProcessStartInfo startInfo = new ProcessStartInfo
            //{
            //    FileName = ffprobe,
            //    Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
            //    UseShellExecute = false,
            //    RedirectStandardOutput = true,
            //    CreateNoWindow = true
            //};

            //using Process process = Process.Start(startInfo);
            //using StreamReader reader = process.StandardOutput;
            //string result = reader.ReadToEnd();
            //var jsonDocument = JsonDocument.Parse(result);
            //var format = jsonDocument.RootElement.GetProperty("format").GetProperty("format_name").GetString();
            //var codec = jsonDocument.RootElement.GetProperty("streams")[0].GetProperty("codec_name").GetString();
            //return new AudioFileInfo { Format = format, Codec = codec };
        }

        // 移除MP3文件开头和结尾的静音部分
        public static int RemoveSilenceFromMp3(string inputFilePath, string outputFilePath, float silenceDbThreshold = -40f)
        {
            // 将分贝阈值转换为振幅阈值
            float silenceThreshold = (float)Math.Pow(10, silenceDbThreshold / 20);

            using var reader = new Mp3FileReader(inputFilePath);
            var sampleProvider = reader.ToSampleProvider();
            var allSamples = ReadAllSamples(sampleProvider);

            // 检测静音部分
            var silentSections = FindSilentSections(allSamples, silenceThreshold);

            // 提取非静音部分并保存为WAV文件
            return SaveNonSilentSections(allSamples, silentSections, sampleProvider.WaveFormat, outputFilePath);
        }

        // 读取所有样本
        private static List<float> ReadAllSamples(ISampleProvider sampleProvider)
        {
            var allSamples = new List<float>();
            var buffer = new float[sampleProvider.WaveFormat.SampleRate * sampleProvider.WaveFormat.Channels];
            int samplesRead;
            while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                allSamples.AddRange(buffer.Take(samplesRead));
            }
            return allSamples;
        }

        // 查找静音部分
        private static List<Tuple<int, int>> FindSilentSections(List<float> samples, float threshold)
        {
            var silentSections = new List<Tuple<int, int>>();
            int startOfSilence = -1;

            for (int i = 0; i < samples.Count; i++)
            {
                bool isSilent = Math.Abs(samples[i]) < threshold;
                //值是静音，还没有开始记录静音
                if (isSilent && startOfSilence == -1)
                {
                    // 静音开始
                    startOfSilence = i;
                }
                //不是静音，但是之前有静音
                else if (!isSilent && startOfSilence != -1)
                {
                    // 静音结束
                    silentSections.Add(Tuple.Create(startOfSilence, i));
                    startOfSilence = -1;
                }
            }
            // 检查文件结尾是否为静音
            if (startOfSilence != -1)
            {
                silentSections.Add(Tuple.Create(startOfSilence, samples.Count));
            }
            return silentSections;
        }

        // 保存非静音部分为WAV文件
        private static int SaveNonSilentSections(List<float> samples, List<Tuple<int, int>> silentSections, WaveFormat waveFormat, string outputPath)
        {
            using (var writer = new WaveFileWriter(outputPath, waveFormat))
            {
                int startOfAudio = 0;
                int endOfAudio = samples.Count;
                var first = silentSections.FirstOrDefault();
                if (first != null && first.Item1 == 0)
                {
                    startOfAudio = first.Item2;
                }
                var last = silentSections.LastOrDefault();
                // 确保最后一个静音段是在文件末尾
                if (last != null && last.Item2 >= samples.Count)
                {
                    endOfAudio = last.Item1;
                }
                // 确保有非静音部分要写入
                if (startOfAudio < endOfAudio)
                {
                    writer.WriteSamples(samples.ToArray(), startOfAudio, endOfAudio - startOfAudio);
                }
                return (int)writer.TotalTime.TotalMilliseconds;
            }
        }


        //public async static MediaInfo GetAudioFileInfo(string filePath)
        //{
        //    // 获取媒体信息
        //    var mediaInfo = await FFmpeg.GetMediaInfo(filePath);

        //    // 输出文件格式
        //    Console.WriteLine("文件格式: " + mediaInfo.Format);

        //    // 输出音频流信息
        //    foreach (var audioStream in mediaInfo.AudioStreams)
        //    {
        //        Console.WriteLine("音频编码: " + audioStream.Codec);
        //    }

        //    // 输出视频流信息
        //    foreach (var videoStream in mediaInfo.VideoStreams)
        //    {
        //        Console.WriteLine("视频编码: " + videoStream.Codec);
        //    }
        //}

        //public static (string, int) GetMp3Duration(string mp3FilePath)
        //{
        //    var fn = mp3FilePath + ".wav";
        //    var len = RemoveSilenceFromMp3(mp3FilePath, fn);
        //    return (fn, len);
        //    // 去除静音部分（可选，根据需要调用）

        //    //try
        //    //{
        //    //    using (var reader = new Mp3FileReader(mp3FilePath))
        //    //    {
        //    //        return (int)reader.TotalTime.TotalMilliseconds;
        //    //    }
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    // Handle the exception if the file could not be read
        //    //    Console.WriteLine("An error occurred while reading the MP3 file: " + ex.Message);
        //    //    return -1; // Return -1 or an appropriate error code
        //    //}
        //}
    }
}
