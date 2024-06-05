using AutoGen.Core;
using Azure.AI.OpenAI;
using Azure.Core.Pipeline;
using System.Diagnostics;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class AgentHelper
    {
        private readonly GPTAgent innerAgent;
        public static string url = "http://192.168.1.139:3000"; // "https://na9yqj2l5a5r.share.zrok.io";
        public static GPTAgent CreateAgent(
            Uri host = null,
            string modelName = "qwen-max",
            string api_key = "sk-YKK60hWFsDN3P2Zx390eEb9a0bEa4321842e739dFd64C0E5",
            string name = null,
            string systemMessage = "You are a helpful AI assistant",
            float temperature = 0.7f,
            int maxTokens = 1024,
            IEnumerable<FunctionDefinition>? functions = null,
            IDictionary<string, Func<string, Task<string>>>? functionMap = null)
        {
            if (host == null)
                host = new Uri(url);
            if (name == null)
            {
                Debug.WriteLine("无名称agent!");
            }
            var client = ConfigOpenAIClientForLMStudio(host, api_key);
            return new GPTAgent(
                name: name,
                systemMessage: systemMessage,
                openAIClient: client,
                modelName: modelName, // model name doesn't matter for LM Studio
                temperature: temperature,
                maxTokens: maxTokens,
                functions: functions,
                functionMap: functionMap);
        }

        public string Name => innerAgent.Name;

        public Task<IMessage> GenerateReplyAsync(
            IEnumerable<IMessage> messages,
            GenerateReplyOptions? options = null,
            System.Threading.CancellationToken cancellationToken = default)
        {
            return innerAgent.GenerateReplyAsync(messages, options, cancellationToken);
        }

        private static OpenAIClient ConfigOpenAIClientForLMStudio(Uri host, string api_key)
        {
            // create uri from host and port

            var handler = new CustomHttpClientHandler(host);
            var httpClient = new HttpClient(handler);
            var option = new OpenAIClientOptions(OpenAIClientOptions.ServiceVersion.V2022_12_01)
            {
                Transport = new HttpClientTransport(httpClient),

            };
            return new OpenAIClient(api_key, option);
        }

        private sealed class CustomHttpClientHandler : HttpClientHandler
        {
            private Uri _modelServiceUrl;

            public CustomHttpClientHandler(Uri modelServiceUrl)
            {
                _modelServiceUrl = modelServiceUrl;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                // request.RequestUri = new Uri($"{_modelServiceUrl}{request.RequestUri.PathAndQuery}");
                Debug.WriteLine($"请求:{request.RequestUri}=>{_modelServiceUrl}");
                var uriBuilder = new UriBuilder(_modelServiceUrl);
                uriBuilder.Path = request.RequestUri.PathAndQuery;
                request.RequestUri = uriBuilder.Uri;

                return base.SendAsync(request, cancellationToken);
            }
        }
    }

}
