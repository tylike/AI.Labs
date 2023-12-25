using OpenAI.Managers;
using OpenAI;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using NAudio.Wave;
using System.Diagnostics;
using System.Runtime.Versioning;
using OpenAI.ObjectModels.ResponseModels;

namespace AI.Labs.Module.BusinessObjects
{

    [SupportedOSPlatform("windows")]
    public static class AIHelper
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
        public static string LocalGetTextFromAudio(string fileName)
        {//-p 8 -t 16
            var cmd = @"D:\ai.stt\main.exe";
            var arg = $@"-l zh  -m D:\ai.stt\ggml-large-v1.bin  -otxt -nt -f {fileName}";
            var psi = new ProcessStartInfo(cmd,arg);
            psi.WorkingDirectory = "d:\\ai.stt\\";
            //psi.RedirectStandardError = true;
            //psi.RedirectStandardOutput = true;
            //psi.UseShellExecute = false;
            var p = new Process();
            p.StartInfo = psi;
            p.EnableRaisingEvents = true;
            p.ErrorDataReceived += (s, e) =>
            {

            };
            p.OutputDataReceived += (s, e) => 
            {
                
            };
            p.Exited += (s, e) => 
            {

            };
            
            p.Start();

            p.WaitForExit();
            
            Thread.Sleep(2000);
            //var error = p.StandardError.ReadToEnd();
            //var rst = p.StandardOutput.ReadToEnd();
            //if (!string.IsNullOrEmpty(error))
            //{
            //    Debug.WriteLine(error);
            //}
            //if (!string.IsNullOrEmpty(rst))
            //{
            //    Debug.WriteLine(rst);
            //}

            var txt = fileName[..^4] + ".txt";
            return File.ReadAllText(txt);
            //var speechEngine = new SpeechRecognitionEngine();
            //speechEngine.SetInputToWaveFile(fileName);
            //speechEngine.LoadGrammar(new DictationGrammar());
            //speechEngine.SpeechRecognized += (s, a) =>
            //{
            //    Debug.WriteLine(a.Result.Text);
            //};
            //var result = speechEngine.Recognize();
            //return result.Text;
        }
        public async static Task<(string Text, bool IsError)> GetTextFromAudio(string fileName)
        {
            //transcript = openai.Audio.translate(model = "whisper-1", file = "openai.mp3", response_format = "text")
            var sdk = new OpenAIService(new OpenAiOptions()
            {
                BaseDomain = "https://api.openai-proxy.org",
                ApiKey =
                 //"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"
                 "sk-7A5enIMIVH4PtxML4TL0M6Khi1ty8INWpQfvR6gykqgfCY6z"
            });

            var sampleFile = await File.ReadAllBytesAsync($"{fileName}");
            var audioResult = await sdk.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
            {
                FileName = fileName,
                File = sampleFile,
                Model = Models.WhisperV1,
                ResponseFormat = StaticValues.AudioStatics.ResponseFormat.Text
            });
            if (audioResult.Successful)
            {
                return (Text: string.Join("\n", audioResult.Text), IsError: false);
            }
            else
            {
                var error = "";
                if (audioResult.Error == null)
                {
                    error = "未知错误";
                }
                else
                {
                    error = string.Join("\n", audioResult.Error.Messages);
                }
                return (Text: error, true);
            }
        }
        public static OpenAIService CreateOpenAIService(string apiKey = "sk-7A5enIMIVH4PtxML4TL0M6Khi1ty8INWpQfvR6gykqgfCY6z", 
            string baseDomain = "http://127.0.0.1:8000"
            //string baseDomain = "https://api.openai-proxy.org"
            )
        {
            return new OpenAIService(
                new OpenAiOptions()
                {
                    BaseDomain = baseDomain,
                    ApiKey = apiKey
                    //"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"
                    //"sk-7A5enIMIVH4PtxML4TL0M6Khi1ty8INWpQfvR6gykqgfCY6z"
                });
        }


        public async static Task<(string Message, bool IsError)> Ask(string systemPrompt, string userMessage, AIModel aiModel)
        {
            var openAiService = CreateOpenAIService(baseDomain: aiModel.ApiUrlBase, apiKey: aiModel.ApiKey);
            // ChatGPT Official API
            var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(systemPrompt),
                    ChatMessage.FromUser(userMessage)
                },
                Model = aiModel.Name,
            });

            return completionResult.GetAIResponse();
        }



        public async static Task<(string Message, bool IsError)> Ask(string systemPrompt,string userMessage, string url = null)
        {
            var openAiService = CreateOpenAIService(baseDomain: url);
            // ChatGPT Official API
            var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>
                {
                    ChatMessage.FromSystem(systemPrompt),
                    ChatMessage.FromUser(userMessage)
                },
                Model = Models.Gpt_3_5_Turbo,
            });

            return completionResult.GetAIResponse();
        }
        public static (string Message, bool IsError) GetAIResponse(this ChatCompletionCreateResponse completionResult)
        {
            if (completionResult.Successful)
            {
                return (completionResult.Choices.First().Message.Content, false);
            }
            else
            {
                return (string.Join("\n", completionResult.Error.Messages), true);
            }
        }
    }
}
