using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// In 2018 it's STILL not possible to adjust a Unity3D Toggle's 'On' state without custom code :(
[RequireComponent(typeof(Image))]
public class UIToggle : Toggle
{
    private Image _imageElement = null;

    protected override void Awake()
    {
        base.Awake();
        _imageElement = GetComponent<Image>();
        this.onValueChanged.AddListener(OnToggleValueChanged);
        OnToggleValueChanged(isOn);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        _imageElement.color = isOn ? new Color(0.65f, 0.65f, 0.65f) : new Color(1, 1, 1);
    }
}