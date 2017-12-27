using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Topshelf.Logging;
using ZKTeco.SyncBackendService.Bases;

namespace ZKTeco.SyncBackendService.Events
{
    /// <summary>
    /// Singleton
    /// </summary>
    internal class EventHub
    {
        private ConcurrentDictionary<EventType, List<IJobHandler>> _subscribers;

        private ConcurrentQueue<EventMessage> _innerHub;

        private static readonly EventHub _instance = new EventHub();

        private System.Timers.Timer _timer;

        private LogWriter _logger;

        private EventHub()
        {
            _subscribers = new ConcurrentDictionary<EventType, List<IJobHandler>>();
            _innerHub = new ConcurrentQueue<EventMessage>();

            _timer = new System.Timers.Timer(5000);
            _timer.Elapsed += OnElapsed;

            _logger = HostLogger.Get<EventHub>();
        }

        public static EventHub Instance
        {
            get { return _instance; }
        }

        public void PublishAsync(EventMessage message)
        {
            ThreadPool.QueueUserWorkItem(w =>
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
            var target = (System.Timers.Timer)sender;
            if (target == null || target.Enabled == false)
            {
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

            target.Start();
            _logger.Info("OnElapsed:timer starts again.");
        }
    }
}
