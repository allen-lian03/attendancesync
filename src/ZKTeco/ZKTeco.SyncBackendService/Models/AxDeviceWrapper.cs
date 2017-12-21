using System.Net;
using System.Threading;
using zkemkeeper;

namespace ZKTeco.SyncBackendService.Models
{
    internal class AxDeviceWrapper
    {
        /// <summary>
        /// 0: disconnected, 1: connected.
        /// </summary>
        private int _connected = 0;

        public AxDeviceWrapper(CZKEMClass axCZKEM, IPEndPoint ip)
        {
            Device = axCZKEM;
            IP = ip.Address.ToString();
            Port = ip.Port;
        }

        public CZKEMClass Device { get; private set; }

        public string IP { get; private set; }

        public int Port { get; private set; }

        public int MachineNumber { get { return 1; } }

        public bool Connnect()
        {
            return Device.Connect_Net(IP, Port);
        }

        public bool RegisterAllEvents()
        {
            Interlocked.CompareExchange(ref _connected, 1, 0);
            return Device.RegEvent(MachineNumber, 65535);
        }

        public void Disconnect()
        {
            Interlocked.CompareExchange(ref _connected, 0, 1);
            Device.Disconnect();
        }

        public void StartRealTimeLogs()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                if (Device.ReadRTLog(MachineNumber))
                {
                    while (Device.GetRTLog(MachineNumber))
                    {
                        if (_connected == 0)
                        {
                            break;
                        }
                    }
                }
            });            
        }
    }
}
