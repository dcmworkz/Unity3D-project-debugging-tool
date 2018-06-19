using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Contains all of the data for a single type of issue.
/// </summary>
public class DebuggingItem
{
    public DebuggingItem(string _name, string _stackTrace, LogType _type)
    {
        name = _name;
        if (_stackTrace == null || _stackTrace == "")
            _stackTrace = " ";

        stacktrace = _stackTrace;
        type = _type;
        logTypeText = _type.ToString();
        DateTime currentDateTime = DateTime.Now;
        lastOccurenceLocalTime = currentDateTime.ToShortTimeString();
    }

    /// <summary>
    /// How many of this type of Debugging Item occurred.
    /// </summary>
    public int count { get; set; }

    /// <summary>
    /// The unique identifier for this type of Debugging Item
    /// </summary>
    public string key { get { return name + stacktrace; } }

    /// <summary>
    /// The last period in time that this Debugging Item occurred, in seconds since the game has been playing. This directly corresponds with Time.realtimeSinceStartup.
    /// </summary>
    public float lastOccurence { get; set; }

    /// <summary>
    /// The last period in time that this Debugging Item occured, in realtime based on your device.
    /// </summary>
    public string lastOccurenceLocalTime { get; set; }

    /// <summary>
    /// String repesentation of the UnityEngine.LogType items.
    /// </summary>
    public string logTypeText { get; private set; }

    /// <summary>
    /// The title of the event.
    /// </summary>
    public string name { get; private set; }

    /// <summary>
    /// Where this event took place
    /// </summary>
    public string stacktrace { get; private set; }

    /// <summary>
    /// The enum representation of the type of internal Unity3D issue, or the type of the custom issue, as defined by the user.
    /// </summary>
    public LogType type { get; set; }
}