using DevExpress.Xpo;
using DevExpress.Persistent.Base;

namespace AI.Labs.Module.BusinessObjects
{
    [NavigationItem]
    public class WordDocument : XPObject
    {
        public WordDocument(Session s):base(s)
        {
                
        }

        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.RichTextPropertyEditor),Size(-1)]
        public string DocumentText
        {
            get { return GetPropertyValue<string>(nameof(DocumentText)); }
            set { SetPropertyValue(nameof(DocumentText), value); }
        }
    }
}
