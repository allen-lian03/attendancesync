using Newtonsoft.Json;
using System;
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
            var workerId = EnsureCurrentWorker(attendanceLog);
            if (string.Empty == workerId)
            {
                _logger.InfoFormat("No map between {workerId} and {enrollNumber}", workerId, attendanceLog.UserId);
                return;
            }



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
    }
}
