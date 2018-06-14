using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "DebuggingOptionsSO", menuName = "Lairinus/Debugging", order = 1)]
public class DebuggingOptionsSO : ScriptableObject
{
    /* Writing this long-ways -> classes don't behave well with ScriptableObjects when porting the project between different machines */

    public Color errorLogBackgroundColor = new Color();
    public Sprite errorLogIcon = null;
    public Color warningLogBackgroundColor = new Color();
    public Sprite warningLogIcon = null;
    public Color logLogBackgroundColor = new Color();
    public Sprite logLogIcon = null;
    public Color assertLogBackgroundColor = new Color();
    public Sprite assertLogIcon = null;
    public Color exceptionLogBackgroundColor = new Color();
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
                    item.backgroundImage.color = errorLogBackgroundColor;
                    item.iconImage.sprite = errorLogIcon;
                }
                break;

            case LogType.Warning:
                {
                    item.backgroundImage.color = warningLogBackgroundColor;
                    item.iconImage.sprite = warningLogIcon;
                }
                break;

            case LogType.Log:
                {
                    item.backgroundImage.color = logLogBackgroundColor;
                    item.iconImage.sprite = logLogIcon;
                }
                break;

            case LogType.Assert:
                {
                    item.backgroundImage.color = assertLogBackgroundColor;
                    item.iconImage.sprite = assertLogIcon;
                }
                break;

            case LogType.Exception:
                {
                    item.backgroundImage.color = exceptionLogBackgroundColor;
                    item.iconImage.sprite = exceptionLogIcon;
                }
                break;
        }
    }
}