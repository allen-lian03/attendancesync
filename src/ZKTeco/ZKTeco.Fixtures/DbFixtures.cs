using Microsoft.VisualStudio.TestTools.UnitTesting;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Models;
using ZKTeco.SyncBackendService.Utils;

namespace ZKTeco.Fixtures
{
    [TestClass]
    public class DbFixtures
    {
        private SqliteConnector _connector;

        [TestInitialize]
        public void InitDb()
        {
            DbInstaller.Install();

            _connector = new SqliteConnector();
        }

        [TestCleanup]
        public void Dispose()
        {
            if (_connector != null)
            {
                _connector.Dispose();
            }
        }

        [TestMethod]
        public void TestEnqueue()
        {
            //_connector.Enqueue(new AttendanceLog("1", 15, 15, 2017, 12, 27, 8, 8, 30, 0, 1, "gate-01", SyncBackendService.Models.DeviceType.InOut));
        }

        [TestMethod]
        public void TestAttendanceLog()
        {
            //_connector.AddAttendanceLog(
            //    new AttendanceLog("1", 15, 15, 2017, 12, 27, 8, 8, 31, 0, 1, "gate-01", DeviceType.InOut));
        }

        [TestMethod]
        public void TestCheckInOut01()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 16, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckIn();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 17, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckOut, status);
        }

        [TestMethod]
        public void TestCheckInOut02()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 9, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckIn();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 9, 10, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckIn, status);
        }

        [TestMethod]
        public void TestCheckInOut03()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 9, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckIn();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 10, 10, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckOut, status);
        }

        [TestMethod]
        public void TestCheckInOut04()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 10, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckIn();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 28, 8, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckIn, status);
        }

        [TestMethod]
        public void TestCheckInOut05()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 18, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckOut();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 28, 7, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckIn, status);
        }

        [TestMethod]
        public void TestCheckInOut06()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 9, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckIn();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 10, 10, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckOut, status);
        }

        [TestMethod]
        public void TestCheckInOut07()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 27, 15, 0, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckIn();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 28, 2, 10, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckOut, status);
        }

        [TestMethod]
        public void TestCheckInOut08()
        {
            var lastLog = new AttendanceLog("1", 15, 15, 2017, 12, 28, 2, 10, 0, 0, 1, "gate-01", DeviceType.InOut);
            lastLog.CheckOut();
            var currLog = new AttendanceLog("1", 15, 15, 2017, 12, 28, 10, 10, 0, 0, 1, "gate-01", DeviceType.InOut);
            var status = currLog.CalculateStatus(lastLog);
            Assert.AreEqual(AttendanceStatus.CheckIn, status);
        }       
    }
}
