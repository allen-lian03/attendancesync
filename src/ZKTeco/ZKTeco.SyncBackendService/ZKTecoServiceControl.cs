using System;
using System.Threading;
using Topshelf;
using Topshelf.Logging;

namespace ZKTeco.SyncBackendService
{
    public class ZKTecoServiceControl : ServiceControl
    {
        private static readonly LogWriter _logger = HostLogger.Get<ZKTecoServiceControl>();

        private ManualResetEvent _signal;

        /// <summary>
        /// it describes OnElapsed is running.
        /// </summary>
        private int _running;

        private Thread[] _pool;

        public ZKTecoServiceControl()
        {
            _signal = new ManualResetEvent(false);
        }

        public bool Start(HostControl host)
        {       
            _logger.Info("Start starts...");
            try
            {
                _pool = new Thread[ZKTecoConfig.DeviceIPs.Length];
            }
            catch (Exception ex)
            {
                _logger.ErrorFormat("Start error:{@ex}", ex);
                return false;
            }            

            ThreadPool.QueueUserWorkItem(w =>
            {
                ExecuteTasks();
            });
            _logger.InfoFormat("Start ends.");
            return true;
        }


        public bool Stop(HostControl host)
        {
            _logger.InfoFormat("Stop starts.");
            _signal.Set();
            _logger.InfoFormat("Stop ends.");
            return true;
        }

        private void ExecuteTasks()
        {
            _logger.InfoFormat("ExecuteTasks starts.");

            _signal.WaitOne();

            _logger.InfoFormat("ExecuteTasks ends.");
        }
    }
}
