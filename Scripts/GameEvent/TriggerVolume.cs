using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(BoxCollider))]
public class TriggerVolume : SerializedMonoBehaviour
{
    [SerializeField]
    private LayerMask _layerMask = 1;
    [SerializeField]
    private string _tag;
    [SerializeField, DisableContextMenu]
    private IGameEvent _enterEvent;
    [SerializeField, DisableContextMenu]
    private IGameEvent _exitEvent;

    private void Start()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsTarget(other.gameObject))
        {
            _enterEvent?.Trigger();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsTarget(other.gameObject))
        {
            _exitEvent?.Trigger();
        }
    }

    private bool IsTarget(GameObject other)
    {
        if ((_layerMask & (1 << other.layer)) == 0)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(_tag) && !other.CompareTag(_tag))
        {
            return false;
        }

        return true;
    }
}