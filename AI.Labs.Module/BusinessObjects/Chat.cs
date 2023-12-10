using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using System.Runtime.Versioning;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Diagnostics;
using DevExpress.ExpressApp.Xpo;
using AI.Labs.Module.BusinessObjects.Contexts;
using Newtonsoft.Json;
using DevExpress.DataAccess.Native.Json;
using DevExpress.XtraRichEdit.Commands;
using DevExpress.DataAccess.Native.Web;
using AI.Labs.Module.BusinessObjects.STT;
using AI.Labs.Module.BusinessObjects.TTS;
using DevExpress.ExpressApp.ConditionalAppearance;
using OpenAI.Utilities.FunctionCalling;
using System.Security.Cryptography;

namespace AI.Labs.Module.BusinessObjects
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
            this.DateTime = DateTime.Now;
            this.Model = Session.Query<AIModel>().FirstOrDefault(t => t.Default);
            MultiRoundChat = true;
            //ReadMessage = true;
        }

        [XafDisplayName("语音识别模型")]
        public STT.STTModel STTModel
        {
            get { return GetPropertyValue<STT.STTModel>(nameof(STTModel)); }
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
            this.Items.Add(item);
            return item;
        }

        public ChatItem Start(string verb, string messageSource)
        {
            var ci = AddItem(null);
            ci.Verb = verb;
            ci.MessageSource = messageSource;
            return ci;
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
            this.DateTime = DateTime.Now;
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

    public abstract class CommonFunctionViewController<TObjectView, TObject> : ObjectViewController<TObjectView, TObject>
        where TObjectView : ObjectView
        where TObject : XPObject
    {

    }

    [SupportedOSPlatform("windows")]
    public abstract class ChatViewController : ObjectViewController<ObjectView, Chat>
    {
        protected SimpleAction ask;
        public ChatViewController()
        {

            ask = new SimpleAction(this, "语音问AI", "ChatAction");
            ask.Execute += Ask_Execute;
            ask.ImageName = "NewComment";

            var clear = new SimpleAction(this, "清除", "ChatAction");
            clear.Execute += Clear_Execute;

            var send = new SimpleAction(this, "发送", "ChatAction");
            send.Execute += Send_Execute;
            record = ask;
            recorder = new Recorder();

            var test = new SimpleAction(this, "测试", null);
            test.Execute += Test_Execute;

            var clearHistory = new SimpleAction(this, "清除记忆", "ChatAction");
            clearHistory.Execute += ClearHistory_Execute;

            var clearRecords = new SimpleAction(this, "清除记录", "ChatAction");
            clearRecords.Execute += ClearRecords_Execute;

            var clearHistoryAndRecords = new SimpleAction(this, "清除记录和记忆", "ChatAction");
            clearHistoryAndRecords.Execute += ClearHistoryAndRecords_Execute;
        }

        private void ClearHistoryAndRecords_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            _history = null;
            var t = ViewCurrentObject.Items.ToArray();
            foreach (var item in t)
            {
                ViewCurrentObject.Items.Remove(item);
            }
        }

        private void ClearRecords_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var t = ViewCurrentObject.Items.ToArray();
            foreach (var item in t)
            {
                ViewCurrentObject.Items.Remove(item);
            }
        }

        private void ClearHistory_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            _history = null;
        }

        #region 发送文字内容
        /// <summary>
        /// 按下发送按钮时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Send_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

            //用户没有输入内容时不能发送!
            if (!string.IsNullOrEmpty(ViewCurrentObject.SendMessage))
            {
                try
                {
                    await SendToAI(ViewCurrentObject.SendMessage, "");
                }
                catch (HttpRequestException ex)
                {
                    throw new UserFriendlyException($"{ex.Message}\n可能的原因:\n1.请检查llm server是否正常工作!\n2.网络是否可以连接到llm server.");
                }
            }
            else
            {
                Application.ShowViewStrategy.ShowMessage("没有输入内容!", InformationType.Error);
            }
        } 
        #endregion

        private void Clear_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            ViewCurrentObject.SendMessage = "";
        }

        private async void Test_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            //var t = await AI.Ask("你好");
            //Debug.Write(t);
        }

        Recorder recorder;
        SimpleAction record;
        ChatItem recordVerbChatItem;
        //多轮会话时使用
        int? currentUsedModelOid = null;
        OpenAIService aiService;
        OpenAIService AIService
        {
            get
            {
                if (aiService == null)
                {
                    if (ViewCurrentObject.Model == null)
                    {
                        throw new UserFriendlyException("应该先设置所使用的语言模型");
                    }
                    aiService = AI.CreateOpenAIService(baseDomain: ViewCurrentObject.Model.ApiUrlBase, apiKey: ViewCurrentObject.Model.ApiKey);
                    currentUsedModelOid = ViewCurrentObject.Model.Oid;
                    //baseDomain: @"https://api.closeai-proxy.xyz"
                    //baseDomain: @"http://192.168.137.3:8000"
                }
                return aiService;
            }
        }

        ChatCompletionCreateRequest _history;
        ChatCompletionCreateRequest history
        {
            get
            {
                if (_history == null)
                {
                    if (ViewCurrentObject.Model == null)
                    {
                        throw new UserFriendlyException("请先选择语言模型!");
                    }
                    _history = new ChatCompletionCreateRequest() { Model = ViewCurrentObject.Model.Name };
                    _history.Messages = new List<ChatMessage>();
                    if (ViewCurrentObject.Role != null)
                    {
                        foreach (var item in ViewCurrentObject.Role.Prompts)
                        {
                            _history.Messages.Add(new ChatMessage(item.ChatRole.ToString(), item.Message));
                        }
                    }
                    ViewCurrentObject.InitializeData(_history, this.ObjectSpace);
                }
                return _history;
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            //设置默认模型            
            ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
        }

        private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Chat.Model))
            {
                var newModel = e.NewValue as AIModel;
                if (newModel != null && newModel.Oid != currentUsedModelOid)
                {
                    aiService = null;
                }
            }
        }

        public static void ReadText(IReadText text)
        {
            Task.Run(() =>
            {
                var msg = text.Message;
                var chat = text.TTSSettingProvider;
                if (!string.IsNullOrEmpty(msg))
                {
                    if (chat.ReadUseSystem)
                    {
                        TTSEngine.ReadText(msg);
                    }
                    else
                    {
                        var voice = chat.VoiceSolution?.DisplayName;
                        var data = TTSEngine.GetTextToSpeechData(msg, voice);
                        //aiTextToVoice.EndVerb();
                        //var playAIAudio = chat.Start("AI音频播放", "System");
                        TTSEngine.Play(data);
                        //playAIAudio.EndVerb();
                    }
                }
            });
        }

        //WhisperEngine sttengine;
        static object playing = new object();
        private async void Ask_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            if (recorder.IsStarted)
            {
                recordVerbChatItem.EndVerb();
                record.Caption = "语音问AI";
                recorder.StopRecord();
                var sw = Stopwatch.StartNew();
                var userVoiceToTextLogs = $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}:开始录制语音并转文字\n";

                var os = this.ObjectSpace;

                var uv = os.CreateObject<STT.VoiceContentBase>();
                var data = File.ReadAllBytes(recorder.SaveFileName);
                //uv.FileName = recorder.SaveFileName;
                os.CommitChanges();

                var userText = STTService.Instance.SpeechRecognition(data); //AI.LocalGetTextFromAudio(uv.FileName);
                sw.Stop();
                userVoiceToTextLogs += $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}:得到文字,用时:{sw.ElapsedMilliseconds}";

                var crlf = string.IsNullOrEmpty(ViewCurrentObject.SendMessage) ? string.Empty : "\r\n";
                ViewCurrentObject.SendMessage += crlf + userText;

                if (ViewCurrentObject.AutoSendSTT)
                {
                    await SendToAI(userText, userVoiceToTextLogs);

                }
            }
            else if (!recorder.IsStarted)
            {
                recorder.SaveFileName = $"d:\\audio\\UserVoice_{DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss")}.wav";
                recorder.StartRecord();
                record.Caption = "停止";
                recordVerbChatItem = ViewCurrentObject.Start("开始录音", "用户");
            }
        }
        SQLHelper helper = new SQLHelper();
        private async Task SendToAI(string userText, string addationLog)
        {
            var chat = ViewCurrentObject;
            var userTextChatItem = chat.Start("用户文字->AI", "用户");
            if (!string.IsNullOrEmpty(addationLog))
            {
                userTextChatItem.Log += addationLog;
            }

            //应先记录用户说了什么.
            userTextChatItem.Message = userText;
            userTextChatItem.EndVerb();

            Application.UIThreadDoEvents();

            //发送给llm的内容：追加
            history.Messages.Add(ChatMessage.FromUser(userText));
            var AINeedToExecuteAFunction = false;
            do
            {
                #region rem
                //if (ViewCurrentObject.StreamOut)
                //{
                //    var completionResult = ((IChatCompletionService)ai.Completions).CreateCompletionAsStream(history);

                //    await foreach (var completion in completionResult)
                //    {
                //        if (completion.Successful)
                //        {
                //            Console.Write(completion.Choices.FirstOrDefault()?.Message);
                //        }
                //        else
                //        {
                //            if (completion.Error == null)
                //            {
                //                throw new Exception("Unknown Error");
                //            }

                //            Console.WriteLine($"{completion.Error.Code}: {completion.Error.Message}");
                //        }
                //    }
                //} 
                #endregion

                var replyChatItem = ViewCurrentObject.Start("AI->用户", "AI");
                ChatMessage functionCall = null;
                //暂不考虑function calling
                //1.流式输出时:
                try
                {
                    #region 流式输出时
                    if (ViewCurrentObject.StreamOut)
                    {
                        var reply = AIService.ChatCompletion.CreateCompletionAsStream(history);
                        await foreach (var item in reply)
                        {
                            if (item.Successful)
                            {
                                var msg = item.Choices.FirstOrDefault()?.Message.Content;
                                if (!string.IsNullOrEmpty(msg))
                                {
                                    replyChatItem.Message += msg;
                                    Application.UIThreadDoEvents();
                                }
                            }
                            else
                            {
                                Debug.WriteLine("错误:" + item.Error.Message);
                            }
                        }

                        if (chat.MultiRoundChat && !string.IsNullOrEmpty(replyChatItem.Message))
                        {
                            history.Messages.Add(ChatMessage.FromAssistant(replyChatItem.Message));
                        }
                    }
                    #endregion

                    #region 非流式输出时
                    else
                    {
                        //2.非流式输出时:
                        var reply = await AIService.ChatCompletion.CreateCompletion(history);
                        
                        if (!reply.Successful)
                        {
                            replyChatItem.Message = reply.Error?.Message;
                            //如果是函数调用时,出错则不在循环,否则可能就是死循环
                            break;
                        }
                        else
                        {
                            
                            var choices = reply.Choices.First();

                            var response = reply.Choices.First().Message;
                            functionCall = response;
                            AINeedToExecuteAFunction = response.ToolCalls?.Any() ?? false;

                            replyChatItem.Message = response.Content;
                            if (AINeedToExecuteAFunction)
                            {
                                //如果是调用函数，则统一到一个地方去处理
                                //replyChatItem.Message += $"\r\n调用:{response.FunctionCall.Name}({response.FunctionCall.Arguments})\r\n";
                                //Console.WriteLine($"Invoking {response.FunctionCall.Name} with params: {response.FunctionCall.Arguments}");
                            }
                            else
                            {
                                chat.ProcessResponse(replyChatItem);
                            }
                        }

                        if (AINeedToExecuteAFunction || chat.MultiRoundChat && !string.IsNullOrEmpty(replyChatItem.Message))
                        {
                            history.Messages.Add(functionCall);
                        }
                    }
                    #endregion
                }
                finally
                {
                    replyChatItem.EndVerb();
                    Application.UIThreadDoEvents();
                }

                //朗读消息时可以全部输出完成了再读，也可以一个逗号、句号、感叹号、回车符后就马上读
                //暂时没有加入这个功能，先实现了全部输出完成了再读。
                if (chat.ReadMessage)
                {
                    var sw = Stopwatch.StartNew();
                    replyChatItem.AddLog("朗读语音:");
                    ReadText(replyChatItem);
                    sw.Stop();
                    replyChatItem.AddLog($"朗读结束,用时:{sw.ElapsedMilliseconds}");
                }

                //history.Messages.Add(response);

                if (AINeedToExecuteAFunction)
                {
                    foreach (var item in functionCall.ToolCalls)
                    {
                        try
                        {
                            var rst = FunctionCallingHelper.CallFunction<string>(item.FunctionCall, helper);
                            replyChatItem.Message = $"{item.FunctionCall.Name}({item.FunctionCall.Arguments})\n";
                            replyChatItem.Message += "调用结果:" + rst;
                            history.Messages.Add(ChatMessage.FromTool(rst, item.Id));
                            //functionCall.Content = rst;
                        }
                        catch (Exception ex)
                        {
                            replyChatItem.Message += "报错了:" + ex.Message;
                            var errorMessage = "请根据报错信息考虑重新调用?报错信息:" + ex.Message;
                            if (ex.InnerException != null)
                            {
                                errorMessage += ex.InnerException.Message;
                                replyChatItem.Message += ex.InnerException.Message;
                            }
                            history.Messages.Add(ChatMessage.FromTool(errorMessage, item.Id));
                        }
                    }


                    //FunctionCallingHelper.CallFunction<string>(functionCall.FunctionCall, helper);
                    //var functionCall = response.FunctionCall;

                    //var obj = (Newtonsoft.Json.Linq.JObject)JsonConvert.DeserializeObject(response.FunctionCallJson, typeof(object));

                    ////response.Content = result.ToString(CultureInfo.CurrentCulture);
                    //var parameters = obj.Properties().FirstOrDefault(t => t.Name == "parameters");
                    //var sql = ((Newtonsoft.Json.Linq.JValue)parameters.First.First.First).Value.ToString();

                    //string rst = "";
                    //try
                    //{

                    //    rst = helper.Execute(sql);
                    //}
                    //catch (Exception ex)
                    //{
                    //    rst = "提示:应该使用t-sql语法,当前操作的数据库是sqlserver.\n报错了:" + ex.ToString();
                    //}
                    ////response.Content = rst;
                    ///
                    //var tellAI = ChatMessage.FromFunction(rst);
                    ////tellAI.Role = "function";
                    //tellAI.FunctionCall = functionCall.FunctionCall;
                    //tellAI.Name = functionCall.FunctionCall.Name;
                    //history.Messages.Add(tellAI);

                    //var aiTextToVoice = ViewCurrentObject.Start("AI调用函数", "AI");
                    //aiTextToVoice.Message = sql + "\n" + rst;
                    //aiTextToVoice.EndVerb();
                }

            } while (AINeedToExecuteAFunction);

            if (!ViewCurrentObject.MultiRoundChat)
            {
                _history = null;
            }

            if(chat.SendMessageAfterClear)
            {
                chat.SendMessage = "";
            }
        }
    }
}
