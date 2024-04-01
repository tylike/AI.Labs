using System.Reflection;
using System.Runtime.Loader;

namespace IPlugins
{


    public interface IPlugin<T> : IPlugin
    {
        void Invoke(T video);
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
            if (assemblyName.Name == "IPlugin" || assemblyName.Name == "AI.Labs.Module")
            //if (assemblyName.Name == typeof(主程序集中的一个类名).Assembly.FullName == assemblyName.FullName)  这样可以吗?

            {
                return AssemblyLoadContext.Default.LoadFromAssemblyName(assemblyName);
            }

            string assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return assemblyPath != null ? LoadFromAssemblyPath(assemblyPath) : null;
        }
    }

}
