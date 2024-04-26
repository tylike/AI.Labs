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
using DevExpress.ExpressApp.Design;

namespace AI.Labs.Win;

public class ApplicationBuilder : IDesignTimeApplicationFactory {
    public static bool SkipLogin { get; set; } = true;
    public static WinApplication BuildApplication(string connectionString) {
        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        var builder = WinApplication.CreateBuilder();
        // Register custom services for Dependency Injection. For more information, refer to the following topic: https://docs.devexpress.com/eXpressAppFramework/404430/
        // builder.Services.AddScoped<CustomService>();
        // Register 3rd-party IoC containers (like Autofac, Dryloc, etc.)
        // builder.UseServiceProviderFactory(new DryIocServiceProviderFactory());
        // builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

        builder.UseApplication<LabsWindowsFormsApplication>();
        builder.Modules
            .AddCloningXpo()
            .AddConditionalAppearance()
            .AddDashboards(options =>
            {
                options.DashboardDataType = typeof(DevExpress.Persistent.BaseImpl.DashboardData);
                options.DesignerFormStyle = DevExpress.XtraBars.Ribbon.RibbonFormStyle.Ribbon;
            })
            .AddFileAttachments()
            .AddNotifications()
            .AddOffice()
            .AddReports(options =>
            {
                options.EnableInplaceReports = true;
                options.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportDataV2);
                options.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
            })
            .AddTreeListEditors()
            .AddValidation(options =>
            {
                options.AllowValidationDetailsAccess = false;
            })
            .AddViewVariants()
            .Add<AI.Labs.Module.LabsModule>()
            .Add<LabsWinModule>();
            //.Add<LabsSTTModule>();

        builder.ObjectSpaceProviders
            .AddXpo((app, opt) =>
            {

                opt.ConnectionString = connectionString;
                opt.ThreadSafe = true;

            })
            //.AddSecuredXpo((application, options) =>
            //{
            //    options.ConnectionString = connectionString;
            //    //options.ThreadSafe = true;
            //})
            .AddNonPersistent();
        if (!SkipLogin)
        {
            builder.Security
                .UseIntegratedMode(options =>
                {
                    options.RoleType = typeof(PermissionPolicyRole);
                    options.UserType = typeof(AI.Labs.Module.BusinessObjects.ApplicationUser);
                    options.UserLoginInfoType = typeof(AI.Labs.Module.BusinessObjects.ApplicationUserLoginInfo);
                    options.UseXpoPermissionsCaching();
                    options.Events.OnSecurityStrategyCreated += securityStrategy =>
                    {
                        // Use the 'PermissionsReloadMode.NoCache' option to load the most recent permissions from the database once
                        // for every Session instance when secured data is accessed through this instance for the first time.
                        // Use the 'PermissionsReloadMode.CacheOnFirstAccess' option to reduce the number of database queries.
                        // In this case, permission requests are loaded and cached when secured data is accessed for the first time
                        // and used until the current user logs out. 
                        // See the following article for more details: https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Security.SecurityStrategy.PermissionsReloadMode.
                        ((SecurityStrategy)securityStrategy).PermissionsReloadMode = PermissionsReloadMode.NoCache;
                    };
                })
                .UsePasswordAuthentication();
        }
        builder.AddBuildStep(application => {
            application.ConnectionString = connectionString;
#if DEBUG
            if(System.Diagnostics.Debugger.IsAttached && application.CheckCompatibilityType == CheckCompatibilityType.DatabaseSchema) {
                application.DatabaseUpdateMode = DatabaseUpdateMode.UpdateDatabaseAlways;
            }
#endif
        });
        //var xpo = typeof(XPOHolder);
        var winApplication = builder.Build();
        return winApplication;
    }

    private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        return args.RequestingAssembly;
    }

    XafApplication IDesignTimeApplicationFactory.Create()
        => BuildApplication(XafApplication.DesignTimeConnectionString);
}
