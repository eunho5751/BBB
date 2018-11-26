using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class LocalTimer : TimerBase, ITimeEffector
{
    [SerializeField, DisableContextMenu, ListDrawerSettings(AlwaysAddDefaultValue = true)]
    private List<TimeComponentBase> _components = new List<TimeComponentBase>();

    private List<ITimeFactor> _factors = new List<ITimeFactor>();

#if UNITY_EDITOR
    private void Reset()
    {
        _components.AddRange(GetComponentsInChildren<TimeComponentBase>());
    }

    [ShowInInspector]
    private void AddAll()
    {
        _components.Clear();
        _components.AddRange(GetComponentsInChildren<TimeComponentBase>());
    }
#endif

    protected override float ComputeTotal()
	{
		float total = base.ComputeTotal();
		foreach (var factor in _factors)
			total *= factor.Value;
		return total;
	}

    public void AddFactor(ITimeFactor factor, bool compute = true)
	{
		_factors.Add(factor);
        if (compute)
            Compute();
	}

	public void RemoveFactor(ITimeFactor factor, bool compute = true)
	{
		_factors.Remove(factor);
        if (compute)
            Compute();
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
            foreach (var comp in _components)
                comp.Apply(value);
        }
    }
}