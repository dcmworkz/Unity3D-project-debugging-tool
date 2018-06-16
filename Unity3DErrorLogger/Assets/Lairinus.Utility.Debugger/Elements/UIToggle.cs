using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// In 2018 it's STILL not possible to adjust a Unity3D Toggle's 'On' state without custom code :(
[RequireComponent(typeof(Image))]
public class UIToggle : MonoBehaviour, IPointerClickHandler
{
    public Color offColor = new Color();
    public Color onColor = new Color();
    private Image _imageElement = null;
    [SerializeField] private bool _isOn = false;
    public bool isOn { get { return _isOn; } }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetState(!_isOn);
    }

    private void HandleStateChange(bool state)
    {
        _isOn = state;
        if (_isOn)
            _imageElement.color = onColor;
        else
            _imageElement.color = offColor;
    }

    public void SetState(bool state)
    {
        HandleStateChange(state);
    }

    private void Awake()
    {
        _imageElement = GetComponent<Image>();
        SetState(_isOn);
    }
}