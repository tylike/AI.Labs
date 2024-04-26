using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win;
using DevExpress.ExpressApp.Updating;
using DevExpress.ExpressApp.Win.Utils;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using AI.Labs.Module.BusinessObjects.STT;
using System.Diagnostics;
using AI.Labs.Module.BusinessObjects.KnowledgeBase;

namespace AI.Labs.Win;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Win.WinApplication._members
public class LabsWindowsFormsApplication : WinApplication {
    public LabsWindowsFormsApplication() {
		//SplashScreen = new DXSplashScreen(typeof(XafSplashScreen), new DefaultOverlayFormOptions());

        ApplicationName = "AI.Labs";
        CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
        UseOldTemplates = false;
        DatabaseVersionMismatch += LabsWindowsFormsApplication_DatabaseVersionMismatch;
        CustomizeLanguagesList += LabsWindowsFormsApplication_CustomizeLanguagesList;
        //WordProcesser.Load();
    }
    private void LabsWindowsFormsApplication_CustomizeLanguagesList(object sender, CustomizeLanguagesListEventArgs e) {
        string userLanguageName = System.Threading.Thread.CurrentThread.CurrentUICulture.Name;
        if(userLanguageName != "en-US" && e.Languages.IndexOf(userLanguageName) == -1) {
            e.Languages.Add(userLanguageName);
        }
    }
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UIHelper.UIInvoke = t =>
        {
            Debug.WriteLine("UI Invoke Start.");
            var wnd = MainWindow;
            if (wnd != null)
            {
                var form = wnd.Template as Form;
                form.Invoke(t);
                Debug.WriteLine("UI Invoke End.");
            }
            else
            {
                Debug.WriteLine("错误:仍在请求使用UI线程执行，但是MainWindow已经不存在了!");
            }
        };
        UIHelper.UIDoEvents = () =>
        {
            Application.DoEvents();
        };
    }
    private void LabsWindowsFormsApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
        e.Updater.Update();
        e.Handled = true;
#else
        if(System.Diagnostics.Debugger.IsAttached) {
            e.Updater.Update();
            e.Handled = true;
        }
        else {
			string message = "The application cannot connect to the specified database, " +
				"because the database doesn't exist, its version is older " +
				"than that of the application or its schema does not match " +
				"the ORM data model structure. To avoid this error, use one " +
				"of the solutions from the https://www.devexpress.com/kb=T367835 KB Article.";

			if(e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
				message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
			}
			throw new InvalidOperationException(message);
        }
#endif
    }
}
