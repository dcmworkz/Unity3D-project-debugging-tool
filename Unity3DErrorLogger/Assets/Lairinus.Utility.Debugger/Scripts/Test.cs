using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Lairinus.Utility;

public class Test : MonoBehaviour
{
    private void Awake()
    {
        // We need to initialze the tool before we use it. Alternatively, you could drag it into a scene, OR you could even call the "AddLogItem" method and pass a MonoBehaviour into it!
        UIDebuggingConsoleTool.Initialize(this);
    }

    private void Update()
    {
        // You can catch errors manually
        try
        {
            DebuggingItem di = null;
            di.count = 1;
        }
        catch (System.Exception ex)
        {
            // Like this:
            UIDebuggingConsoleTool.AddLogItem(ex.Message, ex.StackTrace, LogType.Exception);
        }

        // And you can add your own error messages without a problem.
        UIDebuggingConsoleTool.AddLogItem("Test Issue", "Woot", LogType.Log);

        // It even catches other errors without requiring any additional code! Keep in mind that these errors will show up in the Unity3D Editor Console as well
        string str = "sdlkfj";
        float f = float.Parse(str);
    }
}