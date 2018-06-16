using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// Debugging Item -> A single TYPE of issue occurs. There may be many of the same type of issues within this element.
public class DebuggingItem
{
    public DebuggingItem(string _name, string _stackTrace, LogType _type)
    {
        name = _name;
        if (_stackTrace == null)
            _stackTrace = " ";

        stacktrace = _stackTrace.Trim();
        stacktrace = stacktrace.Replace("\n", "");
        stacktrace = stacktrace.Replace("at ", "\n    at ");
        stacktrace = stacktrace.Insert(0, _name);
        type = _type;
        logTypeText = _type.ToString();
        DateTime dt = DateTime.Now;
        lastOccurenceLocalTime = dt.ToShortTimeString();
    }

    public string key { get { return name + stacktrace; } }
    public string name { get; private set; }
    public string stacktrace { get; private set; }
    public float lastOccurence { get; set; }
    public string lastOccurenceLocalTime { get; set; }
    public int count { get; set; }
    public LogType type { get; set; }
    public string logTypeText { get; private set; }
}