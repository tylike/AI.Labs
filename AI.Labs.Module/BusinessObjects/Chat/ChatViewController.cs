using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using System.Runtime.Versioning;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Diagnostics;
using AI.Labs.Module.BusinessObjects.Contexts;
using AI.Labs.Module.BusinessObjects.STT;
using AI.Labs.Module.BusinessObjects.TTS;
using OpenAI.Utilities.FunctionCalling;
using AI.Labs.Module.BusinessObjects.Sales;
using AI.Labs.Module.Translate;

namespace AI.Labs.Module.BusinessObjects.ChatInfo
{
    public class ChatActionPlaceHolder : ObjectViewController<ObjectView, Chat>
    {
        public ChatActionPlaceHolder()
        {
            var action = new SimpleAction(this, "ChatActionPlaceHolder", "ChatAction");
            action.Active["display"] = false;
        }
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
                    aiService = AIHelper.CreateOpenAIService(ViewCurrentObject.Model);
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
                    _history = new ChatCompletionCreateRequest() { Model = ViewCurrentObject.Model.Name, Temperature = 0.6f };
                    _history.Messages = new List<ChatMessage>();
                    if (ViewCurrentObject.Role != null)
                    {
                        var os = Application.CreateObjectSpace(typeof(PredefinedRole));
                        var role = os.GetObjectsQuery<PredefinedRole>().First(t => t.Oid == ViewCurrentObject.Role.Oid);

                        foreach (var item in role.Prompts)
                        {
                            var msg = item.EnglishMessage;
                            if (string.IsNullOrEmpty(msg))
                            {
                                msg = MicrosoftTranslate.Main( item.Message).Result;
                            }
                            _history.Messages.Add(new ChatMessage(item.ChatRole.ToString(), msg));
                        }
                    }
                    ViewCurrentObject.InitializeData(_history, ObjectSpace);
                }
                return _history;
            }
        }
        //System.MissingMethodException:“Method not found: '!!0 OpenAI.Utilities.FunctionCalling.FunctionCallingHelper.CallFunction(OpenAI.ObjectModels.RequestModels.FunctionCall, System.Object)'.”
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

                var os = ObjectSpace;

                var uv = os.CreateObject<VoiceContentBase>();
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
            userTextChatItem.ChatItemType = ChatItemType.User;
            if (!string.IsNullOrEmpty(addationLog))
            {
                userTextChatItem.Log += addationLog;
            }

            //应先记录用户说了什么.
            userTextChatItem.Message = userText;
            userTextChatItem.EnglishMessage = await MicrosoftTranslate.Main(userText);
            userTextChatItem.EndVerb();

            Application.UIThreadDoEvents();

            //发送给llm的内容：追加
            history.Messages.Add(ChatMessage.FromUser(userText));

            var AINeedToExecuteAFunction = false;

            #region old
            //if (chat.Model.Category == AIModelCategory.GoogleGeminiPro)
            //{
            //    //如果只有一条消息,则上面的处理

            //    //如果有多条消息时,则在第一消息上增加系统提示

            //    var request = new GeminiChatRequest();
            //    Content first = null;
            //    foreach (var m in history.Messages.Where(t => t.Role != "system"))
            //    {
            //        var cnt = new Content();
            //        first ??= cnt;
            //        var cr = Enum.Parse<ChatRole>(m.Role);
            //        switch (cr)
            //        {
            //            //case ChatRole.system:
            //            //    cnt.Role = "system";
            //            //    break;
            //            case ChatRole.user:
            //                cnt.Role = "user";
            //                break;
            //            case ChatRole.assistant:
            //                cnt.Role = "model";
            //                break;
            //            case ChatRole.function:
            //                cnt.Role = "function";
            //                break;
            //            default:
            //                break;
            //        }
            //        cnt.Parts.Add(new ContentPart { Text = m.Content });
            //        request.Contents.Add(cnt);
            //    }

            //    if (first != null)
            //    {
            //        var firstUserMessage = "";
            //        var systems = history.Messages.Where(t => t.Role == "system");
            //        if (systems.Any())
            //        {
            //            firstUserMessage = "# 已知资料:" + string.Join("\n", systems.Select(t => t.Content)) + "\n";
            //        }

            //        var txt = first.Parts.First();
            //        firstUserMessage += "# 用户说:" + txt.Text;
            //        txt.Text = firstUserMessage;
            //    }

            //    var sw = Stopwatch.StartNew();
            //    var rst = await ChatGemini.Send(request);
            //    sw.Stop();

            //    Debug.WriteLine("发送完成！");
            //    if (rst.Error == null)
            //    {
            //        var replyChatItem = ViewCurrentObject.Start("AI->用户", "AI");
            //        replyChatItem.ChatItemType = ChatItemType.Assistant;
            //        replyChatItem.Message = rst.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text;
            //        replyChatItem.EndVerb();
            //        replyChatItem.AddLog($"模型用时:{sw.ElapsedMilliseconds}");
            //        history.Messages.Add(ChatMessage.FromAssistant(replyChatItem.Message));
            //    }
            //    else
            //    {
            //        Debug.WriteLine(rst.Error.Message + "\ncode:" + rst.Error.Code, InformationType.Error);
            //        Application.ShowViewStrategy.ShowMessage(rst.Error.Message + "\ncode:" + rst.Error.Code, InformationType.Error);
            //    }
            //}
            //else 
            #endregion
            {

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
                                        replyChatItem.EnglishMessage += msg;
                                        Application.UIThreadDoEvents();
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("错误:" + item.Error.Message);
                                }
                            }

                            if (chat.MultiRoundChat && !string.IsNullOrEmpty(replyChatItem.EnglishMessage))
                            {
                                history.Messages.Add(ChatMessage.FromAssistant(replyChatItem.EnglishMessage));
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
                                replyChatItem.EnglishMessage = reply.Error?.Message;
                                //如果是函数调用时,出错则不在循环,否则可能就是死循环
                                break;
                            }
                            else
                            {

                                var choices = reply.Choices.First();

                                var response = reply.Choices.First().Message;
                                functionCall = response;
                                AINeedToExecuteAFunction = response.ToolCalls?.Any() ?? false;

                                replyChatItem.EnglishMessage = response.Content;
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

                            if (AINeedToExecuteAFunction || chat.MultiRoundChat && !string.IsNullOrEmpty(replyChatItem.EnglishMessage))
                            {
                                history.Messages.Add(functionCall);
                            }
                        }
                        #endregion
                    }
                    finally
                    {
                        replyChatItem.Message = await MicrosoftTranslate.Main(replyChatItem.EnglishMessage, false);
                        replyChatItem.EndVerb();
                        Application.UIThreadDoEvents();
                    }

                    //朗读消息时可以全部输出完成了再读，也可以一个逗号、句号、感叹号、回车符后就马上读
                    //暂时没有加入这个功能，先实现了全部输出完成了再读。
                    //if (chat.ReadMessage)
                    //{
                    //    var sw = Stopwatch.StartNew();
                    //    replyChatItem.AddLog("朗读语音:");
                    //    ReadText(replyChatItem);
                    //    sw.Stop();
                    //    replyChatItem.AddLog($"朗读结束,用时:{sw.ElapsedMilliseconds}");
                    //}

                    //history.Messages.Add(response);

                    if (AINeedToExecuteAFunction)
                    {
                        foreach (var item in functionCall.ToolCalls)
                        {
                            try
                            {
                                var rst = FunctionCallingHelper.CallFunction<string>(item.FunctionCall, helper);
                                replyChatItem.EnglishMessage = $"{item.FunctionCall.Name}({item.FunctionCall.Arguments})\n";
                                replyChatItem.EnglishMessage += "调用结果:" + rst;
                                history.Messages.Add(ChatMessage.FromTool(rst, item.Id));
                                //functionCall.Content = rst;
                            }
                            catch (Exception ex)
                            {
                                replyChatItem.EnglishMessage += "报错了:" + ex.Message;
                                var errorMessage = "请根据报错信息考虑重新调用?报错信息:" + ex.Message;
                                if (ex.InnerException != null)
                                {
                                    errorMessage += ex.InnerException.Message;
                                    replyChatItem.EnglishMessage += ex.InnerException.Message;
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
            }
            if (!ViewCurrentObject.MultiRoundChat)
            {
                _history = null;
            }

            if (chat.SendMessageAfterClear)
            {
                chat.SendMessage = "";
            }
        }
    }
}
