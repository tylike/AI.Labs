using LLama.Common;
using LLama;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;

namespace LlamaConsole48
{


    public static class YouTubeDownloader
    {
        public static async Task DownloadVideoAsync(string videoIdOrUrl, string outputPath)
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            var youtube = new YoutubeClient();
            var videoId = VideoId.TryParse(videoIdOrUrl);
            // Try to parse the video ID
            if (videoId == null)
            {
                Console.WriteLine("Invalid YouTube video ID or URL.");
                return;
            }

            // Get the video
            var video = await youtube.Videos.GetAsync(videoId.Value);

            // Get the stream manifest
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId.Value);
            var streamInfo = streamManifest.GetMuxedStreams().GetWithHighestVideoQuality();

            if (streamInfo != null)
            {
                // Download the video
                Console.WriteLine($"Downloading {video.Title}...");
                await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{outputPath}{video.Title}.{streamInfo.Container}");

                Console.WriteLine("Download complete!");
            }
            else
            {
                Console.WriteLine("No suitable video stream found.");
            }
        }
    }
    internal class Program
    {
        static void Main(string[] args)
        {

            YouTubeDownloader.DownloadVideoAsync("https://www.youtube.com/watch?v=iMYn7EGDfXw", "d:\\videoInfo\\AutoGen.Agent").Wait();

            string modelPath =
                //"D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-chat.Q5_K_S.gguf"; // change it to your own model path
                "D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\6b-q5\\yi-6b-200k.Q5_K_M.gguf";
            var prompt = "你是一个中国河南生产的大语言模型，你的名字是“噫--”"; // use the "chat-with-bob" prompt here.

            //NativeLibraryConfig.Instance.WithLibrary("D:\\dev\\AI.Labs\\LlamaConsoleSharp\\bin\\x64\\Debug\\net7.0\\runtimes\\win-x64\\native\\cuda11");

            //NativeLibraryConfig.Instance.WithLogs().WithCuda(true);


            // Load a model
            var parameters = new ModelParams(modelPath)
            {
                ContextSize = 1024,
                Seed = 1337,
                GpuLayerCount = 20
            };
            var model = LLamaWeights.LoadFromFile(parameters);

            // Initialize a chat session
            var context = model.CreateContext(parameters);
            var ex = new InteractiveExecutor(context);
            ChatSession session = new ChatSession(ex);

            // show the prompt
            Console.WriteLine();
            Console.Write(prompt);

            // run the inference in a loop to chat with LLM
            while (prompt != "stop")
            {
                var rst = session.ChatAsync(prompt, new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "User:" } });

                //foreach (var text in rst.GetAsyncEnumerator().)
                //{
                //    Console.Write(text);
                //}
                prompt = Console.ReadLine();
            }

            // save the session
            session.SaveSession("SavedSessionPath");
        }
    }
}
