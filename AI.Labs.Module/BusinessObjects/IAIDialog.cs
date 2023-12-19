using DevExpress.ExpressApp;
using OpenAI.ObjectModels.RequestModels;
using AI.Labs.Module.BusinessObjects.ChatInfo;

namespace AI.Labs.Module.BusinessObjects
{
    /// <summary>
    /// 是否可以与AI对话
    /// </summary>
    public interface IAIDialog { }
    public interface IAIDialog<T> : IAIDialog
        where T : IAIDialog<T>
    {
        /// <summary>
        /// 与AI对话时,应给出的提示
        /// 与当前上下文相关
        /// </summary>
        /// <param name="history"></param>
        /// <param name="os"></param>
        /// <param name="chat"></param>
        public abstract static void InitializeContextData(ChatCompletionCreateRequest history, IObjectSpace os, Chat chat, ViewController controller);
        /// <summary>
        /// 当AI回复时,应该如何处理
        /// </summary>
        /// <param name="chat"></param>
        /// <param name="item"></param>
        public abstract static void ChatResponse(Chat chat, ChatItem item);
    }

    //public class InventoryViewController : ObjectViewController<ObjectView, InventoryRecord>
    //{
    //    public InventoryViewController()
    //    {
    //        var initializeContext = new SimpleAction(this, "问AI", null);
    //        initializeContext.ImageName = "NewComment";
    //        initializeContext.Execute += InitializeContext_Execute;
    //    }

    //    private void InitializeContext_Execute(object sender, SimpleActionExecuteEventArgs e)
    //    {
    //        e.ShowViewParameters.CreatedView = Application.CreateListView(typeof(Question), true);
    //        e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;
    //        e.ShowViewParameters.Context = TemplateContext.LookupWindow;
    //        var dc = Application.CreateController<DialogController>();
    //        dc.SaveOnAccept = false;
    //        dc.Accepting += Dc_Accepting;
    //        e.ShowViewParameters.Controllers.Add(dc);
    //        //e.ShowViewParameters.CreatedView.
    //        //告诉chatgpt当前的情况

    //    }

    //    private async void Dc_Accepting(object sender, DialogControllerAcceptingEventArgs e)
    //    {
    //        var prompt = new StringBuilder("你是一个仓库管理员,当前仓库中有如下内容(使用json表示):\n");
    //        var inventoryRecords = View.ObjectSpace.GetObjects<InventoryRecord>();
    //        prompt.Append("{库存情况:[");
    //        foreach (var item in inventoryRecords)
    //        {
    //            prompt.Append($"{{ 仓库:\"{item.Warehouse.Name}\",产品:\"{item.Product.Name}\",数量:{item.Qty},计量单位:\"{item.Product?.Unit?.Name}\" }},\n");
    //        }
    //        prompt.Append("]}");
    //        var obj = e.AcceptActionArgs.SelectedObjects[0] as Question;

    //        prompt.AppendLine("要求:" + obj.Requirement);
    //        prompt.AppendLine("问题:" + obj.Content);

    //        var openAiService = new OpenAIService(new OpenAiOptions()
    //        {
    //            BaseDomain = "https://api.openai-proxy.org",
    //            //ApiKey = "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"
    //            ApiKey = "sk-7A5enIMIVH4PtxML4TL0M6Khi1ty8INWpQfvR6gykqgfCY6z"
    //        });

    //        // ChatGPT Official API
    //        var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
    //        {
    //            Messages = new List<ChatMessage>
    //            {
    //                ChatMessage.FromSystem(prompt.ToString()),
    //            },

    //            Model = Models.ChatGpt3_5Turbo,

    //            //MaxTokens = 50//optional

    //        });

    //        if (completionResult.Successful)
    //        {
    //            //Console.WriteLine(completionResult.Choices.First().Message.Content);
    //            var os = Application.CreateObjectSpace(typeof(AIResponse));
    //            var air = os.CreateObject<AIResponse>();
    //            air.Content = completionResult.Choices.First().Message.Content;
    //            air.Question = os.GetObject(obj);
    //            os.CommitChanges();
    //            var responseView = Application.CreateDetailView(os, air);

    //            Application.ShowViewStrategy.ShowView(
    //                new ShowViewParameters()
    //                {
    //                    CreatedView = responseView,
    //                    TargetWindow = TargetWindow.NewModalWindow,
    //                    Context = TemplateContext.PopupWindow
    //                }, new ShowViewSource(this.Frame, e.AcceptActionArgs.Action)
    //                );
    //            //e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, air);
    //            //e.ShowViewParameters.Context = TemplateContext.PopupWindow;
    //            //e.ShowViewParameters.TargetWindow = TargetWindow.NewModalWindow;

    //            //Application.ShowViewStrategy.ShowMessage(completionResult.Choices.First().Message.Content);
    //        }
    //        else
    //        {
    //            Application.ShowViewStrategy.ShowMessage("调用失败了!");
    //        }
    //    }
    //}

}
