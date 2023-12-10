using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp;
using DevExpress.Xpo;

namespace AI.Labs.Module.BusinessObjects
{
    [NonPersistent]
    public abstract class SimpleXPObject : XPObject
    {

        public SimpleXPObject(Session s) : base(s)
        {

        }

        public override void AfterConstruction()
        {
            base.AfterConstruction();
            Createor = SecuritySystem.CurrentUserName;
            CreateTime = DateTime.Now; //Session.GetServerDataTime();
        }

        protected override void OnSaving()
        {
            LastUpdator = SecuritySystem.CurrentUserName;
            LastUpdateTime = DateTime.Now;// Session.GetServerDataTime();
            base.OnSaving();
        }

        private string _createor;
        [ModelDefault("AllowEdit", "False")]
        [Size(20)]
        public string Createor
        {
            get { return _createor; }
            set { SetPropertyValue(nameof(Createor), ref _createor, value); }
        }

        private DateTime _createTime;
        [ModelDefault("DisplayFormat", "yyyy-MM-dd HH:mm:ss")]
        [ModelDefault("AllowEdit", "False")]
        public DateTime CreateTime
        {
            get { return _createTime; }
            set { SetPropertyValue(nameof(CreateTime), ref _createTime, value); }
        }

        private string _lastUpdator;
        [ModelDefault("AllowEdit", "False")]
        [Size(20)]
        public string LastUpdator
        {
            get { return _lastUpdator; }
            set { SetPropertyValue(nameof(LastUpdator), ref _lastUpdator, value); }
        }

        private DateTime _lastUpdateTime;
        [ModelDefault("AllowEdit", "False")]
        [ModelDefault("DisplayFormat", "yyyy-MM-dd hh:mm:ss")]
        public DateTime LastUpdateTime
        {
            get { return _lastUpdateTime; }
            set { SetPropertyValue(nameof(LastUpdateTime), ref _lastUpdateTime, value); }
        }


    }
}
