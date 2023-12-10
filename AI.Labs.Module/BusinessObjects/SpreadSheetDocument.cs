using DevExpress.Xpo;
using DevExpress.Persistent.Base;

namespace AI.Labs.Module.BusinessObjects
{
    [NavigationItem]
    public class SpreadSheetDocument : XPObject
    {
        public SpreadSheetDocument(Session s):base(s)
        {
                
        }

        [EditorAlias(DevExpress.ExpressApp.Editors.EditorAliases.SpreadsheetPropertyEditor)]
        public byte[] DocumentData
        {
            get { return GetPropertyValue<byte[]>(nameof(DocumentData)); }
            set { SetPropertyValue(nameof(DocumentData), value); }
        }


    }
}
