using UnityEngine;
using Sirenix.OdinInspector;

public abstract class MonoSingleton<T> : SerializedMonoBehaviour where T : MonoBehaviour
{
    private static T _instance = null;
    private static bool _destroyed = false;

    protected virtual void Awake()
    {
        gameObject.name = $"(Singleton) {gameObject.name}";
        //DontDestroyOnLoad(gameObject);
        _destroyed = false;
    }

    protected virtual void OnDestroy()
    {
        _destroyed = true;
    }

    public static T Instance
    {
        get
        {
            if (_destroyed)
                return null;

            if (_instance == null)
            {
                T[] typeRef = GameObject.FindObjectsOfType<T>();
                
                if (typeRef.Length == 0) // No Instances in the hirarchy.
                {
                    GameObject singleton = new GameObject(typeof(T).ToString());
                    _instance = singleton.AddComponent<T>();
                }
                else if (typeRef.Length == 1) // Only one instance in the hirarchy.
                {
                    _instance = typeRef[0];
                }
                else if (typeRef.Length > 1) // Instances more than one in the hirarchy.
                {
                    throw new UnityException($"'{typeof(T)}' Type has instances more than one in the hirarchy!");
                }
            }

            return _instance;
        }
    }
}