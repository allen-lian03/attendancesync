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
            var connector = new WebApiConnector();
            var userId = connector.FindProjectWorkerByFaceId("592e2531b2ddc226f0df2b24", "1").GetAwaiter().GetResult();
            Assert.IsNotNull(userId);
            Assert.AreEqual(24, userId.Length);
        }
    }
}
