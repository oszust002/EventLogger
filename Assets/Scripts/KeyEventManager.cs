using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class KeyEventManager
{
    
    private Dictionary<KeyRepresentation, float> _buttonsDown = new Dictionary<KeyRepresentation, float>();
    private Dictionary<KeyRepresentation, Tuple<string, string>> _keyCodeAliases = new Dictionary<KeyRepresentation, Tuple<string, string>>();


    public void RecognizeKeyEvent(Event _event, float time)
    {
        KeyRepresentation keyRepresentation = KeyRepresentation.FromEvent(_event);
        if (_event.type == EventType.KeyDown)
        {
            RegisterKeyDown(_event, time, keyRepresentation);
        }
        else if (_event.type == EventType.KeyUp)
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

    private void RegisterKeyDown(Event _event, float time, KeyRepresentation keyRepresentation)
    {
        if (_buttonsDown.ContainsKey(keyRepresentation)) return;
        _buttonsDown.Add(keyRepresentation, time);
        EventManager.LogEvent(time, GetEventSettings(keyRepresentation, _event.type.ToString()));
    }

    public bool HasKeyAlias(Event myEvent)
    {
        return _keyCodeAliases.ContainsKey(KeyRepresentation.FromEvent(myEvent));
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
                EventManager.LogEvent(time, GetEventSettings(keyRepresentation, _event.type.ToString()));
            }
            else
            {
                EventManager.LogEvent(time,
                    GetEventSettings(keyRepresentation, _event.type.ToString())
                        .Attribute("HoldTime", holdTime));
            }
        }

        _buttonsDown.Remove(keyRepresentation);
    }
    
    private EventSettings.EventSettingsBuilder GetEventSettings(KeyRepresentation key, string type)
    {
        var tuple = _keyCodeAliases.ContainsKey(key)
            ? _keyCodeAliases[key]
            : Tuple.Create("Player", key.ToString());
        
        return EventSettings.Builder(tuple.Item1, type, tuple.Item2);
    }

    public void AddKeyAlias(KeyRepresentation key, string tag, string aliasName)
    {
        _keyCodeAliases[key] = Tuple.Create(tag, aliasName);
    }
}