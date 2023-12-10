
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;

namespace AI.Labs.Module.BusinessObjects.STT
{
    public class AutoStartSTTServiceController : WindowController
    {
        /// <summary>
        /// 主窗口的自动开始STT服务控制器
        /// </summary>
        public static AutoStartSTTServiceController Instance { get; private set; }
        public AutoStartSTTServiceController()
        {
            this.TargetWindowType = WindowType.Main;
            var showService = new SimpleAction(this, "ShowSTTService", DevExpress.Persistent.Base.PredefinedCategory.Tools);
            showService.Caption = "STT Service";
            showService.Execute += ShowService_Execute;
        }

        private void ShowService_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(os,STTService.Instance);
        }

        IObjectSpace os;
        protected override void OnActivated()
        {
            base.OnActivated();
            Start();
            Instance = this;
        }

        public void Start()
        {
            //Task.Run(() =>

            os = Application.CreateObjectSpace(typeof(STTService));
            if (STTService.Instance == null)
            {
                var s = os.GetObjectsQuery<STTService>().FirstOrDefault();
                if (s == null)
                {
                    throw new UserFriendlyException("Error!can't find the STT Service config ?");
                }
                STTService.Instance = s;
                if (s.State == STTServiceState.Running)
                {
                    s.State = STTServiceState.Stopped;
                }
            }

            if (STTService.Instance.State == STTServiceState.Stopped)
            {
                try
                {
                    var msg = STTService.Instance.Start();
                    os.CommitChanges();
                    Application.ShowViewStrategy.ShowMessage(msg);
                }
                catch (Exception ex)
                {
                    throw new UserFriendlyException(ex);
                }
            }

            //);
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            Stop();
        }

        public void Stop()
        {
            try
            {
                var msg = STTService.Instance.Stop();
                os.CommitChanges();
                Application.ShowViewStrategy.ShowMessage(msg);

            }
            catch (Exception ex)
            {
                throw new UserFriendlyException(ex);
            }
        }
    }
}
