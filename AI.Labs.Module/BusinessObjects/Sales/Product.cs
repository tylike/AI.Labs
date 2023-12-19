using DevExpress.ExpressApp;
using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using OpenAI.ObjectModels.RequestModels;
using DevExpress.ExpressApp.DC;
using AI.Labs.Module.BusinessObjects.ChatInfo;
using System.Text;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;
using System.Text.Json;

namespace AI.Labs.Module.BusinessObjects.Sales
{
    [NavigationItem]
    [XafDisplayName("产品")]
    public class Product : XPObject, IAIDialog<Product>
    {
        public Product(Session s) : base(s)
        {

        }

        //类别：产品所属的类别或分类，例如电子产品、家具等。
        [XafDisplayName("产品分类")]
        public string Category
        {
            get { return GetPropertyValue<string>(nameof(Category)); }
            set { SetPropertyValue(nameof(Category), value); }
        }

        [XafDisplayName("产品名称")]
        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        //产品ID：一个唯一的标识符，用于识别每个产品。
        //产品名称：产品的名称或标题。
        //产品描述：对产品的详细描述，包括特征、功能和规格等信息。
        [Size(-1)]
        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }

        //价格：产品的价格。

        public string Price
        {
            get { return GetPropertyValue<string>(nameof(Price)); }
            set { SetPropertyValue(nameof(Price), value); }
        }
        //品牌：产品所属的品牌或制造商。
        public string Brand
        {
            get { return GetPropertyValue<string>(nameof(Brand)); }
            set { SetPropertyValue(nameof(Brand), value); }
        }

        //规格型号
        [Size(200)]
        public string Model
        {
            get { return GetPropertyValue<string>(nameof(Model)); }
            set { SetPropertyValue(nameof(Model), value); }
        }



        //库存数量：产品当前的库存数量。

        public string StockQty
        {
            get { return GetPropertyValue<string>(nameof(StockQty)); }
            set { SetPropertyValue(nameof(StockQty), value); }
        }

        public static void ChatResponse(Chat chat, ChatItem item)
        {
        }

        public static void InitializeContextData(ChatCompletionCreateRequest history, IObjectSpace os, Chat chat, ViewController controller)
        {
            var prompt = new StringBuilder("我们现有产品(使用json表示):\n");
            var books = os.GetObjects<Product>();
            prompt.Append("{产品:[");
            foreach (var item in books)
            {
                var properties = string.Join(",", item.Properties.Select(t => $"{t.PropertyName}:'{t.PropertyValue}'").ToArray());
                prompt.Append($"{{ 名称:'{item.Name}',库存数量:{item.StockQty},价格,:{item.Price},介绍:'{item.Description}',分类:'{item.Category}' }},{{品牌:'{item.Brand}',规格型号:'{item.Model}',{properties}}}\n");
            }
            prompt.Append("]}");
            history.Messages.Add(ChatMessage.FromSystem(prompt.ToString()));
        }

        //创建日期：产品的创建日期或上市日期。
        //更新日期：产品信息最近一次更新的日期。
        //图片链接：产品的图片链接，可以是指向产品图片的URL地址。
        //评分：产品的评分或评级，如果适用的话。
        //评论：产品的用户评论或反馈。
        //销量：产品的销售数量。
        //条形码：产品的条形码或QR码，用于识别和跟踪产品。
        //生产日期：产品的生产日期。
        //过期日期：如果适用的话，产品的过期日期。
        //材料：产品所使用的主要材料。
        //尺寸：产品的尺寸或尺寸规格。
        //重量：产品的重量。
        //包装信息：产品的包装信息，如包装尺寸、重量等。

        [Association, DevExpress.Xpo.Aggregated]
        public XPCollection<ProductPropertyValue> Properties
        {
            get => GetCollection<ProductPropertyValue>(nameof(Properties));
        }
    }

    public class ProductPropertyValue : XPObject
    {
        public ProductPropertyValue(Session s) : base(s)
        {
        }

        [Association]
        public Product Product
        {
            get { return GetPropertyValue<Product>(nameof(Product)); }
            set { SetPropertyValue(nameof(Product), value); }
        }

        public string PropertyName
        {
            get { return GetPropertyValue<string>(nameof(PropertyName)); }
            set { SetPropertyValue(nameof(PropertyName), value); }
        }

        [Size(-1)]
        public string PropertyValue
        {
            get { return GetPropertyValue<string>(nameof(PropertyValue)); }
            set { SetPropertyValue(nameof(PropertyValue), value); }
        }
    }


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
