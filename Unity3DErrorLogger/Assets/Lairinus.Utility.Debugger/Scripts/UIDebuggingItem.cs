using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The View item that represents the data of the Model "DebuggingItem" class
/// </summary>
public class UIDebuggingItem : MonoBehaviour
{
    public Image iconImage = null;
    public Text titleText = null;
    public Text countText = null;
    public Image backgroundImage = null;
    public Button backgroundButton = null;
    public LogType type = LogType.Assert;
}