using System;
using System.Collections.Generic;
using System.Threading;
using Topshelf;
using Topshelf.Logging;
using zkemkeeper;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService
{
    public class ZKTecoServiceControl : ServiceControl, ILoggable
    {      

        private ManualResetEvent _signal;

        private List<ZKTecoConnector> _connectors;

        public LogWriter Logger { private get; set; }

        public ZKTecoServiceControl()
        {
            _signal = new ManualResetEvent(false);
            Logger = HostLogger.Get<ZKTecoServiceControl>();
        }        

        public bool Start(HostControl host)
        {
            Logger.Info("ZKTecoServiceControl starts...");
            try
            {
                var count = ZKTecoConfig.DeviceIPs.Length;
                // Initialize this thread pool. 
                _connectors = new List<ZKTecoConnector>(count);
                for (var i = 0; i < count; i++)
                {
                    var wrapper = new AxDeviceWrapper(
                        new CZKEMClass(),
                        ZKTecoConfig.DeviceIPs[i]);
                    
                    _connectors.Add(new ZKTecoConnector(wrapper));

                    Logger.InfoFormat("Device ip:{ip}, port:{port}.", ZKTecoConfig.DeviceIPs[i].Address, ZKTecoConfig.DeviceIPs[i].Port);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("Start error:{@ex}", ex);
                return false;
            }            

            ThreadPool.QueueUserWorkItem(w =>
            {
                ExecuteTasks();
            });
            Logger.InfoFormat("Start ends.");
            return true;
        }


        public bool Stop(HostControl host)
        {
            _signal.Set();
            foreach (var connector in _connectors)
            {
                connector.Stop();
            }
            Logger.InfoFormat("ZKTecoServiceControl stops.");
            return true;
        }

        private void ExecuteTasks()
        {
            Logger.InfoFormat("ExecuteTasks starts.");
            foreach (var connector in _connectors)
            {
                connector.Start();
            }
            _signal.WaitOne();
            Logger.InfoFormat("ExecuteTasks ends.");
        }
    }
}
