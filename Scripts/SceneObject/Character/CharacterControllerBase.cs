using System;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(LocalTimer))]
[RequireComponent(typeof(TimeAnimator))]
[RequireComponent(typeof(FootstepEffectEmitter))]
[RequireComponent(typeof(WeaponInventory))]
public abstract class CharacterControllerBase : SceneObject, IDamageable
{
    private static class AnimatorHash
    {
        public static readonly int ReactionId = Animator.StringToHash("ReactionId");
        public static readonly int TakeReaction = Animator.StringToHash("TakeReaction");
    }

    [TitleGroup("Basics")]
    [SerializeField, Tooltip("최대 체력")]
    private int _maxHealth;
    [TitleGroup("Etc")]
    [SerializeField, FMODUnity.EventRef, PropertyOrder(1000)]
    private string _deathSound;

    protected Quaternion _targetRotation;
    private bool _hasRotated;

    private event Action<int> _onHealthChanged;
    private event Action _onCharacterDead;
    private int _currentHealth;
    private Vector3 _prevPosition;

    private AttackInfo _currentAttackInfo;

    protected virtual void Awake()
    {
        Collider = GetComponent<CapsuleCollider>();
        Rigid = GetComponent<Rigidbody>();
        Animator = GetComponent<Animator>();
        Timer = GetComponent<LocalTimer>();
        WeaponInventory = GetComponent<WeaponInventory>();
    }

    protected virtual void OnEnable()
    {
        WeaponInventory.OnWeaponSwapped += OnWeaponSwapped;
    }

    protected virtual void OnDisable()
    {
        WeaponInventory.OnWeaponSwapped -= OnWeaponSwapped;
    }

    protected virtual void Start()
    {
        SetRagdollActive(false);
        CurrentHealth = MaxHealth;
        CheckDead();

        _prevPosition = transform.position;
    }

    private void PlaySound(string path)
    {
        FMODUnity.RuntimeManager.PlayOneShot(path, transform.position);
    }

    private void CheckDead()
    {
        if (IsDead)
        {
            SetRagdollActive(true);
            WeaponInventory.RemoveWeapon(WeaponInventory.CurrentWeapon);
        }
    }

    private void SetRagdollActive(bool active)
    {
        foreach (var bodyPart in GetComponentsInChildren<BodyPart>())
        {
            var collider = bodyPart.Collider;
            var rigid = collider.attachedRigidbody;
            collider.isTrigger = !active;
            rigid.isKinematic = !active;
            rigid.useGravity = active;
            rigid.velocity = Animator.velocity * 2f;
            rigid.angularVelocity = Animator.angularVelocity * 2f;
        }

        if (active)
            Rigid.isKinematic = true;
        Animator.enabled = !active;
    }

    private void OnWeaponSwapped(Weapon from, Weapon to)
    {
        int layerIndex = 0;
        if (from != null)
        {
            layerIndex = Animator.GetLayerIndex(from.LayerName);
            Animator.SetLayerWeight(layerIndex, 0f);
        }

        layerIndex = Animator.GetLayerIndex(to.LayerName);
        Animator.SetLayerWeight(layerIndex, 1f);
    }

    protected virtual void OnReactionEnter()
    {
        ApplyRootMotion = true;
    }

    protected virtual void OnReactionExit()
    {
        ApplyRootMotion = false;
    }

    protected virtual void OnAttackEnter(AttackInfo attackInfo)
    {
        _currentAttackInfo = attackInfo;
    }

    protected virtual void OnAttackExit()
    {
        DisableHit();
    }

    private void EnableHit()
    {
        var weapon = WeaponInventory.CurrentWeapon;
        var proc = weapon.GetComponent<HitProcessor>();
        proc?.EnableCast(new HitProcessorInfo(gameObject, _currentAttackInfo.HitBoxIndices, (int)(weapon.Damage * _currentAttackInfo.DamageMultiplier), _currentAttackInfo.ReactionId));
    }

    private void DisableHit()
    {
        var proc = WeaponInventory.CurrentWeapon.GetComponent<HitProcessor>();
        proc?.DisableCast();
    }

    private void CheckHit()
    {
        var weapon = WeaponInventory.CurrentWeapon;
        var proc = weapon.GetComponent<HitProcessor>();
        proc?.CheckOverlap(new HitProcessorInfo(gameObject, _currentAttackInfo.HitBoxIndices, (int)(weapon.Damage * _currentAttackInfo.DamageMultiplier), _currentAttackInfo.ReactionId));
    }

    protected virtual void OnAnimatorMove()
    {
        if (ApplyRootMotion)
        {
            Vector3 vel = Animator.velocity;
            Move(vel, vel.magnitude);
        }
    }

    protected virtual void FixedUpdate()
    {
        if (_hasRotated)
        {
            Rigid.MoveRotation(_targetRotation);
            _hasRotated = false;
        }
    }

    protected virtual void Update()
    {
        
    }
    
    protected void UpdateVelocity()
    {
        Vector3 pos = transform.position;
        Velocity = (pos - _prevPosition) / Timer.Delta;
        _prevPosition = pos;
    }

    public abstract void Move(Vector3 dir, float speed);

    public void Rotate(Quaternion rotation, float speed)
    {
        _targetRotation = Quaternion.Slerp(Rigid.rotation, rotation, speed * Timer.FixedDelta);
        _hasRotated = true;
    }

    public virtual void TakeDamage(GameObject attacker, int damage, int? reactionId)
    {
        CurrentHealth -= damage;

        if (reactionId.HasValue)
        {
            Animator.SetInteger(AnimatorHash.ReactionId, reactionId.Value);
            Animator.SetTrigger(AnimatorHash.TakeReaction);
        }
        else if (IsDead)
        {
            SetRagdollActive(true);
        }
    }

    protected virtual void OnDead()
    {
        IsDead = true;
        this.enabled = false;
        Collider.enabled = false;

        FMODUnity.RuntimeManager.PlayOneShot(_deathSound, transform.position);
        _onCharacterDead?.Invoke();
    }
    
    public event Action<int> OnHealthChanged
    {
        add { _onHealthChanged += value; }
        remove { _onHealthChanged -= value; }
    }

    public event Action OnCharacterDead
    {
        add { _onCharacterDead += value; }
        remove { _onCharacterDead -= value; }
    }

    public int CurrentHealth
    {
        get
        {
            return _currentHealth;
        }

        set
        {
            _currentHealth = value > 0 ? value : 0;
            _onHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth == 0)
            {
                OnDead();
            }
        }
    }

    protected CapsuleCollider Collider { get; private set; }
    protected Rigidbody Rigid { get; private set; }
    protected Animator Animator { get; private set; }
    protected LocalTimer Timer { get; private set; }
    protected WeaponInventory WeaponInventory { get; private set; }

    protected bool ApplyRootMotion { get; set; }
    protected Vector3 Velocity { get; private set; }

    public int MaxHealth => _maxHealth;
    public bool IsDead { get; private set; }
}