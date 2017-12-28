using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using ZKTeco.SyncBackendService.Configs;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService
{
    /// <summary>
    /// ZKTecoConfig is a helper class for configuration.
    /// It only reads configuration value, 
    /// but ZKTeco.Configuration will write configuration value and may restart the service. 
    /// </summary>
    internal static class ZKTecoConfig
    {
        private static DeviceConfig[] _devices;
        /// <summary>
        /// Device information
        /// </summary>
        public static DeviceConfig[] Devices
        {
            get
            {
                if (_devices == null)
                {
                    var devices = new List<DeviceConfig>(10);
                    var group = ConfigurationManager.GetSection("deviceGroup") as DeviceConfigurationSectionHandler;
                    if (group != null)
                    {
                        foreach (var device in group.Devices)
                        {
                            var config = device as DeviceConfiguration;
                            if (config == null)
                            {
                                continue;
                            }

                            devices.Add(new DeviceConfig
                            {
                                DeviceName = config.Name,
                                IP = config.IP,
                                Port = config.Port,
                                Type = config.Type
                            });
                        }
                    }
                    _devices = devices.ToArray();
                }                
                return _devices;
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
                    var api = GetStringValue("ApiRootUrl");
                    if (!api.EndsWith("/"))
                    {
                        api += "/";
                    }
                    _apiRootUrl = api;
                }
                return _apiRootUrl;
            }
        }

        private static string _apiToken;

        public static string ApiToken
        {
            get
            {
                if (_apiToken == null)
                {
                    _apiToken = GetStringValue("ApiToken");
                }
                return _apiToken;
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
