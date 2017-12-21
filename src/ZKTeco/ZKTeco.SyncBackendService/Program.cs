using Serilog;
using Topshelf;

namespace ZKTeco.SyncBackendService
{
    class Program
    {
        static void Main(string[] args)
        {       
            HostFactory.Run(cfg =>
            {
                var logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.RollingFile(@".\logs\info{Date}.log", retainedFileCountLimit: 10)                
                .CreateLogger();

                cfg.UseSerilog(logger);
                
                cfg.Service<ZKTecoServiceControl>(x =>
                {                   
                    x.ConstructUsing(_ => new ZKTecoServiceControl());
                    x.WhenStarted((s, h) => s.Start(h));
                    x.WhenStopped((s, h) => s.Stop(h));
                });

                cfg.RunAsLocalSystem();
                cfg.StartAutomatically();

                cfg.SetServiceName("ZKTeco-SyncBackendService");
                cfg.SetDisplayName("ZKTeco Sync Backend Service");
                cfg.SetDescription("ZKTeco Synchronize attendance log to CTMS.");

                cfg.OnException(ex =>
                {
                    logger.Error("OnException: {@ex}", ex);
                });
            });
        }
    }
}
