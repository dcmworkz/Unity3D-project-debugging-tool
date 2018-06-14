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
        stacktrace = _stackTrace;
        type = _type;
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
}