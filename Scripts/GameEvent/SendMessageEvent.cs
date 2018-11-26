using UnityEngine;
using Sirenix.OdinInspector;

public class SendMessageEvent : IGameEvent
{
    [SerializeField, DisableContextMenu]
    private GameObject[] _targets = new GameObject[0];
    [SerializeField, Required]
    private string _methodName;
    
    public void Trigger()
    {
        foreach (var target in _targets)
            target.SendMessage(_methodName);
    }
}