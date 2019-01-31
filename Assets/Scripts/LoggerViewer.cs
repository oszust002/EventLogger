using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoggerViewer : MonoBehaviour
{
    public Text LoggerText;    

    private void Start()
    {
        if (LoggerViewManager._loggerViewManager != null)
        {
            LoggerViewManager._loggerViewManager.RefreshView();
        }
    }

    public void SetLogs(List<string> logs)
    {
        var logText = "";

        foreach (var logEvent in logs)
        {
            logText += logEvent;
            logText += "\n";
        }

        LoggerText.text = logText;
    }
    
    
}
