using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZKTeco.SyncBackendService.Models
{
    public enum AttendanceState
    {
        CheckIn = 0,
        CheckOut = 1,
        BreakOut = 2,
        BreakIn = 3,
        OTIn = 4,
        OTOut = 5
    }

    public enum VerificationMode
    {
        Password = 0,
        Fingerprint = 1,
        Card = 2
    }
}
