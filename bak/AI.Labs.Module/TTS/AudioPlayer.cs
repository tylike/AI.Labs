using DevExpress.ExpressApp;
using NAudio.Wave;
using System.Diagnostics;

public class AudioPlayer
{
    public static void MP32WAV(string mp3File)
    {
        string wavFile = mp3File + ".wav";

        using (var reader = new Mp3FileReader(mp3File))
        {
            using (var writer = new WaveFileWriter(wavFile, reader.WaveFormat))
            {
                reader.CopyTo(writer);
            }
        }
    }


    public static void WindowsMediaPlayerPlay(string fp)
    {
        if (File.Exists(fp))
        {
            var pi = new ProcessStartInfo(fp);
            pi.UseShellExecute = true;
            Process.Start(pi);
            //TTSEngine.Play(fp, false);
        }
        else
        {
            throw new UserFriendlyException("文件不存在,请先进行生成!");
        }
    }

    public static void NAudioPlay(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The specified audio file could not be found.");
        }

        using (FileStream fs = File.OpenRead(fileName))
        {
            WaveStream waveStream = null;

            // 尝试读取不同格式的文件头来判断音频格式
            try
            {
                // 尝试WAV格式
                waveStream = new WaveFileReader(fs);
            }
            catch (Exception)
            {
                fs.Position = 0; // 重置流的位置
                try
                {
                    // 尝试MP3格式
                    waveStream = new Mp3FileReader(fs);
                }
                catch (Exception)
                {
                    fs.Position = 0; // 重置流的位置
                    try
                    {
                        // 尝试其他格式，例如AIFF
                        waveStream = new AiffFileReader(fs);
                    }
                    catch (Exception)
                    {
                        throw new InvalidOperationException("Unsupported audio format or corrupted file.");
                    }
                }
            }

            using (waveStream)
            {
                // 使用WaveOutEvent或其他输出设备播放音频
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(waveStream);
                    outputDevice.Play();
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }
    }

    public static void TrimAndMerge()
    {
        //Azure Text-to - Speech(TTS) 生成的音频在末尾有一段空白可能是为了确保播放不会突然中止，这样可以给出一个更自然的结束。目前Azure TTS API中没有直接的选项来去除这个空白。
        //不过，你可以使用后处理工具来修剪音频文件。NAudio是一个流行的.NET音频库，可以用来处理音频文件，包括去除静音部分。下面是一个使用NAudio去除音频末尾空白的基本示例代码：
        //using NAudio.Wave;
        //    using System;
        //    using System.Linq;
        //    public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        //    {
        //        using (WaveFileReader reader = new WaveFileReader(inPath))
        //        {
        //            using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
        //            {
        //                int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

        //                int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
        //                startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

        //                int endPos = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
        //                endPos = endPos - endPos % reader.WaveFormat.BlockAlign;
        //                TrimWavFile(reader, writer, startPos, reader.Length - endPos);
        //            }
        //        }
        //    }
        //    private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, long endPos)
        //    {
        //        reader.Position = startPos;
        //        byte[] buffer = new byte[1024];
        //        while (reader.Position < endPos)
        //        {
        //            int bytesRequired = (int)(endPos - reader.Position);
        //            if (bytesRequired > 0)
        //            {
        //                int bytesToRead = Math.Min(bytesRequired, buffer.Length);
        //                int bytesRead = reader.Read(buffer, 0, bytesToRead);
        //                if (bytesRead > 0)
        //                {
        //                    writer.Write(buffer, 0, bytesRead);
        //                }
        //            }
        //        }
        //    }
        //这段代码会从音频文件的开头和结尾处切掉指定的时间长度。你可以将 cutFromEnd 设置为你估计的空白时间长度，以此来去除末尾的空白。
        //合并多个音频文件可以使用多种工具和库完成，包括但不限于NAudio。以下是一个使用NAudio合并多个WAV文件的示例代码：
        //using NAudio.Wave;
        //    public static void Combine(string outputFile, params string[] inputFiles)
        //    {
        //        byte[] buffer = new byte[1024];
        //        WaveFileWriter waveFileWriter = null;
        //        try
        //        {
        //            foreach (string inputFile in inputFiles)
        //            {
        //                using (WaveFileReader reader = new WaveFileReader(inputFile))
        //                {
        //                    if (waveFileWriter == null)
        //                    {
        //                        // Create a new WaveFileWriter
        //                        waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
        //                    }
        //                    else
        //                    {
        //                        // Check if input file has the same format as the output file
        //                        if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
        //                        {
        //                            throw new InvalidOperationException("Can't concatenate WAV files that don't share the same format");
        //                        }
        //                    }

        //                    int read;
        //                    while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
        //                    {
        //                        waveFileWriter.Write(buffer, 0, read);
        //                    }
        //                }
        //            }
        //        }
        //        finally
        //        {
        //            if (waveFileWriter != null)
        //            {
        //                waveFileWriter.Dispose();
        //            }
        //        }
        //    }
        //    如果你需要合并MP3文件，你可能需要先将它们解码为WAV格式，进行合并，然后再次编码回MP3。
        //    这个过程可以使用NAudio的Mp3FileReader类和LameMP3FileWriter类来完成。
        //    需要注意的是，MP3文件合并可能会有比特率和采样率不一致的问题，你可能需要在合并之前将它们转换为相同的格式。
    }
}
