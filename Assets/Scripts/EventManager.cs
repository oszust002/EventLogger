﻿using System;
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
    private KeyEventManager _keyEventManager = new KeyEventManager();
    
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

                if (!LogAllKeys && !_keyEventManager.HasKeyAlias(myEvent))
                {
                    continue;
                }
                _keyEventManager.RecognizeKeyEvent(myEvent, time);
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

    private void Init()
    {
        if (_eventDictionary == null)
        {
            _eventDictionary = new Dictionary<string, Action>();
        }
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