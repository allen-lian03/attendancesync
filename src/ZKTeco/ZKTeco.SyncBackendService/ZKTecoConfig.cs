using System;
using System.Configuration;
using System.Net;

namespace ZKTeco.SyncBackendService
{
    /// <summary>
    /// ZKTecoConfig is a helper class for configuration.
    /// It only reads configuration value, 
    /// but ZKTeco.Configuration will write configuration value and may restart the service. 
    /// </summary>
    internal static class ZKTecoConfig
    {
        private static IPEndPoint[] _ips;

        public static IPEndPoint[] DeviceIPs
        {
            get
            {
                if (_ips == null)
                {
                    var addresses = GetStringValue("DeviceIPs").Split(';');
                    _ips = new IPEndPoint[addresses.Length];
                    for (var i = 0; i < addresses.Length; i++)
                    {
                        var address = addresses[i].Split(':');
                        switch (address.Length)
                        {
                            case 2:
                                _ips[i] = new IPEndPoint(IPAddress.Parse(address[0]), int.Parse(address[1]));
                                break;
                            case 1:
                                _ips[i] = new IPEndPoint(IPAddress.Parse(address[0]), 4370);
                                break;
                            default:
                                continue;
                        }                     
                    }
                }
                return _ips;
            }
        }

        private static int _retryTimes = -1;
        /// <summary>
        /// How many times does it retries when some errors occur.
        /// </summary>
        public static int RetryTimes
        {
            get
            {
                if (_retryTimes < 0)
                {
                    _retryTimes = GetIntValue("RetryTime", 3);
                }
                return _retryTimes;
            }
        }

        private static string _apiRootUrl;
        /// <summary>
        /// Where does sync data go?
        /// </summary>
        public static string ApiRootUrl
        {
            get
            {
                if (_apiRootUrl == null)
                {
                    _apiRootUrl = GetStringValue("ApiRootUrl");
                }
                return _apiRootUrl;
            }
        }

        private static string _projectCode;
        /// <summary>
        /// Project code which the devices belong to
        /// </summary>
        public static string ProjectCode
        {
            get
            {
                if (_projectCode == null)
                {
                    _projectCode = GetStringValue("ProjectCode");
                }
                return _projectCode;
            }
        }


        private static string _appRootFolder;
        /// <summary>
        /// The service program's root folder.
        /// </summary>
        public static string AppRootFolder
        {
            get
            {
                if (_appRootFolder == null)
                {
                    _appRootFolder = GetStringValue("AppRootFolder",
                        AppDomain.CurrentDomain.BaseDirectory);
                }
                return _appRootFolder;
            }
        }

        private static int GetIntValue(string key, int defaultValue)
        {
            var value = ConfigurationManager.AppSettings[key];
            int result;
            if (int.TryParse(value, out result))
            {
                return result;
            }
            return defaultValue;
        }

        private static string GetStringValue(string key, string defaultValue = "")
        {
            var value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrWhiteSpace(value))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                {
                    throw new ArgumentException(string.Format("Missing '{0}' setting", key));
                }
                return defaultValue;
            }
            return value;
        }
    }
}
