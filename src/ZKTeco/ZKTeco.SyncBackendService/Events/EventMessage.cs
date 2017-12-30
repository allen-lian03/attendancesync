namespace ZKTeco.SyncBackendService.Events
{
    public enum EventType
    {
        SyncWeb = 0
    }

    public class EventMessage
    {
        public EventMessage(EventType type, string id, long referenceId, string content)
        {
            Type = type;
            ReferenceId = referenceId;
            Content = content;
        }

        public EventType Type { get; set; }

        public string Content { get; set; }

        public long ReferenceId { get; set; }

        public string Id { get; set; }
    }
}
