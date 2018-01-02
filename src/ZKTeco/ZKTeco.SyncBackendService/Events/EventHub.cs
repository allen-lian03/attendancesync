using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;

namespace ZKTeco.SyncBackendService.Events
{
    /// <summary>
    /// Singleton
    /// </summary>
    public class EventHub
    {
        /// <summary>
        /// Inner subscribers.
        /// </summary>
        private ConcurrentDictionary<EventType, List<IJobHandler>> _subscribers;
        /// <summary>
        /// Inner message queue.
        /// </summary>
        private ConcurrentQueue<EventMessage> _innerHub;
             
        /// <summary>
        /// Inner timer is used for checking queue.
        /// </summary>
        private System.Timers.Timer _timer;

        /// <summary>
        /// logger
        /// </summary>
        private LogWriter _logger;

        /// <summary>
        /// running flag, 0: not run, 1: running.
        /// </summary>
        private int _running = 0;

        public EventHub()
        {
            _subscribers = new ConcurrentDictionary<EventType, List<IJobHandler>>();
            _innerHub = new ConcurrentQueue<EventMessage>();

            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += OnElapsed;
            _timer.Start();

            _logger = HostLogger.Get<EventHub>();
        }        

        public Task PublishAsync(EventMessage message)
        {
            return Task.Run(() =>
            {
                if (message != null)
                {
                    _innerHub.Enqueue(message);
                }
            });                       
        }

        public void Subscribe(EventType type, IJobHandler handler)
        {
            _subscribers.AddOrUpdate(type, new List<IJobHandler>(10), (key, list) =>
            {
                var index = list.FindIndex(m => m == handler);
                if (index == -1)
                {
                    list.Add(handler);
                }
                return list;
            });
        }

        private void OnElapsed(object sender, ElapsedEventArgs e)
        {
            _logger.Info("OnElapsed runs.");
            if (Interlocked.CompareExchange(ref _running, 1, 0) != 0)
            {
                _logger.InfoFormat("OnElapsed is running. Current Thread[{thread}]", Thread.CurrentThread.ManagedThreadId);
                return;
            }

            var target = (System.Timers.Timer)sender;
            if (target.Enabled == false)
            {
                _logger.Info("Timer has been disabled.");
                return;
            }

            _logger.Info("OnElapsed:timer stops.");
            target.Stop();

            EventMessage msg;
            while(_innerHub.TryDequeue(out msg))
            {
                if (msg != null)
                {
                    List<IJobHandler> handlers;
                    if (_subscribers.TryGetValue(msg.Type, out handlers))
                    {
                        foreach (var h in handlers)
                        {
                            h.Handle(msg);
                        }
                    }
                }
            }

            _logger.Info("OnElapsed:timer starts again.");
            target.Start();
            Interlocked.CompareExchange(ref _running, 0, 1);            
        }
    }
}
