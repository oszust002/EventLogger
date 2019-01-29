using System;

public class EventSettings
{
    public string EventTag { get; }
    public string EventType { get;  }
    public string EventName { get;  }
    public string AttributeName { get; }
    public float AttributeValue { get; }

    private EventSettings(string eventTag, string eventType, string eventName, string attributeName,
        float attributeValue)
    {
        EventTag = eventTag;
        EventType = eventType;
        EventName = eventName;
        AttributeName = attributeName;
        AttributeValue = attributeValue;
    }

    public string ToLogString(float time)
    {
        return $"{time};{EventTag};{EventType};{EventName};{AttributeName};{AttributeValue}";
    }

    public static EventSettingsBuilder Builder(string eventTag, string eventType,  string eventName)
    {
        if (eventTag == null) throw new ArgumentNullException(nameof(eventTag));
        if (eventType == null) throw new ArgumentNullException(nameof(eventType));
        if (eventName == null) throw new ArgumentNullException(nameof(eventName));
        return new EventSettingsBuilder(eventTag, eventType, eventName);
    }

    public class EventSettingsBuilder
    {
        private readonly string _eventTag;
        private readonly string _eventType;
        private readonly string _eventName;
        private string _attributeName = "NONE";
        private float _attributeValue = 0;

        internal EventSettingsBuilder(string eventTag, string eventType, string eventName)
        {
            _eventTag = eventTag;
            _eventType = eventType;
            _eventName = eventName;
        }

        public EventSettingsBuilder Attribute(string name, float value)
        {
            _attributeName = name;
            _attributeValue = value;
            return this;
        }
            
        public static implicit operator EventSettings(EventSettingsBuilder builder)
        {
            return new EventSettings(
                builder._eventTag, 
                builder._eventType, 
                builder._eventName, 
                builder._attributeName, 
                builder._attributeValue);
        }

        public EventSettings Build()
        {
            return this;
        }
    }
}