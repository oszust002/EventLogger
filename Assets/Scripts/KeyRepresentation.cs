using System;
using System.Collections.Generic;
using UnityEngine;

//This class uses bit enum operations because of Flags modifier on EventModifiers 
public class KeyRepresentation
{
    public KeyCode KeyCode { get; private set; }
    public EventModifiers Modifiers { get; private set; }

    private KeyRepresentation(KeyCode keyCode)
    {
        KeyCode = keyCode;
    }

    public KeyRepresentation WithModifiers(params EventModifiers[] eventModifiers)
    {
        foreach (var modifier in eventModifiers)
        {
            Modifiers |= modifier;
        }
        
        if (KeyCode == KeyCode.LeftAlt || KeyCode == KeyCode.RightAlt)
        {
            Modifiers &= ~EventModifiers.Alt;
        }
        if (KeyCode == KeyCode.LeftShift || KeyCode == KeyCode.RightShift)
        {
            Modifiers &= ~EventModifiers.Shift;
        }
        if (KeyCode == KeyCode.LeftControl || KeyCode == KeyCode.RightControl || KeyCode == KeyCode.RightAlt)
        {
            Modifiers &= ~EventModifiers.Control;
        }
        Debug.Log(Modifiers + " " + KeyCode);
        return this;
    }

    public bool WasModifiedByKey(KeyCode keyCode)
    {
        switch (keyCode)
        {
            case KeyCode.LeftControl:
            case KeyCode.RightControl:
                return Modifiers.HasFlag(EventModifiers.Control);
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt:
                return Modifiers.HasFlag(EventModifiers.Alt);
            case KeyCode.LeftShift:
            case KeyCode.RightShift:
                return Modifiers.HasFlag(EventModifiers.Shift);
            default:
                return false;
        }
    }

    public override string ToString()
    {
        var result = "";
        if (Modifiers.HasFlag(EventModifiers.Control))
        {
            result += "Ctrl+";
        }

        if (Modifiers.HasFlag(EventModifiers.Shift))
        {
            result += "Shift+";
        }

        if (Modifiers.HasFlag(EventModifiers.Alt))
        {
            result += "Alt+";
        }

        return result + KeyCode;
    }

    public static KeyRepresentation Create(KeyCode keyCode)
    {
        return new KeyRepresentation(keyCode);
    }

    public static KeyRepresentation FromEvent(Event _event)
    {
        if (_event.keyCode == null || _event.keyCode == KeyCode.None)
        {
            throw new Exception("Cannot map to KeyRepresentation from event without KeyCode!");
        }

        var keyRepresentation = Create(_event.keyCode).WithModifiers(_event.modifiers);
        return keyRepresentation;
    }

    //Equals generated by JetBrains Rider
    protected bool Equals(KeyRepresentation other)
    {
        return KeyCode == other.KeyCode && Equals(Modifiers, other.Modifiers);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((KeyRepresentation) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int) KeyCode * 397) ^ (Modifiers != null ? Modifiers.GetHashCode() : 0);
        }
    }
}