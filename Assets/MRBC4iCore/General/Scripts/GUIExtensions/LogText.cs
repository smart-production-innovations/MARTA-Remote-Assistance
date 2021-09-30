using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// display log messages in the UI for debugging on mobile devices
/// </summary>
[RequireComponent(typeof(Text))]
public class LogText : MonoBehaviour
{
    public bool ShowStackTrace = true;
    public bool InverseOrder = false;

    private Text uiText;
    private ScrollRect scrollRect;


    void Awake()
    {
        uiText = GetComponent<Text>();
        scrollRect = GetComponentInParent<ScrollRect>();
        Application.logMessageReceived += Log;
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= Log;
    }

    /// <summary>
    /// Add a new log message
    /// </summary>
    /// <param name="logString">log message text</param>
    /// <param name="stackTrace">where in the code is the log message triggered</param>
    /// <param name="type">log message type</param>
    public void Log(string logString, string stackTrace, LogType type)
    {
        try
        {
            var oldText = uiText.text;
            var newText = type.ToString() + ": " + logString + "\n";
            if (ShowStackTrace)
                newText += stackTrace + "\n\n";


            if (InverseOrder)
                uiText.text = newText + oldText;
            else
                uiText.text = oldText + newText;
        }
        catch (Exception e)
        {
            uiText.text = "";
        }
    }

}
