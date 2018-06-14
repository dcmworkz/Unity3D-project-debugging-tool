using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Debugging Item -> A single TYPE of issue occurs. There may be many of the same type of issues within this element.
public class DebuggingItem
{
    public DebuggingItem(string _name, string _stackTrace, LogType _type)
    {
        name = _name;
        _stackTrace = stacktrace;
        type = _type;
    }

    public string key { get { return name + stacktrace; } }
    public string name { get; private set; }
    public string stacktrace { get; private set; }
    public float lastOccurance { get; set; }
    public int count { get; set; }
    public LogType type { get; set; }
}