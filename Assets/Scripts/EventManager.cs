using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;


//TODO: Add mouse recognition (up/down/drag)
// example games
// what other events? not much generic, because leaving this to user (other inputs!!)
public class EventManager : MonoBehaviour
{
    public LoggerViewManager LoggerViewManager;
    public bool LogAllKeys = true;
    public bool LogToFile = true;
    public bool LogRawAxisValues;
    public bool Enabled = true;
    
    [Range(0.15f, 0.8f)]
    public float MinimumAxisValueDifference = 0.3f;

    private float lastHorizontalAxis = 0;
    private float lastVerticalAxis = 0;

    private const string LogsFolder = "./logs";
    private StreamWriter writer;
    private string logPath;

    private Dictionary<string, Action> _eventDictionary;

    private static EventManager _eventManager;
    private readonly KeyEventManager _keyEventManager = new KeyEventManager();
    private readonly MouseEventManager _mouseEventManager = new MouseEventManager();
    //Workaround, because build initializes _eventManager somehow
    private bool _configured;
    
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
        if(_eventManager == null)
        {
            DontDestroyOnLoad(gameObject);
            _eventManager = this;
            _eventManager.Init();
        }
        else
        {
            if(_eventManager != this)
            {
                Destroy (gameObject);
            }
            else
            {
                DontDestroyOnLoad(gameObject);
                _eventManager.Init();
            }
        }
        
    }

    private void RecordKeys()
    {
        if (!Enabled)
        {
            return;
        }
        
        var time = Time.time;
        var myEvent = new Event();
        while (Event.PopEvent(myEvent))
        {
            Debug.Log(myEvent.type);
            if (myEvent.isKey)
            {
                if (myEvent.keyCode == KeyCode.None)
                {
                    continue;
                }

                if (!LogAllKeys && !_keyEventManager.HasKeyAlias(myEvent))
                {
                    continue;
                }
                _keyEventManager.RecognizeKeyEvent(myEvent, time);
            }
            else if (myEvent.isMouse)
            {
                if (!LogAllKeys && !_mouseEventManager.HasKeyAlias(myEvent))
                {
                    continue;
                }
                _mouseEventManager.RecognizeMouseEvent(myEvent, time);
            }
        }
        //Remove when events will start sending shift
        CheckKeyCode(KeyCode.LeftShift, time);
        CheckKeyCode(KeyCode.RightShift, time);
        
        HandleAxis();
        foreach (var joystickKeyCode in KeyCodeGroups.SpecificJoystick)
        {
            CheckKeyCode(joystickKeyCode, time);
        }
    }

    private void CheckKeyCode(KeyCode keyCode, float time)
    {
        if (Input.GetKeyDown(keyCode))
        {
            _keyEventManager.RecognizeKeyEvent(BuildEvent(keyCode, EventType.KeyDown), time);
        }
        else if (Input.GetKeyUp(keyCode))
        {
            _keyEventManager.RecognizeKeyEvent(BuildEvent(keyCode, EventType.KeyUp), time);
        }
    }

    private Event BuildEvent(KeyCode joystickKeyCode, EventType keyDown)
    {
        var eventModifiers = GetCurrentModifiers();
        var _event = new Event();
        _event.keyCode = joystickKeyCode;
        _event.type = keyDown;
        _event.modifiers = eventModifiers;
        return _event;

    }

    private EventModifiers GetCurrentModifiers()
    {
        EventModifiers eventModifiers = EventModifiers.None;
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.RightAlt))
        {
            eventModifiers |= EventModifiers.Control;
        }
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            eventModifiers |= EventModifiers.Shift;
        } 
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            eventModifiers |= EventModifiers.Control;
        }

        return eventModifiers;
    }

    private void Init()
    {
        if (_configured)
        {
            return;
        }
        
        if (_eventDictionary == null)
        {
            _eventDictionary = new Dictionary<string, Action>();
        }
        
        logPath = LogsFolder + "/eventlog_" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".log";
        
        if (LoggerViewManager == null)
        {
            LoggerViewManager = FindObjectOfType(typeof(LoggerViewManager)) as LoggerViewManager;
        }
        AddKeyAlias(KeyRepresentation.Create(KeyCode.K), "Playerlll", "KPRESS");
        _configured = true;
        
        InvokeRepeating(nameof(RecordKeys), 0, 0.001f);
    }

    private void HandleAxis()
    {
        float time = Time.time;
        var horizontal = LogRawAxisValues ? Input.GetAxisRaw("Horizontal") : Input.GetAxis("Horizontal");
        var vertical = LogRawAxisValues ? Input.GetAxisRaw("Vertical") : Input.GetAxis("Vertical");

        if (Math.Abs(horizontal - lastHorizontalAxis) > MinimumAxisValueDifference)
        {
            LogEvent(time, "Player", "AxisInput","Horizontal", "Value", horizontal);
            lastHorizontalAxis = horizontal;
        }
        
        if (Math.Abs(vertical - lastVerticalAxis) > MinimumAxisValueDifference)
        {
            LogEvent(time, "Player", "AxisInput","Vertical", "Value", horizontal);
            lastVerticalAxis = vertical;
        }
    }

    public static void Enable()
    {
        if (!Instance.Enabled) 
        {
            Instance.Init();
        }
        Instance.Enabled = true;
    }

    public static void Disable()
    {
        Instance.Enabled = false;
        Instance.writer?.Close();
    }

    public static bool IsEnabled()
    {
        return Instance.Enabled;
    }

    public static void AddKeyAlias(KeyRepresentation key, string tag, string aliasName)
    {
        Instance._keyEventManager.AddKeyAlias(key, tag, aliasName);
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
        if (!Enabled)
        {
            return;
        }
        var eventString = eventSettings.ToLogString(time);
        WriteText(eventString);
        LoggerViewManager.AddEvent(eventString);
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
        if (!LogToFile)
        {
            return;
        }
        
        if (writer == null)
        {
            Directory.CreateDirectory(LogsFolder);
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