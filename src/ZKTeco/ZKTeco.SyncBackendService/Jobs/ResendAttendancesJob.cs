using Quartz;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Jobs
{
    public class ResendAttendancesJob : IJob
    {
        private SqliteConnector _db;

        private WebApiConnector _web;

        private LogWriter _logger;

        public ResendAttendancesJob(SqliteConnector connector)
        {
            _db = connector;
            _web = new WebApiConnector();
            _logger = HostLogger.Get<ResendAttendancesJob>();
        }

        public void Execute(IJobExecutionContext context)
        {
            _logger.Info("ResendAttendancesJob Execute starts...");
            var logs = _db.GetUnsyncAttendanceLogs();
            _logger.InfoFormat("Failed attendance logs count:{count}", logs.Count);
            foreach (var log in logs)
            {
                var workerId = EnsureCurrentWorker(log);
                switch (log.LogStatus)
                {
                    case AttendanceStatus.CheckIn:
                        EnsureCheckIn(log, workerId);
                        break;
                    case AttendanceStatus.CheckOut:
                        EnsureCheckOut(log, workerId);
                        break;
                    default:
                        _logger.ErrorFormat("Not support device type:{@attendance}", log);
                        break;
                }
            }
            _logger.Info("ResendAttendancesJob Execute ends.");
        }        

        private string EnsureCurrentWorker(AttendanceLog log)
        {
            var workerId = _db.GetWorkerIdByEnrollNumberAndProjectId(log.UserId, log.ProjectId);
            if (!string.IsNullOrWhiteSpace(workerId))
            {
                return workerId;
            }

            //request
            workerId = _web.FindProjectWorkerByFaceId(log.ProjectId, log.UserId).GetAwaiter().GetResult();
            if (!string.IsNullOrWhiteSpace(workerId))
            {
                _db.AddWorker(new WorkerInfo
                {
                    UserId = workerId,
                    EnrollNumber = log.UserId,
                    ProjectId = log.ProjectId
                });
                return workerId;
            }

            return string.Empty;
        }

        private void EnsureCheckIn(AttendanceLog attendanceLog, string workerId)
        {
            var ok = _web.CheckIn(attendanceLog.ProjectId, workerId, attendanceLog.LogDate,
                attendanceLog.DeviceName).GetAwaiter().GetResult();
            if (ok)
            {
                _db.UploadAttendanceLogSuccess(attendanceLog.Id);
            }
        }

        private void EnsureCheckOut(AttendanceLog attendanceLog, string workerId)
        {
            var ok = _web.CheckOut(attendanceLog.ProjectId, workerId, attendanceLog.LogDate)
                .GetAwaiter().GetResult();
            if (ok)
            {
                _db.UploadAttendanceLogSuccess(attendanceLog.Id);
            }
        }
        
    }
}
