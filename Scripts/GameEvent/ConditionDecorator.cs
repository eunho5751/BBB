using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class ConditionDecorator : GameEventDecorator
{
    [SerializeField]
    private Func<bool> _condition;

    public override void Trigger()
    {
        if (_condition())
            base.Trigger();
    }
}
