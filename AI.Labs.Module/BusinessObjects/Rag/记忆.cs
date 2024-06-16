using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using DevExpress.XtraGauges.Core.Model;
using Microsoft.Extensions.Azure;

using Newtonsoft.Json;
using System.Text;
using DevExpress.Persistent.Validation;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using System.Diagnostics;
using HtmlAgilityPack;

namespace RagServer.Module.BusinessObjects
{
    public interface IMimeTypeDetection
    {
        string GetFileType(string filename);
    }

    public class MimeTypesDetection : IMimeTypeDetection
    {
        private static readonly Dictionary<string, string> s_extensionTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".bmp", "image/bmp" },
        { ".gif", "image/gif" },
        { ".jpeg", "image/jpeg" },
        { ".jpg", "image/jpeg" },
        { ".png", "image/png" },
        { ".tiff", "image/tiff" },
        { ".json", "application/json" },
        { ".md", "text/plain-markdown" },
        { ".htm", "text/html" },
        { ".html", "text/html" },
        { ".url", "text/x-uri" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".ppt", "application/vnd.ms-powerpoint" },
        { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".txt", "text/plain" },
        { ".pdf", "application/pdf" },
        { ".text_embedding", "float[]" }
    };

        public string GetFileType(string filename)
        {
            string extension = Path.GetExtension(filename);
            if (s_extensionTypes.TryGetValue(extension, out string value))
            {
                return value;
            }

            throw new NotSupportedException("File type not supported: " + filename);
        }
    }


    public class WebScraper
    {
        public class Result
        {
            public bool Success { get; set; }

            public string Text { get; set; } = string.Empty;


            public string Error { get; set; } = string.Empty;

        }


        public WebScraper()
        {

        }

        public async Task<Result> GetTextAsync(string url)
        {
            return await GetAsync(new Uri(url)).ConfigureAwait(continueOnCapturedContext: false);
        }

        private async Task<Result> GetAsync(Uri url)
        {
            Uri url2 = url;
            string text = url2.Scheme.ToUpperInvariant();
            if (text != "HTTP" && text != "HTTPS")
            {
                return new Result
                {
                    Success = false,
                    Error = "Unknown URL protocol: " + url2.Scheme
                };
            }

            HttpClient client = new HttpClient();
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Kernel-Memory");
                var httpResponseMessage = await client.GetAsync(url2);
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    Debug.WriteLine("Error while fetching page {0}, status code: {1}", url2.AbsoluteUri, httpResponseMessage.StatusCode);
                    return new Result
                    {
                        Success = false,
                        Error = $"HTTP error, status code: {httpResponseMessage.StatusCode}"
                    };
                }

                string text2 = httpResponseMessage.Content.Headers.ContentType?.MediaType ?? string.Empty;
                Debug.WriteLine("{0} content type: {1}", url2.AbsoluteUri, text2);
                if (text2.Contains("text/plain", StringComparison.OrdinalIgnoreCase))
                {
                    string text3 = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                    return new Result
                    {
                        Success = true,
                        Text = text3.Trim()
                    };
                }

                if (text2.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    string s = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(continueOnCapturedContext: false);
                    HtmlDocument doc = new HtmlDocument();
                    Stream stream = new MemoryStream(Encoding.UTF8.GetBytes(s));
                    {
                        var configuredAsyncDisposable = stream.ConfigureAwait(continueOnCapturedContext: false);
                        try
                        {
                            doc.Load(stream);
                        }
                        finally
                        {
                            //IAsyncDisposable asyncDisposable = configuredAsyncDisposable as IAsyncDisposable;
                            //if (asyncDisposable != null)
                            //{
                            //    await asyncDisposable.DisposeAsync();
                            //}
                        }
                    }

                    return new Result
                    {
                        Success = true,
                        Text = doc.DocumentNode.InnerText.Trim()
                    };
                }

                return new Result
                {
                    Success = false,
                    Error = "Invalid content type: " + text2
                };
            }
            finally
            {
                if (client != null)
                {
                    ((IDisposable)client).Dispose();
                }
            }
        }

        //private static ResiliencePipeline<HttpResponseMessage> RetryLogic()
        //{
        //    HttpStatusCode[] retriableErrors = new HttpStatusCode[4]
        //    {
        //    HttpStatusCode.RequestTimeout,
        //    HttpStatusCode.InternalServerError,
        //    HttpStatusCode.BadGateway,
        //    HttpStatusCode.GatewayTimeout
        //    };
        //    List<int> delays = new List<int> { 1, 1, 1, 2, 2, 3, 4, 5 };
        //    return new ResiliencePipelineBuilder<HttpResponseMessage>().AddRetry(new RetryStrategyOptions<HttpResponseMessage>
        //    {
        //        ShouldHandle = new PredicateBuilder<HttpResponseMessage>().HandleResult((HttpResponseMessage resp) => retriableErrors.Contains(resp.StatusCode)),
        //        MaxRetryAttempts = 10,
        //        DelayGenerator = delegate (RetryDelayGeneratorArguments<HttpResponseMessage> args)
        //        {
        //            double value = ((args.AttemptNumber < delays.Count) ? delays[args.AttemptNumber] : 5);
        //            return ValueTask.FromResult((TimeSpan?)TimeSpan.FromSeconds(value));
        //        }
        //    }).Build();
        //}
    }


    /// <summary>
    /// 指结构化的记忆信息
    /// </summary>
    [NavigationItem("记忆管理")]
    public class 记忆信息 : 记忆来源
    {
        public 记忆信息(Session s) : base(s)
        {
        }




        [RuleRequiredField]
        public string 标题
        {
            get { return GetPropertyValue<string>(nameof(标题)); }
            set { SetPropertyValue(nameof(标题), value); }
        }
        protected override bool ValidateFileExist => false;
        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (!IsLoading && propertyName == nameof(标题) && string.IsNullOrEmpty(FilePath))
            {
                //var def = Session.Query<系统配置>().FirstOrDefault();
                //if (def != null)
                //{
                   
                //}
                FilePath = Path.Combine(@"d:\RagServer\Source", 标题 + ".txt");
            }
        }

        [RuleRequiredField]
        [Size(-1)]
        public string 内容
        {
            get { return GetPropertyValue<string>(nameof(内容)); }
            set { SetPropertyValue(nameof(内容), value); }
        }

        public override void SaveToFile()
        {
            if (string.IsNullOrEmpty(FilePath))
            {
                throw new UserFriendlyException("没有填写文件路径!");
            }
            var dirName = Path.GetDirectoryName(FilePath);
            if (!Directory.Exists(dirName))
            {
                Directory.CreateDirectory(dirName);
            }

            File.WriteAllText(FilePath, 内容);
        }
    }

    [NavigationItem("记忆管理")]
    public class 网络内容 : 记忆信息
    {
        public 网络内容(Session s) : base(s)
        {

        }
        [RuleRequiredField]
        public string URL
        {
            get { return GetPropertyValue<string>(nameof(URL)); }
            set { SetPropertyValue(nameof(URL), value); }
        }

        [Action(Caption ="抓取内容")]
        public async Task 抓取内容()
        {
            if (string.IsNullOrEmpty(URL))
            {
                throw new UserFriendlyException("错误,没有填写网址!");
            }

            var ws = new WebScraper();
            var cnt = await ws.GetTextAsync(URL);

            if (cnt.Success)
                内容 = cnt.Text;
            else
                throw new UserFriendlyException(cnt.Error);


            if (!string.IsNullOrEmpty(FilePath))
            {
                SaveToFile();
            }
        }
    }

    [NonPersistent]
    public abstract class 记忆来源 : ISingleFile
    {
        public 记忆来源(Session s) : base(s)
        {

        }

        [Action(Caption = "保存文件")]
        public abstract void SaveToFile();

        [Action(Caption ="生成记忆记录")]
        public void CreateMemoryRecord()
        {
            SaveToFile();
            //var def = Session.Query<系统配置>().FirstOrDefault();

            记忆 = new 记忆(Session);
            记忆.源文件路径 = this.FilePath;

            记忆.记忆存储路径 = Path.Combine(@"d:\RagServer\Memory", Path.GetFileName(FilePath));
        }


        public 记忆 记忆
        {
            get { return GetPropertyValue<记忆>(nameof(记忆)); }
            set { SetPropertyValue(nameof(记忆), value); }
        }

    }

    /// <summary>
    /// 记忆是需要模糊搜索匹配的内容
    /// 记忆的来源只有文件
    /// 其他来源的内容需要先保存到文件再导入
    /// </summary>
    [NavigationItem("记忆管理")]
    public class 记忆 : XPObject
    {
        public 记忆(Session s) : base(s)
        {

        }

        [XafDisplayName("提示词")]
        public string Prompt
        {
            get { return GetPropertyValue<string>(nameof(Prompt)); }
            set { SetPropertyValue(nameof(Prompt), value); }
        }


        public string MimeType
        {
            get { return GetPropertyValue<string>(nameof(MimeType)); }
            set { SetPropertyValue(nameof(MimeType), value); }
        }
        static MimeTypesDetection detection = new MimeTypesDetection();
        protected override void OnSaving()
        {
            try
            {
                if (string.IsNullOrEmpty(MimeType) && !string.IsNullOrEmpty(this.源文件路径))
                {
                    MimeType = detection.GetFileType(this.源文件路径);
                }
            }
            catch (NotSupportedException ex)
            {

            }
            base.OnSaving();
        }
        public bool 提取完成
        {
            get { return GetPropertyValue<bool>(nameof(提取完成)); }
            set { SetPropertyValue(nameof(提取完成), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string 记忆存储路径
        {
            get { return GetPropertyValue<string>(nameof(记忆存储路径)); }
            set { SetPropertyValue(nameof(记忆存储路径), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string 源文件路径
        {
            get { return GetPropertyValue<string>(nameof(源文件路径)); }
            set { SetPropertyValue(nameof(源文件路径), value); }
        }

        public enum TextSplitMode
        {
            ByLength,
            ByLine
        }

        [XafDisplayName("分区方法")]
        public TextSplitMode SplitMode
        {
            get { return GetPropertyValue<TextSplitMode>(nameof(SplitMode)); }
            set { SetPropertyValue(nameof(SplitMode), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string 提取文本文件
        {
            get { return GetPropertyValue<string>(nameof(提取文本文件)); }
            set { SetPropertyValue(nameof(提取文本文件), value); }
        }

        [Size(-1)]
        [ModelDefault("RowCount", "0")]
        public string 提取JSON文件
        {
            get { return GetPropertyValue<string>(nameof(提取JSON文件)); }
            set { SetPropertyValue(nameof(提取JSON文件), value); }
        }


        //[Size(-1)]
        //[ModelDefault("RowCount", "0")]
        //public int 服务数量
        //{
        //    get { return GetPropertyValue<int>(nameof(服务数量)); }
        //    set { SetPropertyValue(nameof(服务数量), value); }
        //}

        //[Size(-1)]
        //[ModelDefault("RowCount", "0")]
        //public string 服务命令
        //{
        //    get { return GetPropertyValue<string>(nameof(服务命令)); }
        //    set { SetPropertyValue(nameof(服务命令), value); }
        //}

        [ModelDefault("AllowEdit", "False")]
        public int 分区总数
        {
            get { return GetPropertyValue<int>(nameof(分区总数)); }
            set { SetPropertyValue(nameof(分区总数), value); }
        }

        [ModelDefault("AllowEdit", "False")]
        public int 已分区数量
        {
            get { return GetPropertyValue<int>(nameof(已分区数量)); }
            set { SetPropertyValue(nameof(已分区数量), value); }
        }

        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<记忆分区> 分区
        {
            get => GetCollection<记忆分区>(nameof(分区));
        }
    }

    [NavigationItem("记忆管理")]
    [DefaultListViewOptions(true, NewItemRowPosition.None)]
    public class 记忆分区 : XPObject
    {
        public 记忆分区(Session s) : base(s)
        {

        }

        //MemoryRecord mr;
        //[VisibleInDetailView(false)]
        //[VisibleInListView(false)]
        //public MemoryRecord MemoryRecord
        //{
        //    get
        //    {
        //        if (mr == null)
        //        {
        //            if (!string.IsNullOrEmpty(EmbeddingFileName))
        //            {
        //                mr = new MemoryRecord();
        //                var file = Path.Combine(记忆.记忆存储路径, EmbeddingFileName);
        //                var json = Encoding.UTF8.GetString(File.ReadAllBytes(file));
        //                mr.Vector = new Microsoft.KernelMemory.Embedding(JsonConvert.DeserializeObject<float[]>(json));
        //            }
        //        }
        //        return mr;
        //    }
        //}


        [Association]
        public 记忆 记忆
        {
            get { return GetPropertyValue<记忆>(nameof(记忆)); }
            set { SetPropertyValue(nameof(记忆), value); }
        }

        [Size(-1)]
        public string 内容
        {
            get { return GetPropertyValue<string>(nameof(内容)); }
            set { SetPropertyValue(nameof(内容), value); }
        }

        [Size(-1)]
        public string EmbeddingFileName
        {
            get { return GetPropertyValue<string>(nameof(EmbeddingFileName)); }
            set { SetPropertyValue(nameof(EmbeddingFileName), value); }
        }

        public int TokenCount
        {
            get { return GetPropertyValue<int>(nameof(TokenCount)); }
            set { SetPropertyValue(nameof(TokenCount), value); }
        }
    }
}
