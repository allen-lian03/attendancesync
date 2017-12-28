using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using ZKTeco.SyncBackendService.Configs;

namespace ZKTeco.Fixtures
{
    [TestClass]
    public class ConfigFixture
    {
        [TestMethod]
        public void TestReadSectionGroup()
        {
            var section = ConfigurationManager.GetSection("deviceGroup");
            Assert.IsNotNull(section);
            Assert.IsTrue(section is DeviceConfigurationSectionHandler);
            var h = (DeviceConfigurationSectionHandler)section;
            Assert.AreEqual(2, h.Devices.Count);
            foreach (var d in h.Devices)
            {
                var device = d as DeviceConfiguration;
                Assert.IsNotNull(device);
                Assert.IsTrue(device.Name != string.Empty);
            }
        }
    }
}
