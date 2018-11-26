using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class TimeArea : MonoBehaviour
{
	private class Factor : ITimeFactor
	{
        public Coroutine Coroutine { get; set; }
        public float Value { get; set; } = 1F;
	}

	[SerializeField]
	private AnimationCurve _scaleCurve;
	
	private Pool<Factor> _factorPool;
	private Dictionary<ITimeEffector, Factor> _factorDict = new Dictionary<ITimeEffector, Factor>();
	
	private void Awake()
	{
		_factorPool = new Pool<Factor>(3, 1, () => new Factor());
	}

	private void OnDisable()
	{
        foreach (var pair in _factorDict)
        {
            var effector = pair.Key;
            if (effector != null)
                effector.RemoveFactor(pair.Value);
        }

        _factorDict.Clear();
	}
	
	private void Start()
	{
		var col = GetComponent<Collider>();
		col.isTrigger = true;
	}
	
	private void OnTriggerEnter(Collider col)
	{
        var effector = col.GetComponent<ITimeEffector>();
		if (effector != null)
		{
            var factor = _factorPool.Spawn();
            _factorDict.Add(effector, factor);
            effector.AddFactor(factor, false);
            factor.Coroutine = StartCoroutine(Apply(effector, factor));
		}
	}
	
	private void OnTriggerExit(Collider col)
    {
        var effector = col.GetComponent<ITimeEffector>();
        if (effector != null)
		{
            var factor = _factorDict[effector];
            _factorDict.Remove(effector);
            effector.RemoveFactor(factor, true);
            StopCoroutine(factor.Coroutine);
		}
	}

    private IEnumerator Apply(ITimeEffector effector, Factor factor)
    {
        float maxTime = _scaleCurve[_scaleCurve.length - 1].time;
        float elapsedTime = 0F;

        while (elapsedTime < maxTime)
        {
            if (effector == null)
                yield break;

            factor.Value = _scaleCurve.Evaluate(elapsedTime);
            effector.Compute();
            
            elapsedTime += MainTimer.Instance.Delta;
            yield return null;
        }

        factor.Value = _scaleCurve.Evaluate(maxTime);
        effector.Compute();
    }

    public void Set(float radius, AnimationCurve curve)
    {
        GetComponent<SphereCollider>().radius = radius;
        _scaleCurve = curve;
    }
}