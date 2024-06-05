using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Xpo;
using System.Text.RegularExpressions;

namespace AI.Labs.Module.BusinessObjects.AutoComplexTask
{
    public class AgentGroup : SimpleXPObject
    {
        public AgentGroup(Session s) : base(s)
        {

        }

        [XafDisplayName("小组名称")]
        public string GroupName
        {
            get { return GetPropertyValue<string>(nameof(GroupName)); }
            set { SetPropertyValue(nameof(GroupName), value); }
        }

        [XafDisplayName("小组任务")]
        public string GroupMemo
        {
            get { return GetPropertyValue<string>(nameof(GroupMemo)); }
            set { SetPropertyValue(nameof(GroupMemo), value); }
        }

        //[Association, DevExpress.Xpo.Aggregated]
        //public XPCollection<Agent> Agents
        //{
        //    get
        //    {
        //        return GetCollection<Agent>(nameof(Agents));
        //    }
        //}
    }

    [NavigationItem("自动任务")]
    public class Agent : SimpleXPObject
    {
        public Agent(Session s) : base(s)
        {

        }

        //public AgentGroup AgentGroup
        //{
        //    get { return GetPropertyValue<AgentGroup>(nameof(AgentGroup)); }
        //    set { SetPropertyValue(nameof(AgentGroup), value); }
        //}

        [Size(-1)]
        [XafDisplayName("系统提示")]
        public string SystemPrompt
        {
            get { return GetPropertyValue<string>(nameof(SystemPrompt)); }
            set { SetPropertyValue(nameof(SystemPrompt), value); }
        }
    }


    
}
