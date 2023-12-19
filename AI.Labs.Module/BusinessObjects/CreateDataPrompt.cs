using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects
{
    [NonPersistent]
    public class CreateDataPrompt : XPObject
    {
        public CreateDataPrompt(Session s) : base(s)
        {

        }
        [Size(-1)]
        public string Prompt
        {
            get { return GetPropertyValue<string>(nameof(Prompt)); }
            set { SetPropertyValue(nameof(Prompt), value); }
        }
    }
}