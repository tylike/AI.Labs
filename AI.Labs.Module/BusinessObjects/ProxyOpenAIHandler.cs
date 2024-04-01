#pragma warning disable SKEXP0003 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0003 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class ProxyOpenAIHandler : HttpClientHandler
{
    public static string API_Base = "http://127.0.0.1:8000";
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri != null && request.RequestUri.Host.Equals("api.openai.com", StringComparison.OrdinalIgnoreCase))
        {
            // your proxy url
            request.RequestUri = new Uri($"{API_Base}{request.RequestUri.PathAndQuery}");
        }
        return base.SendAsync(request, cancellationToken);
    }
}
