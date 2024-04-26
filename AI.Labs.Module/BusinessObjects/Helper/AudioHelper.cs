using NAudio.Wave;
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
            FFmpeg.SetExecutablesPath(Path.GetDirectoryName(FFmpegHelper.ffmpegFile));
        }

        public static async Task<IMediaInfo> GetAudioFileInfo(string filePath)
        {
            return await FFmpeg.GetMediaInfo(filePath);            
        }

        #region 移除静音
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

        #endregion
    }
}
