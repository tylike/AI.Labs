using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
#pragma warning disable SKEXP0003 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0052 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

#pragma warning restore SKEXP0011 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class SemanticKernelOpenAI
{
    private const string MemoryCollectionName = "SKGitHub";

    public static async Task Start()
    {   
        #pragma warning disable SKEXP0011 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

        var apiKey = "111";
        var model = "text-embedding-ada-002";
        var memoryWithCustomDb = new MemoryBuilder();

        var http = new HttpClient(new ProxyOpenAIHandler());

        memoryWithCustomDb = memoryWithCustomDb.WithOpenAITextEmbeddingGeneration( model, apiKey, httpClient:http);


        memoryWithCustomDb = memoryWithCustomDb.WithMemoryStore(new VolatileMemoryStore());
        memoryWithCustomDb = memoryWithCustomDb.WithHttpClient(http);
        var t = memoryWithCustomDb.Build();
#pragma warning restore SKEXP0052 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

        
        await RunExampleAsync(t);
    }


    public static async Task RunExampleAsync(ISemanticTextMemory memory)
    {
        await StoreMemoryAsync(memory);

        await SearchMemoryAsync(memory, "How do I get started?");

        /*
        Output:

        Query: How do I get started?

        Result 1:
          URL:     : https://github.com/microsoft/semantic-kernel/blob/main/README.md
          Title    : README: Installation, getting started, and how to contribute

        Result 2:
          URL:     : https://github.com/microsoft/semantic-kernel/blob/main/samples/dotnet-jupyter-notebooks/00-getting-started.ipynb
          Title    : Jupyter notebook describing how to get started with the Semantic Kernel

        */

        await SearchMemoryAsync(memory, "Can I build a chat with SK?");
        /*
        Output:

        Query: Can I build a chat with SK?

        Result 1:
          URL:     : https://github.com/microsoft/semantic-kernel/tree/main/samples/plugins/ChatPlugin/ChatGPT
          Title    : Sample demonstrating how to create a chat plugin interfacing with ChatGPT

        Result 2:
          URL:     : https://github.com/microsoft/semantic-kernel/blob/main/samples/apps/chat-summary-webapp-react/README.md
          Title    : README: README associated with a sample chat summary react-based webapp

        */
    }

    private static async Task SearchMemoryAsync(ISemanticTextMemory memory, string query)
    {
        Console.WriteLine("\nQuery: " + query + "\n");

        var memoryResults = memory.SearchAsync(MemoryCollectionName, query, limit: 2, minRelevanceScore: 0.5);

        int i = 0;
        await foreach (MemoryQueryResult memoryResult in memoryResults)
        {
            Console.WriteLine($"Result {++i}:");
            Console.WriteLine("  URL:     : " + memoryResult.Metadata.Id);
            Console.WriteLine("  Title    : " + memoryResult.Metadata.Description);
            Console.WriteLine("  Relevance: " + memoryResult.Relevance);
            Console.WriteLine();
        }

        Console.WriteLine("----------------------");
    }

    private static async Task StoreMemoryAsync(ISemanticTextMemory memory)
    {
        /* Store some data in the semantic memory.
         *
         * When using Azure AI Search the data is automatically indexed on write.
         *
         * When using the combination of VolatileStore and Embedding generation, SK takes
         * care of creating and storing the index
         */

        Console.WriteLine("\nAdding some GitHub file URLs and their descriptions to the semantic memory.");
        var githubFiles = SampleData();
        var i = 0;
        foreach (var entry in githubFiles)
        {
            await memory.SaveReferenceAsync(
                collection: MemoryCollectionName,
                externalSourceName: "GitHub",
                externalId: entry.Key,
                description: entry.Value,
                text: entry.Value);

            Console.Write($" #{++i} saved.");
        }

        Console.WriteLine("\n----------------------");
    }

    private static Dictionary<string, string> SampleData()
    {
        return new Dictionary<string, string>
        {
            ["https://github.com/microsoft/semantic-kernel/blob/main/README.md"]
                = "README: Installation, getting started, and how to contribute",
            ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks/02-running-prompts-from-file.ipynb"]
                = "Jupyter notebook describing how to pass prompts from a file to a semantic plugin or function",
            ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/notebooks//00-getting-started.ipynb"]
                = "Jupyter notebook describing how to get started with the Semantic Kernel",
            ["https://github.com/microsoft/semantic-kernel/tree/main/samples/plugins/ChatPlugin/ChatGPT"]
                = "Sample demonstrating how to create a chat plugin interfacing with ChatGPT",
            ["https://github.com/microsoft/semantic-kernel/blob/main/dotnet/src/SemanticKernel/Memory/VolatileMemoryStore.cs"]
                = "C# class that defines a volatile embedding store",
        };
    }

}
