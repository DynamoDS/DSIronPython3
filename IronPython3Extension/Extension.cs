using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Dynamo.Extensions;
using Dynamo.Graph.Workspaces;
using Dynamo.Logging;
using Dynamo.PythonServices;

namespace IronPython3Extension
{
    public class IronPython3Extension : IExtension, ILogSource
    {
        private const string PythonEvaluatorAssembly = "python3eval";

        #region ILogSource

        public event Action<ILogMessage> MessageLogged;
        internal void OnMessageLogged(ILogMessage msg)
        {
            if (this.MessageLogged != null)
            {
                MessageLogged?.Invoke(msg);
            }
        }
        #endregion

        public string UniqueId => "04fab216-e8d8-4db1-8fea-ac46367b5a58";

        public string Name => "IronPython3Extension";

        public void Dispose()
        {
            
        }

        /// <summary>
        /// Action to be invoked when the Dynamo has started up and is ready
        /// for user interaction. 
        /// </summary>
        /// <param name="rp"></param>
        public void Ready(ReadyParams rp)
        {
            var extraPath = Path.Combine(new FileInfo(Assembly.GetExecutingAssembly().Location).Directory.Parent.FullName, "extra");
            var alc = new IsolatedPythonContext(Path.Combine(extraPath, $"{PythonEvaluatorAssembly}.dll"));
            var dsIronAssem = alc.LoadFromAssemblyName(new AssemblyName(PythonEvaluatorAssembly));

            //load the engine into Dynamo ourselves.
            LoadPythonEngine(dsIronAssem);

            //we used to do this:
            //but it's not neccesary to load anything into the VM.
            //instead we skip all the extra work and trigger the side effect we want
            //which is re executing the graph after the dsIronPython evaluator is loaded into the PythonEngineManager.
            //rp.StartupParams.LibraryLoader.LoadNodeLibrary(dsIronAssem);

            if (rp.CurrentWorkspaceModel is HomeWorkspaceModel hwm)
            {
                foreach (var n in hwm.Nodes)
                {
                    n.MarkNodeAsModified(true);
                }
                hwm.Run();
            }
        }

        private static void LoadPythonEngine(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException($"Error while loading python engine - assembly {PythonEvaluatorAssembly}.dll was not loaded successfully.");
            }

            // Currently we are using try-catch to validate loaded assembly and Singleton Instance method exist
            // but we can optimize by checking all loaded types against evaluators interface later
            try
            {
                Type eType = null;
                PropertyInfo instanceProp = null;
                try
                {
                    eType = assembly.GetTypes().FirstOrDefault(x => typeof(PythonEngine).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
                    if (eType == null) return;

                    instanceProp = eType?.GetProperty("Instance", BindingFlags.NonPublic | BindingFlags.Static);
                    if (instanceProp == null) return;
                }
                catch
                {
                    // Ignore exceptions from iterating assembly types.
                    return;
                }

                PythonEngine engine = (PythonEngine)instanceProp.GetValue(null);
                if (engine == null)
                {
                    throw new Exception($"Could not get a valid PythonEngine instance by calling the {eType.Name}.Instance method");
                }

                if (PythonEngineManager.Instance.AvailableEngines.All(x => x.Name != engine.Name))
                {
                    PythonEngineManager.Instance.AvailableEngines.Add(engine);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to add a Python engine from assembly {assembly.GetName().Name}.dll with error: {ex.Message}");
            }
        }

        public void Shutdown()
        {

        }

        public void Startup(StartupParams sp)
        {

        }
    }
    internal class IsolatedPythonContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver resolver;

        public IsolatedPythonContext(string libPath)
        {
            resolver = new AssemblyDependencyResolver(libPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
