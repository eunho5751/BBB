using UnityEngine;
using Sirenix.OdinInspector;

public class ParallelEvent : IGameEvent
{
    [SerializeField, DisableContextMenu]
    private IGameEvent[] _events = new IGameEvent[0];

    public void Trigger()
    {
        foreach (var gameEvent in _events)
        {
            gameEvent.Trigger();
        }
    }
}