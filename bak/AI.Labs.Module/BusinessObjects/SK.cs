#pragma warning disable SKEXP0003 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
using Microsoft.Extensions.DependencyInjection;


//
// 摘要:
//     Provides functionality for retrieving instances of HttpClient.
internal static class HttpClientProvider
{
    //
    // 摘要:
    //     Represents a singleton implementation of System.Net.Http.HttpClientHandler that
    //     is not disposable.
    private sealed class NonDisposableHttpClientHandler : HttpClientHandler
    {
        //
        // 摘要:
        //     Gets the singleton instance of Microsoft.SemanticKernel.Http.HttpClientProvider.NonDisposableHttpClientHandler.
        public static NonDisposableHttpClientHandler Instance { get; } = new NonDisposableHttpClientHandler();


        //
        // 摘要:
        //     Private constructor to prevent direct instantiation of the class.
        private NonDisposableHttpClientHandler()
        {
            base.CheckCertificateRevocationList = true;
        }

        //
        // 摘要:
        //     Disposes the underlying resources held by the Microsoft.SemanticKernel.Http.HttpClientProvider.NonDisposableHttpClientHandler.
        //     This implementation does nothing to prevent unintended disposal, as it may affect
        //     all references.
        //
        // 参数:
        //   disposing:
        //     True if called from Microsoft.SemanticKernel.Http.HttpClientProvider.NonDisposableHttpClientHandler.Dispose(System.Boolean),
        //     false if called from a finalizer.
        protected override void Dispose(bool disposing)
        {
        }
    }

    //
    // 摘要:
    //     Retrieves an instance of HttpClient.
    //
    // 返回结果:
    //     An instance of HttpClient.
    public static HttpClient GetHttpClient()
    {
        return new HttpClient(NonDisposableHttpClientHandler.Instance, disposeHandler: false);
    }

    //
    // 摘要:
    //     Retrieves an instance of HttpClient.
    //
    // 返回结果:
    //     An instance of HttpClient.
    public static HttpClient GetHttpClient(HttpClient? httpClient = null)
    {
        return httpClient ?? GetHttpClient();
    }

    //
    // 摘要:
    //     Retrieves an instance of HttpClient.
    //
    // 返回结果:
    //     An instance of HttpClient.
    public static HttpClient GetHttpClient(IServiceProvider? serviceProvider = null)
    {
        return GetHttpClient(serviceProvider?.GetService<HttpClient>());
    }

    //
    // 摘要:
    //     Retrieves an instance of HttpClient.
    //
    // 返回结果:
    //     An instance of HttpClient.
    public static HttpClient GetHttpClient(HttpClient? httpClient, IServiceProvider serviceProvider)
    {
        return httpClient ?? GetHttpClient(serviceProvider?.GetService<HttpClient>());
    }
}
