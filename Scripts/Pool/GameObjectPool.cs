using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class GameObjectPool : MonoBehaviour, IPool<GameObject>
{
    private enum PoolMode
    {
        Manual,
        Duration
    }
    
	[SerializeField, AssetsOnly, Required, DisableInPlayMode]
	private GameObject _prefab;
    [SerializeField, DisableInPlayMode]
    private PoolMode _mode = PoolMode.Manual;
    [SerializeField, ShowIf("_mode", optionalValue:PoolMode.Duration), DisableInPlayMode]
    private float _duration;
	[SerializeField, MinValue(0), DisableInPlayMode]
	private int _initialCapacity;
	[SerializeField, MinValue(0), DisableInPlayMode]
	private int _extendAmount;

    private Pool<GameObject> _pool;

	private void Awake()
	{
		Action<GameObject> onSpawned = instance => instance.SetActive(true);
        Action<GameObject> onDespawned = instance =>
        {
            instance.transform.parent = transform;
            instance.SetActive(false);
        };

		Func<GameObject> objectCreator = () =>
		{
			GameObject newInstance = Instantiate(_prefab, transform);
			newInstance.name = newInstance.name + " (Pooling)";
			newInstance.SetActive(false);
			return newInstance;
		};

		_pool = new Pool<GameObject>(_initialCapacity, _extendAmount, objectCreator, onSpawned, onDespawned);
		PoolManager.Instance.Register(_prefab, this);
	}
	
	private void OnDestroy()
	{
		PoolManager.Instance?.Unregister(_prefab, _pool.UsingInstances);
	}

    public GameObject Spawn()
    {
        var instance = _pool.Spawn();
        if (_mode == PoolMode.Duration)
        {
            var timer = instance.GetComponent<ITimer>() ?? MainTimer.Instance;
            StartCoroutine(DelayedDespawn(timer, instance));
        }

        return instance;
    }

    private IEnumerator DelayedDespawn(ITimer timer, GameObject instance)
    {
        float elapsedTime = 0F;
        while (elapsedTime < _duration)
        {
            yield return null;
            elapsedTime += timer.Delta;
        }

        PoolManager.Instance.Despawn(instance);
    }

    public void Despawn(GameObject instance)
    {
        _pool.Despawn(instance);
    }
}