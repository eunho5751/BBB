using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class GlobalTimer : TimerBase
{
    [SerializeField, DisableInPlayMode, Required, PropertyOrder(-1)]
    private string _key;
    [ShowInInspector, HideInEditorMode, DisableInPlayMode]
    private List<TimerBase> _children = new List<TimerBase>();

    protected virtual void Awake()
    {
        if (!(this is MainTimer))
            MainTimer.Instance.Register(_key, this);
    }

    protected virtual void OnDestroy()
    {
        TotalScale = 1F;

        if (!(this is MainTimer))
            MainTimer.Instance?.Unregister(_key, this);
    }

    public void RegisterChild(TimerBase timer)
    {
        if (_children.Contains(timer))
            return;

        _children.Add(timer);
    }

    public void UnregisterChild(TimerBase timer)
    {
        _children.Remove(timer);
    }

    public override float TotalScale
    {
        get
        {
            return base.TotalScale;
        }

        protected set
        {
            base.TotalScale = value;
            foreach (var child in _children)
                child.Compute();
        }
    }
}


