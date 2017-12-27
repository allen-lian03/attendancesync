using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZKTeco.SyncBackendService.Connectors;

namespace ZKTeco.Fixtures
{
    [TestClass]
    public class WebApiFixtures
    {
        [TestMethod]
        public void TestFindProjectWorkerByFaceId()
        {
            var connector = new CTMSWebApiConnector();
            var userId = connector.FindProjectWorkerByFaceId("58a284fdfc127333859204f0", "face001").GetAwaiter().GetResult();
            Assert.IsNotNull(userId);
        }
    }
}
