using LLama.Common;
using LLama;
using LLama.Native;
using System;
using System.Collections.Generic;
using System.IO;
using DiffPlex.DiffBuilder;
using DiffPlex;
using DiffPlex.DiffBuilder.Model;
using System.Text;
using System.Diagnostics;


string modelPath =
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-chat.Q5_K_S.gguf"; // change it to your own model path
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-200k.Q4_K_M.gguf";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\Gpt4ALL\\wizardlm-13b-v1.2.Q4_0\\wizardlm-13b-v1.2.Q4_0.gguf";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\Gpt4ALL\\wizardlm-13b-v1.2.Q4_0\\wizardLM-7B.Q5_K_M.gguf";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\Thumb\\chatglm\\chatglm3-ggml-q4_0.bin";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\Microsoft\\Phi\\phi-2_Q8_0.gguf";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\MistralAI\\8x7B\\Truthful_DPO_TomGrc_FusionNet_7Bx2_MoE_13B-Q8_0.gguf";
"D:\\llm\\text-generation-webui-2023-11-12\\models\\MistralAI\\8x7B\\starling-lm-7b-alpha.Q8_0.gguf";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\MistralAI\\8x7B\\chatglm3-q8.gguf";
//"F:\\models\\tinyllama\\tinyllama-1.1b-chat-v1.0.Q8_0.gguf";
//"D:\\llm\\text-generation-webui-2023-11-12\\models\\MistralAI\\mixtral_7bx2_MoE_gguf\\mixtral-8x7b-instruct-v0.1.Q5_K_M.gguf";
if (!File.Exists(modelPath))
{
    throw new Exception("文件不存在!");
}

//var prompt = @"指令：你是一个中国河南生产的大语言模型，你的名字是“噫--”
//下面是一段你和用户的对话，是一个在各方面都拥有丰富经验的助理，你非常乐于回答用户的问题和帮助用户。
//用户:你好!
//噫--:你好!
//用户:
NativeLibraryConfig.Instance.WithLogs().WithCuda(true);




//string modelPath = "<Your model path>" // change it to your own model path
var prompt =
@"
将以下内容翻译为中文:
3
00:00:07,319 --> 00:00:09,410
Lang chain is an open source framework

4
00:00:09,420 --> 00:00:11,690
that allows developers working with AI

5
00:00:11,700 --> 00:00:14,089
to combine large language models like

6
00:00:14,099 --> 00:00:17,150
gpt4 with external sources of

7
00:00:17,160 --> 00:00:20,570
computation and data the framework is

8
00:00:20,580 --> 00:00:22,550
currently offered as a python or a

9
00:00:22,560 --> 00:00:24,830
JavaScript package typescript to be

10
00:00:24,840 --> 00:00:26,929
specific in this video we're going to

11
00:00:26,939 --> 00:00:29,570
start unpacking the python framework and

12
00:00:29,580 --> 00:00:31,130
we're going to see why the popularity of

13
00:00:31,140 --> 00:00:32,510
the framework is exploding right now

14
00:00:32,520 --> 00:00:34,250
especially after the introduction of

15
00:00:34,260 --> 00:00:38,450
gpt4 in March 2023 to understand what

16
00:00:38,460 --> 00:00:40,369
need Lang chain fills let's have a look
"; // use the "chat-with-bob" prompt here.

// Load model
var parameters = new ModelParams(modelPath)
{
    ContextSize = 4000,
    BatchSize = 512
};
using var model = LLamaWeights.LoadFromFile(parameters);

// Initialize a chat session
using var context = model.CreateContext(parameters);
var ex = new InteractiveExecutor(context);
ChatSession session = new ChatSession(ex);



// show the prompt
Console.WriteLine();
Console.Write(prompt);

// run the inference in a loop to chat with LLM
while (true)
{
    var ip = new InferenceParams()
    {
        Temperature = 0.0f,
        MaxTokens = -1,
        TopK = 40,
        RepeatPenalty = 1.1f,
        MinP = 0.05f,
        TopP = 0.95f,
        //FrequencyPenalty = 1.1f,
        AntiPrompts = new List<string> { "user:" }
    };
    //第一次是读入了所有
    session.History.AddMessage(AuthorRole.System, "Below is an instruction that describes a task. Write a response that appropriately completes the request.");
    session.History.AddMessage(AuthorRole.User, "将以下内容翻译为中文:\nhello,world!");
    session.History.AddMessage(AuthorRole.Assistant, "你好,世界!");
    var message = new ChatHistory.Message(AuthorRole.User, prompt);

    var sb = new StringBuilder();
    var sw = Stopwatch.StartNew();
    var cnt = 0;
    await foreach (var text in session.ChatAsync(message, ip))
    {
        //var t = (text+"").Replace("\\", "\");
        Console.Write(text);
        sb.Append(text);
        cnt++;
    }
    sw.Stop();

    Console.WriteLine();
    Console.WriteLine($"token:{cnt},用时:{sw.Elapsed.TotalSeconds},平均:{cnt / sw.Elapsed.TotalSeconds}");
    Console.WriteLine(sb.ToString().Replace("\\n", "\n"));



    Console.ForegroundColor = ConsoleColor.Green;
    prompt = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;
}