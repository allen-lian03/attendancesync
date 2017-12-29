using Newtonsoft.Json;
using System;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Events;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService.Jobs
{
    public class SyncToWebHandler : IJobHandler
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

            switch(attendanceLog.DeviceType)
            {
                case DeviceType.In:
                    EnsureCheckIn(attendanceLog, workerId);
                    break;
                case DeviceType.Out:
                    EnsureCheckOut(attendanceLog, workerId);
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

        private void EnsureCheckIn(AttendanceLog attendanceLog, string workerId)
        {
            attendanceLog.CheckIn();
            _db.AddAttendanceLog(attendanceLog);
            var ok = _web.CheckIn(attendanceLog.ProjectId, workerId, attendanceLog.LogDate,
                attendanceLog.DeviceName).GetAwaiter().GetResult();
            if (ok)
            {
                _db.SyncAttendanceLogSuccess(attendanceLog.Id);
            }
        }

        private void EnsureCheckOut(AttendanceLog attendanceLog, string workerId)
        {
            attendanceLog.CheckOut();
            _db.AddAttendanceLog(attendanceLog);
            var ok = _web.CheckOut(attendanceLog.ProjectId, workerId, attendanceLog.LogDate)
                .GetAwaiter().GetResult();
            if (ok)
            {
                _db.SyncAttendanceLogSuccess(attendanceLog.Id);
            }
        }

        private void EnsureCheckInOrCheckOut(AttendanceLog log, string workerId)
        {            
            var attendance = _db.GetLastAttendanceLogByEnrollNumber(log.UserId);
            AttendanceStatus status = AttendanceStatus.Unknown;
            try
            {
                status = log.CalculateStatus(attendance);
            }
            catch (NotSupportedException ex)
            {
                _logger.ErrorFormat("EnsureCheckInOrCheckOut error:{exception}, attendance:{@log}, worker:{id}", 
                    ex.Message, attendance, workerId);
                return;
            }
            
            if (status == AttendanceStatus.CheckIn)
            {
                EnsureCheckIn(log, workerId);
                return;
            }

            if (status == AttendanceStatus.CheckOut)
            {
                EnsureCheckOut(log, workerId);
                return;
            }

            _logger.ErrorFormat("EnsureCheckInOrCheckOut: Unknown status, attendance:{@log}, worker:{id}", attendance, workerId);
        }
    }
}
