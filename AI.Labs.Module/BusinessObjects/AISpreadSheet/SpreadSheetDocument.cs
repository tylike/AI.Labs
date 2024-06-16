using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp;

namespace AI.Labs.Module.BusinessObjects.AISpreadSheet
{
    [XafDisplayName("文档")]
    [NavigationItem("Excel")]
    [DefaultListViewOptions(true, NewItemRowPosition.None)]
    public class SpreadSheetDocument : SimpleXPObject, IAssistant
    {
        public SpreadSheetDocument(Session s) : base(s)
        {

        }

        [Action(Caption ="设置标题")]
        public void SetTitle(string title)
        {
            Title = title;
        }
        [XafDisplayName("标题")]
        public string Title
        {
            get { return GetPropertyValue<string>(nameof(Title)); }
            set { SetPropertyValue(nameof(Title), value); }
        }

        [XafDisplayName("表格")]
        [EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.SpreadsheetPropertyEditor)]
        public byte[] DocumentData
        {
            get { return GetPropertyValue<byte[]>(nameof(DocumentData)); }
            set { SetPropertyValue(nameof(DocumentData), value); }
        }

        [EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.RichTextPropertyEditor)]
        [XafDisplayName("对话")]
        public byte[] AIResponse
        {
            get { return GetPropertyValue<byte[]>(nameof(AIResponse)); }
            set { SetPropertyValue(nameof(AIResponse), value); }
        }

        public AIModel AIModel
        {
            get { return GetPropertyValue<AIModel>(nameof(AIModel)); }
            set { SetPropertyValue(nameof(AIModel), value); }
        }



    }

    
}
