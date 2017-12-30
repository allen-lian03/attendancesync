using Quartz;
using Serilog;
using System.Diagnostics;
using System.IO;
using Topshelf;
using Topshelf.Ninject;
using Topshelf.Quartz;
using Topshelf.Quartz.Ninject;
using ZKTeco.SyncBackendService.Jobs;
using ZKTeco.SyncBackendService.Utils;

namespace ZKTeco.SyncBackendService
{
    class Program
    {
        static void Main(string[] args)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.AppSettings()
                .WriteTo.RollingFile(Path.Combine(ZKTecoConfig.AppRootFolder, "logs", "info{Date}.log"), retainedFileCountLimit: 10)
                .CreateLogger();           

            HostFactory.Run(cfg =>
            {             
                cfg.UseSerilog(logger);

                cfg.UseNinject(new ZKTecoModule());

                cfg.BeforeInstall(_ =>
                {
                    // When building this project, copy libs/*.dll to output directory.
                    logger.Information("RegisterDLLs starts...");
#if x64
                    //x64
                    RegisterDLLs("sdk_install", "x64", logger);
#else
                    //x86
                    RegisterDLLs("sdk_install", "x86", logger);
#endif
                    logger.Information("RegisterDLLs ends.");

                    logger.Information("InstallSyncDatabase starts...");

                    DbInstaller.Install();

                    logger.Information("InstallSyncDatabase ends.");
                });

                cfg.AfterUninstall(() =>
                {
                    logger.Information("UnregisterDLLs starts...");
#if x64
                    //x64
                    RegisterDLLs("sdk_uninstall", "x64", logger);
#else
                    //x86
                    RegisterDLLs("sdk_uninstall", "x86", logger);
#endif
                    logger.Information("UnregisterDLLs ends.");
                });

                cfg.Service<ZKTecoServiceControl>(x =>
                {
                    x.ConstructUsingNinject();
                    x.WhenStarted((s, h) => s.Start(h));
                    x.WhenStopped((s, h) => s.Stop(h));

                    // retry failed attendance logs.
                    x.UseQuartzNinject();
                    x.ScheduleQuartzJob(q =>
                    {
                        q.WithJob(() =>
                        {
                            return JobBuilder.Create<ResendAttendancesJob>().WithIdentity("retryJob", "syncbackend").Build();
                        }).AddTrigger(() =>
                        {
                            return TriggerBuilder.Create()
                            .WithIdentity("retryTrigger", "syncbackend")
                            .StartNow()
                            .WithSimpleSchedule(b => b.WithIntervalInHours(1).RepeatForever())
                            .Build();
                        });
                    });                    
                });

                cfg.RunAsLocalService();
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

        static void RegisterDLLs(string command, string platform, ILogger logger)
        {
            logger.Information("{command}.bat {platform}", command, platform);
            var pi = new ProcessStartInfo("cmd.exe",
                string.Format("/c {0}.bat {1}", Path.Combine(ZKTecoConfig.AppRootFolder, command), platform))
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var p = Process.Start(pi);
            p.OutputDataReceived += (sender, e) => logger.Information("{command} Output:{data}.", command, e.Data);
            p.BeginOutputReadLine();
            p.ErrorDataReceived += (sender, e) => logger.Error("{command} Error:{error}", command, e.Data);
            p.BeginErrorReadLine();

            p.WaitForExit();
            var exitCode = p.ExitCode;
            p.Close();

            logger.Information("{command} ExitCode: {code}.", command, exitCode);
        }
    }
}
