using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggerViewManager : MonoBehaviour
{
    public LoggerViewer LoggerViewer;
    
    public int MaxLines = 10;
    
    private List<string> _loggerLogList = new List<string>();

    public static LoggerViewManager _loggerViewManager;
    
    // Start is called before the first frame update
    void Start()
    {
        if(_loggerViewManager == null)
        {
            DontDestroyOnLoad(gameObject);
            _loggerViewManager = this;
            _loggerViewManager.Init();
        }
        else
        {
            _loggerViewManager.Init();
            if(_loggerViewManager != this)
            {
                Destroy (gameObject);
            }
        }
        
    }

    private void Init()
    {
        LoggerViewer = FindObjectOfType(typeof(LoggerViewer)) as LoggerViewer;
        RefreshView();
    }

    public void AddEvent(string eventString)
    {
        _loggerLogList.Add(eventString);

        if (_loggerLogList.Count > MaxLines)
        {
            _loggerLogList.RemoveAt(0);
        }

        RefreshView();

    }

    public void RefreshView()
    {
        if (LoggerViewer != null)
        {
            LoggerViewer.SetLogs(_loggerLogList);
        }
    }
}
