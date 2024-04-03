﻿using OpenAI.Managers;
using OpenAI;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using NAudio.Wave;
using System.Diagnostics;
using System.Runtime.Versioning;
using OpenAI.ObjectModels.ResponseModels;
using DevExpress.ExpressApp;
using AI.Labs.Module.Translate;
using DevExpress.ExpressApp.Model;

namespace AI.Labs.Module.BusinessObjects
{
    [SupportedOSPlatform("windows")]
    public static class AIHelper
    {

        //public static string LocalGetTextFromAudio(string fileName)
        //{//-p 8 -t 16
        //    var cmd = @"D:\ai.stt\main.exe";
        //    var arg = $@"-l zh  -m D:\ai.stt\ggml-large-v1.bin  -otxt -nt -f {fileName}";
        //    var psi = new ProcessStartInfo(cmd, arg);
        //    psi.WorkingDirectory = "d:\\ai.stt\\";
        //    //psi.RedirectStandardError = true;
        //    //psi.RedirectStandardOutput = true;
        //    //psi.UseShellExecute = false;
        //    var p = new Process();
        //    p.StartInfo = psi;
        //    p.EnableRaisingEvents = true;
        //    p.ErrorDataReceived += (s, e) =>
        //    {

        //    };
        //    p.OutputDataReceived += (s, e) =>
        //    {

        //    };
        //    p.Exited += (s, e) =>
        //    {

        //    };

        //    p.Start();

        //    p.WaitForExit();

        //    Thread.Sleep(2000);
        //    //var error = p.StandardError.ReadToEnd();
        //    //var rst = p.StandardOutput.ReadToEnd();
        //    //if (!string.IsNullOrEmpty(error))
        //    //{
        //    //    Debug.WriteLine(error);
        //    //}
        //    //if (!string.IsNullOrEmpty(rst))
        //    //{
        //    //    Debug.WriteLine(rst);
        //    //}

        //    var txt = fileName[..^4] + ".txt";
        //    return File.ReadAllText(txt);
        //    //var speechEngine = new SpeechRecognitionEngine();
        //    //speechEngine.SetInputToWaveFile(fileName);
        //    //speechEngine.LoadGrammar(new DictationGrammar());
        //    //speechEngine.SpeechRecognized += (s, a) =>
        //    //{
        //    //    Debug.WriteLine(a.Result.Text);
        //    //};
        //    //var result = speechEngine.Recognize();
        //    //return result.Text;
        //}
        //public async static Task<(string Text, bool IsError)> GetTextFromAudio(string fileName)
        //{
        //    //transcript = openai.Audio.translate(model = "whisper-1", file = "openai.mp3", response_format = "text")
        //    var sdk = new OpenAIService(new OpenAiOptions()
        //    {
        //        BaseDomain = "https://api.openai-proxy.org",
        //        ApiKey =
        //         //"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"
        //         "sk-7A5enIMIVH4PtxML4TL0M6Khi1ty8INWpQfvR6gykqgfCY6z"
        //    });

        //    var sampleFile = await File.ReadAllBytesAsync($"{fileName}");
        //    var audioResult = await sdk.Audio.CreateTranscription(new AudioCreateTranscriptionRequest
        //    {
        //        FileName = fileName,
        //        File = sampleFile,
        //        Model = Models.WhisperV1,
        //        ResponseFormat = StaticValues.AudioStatics.ResponseFormat.Text
        //    });
        //    if (audioResult.Successful)
        //    {
        //        return (Text: string.Join("\n", audioResult.Text), IsError: false);
        //    }
        //    else
        //    {
        //        var error = "";
        //        if (audioResult.Error == null)
        //        {
        //            error = "未知错误";
        //        }
        //        else
        //        {
        //            error = string.Join("\n", audioResult.Error.Messages);
        //        }
        //        return (Text: error, true);
        //    }
        //}
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

        //[Obsolete]
        //public static async Task Ask(this XafApplication xaf, string systemPrompt, string userMessage, AIModel aiModel, Action<ChatMessage> processResult,  bool streamOut = true, int streamOutCount = 1, bool refreshUI = true)
        //{
        //    var sw = Stopwatch.StartNew();
            
        //        var t = Ask(systemPrompt, userMessage,, aiModel);
        //        var i = 0;
        //        await foreach (var item in t)
        //        {
        //            if (!string.IsNullOrEmpty(item))
        //            {
        //                streamOutProcessResult(item);
        //                i++;
        //                if (refreshUI && i % streamOutCount == 0)
        //                {
        //                    xaf.UIThreadDoEvents();
        //                }
        //            }
        //            Debug.WriteLine(item);
        //        }
            


        //    if (refreshUI)
        //        xaf.UIThreadDoEvents();
        //    sw.Stop();
        //    Debug.WriteLine($"生成完成!用时:{sw.Elapsed}");
        //    xaf.ShowViewStrategy.ShowMessage($"生成完成!用时:{sw.Elapsed}");
        //}
        
        //[Obsolete]
        //public async static Task<(string Message, bool IsError)> Ask(string systemPrompt, string userMessage, AIModel aiModel)
        //{
        //    var openAiService = CreateOpenAIService(baseDomain: aiModel.ApiUrlBase, apiKey: aiModel.ApiKey);
        //    // ChatGPT Official API
        //    var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
        //    {
        //        Messages = new List<ChatMessage>
        //        {
        //            ChatMessage.FromSystem(systemPrompt),
        //            ChatMessage.FromUser(userMessage)
        //        },
        //        Model = aiModel.Name,
        //    });
        //    return completionResult.GetAIResponse();
        //}

        public async static Task Ask(PredefinedRole role, string userMessage, AIModel aiModel, Action<ChatMessage> processResult, bool streamOut = true)
        {
            var openAiService = CreateOpenAIService(baseDomain: aiModel.ApiUrlBase, apiKey: aiModel.ApiKey);

            #region 组织request
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>(),
                Model = aiModel.Name,
            };

            foreach (var item in role.Prompts)
            {
                var msg = item.Message;
                request.Messages.Add(new ChatMessage(item.ChatRole.ToString(), msg));
            }
            if (!string.IsNullOrEmpty(userMessage))
                request.Messages.Add(ChatMessage.FromUser(userMessage));
            #endregion

            await Ask(processResult, streamOut, openAiService, request);
        }
        

        public async static Task Ask(string systemPrompt, string userMessage, Action<ChatMessage> processResult,AIModel aiModel, bool streamOut = true)
        {
            await Ask(systemPrompt, userMessage, processResult, streamOut, aiModel.ApiUrlBase, aiModel.ApiKey, aiModel.Name);
        }

        /// <summary>
        /// 新版本，别的考虑作废
        /// </summary>
        /// <param name="systemPrompt"></param>
        /// <param name="userMessage"></param>
        /// <param name="processResult"></param>
        /// <param name="streamOut"></param>
        /// <param name="url"></param>
        /// <param name="api_key"></param>
        /// <param name="modelName"></param>
        /// <returns></returns>
        public async static Task Ask(string systemPrompt, string userMessage, Action<ChatMessage> processResult, bool streamOut = true, string url = "http://localhost:8000", string api_key="111", string modelName = "default",float temperature=0)
        {
            var openAiService = CreateOpenAIService(baseDomain: url, apiKey: api_key);

            #region 组织request
            var request = new ChatCompletionCreateRequest
            {
                Messages = new List<ChatMessage>() { 
                    ChatMessage.FromSystem(systemPrompt),
                },
                //Temperature = temperature,
                //MaxTokens = -1,
                //TopP = 0.95f,
                //K
                Model = modelName,
            };
            if (!string.IsNullOrEmpty(userMessage))
                request.Messages.Add(ChatMessage.FromUser(userMessage));
            #endregion
            await Ask(processResult, streamOut, openAiService, request);
        }



        private static async Task Ask(Action<ChatMessage> processResult, bool streamOut, OpenAIService openAiService, ChatCompletionCreateRequest request)
        {
            if (streamOut)
            {
                // ChatGPT Official API
                var completionResult = openAiService.ChatCompletion.CreateCompletionAsStream(request);
                if (completionResult != null)
                {
                    await foreach (var x in completionResult)
                    {
                        if (x.Successful)
                        {
                            processResult(x.Choices.FirstOrDefault().Message);
                        }
                        else
                        {
                            throw new UserFriendlyException(x.Error.Message);
                        }
                    }
                }
            }
            else
            {
                var completionResult = await openAiService.ChatCompletion.CreateCompletion(request);
                processResult(completionResult.Choices.FirstOrDefault().Message);
            }
        }

        //public async static IAsyncEnumerable<string> AskStream(string systemPrompt, string userMessage, AIModel aiModel)
        //{
        //    var openAiService = CreateOpenAIService(baseDomain: aiModel.ApiUrlBase, apiKey: aiModel.ApiKey);
        //    var request = new ChatCompletionCreateRequest
        //    {
        //        Messages = new List<ChatMessage>
        //        {
        //            ChatMessage.FromSystem(systemPrompt),
        //            ChatMessage.FromUser(userMessage)
        //        },
        //        Model = aiModel.Name,
        //    };
        //    // ChatGPT Official API
        //    var completionResult = openAiService.ChatCompletion.CreateCompletionAsStream(request);
        //    if (completionResult != null)
        //    {
        //        await foreach (var x in completionResult)
        //        {
        //            if (x.Successful)
        //            {
        //                yield return x.Choices.FirstOrDefault().Message.Content;
        //            }
        //            else
        //            {
        //                throw new UserFriendlyException(x.Error.Message);
        //            }
        //        }
        //    }
        //}


        //public async static Task Ask(string systemPrompt, string userMessage, Action<string, bool> processResult, string url = null)
        //{
        //    var openAiService = CreateOpenAIService(baseDomain: url);
        //    // ChatGPT Official API
        //    var completionResult = openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
        //    {
        //        Messages = new List<ChatMessage>
        //        {
        //            ChatMessage.FromSystem(systemPrompt),
        //            ChatMessage.FromUser(userMessage)
        //        },
        //        Model = Models.Gpt_3_5_Turbo,
        //    });
        //    await foreach (var x in completionResult)
        //    {
        //        var rst = x.Successful ? x.Choices.FirstOrDefault().Message.Content : x.Error.Message;
        //        processResult(rst, x.Successful);
        //    }
        //}
        //public static (string Message, bool IsError) GetAIResponse(this ChatCompletionCreateResponse completionResult)
        //{
        //    if (completionResult.Successful)
        //    {
        //        return (completionResult.Choices.First().Message.Content, false);
        //    }
        //    else
        //    {
        //        return (string.Join("\n", completionResult.Error.Messages), true);
        //    }
        //}
    }
}