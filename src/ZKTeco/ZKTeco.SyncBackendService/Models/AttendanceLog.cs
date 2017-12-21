using System;

namespace ZKTeco.SyncBackendService.Models
{
    internal class AttendanceLog
    {
        public AttendanceLog(string enrollNumber, AttendanceState state, VerificationMode mode, 
            int year, int month, int day, int hour, int minute, int second, int workCode)
        {
            UserId = enrollNumber;
            State = state;
            Mode = mode;
            LogDate = new DateTime(year, month, day, hour, minute, second);
            WorkCode = workCode;
        }

        public string UserId { get; private set; }

        public AttendanceState State { get; private set; }

        public VerificationMode Mode { get; private set; }

        public int WorkCode { get; private set; }

        public DateTime LogDate { get; private set; }
    }
}
