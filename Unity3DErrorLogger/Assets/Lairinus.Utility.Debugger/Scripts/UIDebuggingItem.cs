using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDebuggingItem : MonoBehaviour
{
    public Image iconImage = null;
    public Text titleText = null;
    public Text stacktraceText = null;
    public Text countText = null;
    public Image backgroundImage = null;
    public Button backgroundButton = null;
    public LogType type = LogType.Assert;
}