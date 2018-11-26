using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoSingleton<CoroutineManager>
{
    private WaitForFixedUpdate _waitForFixedUpdate = new WaitForFixedUpdate();

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }

    public static Coroutine Start(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }

    public static Coroutine Start(string methodName)
    {
        return Instance.StartCoroutine(methodName);
    }

    public static Coroutine Start(string methodName, object value)
    {
        return Instance.StartCoroutine(methodName, value);
    }

    public static void Stop(Coroutine routine)
    {
        Instance.StopCoroutine(routine);
    }

    public static void Stop(IEnumerator routine)
    {
        Instance.StopCoroutine(routine);
    }

    public static void Stop(string methodName)
    {
        Instance.StopCoroutine(methodName);
    }

    public static void StopAll()
    {
        Instance.StopAllCoroutines();
    }

    public static WaitForFixedUpdate WaitForFixedUpdate => Instance._waitForFixedUpdate;
}