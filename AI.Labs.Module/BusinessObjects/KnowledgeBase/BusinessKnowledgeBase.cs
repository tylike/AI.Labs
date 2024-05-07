using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Diagnostics;
using System.Text;
using System.Numerics.Tensors;
using DevExpress.Persistent.Base.General;
using System.ComponentModel;
using DevExpress.Services;
using DevExpress.ExpressApp.ConditionalAppearance;
using System.Reflection.Metadata.Ecma335;
using DevExpress.ExpressApp.Editors;
using DevExpress.XtraSpreadsheet.Commands;

namespace AI.Labs.Module.BusinessObjects.KnowledgeBase
{

    public enum KnowledgeNodeSpliter
    {
        [XafDisplayName("==========")]
        EqualsX10,
        [XafDisplayName("换行回车")]
        Crlf,
        [XafDisplayName("自定义")]
        Customize = 1000
    }

    [NavigationItem("应用场景")]
    [XafDisplayName("记忆内容")]
    [Appearance("无关键词的", TargetItems = "Keyword", Criteria = "Keyword is null or Keyword=''", BackColor = "Red")]
    public class BusinessKnowledgeBase : XPObject, ITreeNode
    {
        public BusinessKnowledgeBase(Session s) : base(s)
        {
        }

        [NonPersistent]
        public int Value
        {
            get { return GetPropertyValue<int>(nameof(Value)); }
            set { SetPropertyValue(nameof(Value), value); }
        }

        [NonPersistent]
        public string ValueNote
        {
            get { return GetPropertyValue<string>(nameof(ValueNote)); }
            set { SetPropertyValue(nameof(ValueNote), value); }
        }



        [Size(-1)]
        //[EditorAlias(EditorAliases.RichTextPropertyEditor)]
        public string Question
        {
            get { return GetPropertyValue<string>(nameof(Question)); }
            set { SetPropertyValue(nameof(Question), value); }
        }


        [XafDisplayName("分隔类型")]
        public KnowledgeNodeSpliter SplitType
        {
            get { return GetPropertyValue<KnowledgeNodeSpliter>(nameof(SplitType)); }
            set { SetPropertyValue(nameof(SplitType), value); }
        }
        [XafDisplayName("自定分隔")]
        public string CustomSpliter
        {
            get { return GetPropertyValue<string>(nameof(CustomSpliter)); }
            set { SetPropertyValue(nameof(CustomSpliter), value); }
        }


        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }


        [Size(-1)]
        public string Summary
        {
            get { return GetPropertyValue<string>(nameof(Summary)); }
            set { SetPropertyValue(nameof(Summary), value); }
        }
        [Size(-1)]
        public string Keyword
        {
            get { return GetPropertyValue<string>(nameof(Keyword)); }
            set { SetPropertyValue(nameof(Keyword), value); }
        }

        [Association]
        public XPCollection<Word> KeyWords => GetCollection<Word>(nameof(KeyWords));

        [Size(-1)]
        public string Response
        {
            get { return GetPropertyValue<string>(nameof(Response)); }
            set { SetPropertyValue(nameof(Response), value); }
        }


        public int Length => (Text + "").Length;

        [Size(-1)]
        public string Text
        {
            get { return GetPropertyValue<string>(nameof(Text)); }
            set { SetPropertyValue(nameof(Text), value); }
        }

        public int Index
        {
            get { return GetPropertyValue<int>(nameof(Index)); }
            set { SetPropertyValue(nameof(Index), value); }
        }

        string ITreeNode.Name => this.Title;

        [Association]
        public BusinessKnowledgeBase Parent
        {
            get { return GetPropertyValue<BusinessKnowledgeBase>(nameof(Parent)); }
            set { SetPropertyValue(nameof(Parent), value); }
        }

        [Association]
        public XPCollection<BusinessKnowledgeBase> Nodes { get => GetCollection<BusinessKnowledgeBase>(nameof(Nodes)); }

        ITreeNode ITreeNode.Parent => this.Parent;

        IBindingList ITreeNode.Children => Nodes;
    }

    [XafDisplayName("词")]
    public class Word : XPObject
    {
        public Word(Session s) : base(s)
        {

        }

        [Association]
        public BusinessKnowledgeBase KeywordOnwer
        {
            get { return GetPropertyValue<BusinessKnowledgeBase>(nameof(KeywordOnwer)); }
            set { SetPropertyValue(nameof(KeywordOnwer), value); }
        }


        public string Text
        {
            get { return GetPropertyValue<string>(nameof(Text)); }
            set { SetPropertyValue(nameof(Text), value); }
        }

        [Association("Synonyms")]
        public XPCollection<Word> Synonyms
        {
            get
            {
                return GetCollection<Word>(nameof(Synonyms));
            }
        }

        [Association("Synonyms")]
        public XPCollection<Word> SynonymOf
        {
            get
            {
                return GetCollection<Word>(nameof(SynonymOf));
            }
        }

        [Association("Relation")]
        public XPCollection<Word> Relations { get => GetCollection<Word>(nameof(Relations)); }
        [Association("Relation")]
        public XPCollection<Word> RelationOf { get => GetCollection<Word>(nameof(RelationOf)); }

    }

    [NavigationItem("对话知识库")]
    public class ChatKnowledge : XPObject
    {
        public ChatKnowledge(Session s) : base(s)
        {

        }

        [Size(-1)]
        public string Question
        {
            get { return GetPropertyValue<string>(nameof(Question)); }
            set { SetPropertyValue(nameof(Question), value); }
        }


        public string QuestionKeywords
        {
            get { return GetPropertyValue<string>(nameof(QuestionKeywords)); }
            set { SetPropertyValue(nameof(QuestionKeywords), value); }
        }

        public string[] QuestionKeywordList { get; set; }

        public async Task GenerateKeyword(XafApplication app)
        {
            //1.提取用户关键词
            //var rst = WordProcesser.GetWords(this.Question); //await GetKeywordsFromAI(app);
            //QuestionKeywords = string.Join(" ", rst);
            //QuestionKeywordList = rst.Select(t=>t.ToLower()).ToArray(); //QuestionKeywords.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower()).Distinct().ToArray();
        }

        private async Task<StringBuilder> GetKeywordsFromAI(XafApplication app,AIModel model)
        {
            var systemPrompt =
                $@"#要求:根据用户提供的文字内容,提取出所有名词,包含但不限于人名、地名、物品、等所有.不要说明,逗号分隔多个词
重要提示:如果用户提供的文字内容中包含了关键词,请在关键词前面加上A:前缀,例如:
#示例:
Q:要整理的文字内容:太阳当空照,我去上学校
A:太阳,学校
Q:要整理的文字内容:合作是一次成功的营销策略
A:合作,营销策略
";

            #region 提取用户问题中的关键词
            var keywords = new StringBuilder();
            var processContent = $"#要整理的文字内容:\n{Question}";
            Debug.WriteLine("---------------------------------------------------------------------------");
            var rst = new StringBuilder();
            //item.Title = "";
            Debug.WriteLine("输出:");
            await AIHelper.Ask(
                systemPrompt,
                processContent,
                t =>
                {
                    rst.Append(t.Content);
                    Debug.Write(t.Content);
                    QuestionKeywords = rst.ToString();
                    app.UIThreadDoEvents();

                },model
                );
            Debug.WriteLine("\t输出完成!");
            #endregion
            return rst;
        }

        public async Task CalcScore()
        {
            var ks = Session.Query<BusinessKnowledgeBase>();
            //比较所有关键词与用户关键词：
            foreach (var dbKnowledgeBaseItem in ks)
            {
                if (!string.IsNullOrEmpty(dbKnowledgeBaseItem.Keyword))
                {
                    var scoreItem = new ScoreItem(Session) { KnowledgeBaseItem = dbKnowledgeBaseItem, ChatKnowledge = this };
                    this.ScoreItems.Add(scoreItem);
                    scoreItem.CalcScore();
                }
            }
            await Task.CompletedTask;
        }
        [Size(-1)]
        public string Answer
        {
            get { return GetPropertyValue<string>(nameof(Answer)); }
            set { SetPropertyValue(nameof(Answer), value); }
        }

        [Association]
        public XPCollection<ScoreItem> ScoreItems { get => GetCollection<ScoreItem>(nameof(ScoreItems)); }

    }


    public class ScoreItem : XPObject
    {
        public ScoreItem(Session s) : base(s)
        {

        }

        [Association]
        public ChatKnowledge ChatKnowledge
        {
            get { return GetPropertyValue<ChatKnowledge>(nameof(ChatKnowledge)); }
            set { SetPropertyValue(nameof(ChatKnowledge), value); }
        }


        public BusinessKnowledgeBase KnowledgeBaseItem
        {
            get { return GetPropertyValue<BusinessKnowledgeBase>(nameof(KnowledgeBaseItem)); }
            set { SetPropertyValue(nameof(KnowledgeBaseItem), value); }
        }

        public IEnumerable<string> KnowledgeKeywords 
        {
            get => //WordProcesser.GetWords(KnowledgeBaseItem.Text).Select(t=>t.Trim().ToLower()).Distinct();
                KnowledgeBaseItem.Keyword.Split(",", StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim().ToLower()).Distinct(); 
        }


        public decimal Score
        {
            get { return GetPropertyValue<decimal>(nameof(Score)); }
            set { SetPropertyValue(nameof(Score), value); }
        }

        [Size(-1)]
        public string Note
        {
            get { return GetPropertyValue<string>(nameof(Note)); }
            set { SetPropertyValue(nameof(Note), value); }
        }

        public void CalcScore()
        {
            var userKeywords = ChatKnowledge.QuestionKeywordList;
            var knowledgeKeywords = KnowledgeKeywords;
            foreach (var item in userKeywords)
            {
                foreach (var dbKeyword in knowledgeKeywords)
                {
                    if (item == dbKeyword)
                    {
                        Score += 100;
                        Note += $"用户关键词:{item}与知识关键词:{dbKeyword}匹配,加分100\n\r\n";
                    }
                    else if (item.IndexOf(dbKeyword) >= 0)
                    {
                        Score += 10;
                        Note += $"用户关键词:{item}包含知识关键词:{dbKeyword},加分60\n\r\n";
                    }
                    else if (dbKeyword.IndexOf(item) >= 0)
                    {
                        Score += 10;
                        Note += $"知识关键词:{dbKeyword}包含用户关键词:{item},加分60\n\r\n";
                    }
                }
            }
        }
    }

    public class ChatKnowledgeBaseViewController : ObjectViewController<DetailView, ChatKnowledge>
    {
        public ChatKnowledgeBaseViewController()
        {
            var preProcess = new SimpleAction(this, "PreProcess", null);
            preProcess.Execute += PreProcess_Execute;
            var query = new SimpleAction(this, "Query", null);
            query.Execute += Query_Execute;
        }

        private async void PreProcess_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewCurrentObject.Question))
                throw new UserFriendlyException("没有输入问题");

            var chatlog = ViewCurrentObject;

            await chatlog.GenerateKeyword(Application);
            ObjectSpace.Delete(chatlog.ScoreItems);
            await chatlog.CalcScore();
        }

        private async void Query_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //var chatlog = ViewCurrentObject;
            //chatlog.Answer = "";

            //var refs = chatlog.ScoreItems.Where(t => t.Score > 0).OrderByDescending(t => t.Score).Take(10);
            //var systemPrompts = new StringBuilder();
            //foreach (var item in refs)
            //{
            //    Debug.WriteLine("参考:");
            //    Debug.WriteLine(item.KnowledgeBaseItem.Text);
            //    systemPrompts.AppendLine($"#参考内容[{item.KnowledgeBaseItem.Oid}-{item.Score}]:{item.KnowledgeBaseItem.Text}");
            //}

            //await AIHelper.Ask(
            //    systemPrompts.ToString(),
            //    $"用户提问:{ViewCurrentObject.Question}",
            //    (item) =>
            //    {
            //        chatlog.Answer += item.Content;
            //        Application.UIThreadDoEvents();
            //    }
            //    );

            ////2.使用用户关键词去查找已经存在的内容
        }
    }
}
