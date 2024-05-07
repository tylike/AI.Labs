using DevExpress.DashboardCommon.DataBinding;
using DevExpress.ExpressApp;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

#warning azure生成的音频,结尾似乎有1段空白等待,如多段合成时可以去掉最好。
#warning 有声书籍多段音频生成后,可以合成一段最好

//SSML(Speech Synthesis Markup Language) 是一个基于 XML 的标记语言，用于控制文本到语音 (TTS) 引擎的输出。
//SSML 允许你指定各种语音合成参数，如发音、音量、语速、音高等。以下是一些常用的 SSML 标签和属性：
//<speak>：这是 SSML 文档的根元素，所有其他元素都被包含在其中。
//<voice>：用于指定发音的声音。可以通过 name 属性指定特定的发音者（例如，特定的男声或女声）。
//<voice name="en-US-Jessa24kRUS">Hello, how are you?</voice>
//<prosody>：用来调整音高（pitch）、语速（rate）、音量（volume）等。
//<prosody pitch="+10%" rate="fast" volume="loud">I am speaking loudly and quickly.</prosody>
//<break>：在语音中插入暂停。可以通过 time 属性指定暂停的时间长度，或使用 strength 属性指定暂停的强度。
//<break time="500ms"/> <!-- 暂停 500 毫秒 -->
//<break strength="medium"/> <!-- 中等强度的暂停 -->
//<emphasis>：强调某个词或短语。
//<emphasis level="strong">This is really important.</emphasis>
//<sub>：指定一个词的替代发音。
//<sub alias="World Wide Web Consortium">W3C</sub>
//<say-as>：将文本解释为不同的格式，如日期、时间、电话号码等。
//<say-as interpret-as="telephone">1234567890</say-as>
//<lang>：指定一段文本的语言。
//<lang xml:lang = "es-ES" > Hola, ¿cómo estás?</lang> <!-- 西班牙语 -->
//<phoneme>：用于精确控制单词或短语的发音。这对于非标准单词或名称非常有用。
//<phoneme alphabet="ipa" ph="tɪˈkeɪ">ticket</phoneme>
//<audio>：允许在语音中插入预先录制的音频片段。
//<audio src="http://www.example.com/sound.wav">A fallback text</audio>
//<p> 和 <s>：分别代表段落和句子，有助于提高合成语音的自然性。
//<p>This is the first paragraph. <s>This is the first sentence.</s> <s>This is the second sentence.</s></p>
//除了这些常见选项，SSML 还有更多高级功能，如调整语言模型、处理特殊字符或使用自定义词典等。
//不同的TTS引擎可能支持SSML的不同子集，所以你应该查阅你使用的TTS服务的文档来了解哪些特性是支持的。
//例如，Azure TTS 服务提供了一系列声音和语言选项，并且支持 SSML 中的多种功能。
//具体可用的选项和声音可能会随时间更新和增加，所以你应该参考 Azure TTS 的官方文档获取最新的信息。

//<speak version="1.0" xmlns="http://www.w3.org/2001/10/synthesis" xml:lang="zh-CN">
//  <voice name="zh-CN-XiaomoNeural">
//    <mstts:express-as xmlns:mstts="https://www.w3.org/2001/mstts" style="cheerful" styledegree="1.5" role="YoungAdultFemale">
//      这是一个使用风格和角色的示例。
//    </mstts:express-as>
//  </voice>
//</speak>

//一般来说，角色的取值可以是 Child、YoungAdult、Adult 或 Senior，
//后面可以跟 Male 或 Female。例如，role=“YoungAdultFemale”。2

public class AzureTTSEngine
{
    // This example requires environment variables named "SPEECH_KEY" and "SPEECH_REGION"
    //static string speechKey = Environment.GetEnvironmentVariable("SPEECH_KEY");
    //static string speechRegion = Environment.GetEnvironmentVariable("SPEECH_REGION");

    static  string OutputSpeechSynthesisResult(SpeechSynthesisResult speechSynthesisResult, string text)
    {
        switch (speechSynthesisResult.Reason)
        {
            case ResultReason.SynthesizingAudioCompleted:
                return null;
            case ResultReason.Canceled:
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(speechSynthesisResult);
                var rst = new StringBuilder($"CANCELED: Reason={cancellation.Reason}");

                if (cancellation.Reason == CancellationReason.Error)
                {
                    rst.AppendLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                    rst.AppendLine($"CANCELED: ErrorDetails=[{cancellation.ErrorDetails}]");
                    rst.AppendLine($"CANCELED: Did you set the speech resource key and region values?");
                }
                return rst.ToString();
            
            default:
                return speechSynthesisResult.Reason.ToString();
        }
    }
    public async static Task<SynthesisVoicesResult> GetVoices(string key, string region)
    {
        var speechConfig = SpeechConfig.FromSubscription(key, region);        
        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            return await speechSynthesizer.GetVoicesAsync();
            //var rst = voices.Voices.Where(t => t.Locale.ToLower().StartsWith("zh-cn")).ToList();
            //var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
        }
    }

    public async static Task PlasySSML(string text, string key, string region, string saveToFile = null)
    {
        var speechConfig = SpeechConfig.FromSubscription(key, region);
        // The language of the voice that speaks.
        if (!string.IsNullOrEmpty(saveToFile))
        {
            speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);
        }

        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            var speechSynthesisResult = await speechSynthesizer.SpeakSsmlAsync(text);
            var error = OutputSpeechSynthesisResult(speechSynthesisResult, text);
            //if(speechSynthesisResult.Reason == ResultReason.)
            if (error == null)
            {
                if (!string.IsNullOrEmpty(saveToFile))
                {
                    File.WriteAllBytes(saveToFile, speechSynthesisResult.AudioData);
                }
            }
            else
            {
                throw new UserFriendlyException(error);
            }
        }
    }

    public async static Task Play(string text, string voiceName, string key, string region, List<byte> resultAudioData = null)
    {
        var speechConfig = SpeechConfig.FromSubscription(key, region);

        // The language of the voice that speaks.
        speechConfig.SpeechSynthesisVoiceName = voiceName;
        if (resultAudioData != null)
        {
            speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Audio24Khz160KBitRateMonoMp3);            
        }
        using (var speechSynthesizer = new SpeechSynthesizer(speechConfig))
        {
            var speechSynthesisResult = await speechSynthesizer.SpeakTextAsync(text);
            
            if (resultAudioData != null)
            {
                
                resultAudioData.AddRange(speechSynthesisResult.AudioData);
            }
        }
        
        //bool shouldPlayAudio = true; // 根据需要设置条件
        //if (shouldPlayAudio)
        //{
        //    // 如需播放音频，你可以使用SoundPlayer类（需要添加System.Media引用）
        //    using var audioStream = AudioDataStream.FromResult(result);
        //    using var player = new System.Media.SoundPlayer(audioStream.AsStream());
        //    player.PlaySync(); // 同步播放
        //}

        //// The default output audio format is 16K 16bit mono
        //var audioClip = AudioClip.Create("SynthesizedAudio", sampleCount, 1, 16000, false);
        //audioClip.SetData(audioData, 0);
        //audioSource.clip = audioClip;
        //audioSource.Play();
    }
}
