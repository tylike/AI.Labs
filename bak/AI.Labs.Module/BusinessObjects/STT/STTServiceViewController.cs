
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;

namespace AI.Labs.Module.BusinessObjects.STT
{
    public class STTServiceViewController : ObjectViewController<ObjectView, STTService>
    {
        public STTServiceViewController()
        {
            var start = new SimpleAction(this, "StartSTTService", null);
            start.Caption = "Start";
            start.ImageName = "GettingStarted";
            start.Execute += Start_Execute;
            start.TargetObjectsCriteria = $"{nameof(STTService.State)} == '{nameof(STTServiceState.Stopped)}'";

            var stop = new SimpleAction(this, "StopSTTService", null);
            stop.Execute += Stop_Execute;
            stop.ImageName = "Stop";
            stop.Caption = "Stop";
            stop.TargetObjectsCriteria = $"{nameof(STTService.State)} == '{nameof(STTServiceState.Running)}'";
        }

        private void Stop_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            AutoStartSTTServiceController.Instance.Stop();
        }

        private void Start_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            AutoStartSTTServiceController.Instance.Start();
        }
    }
}
