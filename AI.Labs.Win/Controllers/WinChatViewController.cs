using AI.Labs.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AI.Labs.Win.Controllers
{
    public class WinChatViewController:ChatViewController
    {
        public WinChatViewController()
        {
            
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            //var t = this.Frame.Template as Form;
            //t.KeyPreview = true;
            //t.KeyUp += (s, e) =>
            //{
            //    if (e.KeyCode == Keys.Alt)
            //    {
            //        ask.DoExecute();
            //    }
            //};
        }
    }
}
