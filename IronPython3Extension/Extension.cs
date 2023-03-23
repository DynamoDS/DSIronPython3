using System;
using System.IO;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.Logging;

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

        public void Ready(ReadyParams sp)
        {
            //stolen from https://github.com/DynamoDS/Dynamo/blob/master/src/IronPythonExtension/IronPythonExtension.cs

            // Searches for DSIronPython engine binary in same folder with extension itself
            var targetDir = Path.GetDirectoryName(Assembly.GetAssembly(this.GetType()).Location);
            var libraryLoader = sp.StartupParams.LibraryLoader;
            Assembly pythonEvaluatorLib = null;
            try
            {
                pythonEvaluatorLib = Assembly.LoadFrom(Path.Combine(targetDir, PythonEvaluatorAssembly + ".dll"));
            }
            catch (Exception ex)
            {
                // Most likely the IronPython engine is excluded in this case
                // but logging the exception message in case for diagnose
                OnMessageLogged(LogMessage.Info(ex.Message));
                return;
            }
            // Import IronPython Engine into VM, so Python node using IronPython engine could evaluate correctly
            if (pythonEvaluatorLib != null)
            {
                libraryLoader.LoadNodeLibrary(pythonEvaluatorLib);
            }
        }

        public void Shutdown()
        {

        }

        public void Startup(StartupParams sp)
        {
        }
    }
}
