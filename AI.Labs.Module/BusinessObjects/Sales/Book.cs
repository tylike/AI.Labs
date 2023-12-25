using AI.Labs.Module.BusinessObjects.ChatInfo;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace AI.Labs.Module.BusinessObjects.Sales
{

    [NavigationItem("应用场景")]
    [DefaultListViewOptions(true, NewItemRowPosition.Bottom)]
    [XafDisplayName("书籍")]
    public class Book : XPObject, IAIDialog<Book>
    {
        public Book(Session s) : base(s)
        {

        }

        [XafDisplayName("书名")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }
        [XafDisplayName("作者")]
        public string Author
        {
            get { return GetPropertyValue<string>(nameof(Author)); }
            set { SetPropertyValue(nameof(Author), value); }
        }

        [Size(-1)]
        [XafDisplayName("摘要")]
        public string Memo
        {
            get { return GetPropertyValue<string>(nameof(Memo)); }
            set { SetPropertyValue(nameof(Memo), value); }
        }

        [XafDisplayName("出版社")]
        public string Publisher
        {
            get { return GetPropertyValue<string>(nameof(Publisher)); }
            set { SetPropertyValue(nameof(Publisher), value); }
        }

        [XafDisplayName("页数")]
        public int Pages
        {
            get { return GetPropertyValue<int>(nameof(Pages)); }
            set { SetPropertyValue(nameof(Pages), value); }
        }
        [XafDisplayName("价格")]
        public decimal Price
        {
            get { return GetPropertyValue<decimal>(nameof(Price)); }
            set { SetPropertyValue(nameof(Price), value); }
        }

        public string ISBN
        {
            get { return GetPropertyValue<string>(nameof(ISBN)); }
            set { SetPropertyValue(nameof(ISBN), value); }
        }
        [Size(2000)]
        [XafDisplayName("主题词")]
        public string Keyword
        {
            get { return GetPropertyValue<string>(nameof(Keyword)); }
            set { SetPropertyValue(nameof(Keyword), value); }
        }

        [XafDisplayName("库存数量")]
        public int StoreQty
        {
            get { return GetPropertyValue<int>(nameof(StoreQty)); }
            set { SetPropertyValue(nameof(StoreQty), value); }
        }

        public static void InitializeContextData(ChatCompletionCreateRequest history, IObjectSpace os, Chat chat, ViewController c)
        {
            var prompt = new StringBuilder("我们现有书籍(使用json表示):\n");
            var books = os.GetObjects<Book>();
            prompt.Append("{书:[");
            foreach (var item in books)
            {
                prompt.Append($"{{ 书名:\"{item.Name}\",库存数量:{item.StoreQty},作者:\"{item.Author}\",摘要:{item.Memo},出版社:\"{item.Publisher}\",页数:{item.Pages},价格:{item.Price},ISBN:'{item.ISBN}',主题词:'{item.Keyword}' }},\n");
            }
            prompt.Append("]}");
            history.Messages.Add(ChatMessage.FromSystem(prompt.ToString()));
        }

        public static void ChatResponse(Chat chat, ChatItem item)
        {

        }
    }

    public class BookViewController:ObjectViewController<ObjectView,Book>
    {
        public BookViewController()
        {
            var createTestData = new SimpleAction(this, "CreateTestBookData", null);

            createTestData.Execute += CreateTestData_Execute;
        }

        private async void CreateTestData_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var obj = ObjectSpace.CreateObject<CreateDataPrompt>();
            obj.Prompt = $@"你来协助测试人员生成测试书籍数据,输出的内容是json格式的,数据尽量真实,每次生成5本书。
生成测试数据,返回的格式为直接程序可以反序列化的JSON,如下:
{{
    {nameof(Book.Name)}:'这里是书名',
    {nameof(Book.Author)}:'作者',
    {nameof(Book.ISBN)}:'ISBN',
    {nameof(Book.Keyword)}:'关键字1,关键字2...,关键字N',
    {nameof(Book.Price)}:40.00,
    {nameof(Book.Memo)}:'书籍内容介绍',
    {nameof(Book.Pages)}:399,
    {nameof(Book.Publisher)}:'出版社',
    {nameof(Book.StoreQty)}:390
}}";
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(this.ObjectSpace, obj,false);
            e.ShowViewParameters.Context = TemplateContext.PopupWindow;
            var dc = Application.CreateController<DialogController>();
            dc.SaveOnAccept = false;
            dc.Accepting += async (s, e) =>
            {
                var request = new GeminiChatRequest();
                var c = new Content();
                c.Parts.Add(new ContentPart() { Text = obj.Prompt });
                request.GenerationConfig = new GenerationConfig { MaxOutputTokens = 9999 };
                request.Contents.Add(c);
                
                //await foreach (var t in ChatGemini.CreateCompletionAsStream(request))
                //{
                //    Debug.WriteLine(t);                    
                //}
                //return;
                
                var rst = await ChatGemini.Send(request);
                if (rst != null)
                {
                    var t = rst.Candidates.FirstOrDefault();
                    var json = rst.Candidates.FirstOrDefault()?.Content.Parts.FirstOrDefault()?.Text;
                    if (json.StartsWith("```json"))
                    {
                        json = json["```json".Length..];
                    }
                    if (json.EndsWith("```"))
                    {
                        json = json[..^3];
                    }
                    using JsonDocument doc = JsonDocument.Parse(json);
                    JsonElement root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in root.EnumerateArray())
                        {
                            // 动态创建Book实例
                            var book = ObjectSpace.CreateObject<Book>();

                            book.Name = TryGetValue<string>(element, "Name");
                            book.Author = TryGetValue<string>(element, "Author");
                            book.ISBN = TryGetValue<string>(element, "ISBN");
                            book.Keyword = TryGetValue<string>(element, "Keyword");
                            book.Price = TryGetValue<decimal>(element, "Price");
                            book.Memo = TryGetValue<string>(element, "Memo");
                            book.Pages = TryGetValue<int>(element, "Pages");
                            book.Publisher = TryGetValue<string>(element, "Publisher");
                            book.StoreQty = TryGetValue<int>(element, "StoreQty");
                        }
                    }
                }
                ObjectSpace.CommitChanges();
                Application.ShowViewStrategy.ShowMessage("数据生成完成!");
                View.RefreshDataSource();

            };
            e.ShowViewParameters.Controllers.Add(dc);
        }

        static T TryGetValue<T>(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement propertyElement))
            {
                try
                {
                    return JsonSerializer.Deserialize<T>(propertyElement.GetRawText());
                }
                catch (JsonException)
                {
                    // Handle or log exception if needed
                    return default;
                }
            }
            return default;
        }
    }
}