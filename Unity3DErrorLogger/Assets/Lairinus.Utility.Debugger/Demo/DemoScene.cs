using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lairinus.Utility;

public class DemoScene : MonoBehaviour
{
    private void Awake()
    {
        // We need to initialze the tool before we use it. Alternatively, you could drag it into a scene, OR you could even call the "AddLogItem" method and pass a MonoBehaviour into it!
        ProjectDebuggingTool.Initialize(this);
    }

    private void Start()
    {
        // Let's add some test issues! We want to pass the MonoBehaviour object here because it takes a frame or so to fully initialize the object
        ProjectDebuggingTool.AddLogItem("Error: Test Error", "Error at DemoScene.cs at Start(): ln 17", LogType.Error, this);
        ProjectDebuggingTool.AddLogItem("Assert: Test Assert", "Assert at DemoScene.cs at Start(): ln 18", LogType.Assert, this);
        ProjectDebuggingTool.AddLogItem("Log: Test Log", "Log at DemoScene.cs at Start(): ln 19", LogType.Log, this);
        ProjectDebuggingTool.AddLogItem("Exception: Test Exception", "Exception at DemoScene.cs at Start(): ln 20", LogType.Exception, this);
        ProjectDebuggingTool.AddLogItem("Warning: Test Warning", "Warning at DemoScene.cs at Start(): ln 21", LogType.Warning, this);
    }

    private void Update()
    {
        // You can catch errors manually.
        try
        {
            ProjectDebuggingItemModel di = null;
            di.count = 1;
        }
        catch (System.Exception ex)
        {
            // Like this:
            ProjectDebuggingTool.AddLogItem(ex.Message, ex.StackTrace, LogType.Exception);
        }

        // And you can add your own error messages without a problem.
        ProjectDebuggingTool.AddLogItem("Test Log", "Testing the log... the message here doesn't have to be a stack trace...", LogType.Log);

        // The code below purposely throws an error to prove that the tool functions as advertised :)
        // It even catches other errors without requiring any additional code! Keep in mind that these errors will show up in the Unity3D Editor Console as well
        string str = "not a number :(";
        float f = float.Parse(str);
    }
}