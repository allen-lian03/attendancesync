using Newtonsoft.Json;
using System;
using System.Linq;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Events;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Jobs
{
    internal class SyncToWebHandler : IJobHandler
    {
        private SqliteConnector _db;
        
        private CTMSWebApiConnector _web;

        private LogWriter _logger;

        public SyncToWebHandler(SqliteConnector connector)
        {
            _db = connector;
            _web = new CTMSWebApiConnector();
            _logger = HostLogger.Get<SyncToWebHandler>();
        }

        public void Handle(EventMessage message)
        {
            if (message == null)
            {
                return;
            }

            var attendanceLog = JsonConvert.DeserializeObject<AttendanceLog>(message.Content);
            // Check attendance log.
            if (!EnsureUniqueAttendanceLog(attendanceLog))
            {
                _logger.InfoFormat("SyncToWebHandler:attendance log (id:{id}) exists", attendanceLog.Id);
                return;
            }
            
            // Check worker.            
            var workerId = EnsureCurrentWorker(attendanceLog);
            if (string.Empty == workerId)
            {
                _logger.InfoFormat("SyncToWebHandler:No map between {workerId} and {enrollNumber}", workerId, attendanceLog.UserId);
                return;
            }

            bool ok;
            switch(attendanceLog.DeviceType)
            {
                case DeviceType.In:
                    _db.AddAttendanceLog(attendanceLog);
                    ok = _web.CheckIn(attendanceLog.ProjectId, workerId, attendanceLog.LogDate, attendanceLog.DeviceName).GetAwaiter().GetResult();
                    if (ok)
                    {
                        _db.SyncAttendanceLogSuccess(attendanceLog.Id);
                    }
                    break;
                case DeviceType.Out:
                    _db.AddAttendanceLog(attendanceLog);
                    ok = _web.CheckOut(attendanceLog.ProjectId, workerId, attendanceLog.LogDate).GetAwaiter().GetResult();
                    if (ok)
                    {
                        _db.SyncAttendanceLogSuccess(attendanceLog.Id);
                    }
                    break;
                case DeviceType.InOut:
                    EnsureCheckInOrCheckOut(attendanceLog, workerId);
                    break;
                default:
                    _logger.ErrorFormat("Not support device type:{@attendance}", attendanceLog);
                    break;
            }         
        }

        private bool EnsureUniqueAttendanceLog(AttendanceLog log)
        {
            if (_db.HasSameAttandanceLog(log.Id))
            {
                // If this attendance log has been recorded,
                // we didn't handle it again.
                return false;
            }
            return true;
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

        private void EnsureCheckInOrCheckOut(AttendanceLog log, string workerId)
        {
            // the first attendance log in the specific date.
            var attendances = _db.GetAttendanceLogsWithinDate(log.UserId, log.LogDate);
            if (attendances.Count == 0)
            {
                _db.AddAttendanceLog(log);
                var ok = _web.CheckIn(log.ProjectId, workerId, log.LogDate, log.DeviceName).GetAwaiter().GetResult();
                if (ok)
                {
                    _db.SyncAttendanceLogSuccess(log.Id);
                }                
                return;
            }
            // TODO: missing in/out implement.
            var sum = attendances.Sum(a => (int)a.DeviceType);
        }
    }
}
