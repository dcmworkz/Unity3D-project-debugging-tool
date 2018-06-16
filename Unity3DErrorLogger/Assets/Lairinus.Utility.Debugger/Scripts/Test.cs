using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lairinus.Utility;

public class Test : MonoBehaviour
{
    private void Start()
    {
        UIDebuggingConsoleTool.AddLogItem("Hi", "Bye", LogType.Assert, this);
    }

    private void Update()
    {
        try
        {
            DebuggingItem di = null;
            di.count = 1;
        }
        catch (System.Exception ex)
        {
            UIDebuggingConsoleTool.AddLogItem(ex.Message, ex.StackTrace, LogType.Exception);
        }

        try
        {
            string str = "sdlkfj";
            float.Parse(str);
        }
        catch (System.Exception ex)
        {
            // We don't care about catching any exceptions that occur inside here
            UIDebuggingConsoleTool.AddLogItem(ex.Message, ex.StackTrace, LogType.Exception);
        }
    }
}