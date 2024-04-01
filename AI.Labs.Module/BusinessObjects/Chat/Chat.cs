using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;
using OpenAI.ObjectModels.RequestModels;
using DevExpress.ExpressApp.Xpo;
using Newtonsoft.Json;
using DevExpress.DataAccess.Native.Json;
using DevExpress.XtraRichEdit.Commands;
using DevExpress.DataAccess.Native.Web;
using AI.Labs.Module.BusinessObjects.STT;
using AI.Labs.Module.BusinessObjects.TTS;
using DevExpress.ExpressApp.ConditionalAppearance;
using System.Security.Cryptography;
using System.Diagnostics;
using DevExpress.CodeParser;

namespace AI.Labs.Module.BusinessObjects.ChatInfo
{
    /// <summary>
    /// 单个机器人聊天模式
    /// </summary>
    [NavigationItem("设置")]
    //[XafDisplayName("对话")]
    public class Chat : XPObject, ITTSSettingProvider
    {
        public Chat(Session s) : base(s)
        {

        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            DateTime = DateTime.Now;
            Model = Session.Query<AIModel>().FirstOrDefault(t => t.IsDefault);
            MultiRoundChat = true;
            //ReadMessage = true;
        }

        [XafDisplayName("语音识别模型")]
        public STTModel STTModel
        {
            get { return GetPropertyValue<STTModel>(nameof(STTModel)); }
            set { SetPropertyValue(nameof(STTModel), value); }
        }

        [XafDisplayName("用户输入")]
        [Size(1000)]
        [ModelDefault("RowCount", "5")]
        public string SendMessage
        {
            get { return GetPropertyValue<string>(nameof(SendMessage)); }
            set { SetPropertyValue(nameof(SendMessage), value); }
        }

        [XafDisplayName("模型")]
        public AIModel Model
        {
            get { return GetPropertyValue<AIModel>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }

        [XafDisplayName("角色")]
        public PredefinedRole Role
        {
            get { return GetPropertyValue<PredefinedRole>(nameof(Role)); }
            set { SetPropertyValue(nameof(Role), value); }
        }

        public VoiceSolution VoiceSolution
        {
            get { return GetPropertyValue<VoiceSolution>(nameof(VoiceSolution)); }
            set { SetPropertyValue(nameof(VoiceSolution), value); }
        }

        [XafDisplayName("朗读消息")]
        public bool ReadMessage
        {
            get { return GetPropertyValue<bool>(nameof(ReadMessage)); }
            set { SetPropertyValue(nameof(ReadMessage), value); }
        }
        
        public bool SendMessageAfterClear
        {
            get { return GetPropertyValue<bool>(nameof(SendMessageAfterClear)); }
            set { SetPropertyValue(nameof(SendMessageAfterClear), value); }
        }


        [XafDisplayName("内置朗读")]
        public bool ReadUseSystem
        {
            get { return GetPropertyValue<bool>(nameof(ReadUseSystem)); }
            set { SetPropertyValue(nameof(ReadUseSystem), value); }
        }

        [XafDisplayName("流式输出")]
        public bool StreamOut
        {
            get { return GetPropertyValue<bool>(nameof(StreamOut)); }
            set { SetPropertyValue(nameof(StreamOut), value); }
        }


        [XafDisplayName("聊天主题")]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [XafDisplayName("创建日期")]
        [ModelDefault("DisplayFormat", "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime DateTime
        {
            get { return GetPropertyValue<DateTime>(nameof(DateTime)); }
            set { SetPropertyValue(nameof(DateTime), value); }
        }


        public bool AutoSendSTT
        {
            get { return GetPropertyValue<bool>(nameof(AutoSendSTT)); }
            set { SetPropertyValue(nameof(AutoSendSTT), value); }
        }

        [Association, DevExpress.Xpo.Aggregated
            //,EditorAlias(LabsModule.HtmlTemplateItemsPropertyEditor)
            ]
        public XPCollection<ChatItem> Items
        {
            get
            {
                return GetCollection<ChatItem>(nameof(Items));
            }
        }

        [XafDisplayName("多轮会话")]
        [ToolTip("单轮会话更省钱")]
        public bool MultiRoundChat
        {
            get { return GetPropertyValue<bool>(nameof(MultiRoundChat)); }
            set { SetPropertyValue(nameof(MultiRoundChat), value); }
        }

        public ChatItem AddItem(string message)
        {
            var item = new ChatItem(Session);
            item.Message = message;
            Items.Add(item);
            return item;
        }

        public ChatItem Start(string verb, string messageSource)
        {
            var ci = AddItem(null);
            ci.Verb = verb;
            ci.MessageSource = messageSource;
            return ci;
        }

        public void ChatItemEndVerb()
        {

        }

        public Action<ChatCompletionCreateRequest, IObjectSpace> InitializeDataAction { get; set; }

        public void InitializeData(ChatCompletionCreateRequest history, IObjectSpace os)
        {
            InitializeDataAction?.Invoke(history, os);
        }

        public Action<ChatItem> ProcessResponseAction { get; set; }
        public void ProcessResponse(ChatItem item)
        {
            ProcessResponseAction?.Invoke(item);
        }

        [NonPersistent]
        public ObjectView SourceView { get; set; }
    }

    public class Prompt : XPObject
    {
        public Prompt(Session s) : base(s)
        {

        }
        [Association]
        public PredefinedRole Role
        {
            get { return GetPropertyValue<PredefinedRole>(nameof(Role)); }
            set { SetPropertyValue(nameof(Role), value); }
        }

        [XafDisplayName("角色")]
        public ChatRole ChatRole
        {
            get { return GetPropertyValue<ChatRole>(nameof(ChatRole)); }
            set { SetPropertyValue(nameof(ChatRole), value); }
        }
        [XafDisplayName("消息")]
        [Size(-1)]
        public string Message
        {
            get { return GetPropertyValue<string>(nameof(Message)); }
            set { SetPropertyValue(nameof(Message), value); }
        }
        [XafDisplayName("消息")]
        [Size(-1)]
        public string EnglishMessage
        {
            get { return GetPropertyValue<string>(nameof(EnglishMessage)); }
            set { SetPropertyValue(nameof(EnglishMessage), value); }
        }

    }
    public enum ChatRole
    {
        [XafDisplayName("系统")]
        system,
        [XafDisplayName("用户")]
        user,
        [XafDisplayName("AI")]
        assistant,
        [XafDisplayName("函数")]
        function
    }
    public enum ChatItemType
    {
        System,
        User,
        Assistant,
        Function
    }

    [XafDisplayName("交互内容")]
    [Appearance("UserStyle", Criteria = "MessageSource=='AI'", BackColor = "#F9FF8D", TargetItems = "*")]
    [Appearance("AIStyle", Criteria = "MessageSource=='用户'", BackColor = "#B9FFC9", TargetItems = "*")]
    public class ChatItem : XPObject, IReadText
    {
        public ChatItem(Session s) : base(s)
        {

        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            DateTime = DateTime.Now;
        }


        public ChatItemType ChatItemType
        {
            get { return GetPropertyValue<ChatItemType>(nameof(ChatItemType)); }
            set { SetPropertyValue(nameof(ChatItemType), value); }
        }

        [Association]
        public Chat Chat
        {
            get { return GetPropertyValue<Chat>(nameof(Chat)); }
            set { SetPropertyValue(nameof(Chat), value); }
        }

        [XafDisplayName("动作说明")]
        [Size(200)]
        public string Verb
        {
            get { return GetPropertyValue<string>(nameof(Verb)); }
            set { SetPropertyValue(nameof(Verb), value); }
        }

        [XafDisplayName("消息来源")]
        public string MessageSource
        {
            get { return GetPropertyValue<string>(nameof(MessageSource)); }
            set { SetPropertyValue(nameof(MessageSource), value); }
        }


        [XafDisplayName("消息")]
        [Size(-1)]
        public string Message
        {
            get { return GetPropertyValue<string>(nameof(Message)); }
            set { SetPropertyValue(nameof(Message), value); }
        }

        [XafDisplayName("英文消息")]
        [Size(-1)]
        public string EnglishMessage
        {
            get { return GetPropertyValue<string>(nameof(EnglishMessage)); }
            set { SetPropertyValue(nameof(EnglishMessage), value); }
        }

        public string DisplayMessage
        {
            get => Message + "\n" + EnglishMessage;
        }


        public void AddLog(string message)
        {
            var crlf = !string.IsNullOrEmpty(Log) ? "\r\n" : "";
            Log += crlf + message;
        }

        [Size(-1)]
        public string Log
        {
            get { return GetPropertyValue<string>(nameof(Log)); }
            set { SetPropertyValue(nameof(Log), value); }
        }


        [XafDisplayName("开始时间")]
        [ModelDefault("DisplayFormat", "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime DateTime
        {
            get { return GetPropertyValue<DateTime>(nameof(DateTime)); }
            set { SetPropertyValue(nameof(DateTime), value); }
        }

        [XafDisplayName("结束时间")]
        [ModelDefault("DisplayFormat", "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime EndTime
        {
            get { return GetPropertyValue<DateTime>(nameof(EndTime)); }
            set { SetPropertyValue(nameof(EndTime), value); }
        }

        [XafDisplayName("耗时(秒)")]
        public int ElapsedSeconds
        {
            get { return GetPropertyValue<int>(nameof(ElapsedSeconds)); }
            set { SetPropertyValue(nameof(ElapsedSeconds), value); }
        }

        public void EndVerb()
        {
            EndTime = DateTime.Now;

            if (this.ChatItemType == ChatItemType.Assistant && !string.IsNullOrEmpty(Message))
            {
                ReadMessage();
            }
        }

        /// <summary>
        /// 用户可以手动进行朗读消息，无条件的
        /// </summary>
        /// <returns></returns>
        [Action]
        public async Task ReadMessage()
        {
            if (!string.IsNullOrEmpty(Message))
            {
                var sw = Stopwatch.StartNew();
                var msg = this.Message;
                var chat = this.Chat;
                chat.VoiceSolution.Read(msg);
                //await TTSEngine.ReadText(msg, chat.VoiceSolution?.DisplayName, chat.ReadUseSystem);
                sw.Stop();
                this.AddLog($"朗读消息用时:{sw.ElapsedMilliseconds}");
            }
        }
        /// <summary>
        /// 发言者
        /// </summary>
        public ApplicationUser User
        {
            get { return GetPropertyValue<ApplicationUser>(nameof(User)); }
            set { SetPropertyValue(nameof(User), value); }
        }

        ITTSSettingProvider IReadText.TTSSettingProvider => Chat;

        protected override void OnChanged(string propertyName, object oldValue, object newValue)
        {
            base.OnChanged(propertyName, oldValue, newValue);
            if (!IsLoading)
            {
                if (propertyName == nameof(DateTime) || propertyName == nameof(EndTime))
                    ElapsedSeconds = (int)(EndTime - DateTime).TotalSeconds;
            }
        }
    }
}
