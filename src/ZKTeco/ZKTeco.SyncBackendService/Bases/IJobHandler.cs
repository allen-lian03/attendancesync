using ZKTeco.SyncBackendService.Events;

namespace ZKTeco.SyncBackendService.Bases
{
    public interface IJobHandler
    {
        void Handle(EventMessage message);
    }
}
