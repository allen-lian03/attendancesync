using Ninject.Modules;
using Quartz;
using ZKTeco.SyncBackendService.Bases;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Events;
using ZKTeco.SyncBackendService.Jobs;
using ZKTeco.SyncBackendService.Models;

namespace ZKTeco.SyncBackendService
{
    public class ZKTecoModule : NinjectModule
    {
        public override void Load()
        {
            Bind<EventHub>().ToSelf().InSingletonScope();
            Bind<SqliteConnector>().ToSelf().InSingletonScope();
            Bind<WebApiConnector>().ToSelf().InSingletonScope();

            Bind<ZKTecoServiceControl>().ToSelf();
            Bind<AttendanceQueue>().ToSelf();

            Bind<IJobHandler>().To<UploadAttendanceLogsHandler>();
            Bind<IJob>().To<ResendAttendancesJob>().Named("retry");
        }        
    }
}
