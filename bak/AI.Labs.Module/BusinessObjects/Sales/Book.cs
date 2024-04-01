using AI.Labs.Module.BusinessObjects.ChatInfo;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using OpenAI.ObjectModels.RequestModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
}