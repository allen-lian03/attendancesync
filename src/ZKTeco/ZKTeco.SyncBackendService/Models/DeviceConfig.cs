namespace ZKTeco.SyncBackendService.Models
{

    public class DeviceConfig
    {
        public string DeviceName { get; set; }

        public string IP { get; set; }

        public int Port { get; set; }

        public DeviceType Type { get; set; }
    }
}
