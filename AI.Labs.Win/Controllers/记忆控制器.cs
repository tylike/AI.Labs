using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using RagServer.Module.BusinessObjects;
using System.IO;

namespace AI.Labs.Win.Controllers
{
    public class 记忆控制器 : ObjectViewController<ObjectView, 记忆>
    {
        public 记忆控制器()
        {
            var 创建知识 = new SimpleAction(this, "创建知识", null);
            创建知识.Caption = "创建知识";
            创建知识.ToolTip = "选择一个或多个文件,分段";
            创建知识.Execute += 创建知识_Execute;

            //var importMemory = new SimpleAction(this, "导入记忆", PredefinedCategory.Edit);
            //importMemory.Execute += ImportMemory_Execute;
            //var createSummary = new SimpleAction(this, "创建总结", null);
            //createSummary.Execute += CreateSummary_Execute;
            //var testembedding = new SimpleAction(this, "TestEmbedding", null);
            //testembedding.Execute += Testembedding_Execute;
            //var clearMemory = new SimpleAction(this, "ClearMemory", null);
            //clearMemory.Execute += ClearMemory_Execute;

            ////var startEmbeddingServer = new SimpleAction(this, "开始嵌入服务", null);
            ////startEmbeddingServer.Execute += StartEmbeddingServer_Execute;
            ////var stopEmbeddingServer = new SimpleAction(this, "停止嵌入服务", null);
            ////stopEmbeddingServer.Execute += StopEmbeddingServer_Execute;

            //var updateEmbedding = new SimpleAction(this, "更新集合", null);
            //updateEmbedding.Execute += UpdateEmbedding_Execute;

            //var testQdrant = new SimpleAction(this, "TestQdrant", null);
            //testQdrant.Execute += TestQdrant_Execute;

            //var extractText = new SimpleAction(this, "ExtractText", null);
            //extractText.Caption = "提取文本";
            //extractText.Execute += ExtractText_Execute;
            //var splitText = new SimpleAction(this, "SplitTextFile", null);
            //splitText.Caption = "拆分文本";
            //splitText.Execute += SplitText_Execute;

            //var generateEmbedding = new SimpleAction(this, "GenerateEmbedding", null);
            //generateEmbedding.Caption = "生成嵌入";
            //generateEmbedding.Execute += GenerateEmbedding_Execute;
            //var getWebpage = new SimpleAction(this, "GetWebPageContent", null);
            //getWebpage.Caption = "抓取网页";
            //getWebpage.Execute += GetWebpage_Execute;
        }


        private void 创建知识_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ofd.FileNames.ToList().ForEach(t =>
                {
                    var memory = ObjectSpace.CreateObject<记忆>();
                    memory.源文件路径 = t;
                    memory.提取完成 = false;
                    var lines = File.ReadAllLines(t);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrEmpty(line.Trim()))
                        {
                            var paragraph = ObjectSpace.CreateObject<记忆分区>();
                            paragraph.内容 = line;
                            memory.分区.Add(paragraph);
                        }
                    }
                    ObjectSpace.CommitChanges();
                });
            }
            
        }

        //TextPartitioningOptions textPartitioning;
        //TextPartitioningOptions TextPartitioningOptions
        //{
        //    get
        //    {
        //        if(textPartitioning == null)
        //        {
        //            textPartitioning = new TextPartitioningOptions
        //            {
        //                MaxTokensPerParagraph = 1000,
        //                MaxTokensPerLine = 1000,
        //                OverlappingTokens = 20,
        //            };
        //        }
        //        return textPartitioning;
        //    }
        //}

        private async void SplitText_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //var records = e.SelectedObjects.OfType<记忆>().Where(t=>t.提取完成);
            //if (!records.Any())
            //{
            //    throw new UserFriendlyException("没有“提取完成”的记录!应先进行提取操作!");
            //}

            //var split = new 文本拆分(Server.EmbeddingGenerator,ObjectSpace,TextPartitioningOptions);
            //var rst = await split.InvokeAsync(records);
            //Application.ShowViewStrategy.ShowMessage(rst ? $"拆分文本成功{records.Count()}!" : "拆分文本失败!");
            //ObjectSpace.CommitChanges();
        }

        private async void ExtractText_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //var extract = new 文本提取();
            //var rst = await extract.InvokeAsync(e.SelectedObjects.OfType<记忆>());
            //Application.ShowViewStrategy.ShowMessage(rst ? "提取文本成功!" : "提取文本失败!");
            //ObjectSpace.CommitChanges();
        }
    }
}
