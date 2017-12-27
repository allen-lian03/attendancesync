namespace ZKTeco.SyncBackendService.Events
{
    public enum EventType
    {
        SyncWeb = 0,
        Archieve,
    }

    public class EventMessage
    {
        public EventMessage(EventType type, long key, string content)
        {
            Type = type;
            Key = key;
            Content = content;
        }

        public EventType Type { get; set; }

        public string Content { get; set; }

        public long Key { get; set; }
    }
}
