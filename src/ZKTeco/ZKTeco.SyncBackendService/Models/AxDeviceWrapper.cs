using System.Diagnostics;
using System.Net;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Timers;
using Topshelf.Logging;
using zkemkeeper;

namespace ZKTeco.SyncBackendService.Models
{    
    internal class AxDeviceWrapper
    {
        /// <summary>
        /// 0: disconnected, 1: connected.
        /// </summary>
        private int _connected = 0;

        /// <summary>
        /// Timer is running?
        /// 0: no, 1: yes.
        /// </summary>
        private int _running = 0;

        /// <summary>
        /// Log Writer
        /// </summary>
        private LogWriter _logger;

        /// <summary>
        /// Grab logs from device periodically
        /// </summary>
        private System.Timers.Timer _timer;

        public AxDeviceWrapper(CZKEMClass axCZKEM, IPEndPoint ip)
        {
            Device = axCZKEM;
            IP = ip.Address.ToString();
            Port = ip.Port;

            _logger = HostLogger.Get<AxDeviceWrapper>();
            _timer = new System.Timers.Timer(2000);
            _timer.Elapsed += OnElapsed;
        }
        
        public CZKEMClass Device { get; private set; }

        public string IP { get; private set; }

        public int Port { get; private set; }

        public int MachineNumber { get { return 1; } }

        [HandleProcessCorruptedStateExceptions]
        public bool Connnect()
        {
            try
            {
                return Device.Connect_Net(IP, Port);
            }
            catch
            {
                return false;
            }            
        }

        public bool RegisterAllEvents()
        {
            Interlocked.CompareExchange(ref _connected, 1, 0);
            return Device.RegEvent(MachineNumber, 65535);
        }

        public void Disconnect()
        {
            _timer.Stop();
            _timer.Elapsed -= OnElapsed;

            Interlocked.CompareExchange(ref _connected, 0, 1);
            Device.Disconnect();        
        }

        public void StartRealTimeLogs()
        {
            _timer.Start();          
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref _running, 1, 0) == 1)
            {
                // running.
                _logger.InfoFormat("OnElapsed is running under thread({id}).", Thread.CurrentThread.ManagedThreadId);
                return;
            }

            var target = (System.Timers.Timer)sender;
            if (!target.Enabled)
            {
                _logger.Info("Device timer stops.");
                return;
            }

            if (_connected == 0)
            {
                _logger.Info("Device connection will disconnect...");
                return;
            }

            _timer.Stop();
            _logger.InfoFormat("OnElapsed({id}) starts...", Thread.CurrentThread.ManagedThreadId);

            var watch = new Stopwatch();
            watch.Start();
            if (Device.ReadRTLog(MachineNumber))
            {
                _logger.Info("Device.ReadRTLog");
                while (Device.GetRTLog(MachineNumber))
                {
                    ;                 
                }
                _logger.Info("Device.GetRTLog");
            }
            watch.Stop();

            _logger.InfoFormat("Stopwatch:{ms}.", watch.ElapsedMilliseconds);
            _logger.InfoFormat("OnElapsed({id}) ends.", Thread.CurrentThread.ManagedThreadId);

            Interlocked.CompareExchange(ref _running, 0, 1);

            _timer.Start();
        }
    }
}
