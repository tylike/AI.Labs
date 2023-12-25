using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.DC;

namespace AI.Labs.Module.BusinessObjects
{
    public interface IWordDocument
    {
        /// <summary>
        /// 纯文字内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// word二进制
        /// </summary>
        [EditorAlias(EditorAliases.RichTextPropertyEditor)]
        public byte[] WordDocument { get; set; }
    }

    [NavigationItem]
    [XafDisplayName("Word集成")]
    public class WordDocumentRecord : XPObject,IWordDocument
    {
        public WordDocumentRecord(Session s):base(s)
        {
                
        }

        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [Size(-1)]
        [VisibleInDetailView(false)]
        public string Content
        {
            get { return GetPropertyValue<string>(nameof(Content)); }
            set { SetPropertyValue(nameof(Content), value); }
        }


        public byte[] WordDocument
        {
            get { return GetPropertyValue<byte[]>(nameof(WordDocument)); }
            set { SetPropertyValue(nameof(WordDocument), value); }
        }


    }
}
