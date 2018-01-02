using Newtonsoft.Json;
using ZKTeco.SyncBackendService.Connectors;
using ZKTeco.SyncBackendService.Events;

namespace ZKTeco.SyncBackendService.Models
{
    public class AttendanceQueue
    {
        private SqliteConnector _db;

        private EventHub _hub;

        public AttendanceQueue(SqliteConnector connector, EventHub hub)
        {
            _db = connector;
            _hub = hub;
        }

        public void Enqueue(AttendanceLog model)
        {
            var message = JsonConvert.SerializeObject(model);
            var id = _db.Enqueue(model.Id, message);
            _hub.PublishAsync(new EventMessage(EventType.SyncWeb, id, model.Id, message)).GetAwaiter().GetResult();
        }        
    }
}
