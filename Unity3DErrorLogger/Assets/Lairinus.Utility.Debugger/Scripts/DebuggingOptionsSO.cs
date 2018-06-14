using UnityEngine;
using System.Collections;

// Contains UI styling options, and handles appropriately styling the elements
[CreateAssetMenu(fileName = "DebuggingOptionsSO", menuName = "Lairinus/Debugging", order = 1)]
public class DebuggingOptionsSO : ScriptableObject
{
    /* Writing this long-ways -> classes don't behave well with ScriptableObjects when porting the project between different machines */
    public Color errorLogIconColor = new Color();
    public Sprite errorLogIcon = null;
    public Color warningLogIconColor = new Color();
    public Sprite warningLogIcon = null;
    public Color logLogIconColor = new Color();
    public Sprite logLogIcon = null;
    public Color assertLogIconColor = new Color();
    public Sprite assertLogIcon = null;
    public Color exceptionLogIconColor = new Color();
    public Sprite exceptionLogIcon = null;

    /// <summary>
    /// Sets the DebuggingItem's style to match the pre-defined convention
    /// </summary>
    /// <param name="item"></param>
    /// <param name="type"></param>
    public void SetDebuggingItem(UIDebuggingItem item, LogType type)
    {
        if (item == null)
            return;

        switch (type)
        {
            case LogType.Error:
                {
                    item.iconImage.color = errorLogIconColor;
                    item.iconImage.sprite = errorLogIcon;
                }
                break;

            case LogType.Warning:
                {
                    item.iconImage.color = warningLogIconColor;
                    item.iconImage.sprite = warningLogIcon;
                }
                break;

            case LogType.Log:
                {
                    item.iconImage.color = logLogIconColor;
                    item.iconImage.sprite = logLogIcon;
                }
                break;

            case LogType.Assert:
                {
                    item.iconImage.color = assertLogIconColor;
                    item.iconImage.sprite = assertLogIcon;
                }
                break;

            case LogType.Exception:
                {
                    item.iconImage.color = exceptionLogIconColor;
                    item.iconImage.sprite = exceptionLogIcon;
                }
                break;
        }
    }
}