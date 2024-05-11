using DevExpress.ExpressApp;
using System.Diagnostics;
#pragma warning disable SKEXP0028 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

namespace AI.Labs.Module.Controllers
{
    public class SemanticKernelMemory
    {
        static LLamaWeights model;
        static LLamaEmbedder embedding;
        static ModelParams ModelParameters;
        public static void StartServer(string modelPath = "D:\\llm\\text-generation-webui-2023-11-12\\models\\Gpt4ALL\\wizardlm-13b-v1.2.Q4_0\\wizardlm-13b-v1.2.Q4_0.gguf")
        {
            modelPath = "D:\\llm\\text-generation-webui-2023-11-12\\models\\MistralAI\\8x7B\\mixtral-8x7b-v0.1.Q8_0.gguf";
            modelPath = "D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-chat.Q5_K_S.gguf";
            //dolphin-2.6-mixtral-8x7b.Q5_K_M.gguf
            modelPath = "D:\\llm\\text-generation-webui-2023-11-12\\models\\YI\\34b-q8\\yi-34b-chat.Q5_K_S.gguf";
            var seed = 1337u;
            // Load weights into memory
            ModelParameters = new ModelParams(modelPath)
            {
                Seed = seed,
                EmbeddingMode = true
            };
            model = LLamaWeights.LoadFromFile(ModelParameters);
            embedding = new LLamaEmbedder(model, ModelParameters);
        }

        public async static Task<ISemanticTextMemory> GetMemory()
        {
#pragma warning disable SKEXP0011 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            var apiKey = "sk-PAhf15OdDXajqcYG6gEFQdBxWKzmlVwjBIZtDc0DtaQGsUhI";
            var model = "text-embedding-ada-002";
            var memoryWithCustomDb = new MemoryBuilder();
            ProxyOpenAIHandler.API_Base = "https://api.chatanywhere.tech";
            var http = new HttpClient(new ProxyOpenAIHandler());
            var store = Directory.GetCurrentDirectory() + "/MemoryDB.sqlite";
            var sqlMemory = await SqliteMemoryStore.ConnectAsync(store);


            memoryWithCustomDb = memoryWithCustomDb.WithOpenAITextEmbeddingGeneration(model, apiKey, httpClient: http);


            memoryWithCustomDb = memoryWithCustomDb.WithMemoryStore(sqlMemory);
            memoryWithCustomDb = memoryWithCustomDb.WithHttpClient(http);
            var t = memoryWithCustomDb.Build();

            return t;
        }



        private const string MemoryCollectionName = "SKGitHub";
        static ISemanticTextMemory memory;
        public async static void LoadMemory(Dictionary<string, string> userDatas)
        {

            //if (embedding == null || model == null)
            //{
            //    throw new UserFriendlyException("还没有启动LLM服务！");
            //}

            //var store = Directory.GetCurrentDirectory() + "/MemoryDB.sqlite";
            //var sqlMemory = await SqliteMemoryStore.ConnectAsync(store);

            //memory = new MemoryBuilder()
            //    .WithTextEmbeddingGeneration(new LLamaSharpEmbeddingGeneration(embedding))
            //    .WithMemoryStore(sqlMemory)
            //    .Build();

            //static TokenCredential CreateDelegatedToken(string token)
            //{
            //    AccessToken accessToken = new AccessToken(token, DateTimeOffset.Now.AddDays(180.0));
            //    return DelegatedTokenCredential.Create((TokenRequestContext _, CancellationToken _) => accessToken);
            //}
            //var token = "sk-PAhf15OdDXajqcYG6gEFQdBxWKzmlVwjBIZtDc0DtaQGsUhI";

            //ProxyOpenAIHandler.API_Base = "https://api.chatanywhere.tech";

            //var openAIClient = new OpenAIClient(token,)

#pragma warning disable SKEXP0011 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            //var openAIEmbedding = new OpenAITextEmbeddingGenerationService("text-embedding-ada-002", openAIClient);
#pragma warning restore SKEXP0011 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            //memory = new MemoryBuilder()
            //    .WithTextEmbeddingGeneration(new LLamaSharpEmbeddingGeneration(openAIEmbedding))
            //    .WithMemoryStore(sqlMemory)
            //    .Build();
            //memory = new MemoryBuilder()
            //    .WithTextEmbeddingGeneration(openAIEmbedding)
            //    .WithMemoryStore(sqlMemory)
            //    .Build();
            //new OpenAITextEmbeddingGenerationService("text-ada",)
            memory = await GetMemory();

            int i = 0;
            foreach (var entry in userDatas)
            {
                var find = await memory.GetAsync(MemoryCollectionName, entry.Key);
                if (find == null)
                {
                    var sw = Stopwatch.StartNew();
                    //memory.SaveReferenceAsync(
                    //    collection: MemoryCollectionName,
                    //    externalSourceName: "AI.Labs.System",
                    //    externalId: entry.Key,
                    //    description: entry.Value,
                    //    text: entry.Value);
                    await memory.SaveInformationAsync(MemoryCollectionName, entry.Value, entry.Key);
                    sw.Stop();
                    Debug.WriteLine($"第:{i++}/{userDatas.Count},用时:{sw.Elapsed}");

                }
            }
            await Task.CompletedTask;
        }

        public async static IAsyncEnumerable<MemoryQueryResult> QueryMemory(string query)
        {
            //if (embedding == null || model == null)
            //{
            //    throw new UserFriendlyException("还没有启动LLM服务！");
            //}
            if (memory == null)
            {
                throw new UserFriendlyException("还没加载记忆!");
            }
            var t = memory.SearchAsync(MemoryCollectionName, query, limit: 10, minRelevanceScore: 0.5);
            await foreach (var x in t)
            {
                yield return x;
            }
        }

        public static void Stop()
        {

        }

        public static async IAsyncEnumerable<string> Ask(string prompt)
        {
            if (model == null)
            {
                throw new UserFriendlyException("还没有启动llm server!");
            }
            var ip = new InferenceParams() { Temperature = 0.6f, AntiPrompts = new List<string> { "user:" } };
            var se = new StatelessExecutor(model, ModelParameters);
            await foreach (var x in se.InferAsync(prompt, ip))
            {
                yield return x;
            }
        }
    }

    //#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    //    public sealed class LLamaSharpEmbeddingGeneration : ITextEmbeddingGenerationService, IAIService
    //#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    //    {
    //        private LLamaEmbedder _embedder;

    //        private readonly Dictionary<string, string> _attributes = new Dictionary<string, string>();

    //        public IReadOnlyDictionary<string, string> Attributes => _attributes;

    //        IReadOnlyDictionary<string, object> IAIService.Attributes => throw new NotImplementedException();

    //        public LLamaSharpEmbeddingGeneration(LLamaEmbedder embedder)
    //        {
    //            _embedder = embedder;
    //        }

    //        public async Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, CancellationToken cancellationToken = default(CancellationToken))
    //        {
    //            return await Task.FromResult(data.Select((string text) => new ReadOnlyMemory<float>(_embedder.GetEmbeddings(text))).ToList());
    //        }

    //        public Task<IList<ReadOnlyMemory<float>>> GenerateEmbeddingsAsync(IList<string> data, Kernel kernel = null, CancellationToken cancellationToken = default)
    //        {
    //            IList<ReadOnlyMemory<float>> t = data.Select(
    //                    (string text) => new ReadOnlyMemory<float>(
    //                    _embedder.GetEmbeddings(text)
    //                    )
    //                ).ToList();
    //            return Task.FromResult(
    //                t
    //                );
    //        }
    //    }
}


#pragma warning restore SKEXP0028 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
