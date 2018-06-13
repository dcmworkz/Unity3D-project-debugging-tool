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
    public Color messageLogBackgroundColor = new Color();
    public Sprite messageLogIcon = null;
    public Color customBackgroundColor = new Color();
    public Sprite customLogIcon = null;
}