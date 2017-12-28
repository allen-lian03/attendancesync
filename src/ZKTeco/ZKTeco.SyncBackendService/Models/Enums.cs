using System;

namespace ZKTeco.SyncBackendService.Models
{
    /// <summary>
    /// it describes one device is checkin, checkout or checkin&checkout.
    /// </summary>
    public enum DeviceType
    {
        /// <summary>
        /// Out type means, workers only go out from this door, not go in.
        /// </summary>
        Out = -1,
        /// <summary>
        /// InOut type means, workers may go in / out from this door.
        /// </summary>
        InOut = 0,
        /// <summary>
        /// In type means, workers only go in from this door, not go out.
        /// </summary>
        In = 1    
    }

    [Obsolete]
    public enum AttendanceState
    {
        CheckIn = 0,
        CheckOut = 1,
        BreakOut = 2,
        BreakIn = 3,
        OTIn = 4,
        OTOut = 5
    }

    [Obsolete]
    public enum VerificationMode
    {
        Password = 0,
        Fingerprint = 1,
        Card = 2
    }
}
