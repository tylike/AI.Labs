using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using System.Text.Json;

namespace AI.Labs.Module.BusinessObjects.Sales
{
    public class ProductViewController : ObjectViewController<ObjectView, Product>
    {
        public ProductViewController()
        {
            var createTestData = new SimpleAction(this, "CreateProductData", null);

            createTestData.Execute += CreateTestData_Execute;
        }

        private void CreateTestData_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            var obj = ObjectSpace.CreateObject<CreateDataPrompt>();
            obj.Prompt = $@"你来协助测试人员生成测试“产品”数据,输出的内容是json格式的,数据尽量真实,每次生成5个产品。
生成测试数据,返回的格式为直接程序可以反序列化的JSON
重要:不要包含开头的```json和结尾的```字符。不要markdown格式。
,如下:
[
{{
    {nameof(Product.Name)}:'产品名称',
    {nameof(Product.Description)}:'产品详细说明',
    {nameof(Product.Model)}:'规格型号',
    {nameof(Product.Price)}:'价格',
    {nameof(Product.Category)}:'产品分类、关键字列表,逗号分隔的字符串',
    {nameof(Product.StockQty)}:'库存数量',
    {nameof(Product.Brand)}:'品牌',
    {nameof(Product.Properties)}:[{{'其他属性名称1':'其他属性值1'}},{{'其他属性名称2':'其他属性值2'}}] //注:如果有则写,非必填
}},
{{
    //产品二格式相同
}}
]
";
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
                    using JsonDocument doc = JsonDocument.Parse(json);
                    JsonElement root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement element in root.EnumerateArray())
                        {
                            var product = ObjectSpace.CreateObject<Product>();

                            var properties = element.GetProperty(nameof(Product.Properties));
                            if (properties.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var item in properties.EnumerateArray())
                                {
                                    //如何得到item有哪些属性、值?
                                    foreach (var p in item.EnumerateObject())
                                    {
                                        var xp = ObjectSpace.CreateObject<ProductPropertyValue>();
                                        xp.PropertyName = p.Name;
                                        xp.PropertyValue = p.Value.ToString();
                                        product.Properties.Add(xp);
                                    }
                                }
                            }
                            // 动态创建Book实例
                            product.Name = TryGetValue<string>(element, nameof(Product.Name));
                            product.Description = TryGetValue<string>(element, nameof(Product.Description));
                            product.Model = TryGetValue<string>(element, nameof(Product.Model));
                            product.Price = TryGetValue<string>(element, nameof(Product.Price));
                            product.Category = TryGetValue<string>(element, nameof(Product.Category));
                            product.StockQty = TryGetValue<string>(element, nameof(Product.StockQty));
                            product.Brand = TryGetValue<string>(element, nameof(Product.Brand));
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
