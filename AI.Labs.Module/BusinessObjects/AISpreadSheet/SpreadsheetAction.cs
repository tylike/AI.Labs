using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects.AISpreadSheet
{
    /// <summary>
    /// 用于定义批量执行任务
    /// 输入:选中的一个或一列或一行,或多列、多行数据
    /// 输出:根据提示词,llm将进行处理,并将结果输出到指定位置
    /// 指定位置的规则:
    /// 如输入是A1:A10，即选中10个单元格
    /// 结果偏移数量:1，即将在B1:B10输出结果
    /// 选中的单元格必须为一行或一列。
    /// 如果是行，则在 输入行数+偏移行数
    /// 如果是列，则在 输入列数+偏移列数
    /// 输出结果
    /// </summary>
    [XafDisplayName("动作定义")]
    [NavigationItem("Excel")]
    public class SpreadsheetAction : SimpleXPObject
    {
        public SpreadsheetAction(Session s) : base(s)
        {

        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            ImageName = "ModelEditor_GenerateContent";
            UserPrompt = "内容:{T}";
        }

        [XafDisplayName("快捷方式")]
        [ToolTip("快捷键")]
        public string ShortcutKey
        {
            get { return GetPropertyValue<string>(nameof(ShortcutKey)); }
            set { SetPropertyValue(nameof(ShortcutKey), value); }
        }

        [XafDisplayName("标题")]
        public string Caption
        {
            get { return GetPropertyValue<string>(nameof(Caption)); }
            set { SetPropertyValue(nameof(Caption), value); }
        }

        [XafDisplayName("快捷方式帮助")]
        public string Tooltip
        {
            get { return GetPropertyValue<string>(nameof(Tooltip)); }
            set { SetPropertyValue(nameof(Tooltip), value); }
        }

        [XafDisplayName("快捷方式图标")]
        public string ImageName
        {
            get { return GetPropertyValue<string>(nameof(ImageName)); }
            set { SetPropertyValue(nameof(ImageName), value); }
        }

        [XafDisplayName("输出位置")]
        [ToolTip("如果选中的是行,则在行号+本处设置值,列则同理.如:选中的是A1:A10，即为一列，本处设置值为1,则输出结果到B1到B10")]
        public int OutputOffset
        {
            get { return GetPropertyValue<int>(nameof(OutputOffset)); }
            set { SetPropertyValue(nameof(OutputOffset), value); }
        }

        [XafDisplayName("消息模板")]
        [Size(-1)]
        [ToolTip("例如:“内容:{T}”，程序在执行时在word文档中将{T}替换成用户选中的内容。在excel中，{T}是选中单元格的内容,{F}将被替换成用户选中的公式.")]
        public string UserPrompt
        {
            get { return GetPropertyValue<string>(nameof(UserPrompt)); }
            set { SetPropertyValue(nameof(UserPrompt), value); }
        }
        [Size(-1)]
        [XafDisplayName("系统提示")]
        public string SystemPrompt
        {
            get { return GetPropertyValue<string>(nameof(SystemPrompt)); }
            set { SetPropertyValue(nameof(SystemPrompt), value); }
        }

        [XafDisplayName("温度")]
        public int Temperature
        {
            get { return GetPropertyValue<int>(nameof(Temperature)); }
            set { SetPropertyValue(nameof(Temperature), value); }
        }
        [XafDisplayName("最大长度")]
        public int MaxToken
        {
            get { return GetPropertyValue<int>(nameof(MaxToken)); }
            set { SetPropertyValue(nameof(MaxToken), value); }
        }

        [XafDisplayName("语言模型")]
        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

    }
}
