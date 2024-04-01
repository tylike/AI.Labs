using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects
{
    [NonPersistent]
    public abstract class StringKeyBase : XPLiteObject
    {
        [Key(AutoGenerate = false)]
        [ModelDefault("AllowEdit","False")]
        [VisibleInDetailView(false)]
        public string Oid
        {
            get { return GetPropertyValue<string>(nameof(Oid)); }
            set { SetPropertyValue(nameof(Oid), value); }
        }

        public StringKeyBase(Session s):base(s)
        {
                
        }
    }
}
