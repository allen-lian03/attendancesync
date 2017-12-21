using Topshelf.Logging;

namespace ZKTeco.SyncBackendService.Bases
{
    public interface ILoggable
    {
        LogWriter Logger { set; }
    }
}
