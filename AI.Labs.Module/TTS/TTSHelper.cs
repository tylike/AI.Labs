using AI.Labs.Module.BusinessObjects.TTS;
using EdgeTTSSharp;
using System.Diagnostics;

public static class TTSHelper
{
    public static async Task<byte[]> Text2AudioData(string text,string voiceName,VoiceEngine engine,
        string apiKey = null,
        string baseUrl = null)
    {
        var rst = new List<byte>();
        if (engine == VoiceEngine.EdgeTTS)
        {
            try
            {
                await EdgeTTS.PlayText(text, voiceName, play: false, resultBytes: rst);
                Debug.WriteLine("EdgeTTS.生成音频:调用完成!");
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        else
        {
            await AzureTTSEngine.Play(text, voiceName, apiKey, baseUrl, rst);
            Debug.WriteLine("AzureTTS.生成音频:调用完成!");
        }
        return rst.ToArray();
    }

    public static async Task SaveAudioDataToFile(this byte[] data, string fileName)
    {
        await File.WriteAllBytesAsync(fileName, data);
    }
}