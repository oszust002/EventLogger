using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


//TODO: Add mouse recognition (up/down/drag)
// example games
// what other events? not much generic, because leaving this to user (other inputs!!)
// Saving to file (mostly done)
// joysticks/other mouse events (use KeyCodeGroups and Input.Get*)
// axis (same as above, change when value significantly changes)
public class EventManager : MonoBehaviour
{
    public LoggerViewer LoggerViewer;
    public bool LogAllKeys = true;
    
    
    private StreamWriter writer;
    private string logPath;

    private Dictionary<string, Action> _eventDictionary;

    private static EventManager _eventManager;

    private Dictionary<KeyRepresentation, float> _buttonsDown = new Dictionary<KeyRepresentation, float>();
    private Dictionary<KeyRepresentation, Tuple<string, string>> _keyCodeAliases = new Dictionary<KeyRepresentation, Tuple<string, string>>();
    

    public static EventManager Instance
    {
        get
        {
            if (!_eventManager)
            {
                _eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
                if (!_eventManager || _eventManager == null)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    _eventManager.Init();
                }
            }

            return _eventManager;
        }
    }

    private void Start()
    {
        logPath = "./logs/eventlog_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".log";
        AddKeyAlias(KeyRepresentation.Create(KeyCode.K), "Playerlll", "KPRESS");
        InvokeRepeating(nameof(RecordKeys), 0, 0.001f);
    }

    private void RecordKeys()
    {
        var myEvent = new Event();
        while (Event.PopEvent(myEvent))
        {
//            Debug.Log(myEvent);
            var time = Time.time;
            if (myEvent.isKey)
            {
                if (myEvent.keyCode == KeyCode.None)
                {
                    continue;
                }

                if (!LogAllKeys && !_keyCodeAliases.ContainsKey(KeyRepresentation.FromEvent(myEvent)))
                {
                    continue;
                }

//                Debug.Log(myEvent.rawType);

                RecognizeKeyEvent(myEvent, time);
            }
            else if (myEvent.isMouse)
            {
                RecognizeMouseEvent(myEvent, time);
            }
        }
//        Debug.Log(Input.GetKey(KeyCode.LeftControl));
//        Debug.Log(Input.GetAxis("Horizontal"));
        foreach (var joystickKeyCode in KeyCodeGroups.SpecificJoystick)
        {
            
            if (Input.GetKey(joystickKeyCode))
            {
                Debug.Log(joystickKeyCode);
            }
        }
    }

    private void RecognizeMouseEvent(Event _event, float time)
    {
        Debug.Log(_event.keyCode);
    }

    private void RecognizeKeyEvent(Event _event, float time)
    {
        KeyRepresentation keyRepresentation = KeyRepresentation.FromEvent(_event);
        if (_event.rawType == EventType.KeyDown)
        {
            if (_buttonsDown.ContainsKey(keyRepresentation)) return;
            _buttonsDown.Add(keyRepresentation, time);
            LogEvent(time, GetEventSettings(keyRepresentation, _event.rawType.ToString()));
        }
        else if (_event.rawType == EventType.KeyUp)
        {
            if (IsSingleDownModifierKey(_event))
            {
                var representationKeys = GetModifiedRepresentationKeys(_event);
                representationKeys.Add(keyRepresentation);
                representationKeys.ForEach(representation => LogKeyDown(_event, time, representation));
            }
            else
            {
                LogKeyDown(_event, time, keyRepresentation);
            }
        }
    }

    private List<KeyRepresentation> GetModifiedRepresentationKeys(Event _event)
    {
        return _buttonsDown.Keys
            .Where(representation => representation.WasModifiedByKey(_event.keyCode)).ToList();
    }

    private bool IsSingleDownModifierKey(Event _event)
    {
        if (!(_event.keyCode == KeyCode.LeftAlt || _event.keyCode == KeyCode.RightAlt ||
              _event.keyCode == KeyCode.LeftControl || _event.keyCode == KeyCode.RightControl ||
              _event.keyCode == KeyCode.LeftShift || _event.keyCode == KeyCode.RightShift))
        {
            return false;
        }
        
        return !_buttonsDown.Keys.Any(representation =>
        {
            switch (_event.keyCode)
            {
                case KeyCode.LeftAlt:
                    return representation.KeyCode.Equals(KeyCode.RightAlt);
                case KeyCode.RightAlt:
                    return representation.KeyCode.Equals(KeyCode.LeftAlt);
                case KeyCode.LeftControl:
                    return representation.KeyCode.Equals(KeyCode.RightControl);
                case KeyCode.RightControl:
                    return representation.KeyCode.Equals(KeyCode.LeftControl);
                case KeyCode.LeftShift:
                    return representation.KeyCode.Equals(KeyCode.RightShift);
                case KeyCode.RightShift:
                    return representation.KeyCode.Equals(KeyCode.LeftShift);
                default:
                    return false;
            }
        });
        
    }

    private void LogKeyDown(Event _event, float time, KeyRepresentation keyRepresentation)
    {
        if (_buttonsDown.ContainsKey(keyRepresentation))
        {
            var holdTime = time - _buttonsDown[keyRepresentation];
            if (holdTime < 0.2f)
            {
                LogEvent(time, GetEventSettings(keyRepresentation, _event.rawType.ToString()));
            }
            else
            {
                LogEvent(time,
                    GetEventSettings(keyRepresentation, _event.rawType.ToString())
                        .Attribute("HoldTime", holdTime));
            }
        }

        _buttonsDown.Remove(keyRepresentation);
    }

    private void Init()
    {
        if (_eventDictionary == null)
        {
            _eventDictionary = new Dictionary<string, Action>();
        }
    }

    public static void AddKeyAlias(KeyRepresentation key, string tag, string aliasName)
    {
        Instance._keyCodeAliases.Add(key, Tuple.Create(tag, aliasName));
    }

    private EventSettings.EventSettingsBuilder GetEventSettings(KeyRepresentation key, string type)
    {
        var tuple = _keyCodeAliases.ContainsKey(key)
            ? _keyCodeAliases[key]
            : Tuple.Create("Player", key.ToString());
        
        return EventSettings.Builder(tuple.Item1, type, tuple.Item2);
    }

    public static void LogEvent(float time, string tag, string type, string eventName, string attributeName = null,
        float? attributeValue = null)
    {
        var eventSettingsBuilder = EventSettings.Builder(tag, type, eventName);

        var attrName = string.IsNullOrEmpty(attributeName) ? "NONE" : attributeName;
        var attrValue = attributeValue ?? 0;
        eventSettingsBuilder.Attribute(attrName, attrValue);

        LogEvent(time, eventSettingsBuilder);
    }

    public static void LogEvent(float time, EventSettings eventSettings)
    {
        Instance.Log(time, eventSettings);
    }

    private void Log(float time, EventSettings eventSettings)
    {
        var eventString = eventSettings.ToLogString(time);
        WriteText(eventString);
        LoggerViewer.AddEvent(eventString);
    }

    public static void StartListening(string eventName, Action listener)
    {
        Action thisEvent = null;
        if (Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            Instance._eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            Instance._eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, Action listener)
    {
        if (_eventManager == null) return;
        Action thisEvent;
        if (Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            Instance._eventDictionary[eventName] = thisEvent;
        }
    }

    public static void TriggerEvent(string tag, string eventName)
    {
        float time = Time.time;
        Action thisEvent;
        if (Instance._eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            Instance.Log(time, EventSettings.Builder(tag, "Invoke", eventName));
            thisEvent.Invoke();
        }
    }
    
    private void WriteText(string line)
    {
        if (writer == null)
        {
            var fileExists = File.Exists(logPath);
            writer = File.AppendText(logPath);
            if (!fileExists)
            {
                writer.WriteLine("time;eventTag;eventType;eventName;attributeName;attributeValue");
            } else
            {
                writer.WriteLine("");
            }
        }
        writer.WriteLine(line);
    }

    private void OnApplicationQuit()
    {
        writer?.Close();
    }
}