using UnityEngine;

[RequireComponent(typeof(LocalTimer))]
public class Bullet : MonoBehaviour
{
    [SerializeField]
    private LayerMask _hitLayer;
    [SerializeField]
    private TextureEffectMap _hitEffectMap;

    private LocalTimer _timer;
    private Vector3 _prevPosition;
    private GameObject _shooter;
    private float _speed;
    private int _damage;
    private RaycastHit _hitInfo;

    private void Awake()
    {
        _timer = GetComponent<LocalTimer>();
    }

    private void OnEnable()
    {
        _prevPosition = transform.position;
    }

    private void Update()
    {
        int layer = _hitLayer;
        float dist = _speed * _timer.Delta;
        if (Physics.Linecast(_prevPosition, transform.position, out _hitInfo, layer))
        {
            var sceneObj = _hitInfo.transform.GetComponent<SceneObject>() ?? _hitInfo.transform.GetComponent<BodyPart>().Root;
            if (sceneObj != null && _shooter.layer != sceneObj.gameObject.layer)
            {
                var damageable = sceneObj as IDamageable;
                damageable?.TakeDamage(_shooter, _damage);

                if (_hitEffectMap != null)
                    TextureEffectEmitter.Emit(_hitEffectMap, sceneObj.TextureType, _hitInfo.point, Quaternion.LookRotation(_hitInfo.normal));
            }

            PoolManager.Instance.Despawn(gameObject);
            return;
        }

        _prevPosition = transform.position;
        transform.position += transform.forward * dist; 
    }

    public void Set(GameObject shooter, Vector3 position, Quaternion rotation, float speed, int damage)
    {
        transform.SetPositionAndRotation(position, rotation);
        _shooter = shooter;
        _speed = speed;
        _damage = damage;
    }
}
