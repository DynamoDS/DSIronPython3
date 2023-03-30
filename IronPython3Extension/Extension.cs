using System;
using System.IO;
using System.Reflection;
using Dynamo.Extensions;
using Dynamo.Logging;

namespace IronPython3Extension
{
    public class IronPython3Extension : IExtension, ILogSource
    {
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

        }

        public void Shutdown()
        {

        }

        public void Startup(StartupParams sp)
        {

        }
    }
}
