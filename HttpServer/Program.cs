// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Net;
using System.Text;

Console.WriteLine("Hello, World!");
PG.RunHttp();


Console.ReadKey();

class PG
{
    static HttpListener listener;
    static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public static HttpListener RunHttp()
    {
        // 创建和配置HttpListener
        listener = new HttpListener();
        listener.Prefixes.Add("http://127.0.0.1:19012/");
        listener.Start();
        Console.WriteLine("HTTP Listener started.");
        // 在后台线程中处理连接
        Task.Run(() => HandleIncomingConnections(cancellationTokenSource.Token));

        return listener;
    }


    //private static void HandleIncomingConnections(CancellationToken token)
    //{
    //    try
    //    {
    //        while (!token.IsCancellationRequested)
    //        {
    //            if (listener.IsListening)
    //            {
    //                IAsyncResult result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
    //                // 等待请求到达或取消请求
    //                if (WaitHandle.WaitAny(new[] { result.AsyncWaitHandle, token.WaitHandle }) == 1)
    //                {
    //                    // Cancel was signaled
    //                    break;
    //                }
    //                listener.EndGetContext(result);
    //            }
    //        }
    //    }
    //    catch (HttpListenerException)
    //    {
    //        // HttpListenerException 可能在调用 Close() 时抛出
    //    }
    //}

    //private static void ListenerCallback(IAsyncResult result)
    //{
    //    if (result.AsyncState is HttpListener listener)
    //    {
    //        try
    //        {
    //            // 调用 EndGetContext 来完成异步操作
    //            HttpListenerContext context = listener.EndGetContext(result);
    //            HttpListenerRequest request = context.Request;
    //            HttpListenerResponse response = context.Response;

    //            // 从请求中读取 POST 数据
    //            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
    //            {
    //                string postData = reader.ReadToEnd();
    //                Debug.WriteLine($"http:{postData}");
    //                //Console.WriteLine(postData);
    //            }

    //            // 发送响应
    //            string responseString = "Progress received.";
    //            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
    //            response.ContentLength64 = buffer.Length;
    //            response.OutputStream.Write(buffer, 0, buffer.Length);
    //            response.OutputStream.Close();
    //        }
    //        catch (Exception ex)
    //        {
    //            Console.WriteLine(ex.Message);
    //        }
    //    }
    //}


    private static void HandleIncomingConnections(CancellationToken token)
    {
        // 监听循环应该只开始监听而不阻塞
        try
        {
            if (listener.IsListening)
            {
                // 开始异步操作以监听请求
                listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
            }
        }
        catch (HttpListenerException ex)
        {
            // HttpListenerException 可能在调用 Close() 时抛出
            Console.WriteLine($"HttpListenerException: {ex.Message}");
        }
        catch (ObjectDisposedException ex)
        {
            // HttpListener 已经被关闭
            Console.WriteLine($"ObjectDisposedException: {ex.Message}");
        }
    }

    private static void ListenerCallback(IAsyncResult result)
    {
        if (result.AsyncState is HttpListener listener)
        {
            // 完成异步操作，并获取 HttpListenerContext 对象
            HttpListenerContext context = null;
            try
            {
                context = listener.EndGetContext(result);
            }
            catch (Exception ex)
            {
                // 如果在异步操作时 HttpListener 被停止或关闭，捕获异常
                Console.WriteLine(ex.Message);
                return;
            }

            // 处理请求
            // 处理请求
            if (context != null)
            {
                // 使用 Task.Run 来异步处理请求
                Task.Run(() => ProcessRequest(context));

                // 继续监听下一个请求
                if (!cancellationTokenSource.IsCancellationRequested)
                {
                    listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                }
            }
        }
    }


    private static void ProcessRequest(HttpListenerContext context)
    {
        try
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            // 从请求中读取 POST 数据
            using (StreamReader reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")} http: {line}");
                }
                //string postData = reader.ReadToEnd();
                //Debug.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.fff")}http:{postData}");
            }

            // 发送响应
            string responseString = "Progress received.";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
        }
        finally
        {
            // 确保响应流被关闭
            context.Response.OutputStream.Close();
        }
    }
}