using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MouseEventManager
{
    
    private Dictionary<KeyRepresentation, float> _buttonsDown = new Dictionary<KeyRepresentation, float>();
    private Dictionary<KeyRepresentation, Tuple<string, string>> _keyCodeAliases = new Dictionary<KeyRepresentation, Tuple<string, string>>();
    
    public void RecognizeMouseEvent(Event _event, float time)
    {
        var mouseKeyCode = GetMouseKeyCode(_event);
        var keyRepresentation = KeyRepresentation.Create(mouseKeyCode).WithModifiers(_event.modifiers);
        if (mouseKeyCode == KeyCode.None)
        {
            return;
        }

        if (_event.type == EventType.MouseDown)
        {
            if (_buttonsDown.ContainsKey(keyRepresentation)) return;
            _buttonsDown.Add(keyRepresentation, time);
            var eventSettingsBuilder = GetEventSettings(keyRepresentation, _event.type.ToString());
            
            EventManager.LogEvent(time, eventSettingsBuilder.Attribute("PositionX", _event.mousePosition.x));
            EventManager.LogEvent(time, eventSettingsBuilder.Attribute("PositionY", _event.mousePosition.y));
        }
        else if (_event.type == EventType.MouseUp)
        {
            var firstOrDefault = _buttonsDown.Keys.FirstOrDefault(representation => representation.KeyCode == mouseKeyCode);
            if (firstOrDefault != null)
            {
                var eventSettingsBuilder = GetEventSettings(firstOrDefault, _event.type.ToString());
            
                EventManager.LogEvent(time, eventSettingsBuilder.Attribute("PositionX", _event.mousePosition.x));
                EventManager.LogEvent(time, eventSettingsBuilder.Attribute("PositionY", _event.mousePosition.y));
                _buttonsDown.Remove(firstOrDefault);
            }
        }
    }
    
    private EventSettings.EventSettingsBuilder GetEventSettings(KeyRepresentation key, string type)
    {
        var tuple = _keyCodeAliases.ContainsKey(key)
            ? _keyCodeAliases[key]
            : Tuple.Create("Player", key.ToString());
        
        return EventSettings.Builder(tuple.Item1, type, tuple.Item2);
    }

    private KeyCode GetMouseKeyCode(Event _event)
    {
        switch (_event.button)
        {
                case 0:
                    return KeyCode.Mouse0;
                case 1:
                    return KeyCode.Mouse1;
                case 2:
                    return KeyCode.Mouse2;
                case 3:
                    return KeyCode.Mouse3;
                case 4:
                    return KeyCode.Mouse4;
                case 5:
                    return KeyCode.Mouse5;
                case 6:
                    return KeyCode.Mouse6;
                default:
                    return KeyCode.None;
        }
    }

    public bool HasKeyAlias(Event myEvent)
    {
        return _keyCodeAliases.ContainsKey(KeyRepresentation.FromEvent(myEvent));
    }
    
    public void AddKeyAlias(KeyRepresentation key, string tag, string aliasName)
    {
        _keyCodeAliases[key] = Tuple.Create(tag, aliasName);
    }
    
    
}