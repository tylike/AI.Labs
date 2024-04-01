using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using System.Text.Json;

namespace AI.Labs.Module.BusinessObjects.Sales
{
    public class BookViewController : ObjectViewController<ObjectView, Book>
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
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(this.ObjectSpace, obj, false);
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