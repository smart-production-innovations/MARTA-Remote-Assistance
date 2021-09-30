using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogHelper : MonoBehaviour
{
    public Text text;
    #region unity loop
    protected void Awake()
    {
        Application.logMessageReceived += Log;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= Log;
    }
    #endregion

    /// <summary>
    /// Adds a new debug message to the message view
    /// </summary>
    /// <param name="logString">text message</param>
    /// <param name="stackTrace">debug stack</param>
    /// <param name="type">debug type (error, warning, log)</param>
    public void Log(string logString, string stackTrace, LogType type)
    {
        if (text && logString.StartsWith("[TEST]") && type == LogType.Log)
        {
            text.text = logString;
        }
    }
}
