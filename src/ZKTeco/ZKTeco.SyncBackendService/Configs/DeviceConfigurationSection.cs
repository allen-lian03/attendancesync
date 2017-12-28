using System.Configuration;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Configs
{
    public class DeviceConfigurationSectionHandler: ConfigurationSection
    {
        [ConfigurationProperty("devices")]
        [ConfigurationCollection(typeof(DeviceConfigurationCollection))]
        public DeviceConfigurationCollection Devices
        {
            get { return (DeviceConfigurationCollection)base["devices"]; }
            set { base["devices"] = value; }
        }
        
    }

    [ConfigurationCollection(typeof(DeviceConfigurationCollection))]
    public class DeviceConfigurationCollection : ConfigurationElementCollection
    { 
        protected override ConfigurationElement CreateNewElement()
        {
            return new DeviceConfiguration();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            var config = (DeviceConfiguration)element;
            return config.Name;
        }

        public DeviceConfiguration this[int index]
        {
            get { return (DeviceConfiguration)BaseGet(index); }
        }

        protected override string ElementName
        {
            get { return "device"; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMapAlternate; }
        }
    }

    public class DeviceConfiguration : ConfigurationElement
    {
        [ConfigurationProperty("name",
            IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("ip",
            IsRequired = true,
            IsKey = true)]
        public string IP
        {
            get { return (string)this["ip"]; }
            set { this["ip"] = value; }
        }

        [ConfigurationProperty("port",
            DefaultValue = 4370,
            IsRequired = false)]
        public int Port
        {
            get { return (int)this["port"]; }
            set { this["port"] = value; }
        }

        [ConfigurationProperty("type",
            IsRequired = true)]
        public DeviceType Type
        {
            get { return (DeviceType)this["type"]; }
            set { this["type"] = value; }
        }
    }
}
