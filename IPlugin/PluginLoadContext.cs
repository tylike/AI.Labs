using DevExpress.ExpressApp;
using System.Reflection;
using System.Runtime.Loader;

namespace IPlugins
{


    public interface IPlugin<T> : IPlugin
    {
        void Invoke(T video,Controller controller);
    }

    public class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public Type BasePluginInterface => typeof(IPlugin);

        public PluginLoadContext(string pluginPath) : base(true)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == "IPlugin" || assemblyName.Name == "AI.Labs.Module" || assemblyName.Name.StartsWith("DevExpress"))
            {
                return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            }

            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }
    }

}
