using AutoGen.Core;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class OpenAILikeConfig : ILLMConfig
    {
        public string Host { get; }

        public int Port { get; }

        public Uri Uri { get; }

        public OpenAILikeConfig(string host, int port, bool https)
        {
            Host = host;
            Port = port;
            var http = https ? "https" : "http";
            Uri = new Uri($"{http}://{host}:{port}");
        }

        public OpenAILikeConfig(Uri uri)
        {
            Uri = uri;
            Host = uri.Host;
            Port = uri.Port;
        }
    }

}
