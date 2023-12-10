using LLama.Common;
using LLama;
using LLama.Native;
using System;
using System.Collections.Generic;


string modelPath =
    //"D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-chat.Q5_K_S.gguf"; // change it to your own model path
    "D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-200k.Q4_K_M.gguf";
//var prompt = @"指令：你是一个中国河南生产的大语言模型，你的名字是“噫--”
//下面是一段你和用户的对话，是一个在各方面都拥有丰富经验的助理，你非常乐于回答用户的问题和帮助用户。
//用户:你好!
//噫--:你好!
//用户:
//"; // use the "chat-with-bob" prompt here.

////NativeLibraryConfig.Instance.WithLibrary("D:\\dev\\AI.Labs\\LlamaConsoleSharp\\bin\\x64\\Debug\\net7.0\\runtimes\\win-x64\\native\\cuda12\\libllama.dll");

NativeLibraryConfig.Instance.WithLogs().WithCuda(true);


//// Load a model
//var parameters = new ModelParams(modelPath)
//{
//    ContextSize = 2048,
//    Seed = 1337,
//    GpuLayerCount = 30,

//    //MainGpu=2,
//};
//using var model = LLamaWeights.LoadFromFile(parameters);

//// Initialize a chat session
//using var context = model.CreateContext(parameters);
//var ex = new InteractiveExecutor(context);
//ChatSession session = new ChatSession(ex);

//// show the prompt
//Console.WriteLine();
//Console.Write(prompt);

//// run the inference in a loop to chat with LLM
//while (prompt != "stop")
//{
//    await foreach (var text in session.ChatAsync(prompt, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "用户:" } }))
//    {
//        Console.Write(text);
//    }
//    prompt = Console.ReadLine();
//}

//// save the session
//session.SaveSession("SavedSessionPath");



//string modelPath = "<Your model path>" // change it to your own model path
var prompt = "Transcript of a dialog, where the User interacts with an Assistant named Bob. Bob is helpful, kind, honest, good at writing, and never fails to answer the User's requests immediately and with precision.\r\n\r\nUser: Hello, Bob.\r\nBob: Hello. How may I help you today?\r\nUser: Please tell me the largest city in Europe.\r\nBob: Sure. The largest city in Europe is Moscow, the capital of Russia.\r\nUser:"; // use the "chat-with-bob" prompt here.

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
    await foreach (var text in session.ChatAsync(prompt, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "User:" } }))
    {
        Console.Write(text);
    }

    Console.ForegroundColor = ConsoleColor.Green;
    prompt = Console.ReadLine();
    Console.ForegroundColor = ConsoleColor.White;
}