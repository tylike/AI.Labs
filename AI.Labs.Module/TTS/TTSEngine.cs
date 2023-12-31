﻿using DevExpress.ExpressApp.Xpo;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Speech.Synthesis;
using AI.Labs.Module.BusinessObjects.TTS;
using AI.Labs.Module.BusinessObjects.ChatInfo;
using EdgeTTSSharp;

public enum TTSState
{
    未生成,
    生成中,
    已生成
}
public static class TTSEngine
{
    static TTSEngine()
    {
        var current = Assembly.GetExecutingAssembly().Location;
        var currentDir = Path.GetDirectoryName(current);
        var tempDir = Path.Combine(currentDir, "temp");
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }

        var cacheDir = Path.Combine(currentDir, "cache");
        if (!Directory.Exists(cacheDir))
        {
            Directory.CreateDirectory(cacheDir);
        }
        // 创建SpeechSynthesizer实例
        // 设置朗读的语音
        synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);

        //var ms = new MemoryStream();
        //synthesizer.SetOutputToWaveStream(ms);
        synthesizer.SetOutputToDefaultAudioDevice();
    }
    static SpeechSynthesizer synthesizer = new SpeechSynthesizer();

    public static async Task ReadText(string text, string voice = null, bool useSAPI = false)
    {
        if (useSAPI)
        {
            synthesizer.SpeakAsync(text);
            //sapi中没有支持音色人声
        }
        else
        {
            // See https://aka.ms/new-console-template for more information

            //string msg = string.Empty;
            //Console.WriteLine("请输入文本内容.");
            //msg = Console.ReadLine();
            // 获取xiaoxiao语音包
            if (voice == null)
            {
                voice = "zh-CN-XiaoxiaoNeural";
            }
            // 文字转语音，并且设置语速
            EdgeTTS.PlayText(text, voice);
        }
        await Task.CompletedTask;
    }

    static string GetTempFile(string extFileName = ".mp3")
    {
        var current = Assembly.GetExecutingAssembly().Location;
        var dir = Path.GetDirectoryName(current);
        var tempDir = Path.Combine(dir, "temp");
        var tempFile = Path.Combine(tempDir, Guid.NewGuid().ToString() + extFileName);
        if (!Directory.Exists(tempDir))
        {
            Directory.CreateDirectory(tempDir);
        }
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
        return tempFile;
    }

    public static string GetCacheFile(string fileName)
    {
        var current = Assembly.GetExecutingAssembly().Location;
        var dir = Path.GetDirectoryName(current);
        var cacheDir = Path.Combine(dir, "cache");
        var cacheFile = Path.Combine(cacheDir, fileName);
        return cacheFile;
        //return File.Exists(cacheFile);
        //缓存文件保存到exe\cache\datetime_fileName.ext fileName = 2023_10_23_11_23_00_000_AudioFile001.mp3 格式
    }

    public static bool CacheFileIsExist(string fileName)
    {
        var fn = GetCacheFile(fileName);
        return File.Exists(fn);
    }

    public static void WriteCacheFile(string fileName, byte[] data)
    {
        var fn = GetCacheFile(fileName);
        File.WriteAllBytes(fn, data);
    }

    public static byte[] ReadCacheFile(string fileName)
    {
        var fn = GetCacheFile(fileName);
        return File.ReadAllBytes(fn);
    }

    public static void Play(string fileName,bool isWav = false)
    {
        Play(File.ReadAllBytes(fileName), isWav);
    }

    public static void Play(byte[] fileContent, bool isWav = false)
    {
        IWaveProvider audioFile = null;
        if (isWav)
        {
            audioFile = new WaveFileReader(new MemoryStream(fileContent));
        }
        else
        {
            audioFile = new Mp3FileReader(new MemoryStream(fileContent));
        }
        var idisposeable = audioFile as IDisposable;
        using (idisposeable)
        {
            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.Play();
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(10);
                }
            }
        }
    }

    /// <summary>
    /// 输入文字、声音角色名称，返回音频文件(mp3)的二进制数据
    /// </summary>
    /// <param name="text"></param>
    /// <param name="defaultVoice"></param>
    /// <returns></returns>
    public static byte[] GetTextToSpeechData(
        this string text,
        string defaultVoice = "zh-CN-XiaoyiNeural")
    {
        var tempFile = TextToSpeech(text, defaultVoice);
        try
        {
            return File.ReadAllBytes(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }



    public static byte[] GetWaveWithSystem(string text)
    {

        // 创建SpeechSynthesizer实例
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        // 设置朗读的语音
        synthesizer.SelectVoiceByHints(VoiceGender.Female, VoiceAge.Adult);
        var ms = new MemoryStream();
        synthesizer.SetOutputToWaveStream(ms);
        // 朗读文本
        synthesizer.Speak(text);
        ms.Flush();
        // 关闭SpeechSynthesizer实例
        synthesizer.Dispose();
        return ms.ToArray();
    }

    /// <summary>
    /// 输入文字、声音角色名称，返回音频文件(mp3)的二进制数据
    /// </summary>
    /// <param name="text"></param>
    /// <param name="cacheFileName"></param>
    /// <param name="defaultVoice"></param>
    public static void PlayTTSWithCache(
        this string text,
        [Required] string cacheFileName,
        string defaultVoice = "zh-CN-XiaoyiNeural"
        )
    {
        if (!CacheFileIsExist(cacheFileName))
        {
            var cfn = GetCacheFile(cacheFileName);
            TextToSpeech(text, defaultVoice, outputFile: cfn);
        }

        byte[] data = null;
        data = ReadCacheFile(cacheFileName);
        Play(data);
        return;
    }

    /// <summary>
    /// 内部使用: 输入文字、声音角色名称，朗读该Text,返回音频文件(mp3)的临时文件路径
    /// </summary>
    /// <param name="text"></param>
    /// <param name="defaultVoice"></param>
    /// <param name="outputFile">如果不指定，则保存临时目录</param>
    /// <returns></returns>
    public static string TextToSpeech(
        this string text,
        string defaultVoice = "zh-CN-XiaoyiNeural",
        string outputFile = null
        )
    {
        if (string.IsNullOrEmpty(defaultVoice))
        {
            defaultVoice = "zh-CN-XiaoyiNeural";
        }
        if (string.IsNullOrEmpty(outputFile))
        {
            outputFile = GetTempFile();
        }

        EdgeTTS.PlayText(text, outputFile);

        return outputFile;
    }

}
