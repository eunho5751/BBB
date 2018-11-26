using UnityEngine;
using Sirenix.OdinInspector;

public class Weapon : SerializedMonoBehaviour
{
    [SerializeField, Required, Tooltip("무기 소켓 이름")]
    private string _socketName;
    [SerializeField, Required, Tooltip("애니메이터 레이어 이름")]
    private string _layerName;
    [SerializeField, Tooltip("무기 데미지")]
    private int _damage;
    [SerializeField, Tooltip("버려질 수 있는지 여부")]
    private bool _droppable;

    private GameObject _owner;
    
    protected virtual void Awake()
    {

    }

    protected virtual void Start()
    {
        
    }

    private void SetPhysicsActive(bool active)
    {
        var rigid = GetComponent<Rigidbody>();
        var collider = GetComponent<Collider>();
        if (rigid && collider)
        {
            rigid.useGravity = active;
            rigid.isKinematic = !active;
            collider.isTrigger = !active;
        }
    }
    
    public virtual GameObject Owner
    {
        get
        {
            return _owner;
        }

        set
        {
            _owner = value;

            if (_owner == null )
                SetPhysicsActive(true);
        }
    }
    
    public string SocketName => _socketName;
    public string LayerName => _layerName;
    public int Damage => _damage;
    public bool Droppable => _droppable;
}