using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public class RangedWeapon : Weapon
{
    [SerializeField, Required, Tooltip("총구 트랜스폼")]
    private Transform _muzzle;
    [SerializeField, Required]
    private ParticleSystem _muzzleEffect;
    [SerializeField, FMODUnity.EventRef]
    private string _fireSound;
    [SerializeField, Required, AssetsOnly, Tooltip("총탄 프리팹")]
    private GameObject _bullet;
    [SerializeField, Tooltip("최대 총알 수")]
    private int _maxAmmo;
    [SerializeField, Tooltip("총탄 속도")]
    private float _bulletSpeed;
    [SerializeField, Tooltip("발사 간격")]
    private float _fireRate;
    
    private int _currentAmmo;
    private float _elapsedFireTime;
    private LocalTimer _timer;

    protected override void Start()
    {
        base.Start();

        Reload();
    }

    private void OnDrawGizmosSelected()
    {
        if (_muzzle == null)
            return;

        Color color = Gizmos.color;
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(_muzzle.position, 0.05f);
        Gizmos.DrawRay(_muzzle.position, _muzzle.forward);
        Gizmos.color = color;
    }

    private void Update()
    {
        _elapsedFireTime += _timer.Delta;
    }
    
    public void Fire()
    {
        var bullet = PoolManager.Instance.Spawn(_bullet).GetComponent<Bullet>();
        bullet.Set(Owner, _muzzle.position, _muzzle.rotation, _bulletSpeed, Damage);
        FMODUnity.RuntimeManager.PlayOneShot(_fireSound, _muzzle.position);
        //var sound = FMODUnity.RuntimeManager.CreateInstance(_fireSound);
        //sound.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(_muzzle.position));
        //sound.setPitch(_timer.TotalScale);
        //sound.start();
        //sound.release();

        _muzzleEffect.Play();
        
        _elapsedFireTime = 0f;
        CurrentAmmo--;
    }

    public void Reload()
    {
        CurrentAmmo = _maxAmmo;
    }

    public override GameObject Owner
    {
        get
        {
            return base.Owner;
        }

        set
        {
            base.Owner = value;
            if (value != null)
                _timer = value.GetComponent<LocalTimer>();
        }
    }

    public int CurrentAmmo
    {
        get { return _currentAmmo; }
        set { _currentAmmo = Mathf.Max(0, Mathf.Min(value, _maxAmmo)); }
    }

    public bool IsShootable => _elapsedFireTime > _fireRate && CurrentAmmo > 0;
}