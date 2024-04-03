using System.Configuration;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ApplicationBuilder;
using DevExpress.ExpressApp.Win.ApplicationBuilder;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Win;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.XtraEditors;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Win.Utils;
using System.Reflection;
using DevExpress.Xpo.DB;
using AI.Labs.Module.BusinessObjects.VideoTranslate;
using AI.Labs.Module.BusinessObjects;
using System.IO;

namespace AI.Labs.Win;

static class Program {
    private static bool ContainsArgument(string[] args, string argument) {
        return args.Any(arg => arg.TrimStart('/').TrimStart('-').ToLower() == argument.ToLower());
    }
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    public static int Main(string[] args) {
        //GoogleTranslateFix.FixFile("D:\\videoInfo\\ChatGPT VS Gemini ultra\\cn.srt", "D:\\videoInfo\\ChatGPT VS Gemini ultra\\cn.fixed.srt");
        //(TimeSpan loc, int ms, string text)[] times = new (TimeSpan loc, int ms, string text)[]
        //{
        //    (TimeSpan.Parse("00:00:00"), 1000, "这是1个测试"),
        //    //(TimeSpan.Parse("00:00:01"), 1000, "这是2个测试"),
        //    //(TimeSpan.Parse("00:00:02"), 1000, "这是3个测试"),
        //    //(TimeSpan.Parse("00:00:03"), 1000, "这是4个测试"),
        //    //(TimeSpan.Parse("00:00:04"), 1000, "这是5个测试"),
        //    //(TimeSpan.Parse("00:00:05"), 1000, "这是6个测试"),
        //    //(TimeSpan.Parse("00:00:06"), 1000, "这是7个测试"),
        //    //(TimeSpan.Parse("00:00:07"), 1000, "这是8个测试"),
        //    //(TimeSpan.Parse("00:00:08"), 1000, "这是9个测试")
        //};
        //VideoHelper.AddFreezeFramesWithText("D:\\videoInfo\\39\\GGUF-GPTQ.mp4", "D:\\videoInfo\\39\\GGUF-GPTQ-yan.mp4", times);
        //Console.ReadLine();

        if(ContainsArgument(args, "help") || ContainsArgument(args, "h")) {
            Console.WriteLine("Updates the database when its version does not match the application's version.");
            Console.WriteLine();
            Console.WriteLine($"    {Assembly.GetExecutingAssembly().GetName().Name}.exe --updateDatabase [--forceUpdate --silent]");
            Console.WriteLine();
            Console.WriteLine("--forceUpdate - Marks that the database must be updated whether its version matches the application's version or not.");
            Console.WriteLine("--silent - Marks that database update proceeds automatically and does not require any interaction with the user.");
            Console.WriteLine();
            Console.WriteLine($"Exit codes: 0 - {DBUpdaterStatus.UpdateCompleted}");
            Console.WriteLine($"            1 - {DBUpdaterStatus.UpdateError}");
            Console.WriteLine($"            2 - {DBUpdaterStatus.UpdateNotNeeded}");
            return 0;
        }
        DevExpress.ExpressApp.FrameworkSettings.DefaultSettingsCompatibilityMode = DevExpress.ExpressApp.FrameworkSettingsCompatibilityMode.Latest;
#if EASYTEST
        DevExpress.ExpressApp.Win.EasyTest.EasyTestRemotingRegistration.Register();
#endif
        WindowsFormsSettings.LoadApplicationSettings();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
		DevExpress.Utils.ToolTipController.DefaultController.ToolTipType = DevExpress.Utils.ToolTipType.SuperTip;
        if(Tracing.GetFileLocationFromSettings() == DevExpress.Persistent.Base.FileLocation.CurrentUserApplicationDataFolder) {
            Tracing.LocalUserAppDataPath = Application.LocalUserAppDataPath;
        }
        Tracing.Initialize();

        string connectionString = null;
        //if(ConfigurationManager.ConnectionStrings["ConnectionString"] != null) {
        //    connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
        //}
        var dbPath = Path.Combine(Application.StartupPath, "ai.labs.s3db");//"D:\\dev\\AI.Labs\\AI.Labs.Win\\bin\\Debug\\net7.0-windows\\ai.labs.s3db"
        connectionString = DevExpress.Xpo.DB.SQLiteConnectionProvider.GetConnectionString(dbPath);
#if EASYTEST
        if(ConfigurationManager.ConnectionStrings["EasyTestConnectionString"] != null) {
            connectionString = ConfigurationManager.ConnectionStrings["EasyTestConnectionString"].ConnectionString;
        }
#endif
        ArgumentNullException.ThrowIfNull(connectionString);
        var winApplication = ApplicationBuilder.BuildApplication(connectionString);

        if (ContainsArgument(args, "updateDatabase")) {
            using var dbUpdater = new WinDBUpdater(() => winApplication);
            return dbUpdater.Update(
                forceUpdate: ContainsArgument(args, "forceUpdate"),
                silent: ContainsArgument(args, "silent"));
        }

        try {
            winApplication.Setup();
            winApplication.Start();
        }
        catch(Exception e) {
            winApplication.StopSplash();
            winApplication.HandleException(e);
        }
        return 0;
    }
}
