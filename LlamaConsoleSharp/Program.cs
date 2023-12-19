using LLama.Common;
using LLama;
using LLama.Native;
using System;
using System.Collections.Generic;
using System.IO;

string modelPath =
    //"D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-chat.Q5_K_S.gguf"; // change it to your own model path
    //"D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-200k.Q4_K_M.gguf";
    "D:\\llm\\text-generation-webui-2023-11-12\\models\\Gpt4ALL\\wizardlm-13b-v1.2.Q4_0\\wizardlm-13b-v1.2.Q4_0.gguf";
    //"D:\\llm\\text-generation-webui-2023-11-12\\models\\Gpt4ALL\\wizardlm-13b-v1.2.Q4_0\\wizardLM-7B.Q5_K_M.gguf";
    //"D:\\llm\\text-generation-webui-2023-11-12\\models\\Thumb\\chatglm\\chatglm3-ggml-q4_0.bin";

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
@"system:你是一个人工智能助手,你的名字是小张!
user:你好,你是谁?
assistant:您好!我是小张!请问有什么可以帮您的?
user:
"; // use the "chat-with-bob" prompt here.

// Load model
var parameters = new ModelParams(modelPath)
{
    ContextSize = 1024
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
    var ip = new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "user:" } };
    //第一次是读入了所有
    await foreach (var text in session.ChatAsync(prompt,ip ))
    {
        Console.Write(text);
    }

    Console.ForegroundColor = ConsoleColor.Green;
    prompt = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;
}