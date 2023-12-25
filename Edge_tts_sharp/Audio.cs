using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NAudio;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace EdgeTTSSharp
{
    public class Audio
    {
        /// <summary>
        /// 播放流媒体
        /// </summary>
        /// <param name="source"></param>
        /// <param name="volume">音量大小，0-1的浮点型数值</param>
        public static void PlayToByte(byte[] source, float volume = 1.0f)
        {
            using (var ms = new MemoryStream(source))
                using (var sr = new StreamMediaFoundationReader(ms))
                    using (var waveOut = new WaveOutEvent())
                    {
                        waveOut.Init(sr);
                        // 0 - 1
                        waveOut.Volume = volume;
                        waveOut.Play();
                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(1000);
                        }
                    }
        }
        /// <summary>
        /// 播放指定路径的音频
        /// </summary>
        /// <param name="AudioPath"></param>
        /// <param name="volume">音量大小，0-1的浮点型数值</param>
        public static void PlayAudio(string AudioPath, float volume = 1.0f)
        {
            using (var audioFile = new AudioFileReader(AudioPath))
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                // 0 - 1
                outputDevice.Volume = volume;
                outputDevice.Play(); // 异步执行

                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}
