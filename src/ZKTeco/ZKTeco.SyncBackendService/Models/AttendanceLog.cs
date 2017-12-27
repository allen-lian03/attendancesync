using System;
using ZKTeco.SyncBackendService.Utils;

namespace ZKTeco.SyncBackendService.Models
{
    public class AttendanceLog
    {
        public AttendanceLog(string enrollNumber, int state, int mode, 
            int year, int month, int day, int hour, int minute, int second, int workCode, int machineNumber)
        {
            UserId = enrollNumber;
            State = state;
            Mode = mode;
            LogDate = new DateTime(year, month, day, hour, minute, second);
            WorkCode = workCode;
            MachineId = machineNumber;

            ProjectId = ZKTecoConfig.ProjectCode;
            Id = IdGenerator.GenerateId(enrollNumber, LogDate, ProjectId);            
        }

        public long Id { get; private set; }

        public string UserId { get; private set; }

        public int State { get; private set; }

        public int Mode { get; private set; }

        public int WorkCode { get; private set; }

        public DateTime LogDate { get; private set; }

        public string ProjectId { get; private set; }

        public int MachineId { get; private set; }        
    }
}
