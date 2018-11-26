using System.Collections.Generic;
using UnityEngine;

public class PoolManager : ScriptableObject
{
    private static PoolManager _instance = null;
    private static bool _destroyed = false;

    private Dictionary<GameObject, IPool<GameObject>> _poolDict = new Dictionary<GameObject, IPool<GameObject>>();
    private Dictionary<GameObject, IPool<GameObject>> _instanceDict = new Dictionary<GameObject, IPool<GameObject>>();

    private void OnEnable()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
            return;
#endif
        _destroyed = false; 
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlaying)
            return;
#endif

        _destroyed = true;
    }

    public void Register(GameObject prefab, GameObjectPool pool)
    {
        _poolDict.Add(prefab, pool);
    }

    public void Unregister(GameObject prefab, IReadOnlyCollection<GameObject> instances)
    {
        _poolDict.Remove(prefab);
        foreach (var instance in instances)
            _instanceDict.Remove(instance);
    }

    public GameObject Spawn(GameObject prefab)
    {
        var pool = GetPool(prefab);
        var instance = pool.Spawn();
        _instanceDict.Add(instance, pool);
        return instance;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position)
    {
        var instance = Spawn(prefab);
        instance.transform.position = position;
        return instance;
    }

    public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        var instance = Spawn(prefab);
        instance.transform.SetPositionAndRotation(position, rotation);
        return instance;
    }

    public void Despawn(GameObject instance)
    {
        IPool<GameObject> pool = null;
        if (_instanceDict.TryGetValue(instance, out pool))
            pool.Despawn(instance);
        _instanceDict.Remove(instance);
    }

    private IPool<GameObject> GetPool(GameObject prefab)
    {
        IPool<GameObject> pool = null;
        _poolDict.TryGetValue(prefab, out pool);
        return pool;
    }

    public static PoolManager Instance
    {
        get
        {
            if (_instance == null && !_destroyed)
                _instance = CreateInstance<PoolManager>();
            return _instance;
        }
    }
}