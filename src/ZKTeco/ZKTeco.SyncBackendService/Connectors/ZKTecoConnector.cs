using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Topshelf.Logging;
using zkemkeeper;

namespace ZKTeco.SyncBackendService.Connectors
{
    internal class ZKTecoConnector
    {
        /// <summary>
        /// If failing to connect to the device, 
        /// it will reconnect several times.
        /// </summary>
        private int _retryTimes = 0;

        private CZKEM _axCZKEM;

        private LogWriter _logger;

        /// <summary>
        /// Device's IP
        /// </summary>
        private string _ip;

        /// <summary>
        /// Device's port
        /// </summary>
        private int _port;

        public ZKTecoConnector(CZKEM axCZKEM, string ip, int port)
        {
            _axCZKEM = axCZKEM;
            _ip = ip;
            _port = port;
            _logger = HostLogger.Get<ZKTecoConnector>();
        }

        public void Connect()
        {
            while (!_axCZKEM.Connect_Net(_ip, _port))
            {
                if (_retryTimes > ZKTecoConfig.RetryTimes)
                {
                    int errorCode = 0;
                    _axCZKEM.GetLastError(ref errorCode);
                    _logger.ErrorFormat("Unable to connect the device({IP}:{Port}), ErrorCode({ErrorCode})",
                        _ip, _port, errorCode);
                    break;
                }

                Thread.Sleep(5000);
                _retryTimes++;
            }
        }

        public void RegisterEvents()
        {
            //if (_axCZKEM.RegEvent())
            //{
            //    _axCZKEM.OnAttTransactionEx += OnAttTransactionEx;
            //}            
        }

        /// <summary>
        /// If your fingerprint(or your card) passes the verification,this event will be triggered
        /// </summary>
        /// <param name="enrollNumber">UserID of a user</param>
        /// <param name="isInValid">Whether a record is valid. 1: Not valid. 0: Valid.</param>
        /// <param name="attState"></param>
        /// <param name="verifyMethod"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="workCode">
        /// work code returned during verification. 
        /// Return 0 when the device does not support work code.
        /// </param>
        private void OnAttTransactionEx(string enrollNumber, int isInValid, int attState, int verifyMethod, 
            int year, int month, int day, int hour, int minute, int second, int workCode)
        {
            if (isInValid > 0)
            {
                // The current user doesn't pass the verification.
                return;
            }

            

        }
    }
}
