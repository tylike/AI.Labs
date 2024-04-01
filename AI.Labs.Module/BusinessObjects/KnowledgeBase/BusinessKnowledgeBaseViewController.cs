using AI.Labs.Module.Controllers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using Microsoft.SemanticKernel.Memory;
using System.Diagnostics;
using System.Text;

namespace AI.Labs.Module.BusinessObjects.KnowledgeBase
{
    public class BusinessKnowledgeBaseViewController : ObjectViewController<ObjectView, BusinessKnowledgeBase>
    {
        public BusinessKnowledgeBaseViewController()
        {
            var loadMemory = new SimpleAction(this, "LoadKnowledgeBase", null);
            loadMemory.Execute += LoadMemory_Execute;
            var query = new ParametrizedAction(this, "QueryMemory", null, typeof(string));
            query.Execute += Query_Execute;

            var initData = new SimpleAction(this, "InitializeKnowledgeData", null);
            initData.Execute += InitData_Execute;

            var summary = new SimpleAction(this, "SummaryKnowledgeBase", null);
            summary.Execute += Summary_Execute;
            var title = new SimpleAction(this, "TitleKnowledgeBase", null);
            var keyword = new SimpleAction(this, "KeywordKnowledgeBase", null);

            var splitNode = new SimpleAction(this, "SplitKnowledgeBaseNode", null);
            splitNode.Execute += SplitNode_Execute;

            var 总结标题 = new SimpleAction(this, "总结标题", null);
            总结标题.Execute += 总结标题_Execute;

            var 总结重点 = new SimpleAction(this, "总结重点", null);
            总结重点.Execute += 总结重点_Execute;

            var 总结关键词 = new SimpleAction(this, "总结关键词", null);
            总结关键词.Execute += 总结关键词_Execute;

            var 修复关键词 = new SimpleAction(this, "修复关键词", null);
            修复关键词.Execute += 修复关键词_Execute;
        }

        private void 修复关键词_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var keywords = ObjectSpace.GetObjectsQuery<BusinessKnowledgeBase>().ToList();
            foreach (var item in keywords)
            {
                if (!string.IsNullOrEmpty(item.Keyword))
                {
                    if (item.Keyword.ToUpper().Trim().StartsWith("A:"))
                        item.Keyword = item.Keyword.Trim()[2..].Replace("、", ",").Trim();
                    item.Keyword = item.Keyword.Trim(':').Trim();
                    item.Keyword = string.Join(",", item.Keyword.Split(',', StringSplitOptions.RemoveEmptyEntries).Distinct());
                }

            }
            ObjectSpace.CommitChanges();
        }

        private async void 总结关键词_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (BusinessKnowledgeBase item in e.SelectedObjects)
            {
                item.Keyword = string.Join(",", WordProcesser.GetWords(item.Text).Distinct());
                Application.UIThreadDoEvents();
            }
            ObjectSpace.CommitChanges();
            return;
            var act = sender as SimpleAction;

            var systemPrompt =
                $@"#要求:根据用户提供的文字内容,提取出所有名词,包含但不限于人名、地名、物品、等所有.不要说明,逗号分隔多个词
#示例:
Q:要整理的文字内容:太阳当空照,我去上学校
A:太阳,学校
";
            var all = e.SelectedObjects.OfType<BusinessKnowledgeBase>().Where(t => t.Nodes.Count == 0);
            var idx = 0;
            var last = all.FirstOrDefault();
            last.Keyword = "";
            await AskAI(systemPrompt,
                t => $"#用户输入:\nQ:{t.Text}",
                all,
                (item, rst) =>
                {
                    if (last != item)
                    {
                        idx++;
                        act.Caption = $"总结中:{idx}/{all.Count()}";
                        last = item;
                        last.Keyword = "";
                    }
                    item.Keyword += rst;
                }
                );
            act.Caption = "总结关键词";
        }

        private async void 总结重点_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var systemPrompt = $@"#要求:根据用户提供的文字内容,整理出重点、核心、中心内容.";
            //
            await AskAI(systemPrompt,
                t => $"#要整理的文字内容:\n{t.Text}\n这段文字的重点内容是:",
                e.SelectedObjects.OfType<BusinessKnowledgeBase>(),
                (item, rst) =>
                {
                    item.Summary += rst;
                }
                );
        }

        private async void 总结标题_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var systemPrompt = $@"#要求:根据用户提供的文字内容,整理出标题,标题应尽量表达文章中的重点、核心、中心内容.";
            //
            await AskAI(systemPrompt,
                t => $"#要整理的文字内容:\n{t.Text}\n这段文字的标题是:",
                e.SelectedObjects.OfType<BusinessKnowledgeBase>(),
                (item, rst) =>
                {
                    item.Title += rst;
                }
                );
        }

        private async Task AskAI(string systemPrompt, Func<BusinessKnowledgeBase, string> userPrompt, IEnumerable<BusinessKnowledgeBase> items, Action<BusinessKnowledgeBase, string> processAction)
        {
            foreach (KnowledgeBase.BusinessKnowledgeBase item in items)
            {
                Debug.WriteLine("---------------------------------------------------------------------------");
                var rst = new StringBuilder();
                //item.Title = "";
                Debug.WriteLine("输出:");
                await AIHelper.Ask(
                    systemPrompt,
                    userPrompt(item),
                    t =>
                    {
                        rst.Append(t.Content);
                        //item.Title += t.Content;
                        processAction(item, t.Content);
                        Debug.Write(t.Content);
                        Application.UIThreadDoEvents();
                    }
                    );
                Debug.WriteLine("\t输出完成!");
                ObjectSpace.CommitChanges();
                Application.UIThreadDoEvents();
            }
        }

        private void SplitNode_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var spliter = "";
            switch (ViewCurrentObject.SplitType)
            {
                case KnowledgeNodeSpliter.EqualsX10:
                    spliter = "==========";
                    break;
                case KnowledgeNodeSpliter.Crlf:
                    spliter = "\n";
                    break;
                case KnowledgeNodeSpliter.Customize:
                    spliter = ViewCurrentObject.CustomSpliter;
                    break;
                default:
                    throw new UserFriendlyException("错误,未处理的分隔类型!");
            }
            var txts = ViewCurrentObject.Text.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in txts)
            {
                var t = item.Trim();
                if (!string.IsNullOrEmpty(t))
                {
                    var n = ObjectSpace.CreateObject<BusinessKnowledgeBase>();
                    ViewCurrentObject.Nodes.Add(n);
                    n.Text = t;
                }
            }
        }

        private async void Summary_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            foreach (KnowledgeBase.BusinessKnowledgeBase item in e.SelectedObjects)
            {
                Debug.WriteLine("---------------------------------------------------------------------------");

                var systemPrompt = $@"#要求:根据用户提供的文字内容,整理出标题、内容总结、关键词.
#返回结果格式(严格按照以下格返回结果):
标题:对于这段文字合适的标题
总结:内容总结,在此行中描写完成,不要换行。
关键词:关键词1,关键词2,....关键词n
assistant:ok!
";
                var rst = new StringBuilder();
                item.Response = "";
                await AIHelper.Ask(
                    systemPrompt,
                    $"#要整理的文字内容:\n{item.Text}",
                    t =>
                    {
                        rst.Append(t.Content);
                        item.Response += t.Content;
                        Debug.WriteLine(t);
                        Application.UIThreadDoEvents();
                    }
                    );

                var f = rst.ToString().Split('\n');
                item.Title = f.FirstOrDefault();
                item.Summary = f.Skip(1).FirstOrDefault();
                item.Keyword = f.Skip(2).FirstOrDefault();
                item.Response = rst.ToString();
                ObjectSpace.CommitChanges();
                Application.UIThreadDoEvents();

            }
        }

        private void InitData_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var txt = System.IO.File.ReadAllText("d:\\ltxz.txt");
            var ps = txt.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int idx = 0;
            foreach (var item in ps)
            {
                if (item.Trim().Length > 0)
                {
                    var p = ObjectSpace.CreateObject<BusinessKnowledgeBase>();
                    p.Text = item;
                    p.Index = idx;
                    idx++;
                }
            }
            ObjectSpace.CommitChanges();
        }

        private async void Query_Execute(object sender, ParametrizedActionExecuteEventArgs e)
        {
            var key = (e.ParameterCurrentValue + "").ToString();
            if (!string.IsNullOrEmpty(key))
            {
                var t = SemanticKernelMemory.QueryMemory(key);
                var rst = new List<MemoryQueryResult>();
                await foreach (var item in t)
                {
                    rst.Add(item);
                }
                throw new UserFriendlyException(string.Join("\n", rst.OrderByDescending(t => t.Relevance).Select(t => $"[{t.Relevance}]-{t.Metadata.Id}:{t.Metadata.Text}")));
                //Debug.Write(rst);
            }
        }

        private void LoadMemory_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = e.SelectedObjects.OfType<BusinessKnowledgeBase>().ToDictionary(k => k.Oid.ToString(), k => k.Text);
            SemanticKernelMemory.LoadMemory(t);
            Application.ShowViewStrategy.ShowMessage("记忆加载完成!");
        }
    }
}
