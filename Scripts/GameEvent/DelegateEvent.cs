using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class DelegateEvent : IGameEvent
{
    [SerializeField, HideReferenceObjectPicker, DisableContextMenu]
    private UnityEvent _function = new UnityEvent();

    public void Trigger()
    {
        _function.Invoke();
    }
}