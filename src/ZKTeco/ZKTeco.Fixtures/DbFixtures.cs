using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZKTeco.SyncBackendService.Connectors;

namespace ZKTeco.Fixtures
{
    [TestClass]
    public class DbFixtures
    {
        [TestInitialize]
        public void InitDb()
        {
            SqliteConnector.InstallSyncDatabase();
        }

        [TestMethod]
        public void TestEnqueue()
        {
            var connector = new SqliteConnector();
            connector.Enqueue(new SyncBackendService.Models.AttendanceLog("1", 15, 15, 2017, 12, 27, 8, 8, 30, 0, 1));
        }

        [TestMethod]
        public void TestAttendanceLog()
        {
            var connector = new SqliteConnector();
            connector.InsertAttendanceLog(new SyncBackendService.Models.AttendanceLog("1", 15, 15, 2017, 12, 27, 8, 8, 31, 0, 1), false);
        }
    }
}
