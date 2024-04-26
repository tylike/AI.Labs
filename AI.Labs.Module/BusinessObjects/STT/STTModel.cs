using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;

namespace AI.Labs.Module.BusinessObjects.STT
{

    [NavigationItem("STT")]
    public class STTModel : XPObject
    {
        public STTModel(Session s):base(s)
        {

        }

        public string Name
        {
            get { return GetPropertyValue<string>(nameof(Name)); }
            set { SetPropertyValue(nameof(Name), value); }
        }

        public string Description
        {
            get { return GetPropertyValue<string>(nameof(Description)); }
            set { SetPropertyValue(nameof(Description), value); }
        }

        public string ModelFilePath
        {
            get { return GetPropertyValue<string>(nameof(ModelFilePath)); }
            set { SetPropertyValue(nameof(ModelFilePath), value); }
        }

        public string ServerApplication
        {
            get { return GetPropertyValue<string>(nameof(ServerApplication)); }
            set { SetPropertyValue(nameof(ServerApplication), value); }
        }

        public bool IsDefault
        {
            get { return GetPropertyValue<bool>(nameof(IsDefault)); }
            set { SetPropertyValue(nameof(IsDefault), value); }
        }


    }
}
