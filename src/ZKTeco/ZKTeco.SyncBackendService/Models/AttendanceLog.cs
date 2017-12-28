﻿using System;
using ZKTeco.SyncBackendService.Utils;

namespace ZKTeco.SyncBackendService.Models
{
    public class AttendanceLog
    {
        private AttendanceLog(string enrollNumber, int state, int mode, 
            int workCode, int machineNumber, string deviceName, DeviceType type)
        {
            UserId = enrollNumber;
            State = state;
            Mode = mode;
            WorkCode = workCode;
            MachineId = machineNumber;
            DeviceName = deviceName;
            DeviceType = type;
        }

        public AttendanceLog(string enrollNumber, int state, int mode, 
            int year, int month, int day, int hour, int minute, int second, 
            int workCode, int machineNumber, string deviceName, DeviceType type) 
            : this(enrollNumber, state, mode, workCode, machineNumber, deviceName, type)
        {           
            LogDate = new DateTime(year, month, day, hour, minute, second);           
            ProjectId = ZKTecoConfig.ProjectCode;
            Id = IdGenerator.GenerateId(enrollNumber, LogDate, ProjectId);            
        }

        /// <summary>
        /// Read instance from database.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="enrollNumber"></param>
        /// <param name="state"></param>
        /// <param name="mode"></param>
        /// <param name="logDate"></param>
        /// <param name="workCode"></param>
        /// <param name="machineNumber"></param>
        /// <param name="projectId"></param>
        /// <param name="deviceName"></param>
        /// <param name="type"></param>
        public AttendanceLog(long id, string enrollNumber, int state, int mode,
            DateTime logDate, int workCode, int machineNumber, string projectId,
            string deviceName, DeviceType type) : this(enrollNumber, state, mode, workCode, machineNumber, deviceName, type)
        {
            Id = id;            
            LogDate = logDate;
            ProjectId = projectId;
        }

        public long Id { get; private set; }

        public string UserId { get; private set; }

        public int State { get; private set; }

        public int Mode { get; private set; }

        public int WorkCode { get; private set; }

        public DateTime LogDate { get; private set; }

        public string ProjectId { get; private set; }

        public int MachineId { get; private set; }
        /// <summary>
        /// Fingerprint/Facial recognition device name.
        /// It is one device worker is using.
        /// </summary>
        public string DeviceName { get; private set; }
        /// <summary>
        /// It describes the device is checkin, checkout or checkin&out.
        /// </summary>
        public DeviceType DeviceType { get; private set; }
    }
}
