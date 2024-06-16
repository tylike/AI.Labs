using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using DevExpress.ExpressApp.Xpo;
using System.Runtime.InteropServices;
using AI.Labs.Module.BusinessObjects;


namespace RagServer.Module.BusinessObjects
{
    //public class Server
    //{
    //    static Server()
    //    {
    //        DevExpress.Xpo.SimpleDataLayer.SuppressReentrancyAndThreadSafetyCheck = true;

    //    }
    //    static ITextTokenizer Tokenizer = new DefaultGPTTokenizer();

    //    static ITextEmbeddingGenerator embeddingGenerator;

    //    public static ITextEmbeddingGenerator EmbeddingGenerator
    //    {
    //        get
    //        {
    //            if(embeddingGenerator== null)
    //            {
    //                var openAIConfig = new OpenAIConfig();
    //                openAIConfig.TextModelMaxTokenTotal = 1024;
    //                openAIConfig.EmbeddingModelMaxTokenTotal = 1024;
    //                openAIConfig.APIKey = "1";
    //                openAIConfig.TextModel = "qwen";
    //                openAIConfig.EmbeddingModel = "qwen";
    //                //openAIConfig.Endpoint = $"http://127.0.0.1:{i.ToString("9000")}";
    //                openAIConfig.Validate();



                    
    //                var embeddingHttpClient = new HttpClient(new ProxyOpenAIHandler(true));
    //                embeddingGenerator = new OpenAITextEmbeddingGenerator(openAIConfig, 
    //                    log: DefaultLogger<OpenAITextEmbeddingGenerator>.Instance,
    //                    textTokenizer: Tokenizer, httpClient: embeddingHttpClient);
    //            }
    //            return embeddingGenerator;
    //        }
    //    }
    //    static ITextGenerator _textGenerator;
    //    public static ITextGenerator TextGenerator
    //    {
    //        get
    //        {
    //            if(_textGenerator == null)
    //            {
    //                var chatConfig = new OpenAIConfig();
    //                chatConfig.TextModelMaxTokenTotal = 2048;
    //                chatConfig.APIKey = "1";
    //                chatConfig.TextModel = "qwen";
    //                chatConfig.EmbeddingModel = "qwen";
    //                chatConfig.Endpoint = "http://127.0.0.1:8000";
    //                chatConfig.Validate();
    //                var chatHttpClient = new HttpClient(new ProxyOpenAIHandler(false));

    //                _textGenerator = new OpenAITextGenerator(chatConfig,
    //                    textTokenizer: Tokenizer, 
    //                    log: DefaultLogger<OpenAITextGenerator>.Instance, 
    //                    httpClient: chatHttpClient
    //                    );
    //            }
    //            return _textGenerator;
    //        }
    //    }

    //}
    
    public class 记忆分区控制器 : ObjectViewController<ObjectView, 记忆分区>
    {
        public 记忆分区控制器()
        {
            var generateEmbedding = new SimpleAction(this, "GenerateEmbedding", null);
            generateEmbedding.Caption = "生成嵌入";
            generateEmbedding.Execute += GenerateEmbedding_Execute;
        }

        private async void GenerateEmbedding_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //var emb = new 嵌入生成(Server.EmbeddingGenerator);
            //foreach (var item in e.SelectedObjects.OfType<记忆分区>())
            //{
            //    await emb.InvokeAsync(item,Server.EmbeddingGenerator);
            //}
            //Application.ShowViewStrategy.ShowMessage("嵌入生成完成!");
            //ObjectSpace.CommitChanges();
        }
    }


    public class Result
    {
        public string Snippet { get; set; }
        public int Weight { get; set; }
        public List<FindResult> Keywords { get; } = new List<FindResult>();
    }
    public class FindResult
    {
        public string Keyword { get; set; }
        public int Position { get; set; }
    }

    public interface IProgressReport
    {
        void Report(object message, int progress, int count);
        int Progress { get; }
        int Count { get; set; }
        event EventHandler OnMessage;
    }
    public interface ISummaryProgressReport : IProgressReport
    {

    }
    public interface IEmbeddingProgressReport : IProgressReport { }
    public class ProgressReport : IProgressReport
    {
        public int Progress { get; private set; }
        public object Message { get; private set; }
        public int Count { get; set; }
        public TimeSpan? Start { get; private set; }
        public TimeSpan? End { get; private set; }
        public TimeSpan Duration
        {
            get
            {
                if (Start.HasValue && End.HasValue)
                {
                    return End.Value.Subtract(Start.Value);
                }
                return TimeSpan.Zero;
            }
        }
        public void Report(object message, int progress, int count)
        {
            if (!Start.HasValue)
            {
                Start = DateTime.Now.TimeOfDay;
            }
            End = DateTime.Now.TimeOfDay;

            Progress = progress;
            Message = message;
            Count = count;
            //Debug.WriteLine(Start.ToString() +":"+End.ToString());
            OnMessage?.Invoke(this, null);
        }

        public event EventHandler OnMessage;
    }
    public class SummaryProgressReport : ProgressReport, ISummaryProgressReport
    {

    }
    public class EmbeddingProgressReport : ProgressReport, IEmbeddingProgressReport
    {

    }

}
