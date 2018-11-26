using UnityEngine;
using Sirenix.OdinInspector;

public abstract class GameEventDecorator : IGameEvent
{
    [SerializeField, Required, PropertyOrder(1)]
    private IGameEvent _event;

    public virtual void Trigger()
    {
        _event.Trigger();
    }
}

