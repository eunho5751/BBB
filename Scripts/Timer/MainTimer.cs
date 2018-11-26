using System.Collections.Generic;
using UnityEngine;

public class MainTimer : GlobalTimer
{
    private static MainTimer _instance = null;
    private static bool _destroyed = false;

    private Dictionary<string, GlobalTimer> _timerDict = new Dictionary<string, GlobalTimer>();
    private bool _initialized = false;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        _instance = this;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _destroyed = true;
    }

    protected override void Start()
    {
        if (!_initialized)
        {
            Compute();
            _initialized = true;
        }
    }

    public void Register(string key, GlobalTimer timer)
    {
        _timerDict.Add(key, timer);
    }

    public void Unregister(string key, GlobalTimer timer)
    {
        _timerDict.Remove(key);
    }

    public GlobalTimer GetTimer(string key)
    {
        GlobalTimer timer = null;
        _timerDict.TryGetValue(key, out timer);
        return timer;
    }

    public static float RawFixedDelta => Time.fixedDeltaTime;
    public static float RawDelta => Time.deltaTime;

    public static MainTimer Instance
    {
        get
        {
            if (_instance == null && !_destroyed)
            {
                GameObject obj = new GameObject("Main Timer");
                _instance = obj.AddComponent<MainTimer>();
            }

            return _instance;
        }
    }
}
