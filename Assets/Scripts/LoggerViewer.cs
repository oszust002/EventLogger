using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoggerViewer : MonoBehaviour
{
    public Text LoggerText;
    
    public int MaxLines = 10;
    
    private List<string> _loggerLogList = new List<string>();


    private void Start()
    {
        LoggerText.text = "";
    }

    public void AddEvent(string eventString)
    {
        _loggerLogList.Add(eventString);

        if (_loggerLogList.Count > MaxLines)
        {
            _loggerLogList.RemoveAt(0);
        }

        var logText = "";

        foreach (var logEvent in _loggerLogList)
        {
            logText += logEvent;
            logText += "\n";
        }

        if (LoggerText != null)
        {
            LoggerText.text = logText;
        }

    }
    
    
}
