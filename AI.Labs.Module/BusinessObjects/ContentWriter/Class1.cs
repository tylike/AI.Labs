using DevExpress.ExpressApp.DC;
using DevExpress.Xpo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Pinyin4net;
using Pinyin4net.Format;
using DevExpress.Data.Helpers;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Pdf.Native;

namespace AI.Labs.Module.BusinessObjects.ContentWriter
{
    [NavigationItem("写作词库")]
    public class 批量任务 : SimpleXPObject
    {
        public 批量任务(Session s) : base(s)
        {

        }

        public 任务 任务
        {
            get { return GetPropertyValue<任务>(nameof(任务)); }
            set { SetPropertyValue(nameof(任务), value); }
        }

        [Association]
        public XPCollection<WordItem> 词汇
        {
            get { return GetCollection<WordItem>(nameof(词汇)); }
        }
    }

    [NavigationItem("写作词库")]
    [XafDefaultProperty(nameof(TaskName))]
    public class 任务 : SimpleXPObject
    {
        public 任务(Session s) : base(s)
        {

        }

        [XafDisplayName("任务名称")]
        public string TaskName
        {
            get { return GetPropertyValue<string>(nameof(TaskName)); }
            set { SetPropertyValue(nameof(TaskName), value); }
        }

        public enum 输出位置
        {
            原记录,
            新记录,
            查找记录
        }
        [XafDisplayName("输出位置")]
        public 输出位置 OutputLocation
        {
            get { return GetPropertyValue<输出位置>(nameof(OutputLocation)); }
            set { SetPropertyValue(nameof(OutputLocation), value); }
        }

        public string 输出属性名称
        {
            get { return GetPropertyValue<string>(nameof(输出属性名称)); }
            set { SetPropertyValue(nameof(输出属性名称), value); }
        }

        public bool 追加内容
        {
            get { return GetPropertyValue<bool>(nameof(追加内容)); }
            set { SetPropertyValue(nameof(追加内容), value); }
        }


        [XafDisplayName("系统提示")]
        [Size(-1)]
        public string SystemPrompt
        {
            get { return GetPropertyValue<string>(nameof(SystemPrompt)); }
            set { SetPropertyValue(nameof(SystemPrompt), value); }
        }

        [XafDisplayName("用户提示")]
        [Size(-1)]
        public string UserPrompt
        {
            get { return GetPropertyValue<string>(nameof(UserPrompt)); }
            set { SetPropertyValue(nameof(UserPrompt), value); }
        }


    }

    public class WordItemViewController : ObjectViewController<ObjectView, WordItem>
    {
        public WordItemViewController()
        {
            TargetViewNesting = Nesting.Nested;
            var executeTask = new SimpleAction(this, "执行任务", null);
            executeTask.Execute += ExecuteTask_Execute;

            var quickSetCategory = new SimpleAction(this, "快速分类", null);
            quickSetCategory.Execute += QuickSetCategory_Execute;
        }

        private void QuickSetCategory_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var first = e.SelectedObjects.OfType<WordItem>().FirstOrDefault(t => t.分类 != null);
            foreach (WordItem item in e.SelectedObjects)
            {
                if (item.分类 == null)
                {
                    item.分类 = first.分类;
                }
            }
        }

        private async void ExecuteTask_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var mtask = (Frame as NestedFrame).ViewItem.CurrentObject as 批量任务;
            var p = mtask.任务.UserPrompt;

            foreach (WordItem item in e.SelectedObjects)
            {
                if (mtask.任务.OutputLocation == 任务.输出位置.原记录)
                {
                    if (!mtask.任务.追加内容)
                    {
                        item.SetMemberValue(mtask.任务.输出属性名称, "");
                    }
                }

                var userPrompt = p.Replace("{Words}", item.Words);

                var rst = (string)item.GetMemberValue(mtask.任务.输出属性名称);

                await AIHelper.Ask(userPrompt, p =>
                {
                    rst += p.Content;
                    item.SetMemberValue(mtask.任务.输出属性名称, rst);
                }, systemPrompt: mtask.任务.SystemPrompt);
            }
        }
    }

    [NavigationItem("写作词库")]
    [XafDefaultProperty(nameof(分类名称))]
    public class 写作方法分类 : SimpleXPObject
    {
        public 写作方法分类(Session s) : base(s)
        {

        }

        public string 作用范围
        {
            get { return GetPropertyValue<string>(nameof(作用范围)); }
            set { SetPropertyValue(nameof(作用范围), value); }
        }

        public string 分类名称
        {
            get { return GetPropertyValue<string>(nameof(分类名称)); }
            set { SetPropertyValue(nameof(分类名称), value); }
        }
    }


    /// <summary>
    /// 词汇项目
    /// </summary>
    [NavigationItem("写作词库")]
    public class WordItem : SimpleXPObject
    {
        public WordItem(Session s) : base(s)
        {

        }

        public 写作方法分类 分类
        {
            get { return GetPropertyValue<写作方法分类>(nameof(分类)); }
            set { SetPropertyValue(nameof(分类), value); }
        }

        [Association]
        public 批量任务 任务
        {
            get { return GetPropertyValue<批量任务>(nameof(任务)); }
            set { SetPropertyValue(nameof(任务), value); }
        }


        /// <summary>
        /// 歇后语，俏皮话，谚语，成语等，用于生成文本
        /// 成语改编，谚语改编，俏皮话改编
        /// </summary>
        [XafDisplayName("词汇")]
        [ToolTip("占位符:{Words}")]
        public string Words
        {
            get { return GetPropertyValue<string>(nameof(Words)); }
            set { SetPropertyValue(nameof(Words), value); }
        }
        public int 长度
        {
            get
            {
                return ("" + Words).Length;
            }
        }
        [Size(-1)]
        public string 用法
        {
            get { return GetPropertyValue<string>(nameof(用法)); }
            set { SetPropertyValue(nameof(用法), value); }
        }

        public string 前言
        {
            get { return GetPropertyValue<string>(nameof(前言)); }
            set { SetPropertyValue(nameof(前言), value); }
        }

        public string 后语
        {
            get { return GetPropertyValue<string>(nameof(后语)); }
            set { SetPropertyValue(nameof(后语), value); }
        }


        public bool 成语
        {
            get { return GetPropertyValue<bool>(nameof(成语)); }
            set { SetPropertyValue(nameof(成语), value); }
        }


        [Size(-1)]
        public string 同义词
        {
            get { return GetPropertyValue<string>(nameof(同义词)); }
            set { SetPropertyValue(nameof(同义词), value); }
        }

        [ToolTip("让AI用同样的方法,发挥写出的结果")]
        [Size(-1)]
        public string 发挥
        {
            get { return GetPropertyValue<string>(nameof(发挥)); }
            set { SetPropertyValue(nameof(发挥), value); }
        }


        protected override void OnSaving()
        {
            base.OnSaving();
            if (string.IsNullOrEmpty(同义词) && 成语)
            {
                同义词 = string.Join(Environment.NewLine, PinyinHelper.FindHomophone(Words, true).Select(t => string.Join(",", t)));
            }
        }

        //[Action]
        //public void DoTest()
        //{
        //    //PY.Main();
        //    var rst = PinyinHelper.FindHomophone("三长两短",true);
        //}

        [Action(AutoCommit = true)]
        public void Import()
        {
            var lines = File.ReadAllLines("C:\\Users\\46035\\Desktop\\小约翰.txt");
            foreach (var item in lines)
            {
                var n = new WordItem(Session);
                n.Words = item;
            }

        }

        [Action(AutoCommit = true)]
        public async void 生成解释()
        {
            同义词 = "";
            await AIHelper.Ask("用一个词表达这句话:\n" + Words, p =>
            {
                同义词 += p.Content;
            },
            systemPrompt: "你是一个实用助手,根据用户要求回答问题",
            uiContext: SynchronizationContext.Current
            );
        }
    }


    class PY
    {
        // 预定义的拼音到汉字的映射，包括声调
        static readonly Dictionary<string, List<string>> pinyinToneToHanzi = new Dictionary<string, List<string>>
    {
        { "san1", new List<string> { "三", "叁" } },
        { "zhang1", new List<string> { "张", "章" } },
        { "chang2", new List<string> { "长", "常" } },
        { "duan3", new List<string> { "短", "端" } },
        // 可以继续添加更多映射
    };

        // 拼音声调符号到数字的映射
        static readonly Dictionary<char, int> toneMarksToNumbers = new Dictionary<char, int>
    {
        { 'ā', 1 }, { 'á', 2 }, { 'ǎ', 3 }, { 'à', 4 },
        { 'ē', 1 }, { 'é', 2 }, { 'ě', 3 }, { 'è', 4 },
        { 'ī', 1 }, { 'í', 2 }, { 'ǐ', 3 }, { 'ì', 4 },
        { 'ō', 1 }, { 'ó', 2 }, { 'ǒ', 3 }, { 'ò', 4 },
        { 'ū', 1 }, { 'ú', 2 }, { 'ǔ', 3 }, { 'ù', 4 },
        { 'ǖ', 1 }, { 'ǘ', 2 }, { 'ǚ', 3 }, { 'ǜ', 4 }
    };

        static int GetToneNumber(string pinyin)
        {
            foreach (var kvp in toneMarksToNumbers)
            {
                if (pinyin.Contains(kvp.Key))
                {
                    return kvp.Value;
                }
            }
            // 默认声调为 5（无声调）
            return 5;
        }

        static string NormalizePinyinWithToneNumber(string pinyin)
        {
            int toneNumber = GetToneNumber(pinyin);
            string normalized = new string(pinyin.Where(c => !toneMarksToNumbers.ContainsKey(c)).ToArray());
            return $"{normalized}{toneNumber}";
        }

        public static void Main()
        {
            // 输入成语
            string chengyu = "三长两短";

            // 使用 Pinyin4Net 获取拼音，带声调

            var outputFormat = new HanyuPinyinOutputFormat();
            //{
            //    ToneType = HanyuPinyinToneType.WITH_TONE_MARK, // 使用标记声调
            //    CaseType = HanyuPinyinCaseType.LOWERCASE,
            //    //VCharType = HanyuPinyinVCharType.WITH_V
            //};

            var normalizedPinyins = PinyinHelper.ToHanyuPinyinStringArray('三', outputFormat);

            //var normalizedPinyins = chengyu.Select(
            //    t=> PinyinHelper.ToHanyuPinyinStringArray(t, outputFormat ) 
            //    ).ToList();

            //Console.WriteLine($"成语：{chengyu}");
            //Console.WriteLine("同音字：");

            //for (int i = 0; i < normalizedPinyins.Count; i++)
            //{
            //    string pinyin = normalizedPinyins[i];
            //    if (pinyinToneToHanzi.TryGetValue(pinyin, out List<string> homophones))
            //    {
            //        Console.WriteLine($"{chengyu[i]} ({pinyin}): {string.Join(", ", homophones)}");
            //    }
            //    else
            //    {
            //        Console.WriteLine($"{chengyu[i]} ({pinyin}): 无同音字");
            //    }
            //}
        }
    }


}
