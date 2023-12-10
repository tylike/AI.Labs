using DevExpress.ExpressApp;
using DevExpress.Xpo;
using DevExpress.XtraSpreadsheet.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Persistent.Base;
using OpenAI.Managers;
using OpenAI;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp.DC;
using static OpenAI.ObjectModels.Models;
using System.Diagnostics;
using DevExpress.ExpressApp.Model;
using System.Runtime.Versioning;
using DevExpress.XtraRichEdit.Import.Html;

namespace AI.Labs.Module.BusinessObjects
{
    [NavigationItem]
    [XafDisplayName("仓库")]
    public class Warehouse : BaseObject
    {
        public Warehouse(Session s) : base(s)
        {

        }
        [XafDisplayName("仓库名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }
    }

    [NavigationItem]
    [XafDisplayName("产品")]
    public class Product : BaseObject
    {
        public Product(Session s) : base(s)
        {

        }

        [XafDisplayName("产品分类")]
        public ProductCategory Category
        {
            get { return GetPropertyValue<ProductCategory>(nameof(Category)); }
            set { SetPropertyValue(nameof(Category), value); }
        }

        [XafDisplayName("产品名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        [XafDisplayName("单位")]
        public ProductUnit Unit
        {
            get { return GetPropertyValue<ProductUnit>(nameof(Unit)); }
            set { SetPropertyValue(nameof(Unit), value); }
        }        
    }

    [XafDisplayName("产品分类")]
    public class ProductCategory : XPObject
    {
        public ProductCategory(Session s):base(s)
        {
        }
        [XafDisplayName("名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }
    }

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
        public abstract static void InitializeContextData(ChatCompletionCreateRequest history,IObjectSpace os,Chat chat,ViewController controller);
        /// <summary>
        /// 当AI回复时,应该如何处理
        /// </summary>
        /// <param name="chat"></param>
        /// <param name="item"></param>
        public abstract static void ChatResponse(Chat chat,ChatItem item);
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

    [XafDisplayName("回复")]
    [NavigationItem]
    public class AIResponse : XPObject
    {
        public AIResponse(Session s) : base(s)
        {

        }
        [XafDisplayName("问题")]
        public Question Question
        {
            get { return GetPropertyValue<Question>(nameof(Question)); }
            set { SetPropertyValue(nameof(Question), value); }
        }
        [XafDisplayName("回复内容")]
        [Size(-1)]
        public string Content
        {
            get { return GetPropertyValue<string>(nameof(Content)); }
            set { SetPropertyValue(nameof(Content), value); }
        }


    }

    [NavigationItem]
    [XafDisplayName("产品单位")]
    public class ProductUnit : BaseObject
    {
        public ProductUnit(Session s) : base(s)
        {

        }

        [XafDisplayName("名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }
    }
}
