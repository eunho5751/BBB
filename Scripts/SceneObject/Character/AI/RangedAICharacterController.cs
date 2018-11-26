using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public enum RangedAIState
{
    Idle,
    Strafe,
    Attack,
    Hit,
    Dead
}

public class RangedAICharacterController : AICharacterControllerBase<RangedAIState>
{
    [TitleGroup("Strafe")]
    [SerializeField]
    private float _strafeRange;
    [SerializeField]
    private float _dangerRange;
    [SerializeField]
    private float _strafeSpeed;
    [SerializeField]
    private float _strafeRotateSpeed;
    [SerializeField]
    private float _strafeCooldown;
    [SerializeField]
    private float _strafeTime;
    [TitleGroup("Attack")]
    [SerializeField]
    private float _attackRotateSpeed;
    [SerializeField]
    private float _attackRequestCooldown;
    [TitleGroup("Etc")]
    [SerializeField, AssetsOnly]
    private GameObject _attackIcon;

    private RangedWeapon _weapon;
    private float _strafeDir;
    private float _strafeElapsedTime;
    private float _strafeElapsedCooldown;
    private float _attackRequestElapsedCooldown;
    private bool _isBackwards;

    protected override void Start()
    {
        base.Start();

        _weapon = WeaponInventory.CurrentWeapon as RangedWeapon;
    }

    private void Activate()
    {
        CurrentState = RangedAIState.Strafe;
    }

    protected override void OnStateEnter(RangedAIState state)
    {
        switch (state)
        {
            case RangedAIState.Idle:
                {

                }
                break;

            case RangedAIState.Strafe:
                {
                    _strafeDir = Random.value > 0.5f ? 1f : -1f;
                    Animator.SetFloat(AnimatorHash.VelocityX, 0f);
                    Animator.SetFloat(AnimatorHash.VelocityZ, 0f);
                }
                break;

            case RangedAIState.Attack:
                {
                    _weapon.Reload();
                }
                break;

            case RangedAIState.Hit:
                {
                    AttackTarget.SendMessage("CompleteAttack", gameObject);
                    RVOController.locked = true;

                    if (IsDead)
                        CurrentState = RangedAIState.Dead;
                }
                break;
        }
    }

    protected override void OnStateUpdate(RangedAIState state)
    {
        switch (state)
        {
            case RangedAIState.Strafe:
                {
                    Vector3 diff = AttackTarget.position - transform.position;
                    Vector3 dir = diff;
                    dir.y = 0f;

                    float sqrDist = diff.sqrMagnitude;
                    if (sqrDist > _strafeRange * _strafeRange)
                    {
                        // Move To Target
                        Move(dir, _strafeSpeed);
                    }
                    else
                    {
                        // Request Attack
                        if (_attackRequestElapsedCooldown < _attackRequestCooldown)
                        {
                            _attackRequestElapsedCooldown += Timer.Delta;
                            if (_attackRequestElapsedCooldown >= _attackRequestCooldown)
                            {
                                AttackTarget.SendMessage("RequestAttack", gameObject);
                                _attackRequestElapsedCooldown = 0f;
                            }
                        }

                        // Random Strafe / Step Back
                        if (_strafeElapsedCooldown < _strafeCooldown)
                        {
                            if (_isBackwards)
                            {
                                float range = Mathf.Min(_strafeRange, _dangerRange + 0.5f);
                                if (sqrDist <= range * range)
                                    Move(-dir, _strafeSpeed * 0.5f);
                                else
                                    _isBackwards = false;
                            }
                            else if (sqrDist > _dangerRange * _dangerRange)
                            {
                                _strafeElapsedCooldown += Timer.Delta;
                                if (_strafeElapsedCooldown >= _strafeCooldown)
                                {
                                    _strafeDir = Random.value > 0.5f ? 1f : -1f;
                                    _strafeElapsedTime = _strafeElapsedTime < _strafeTime ? _strafeTime - _strafeElapsedTime : 0f;
                                    _strafeElapsedCooldown = 0f;
                                }

                                if (_strafeElapsedTime < _strafeTime)
                                {
                                    Vector3 moveDir = _strafeDir * (Quaternion.AngleAxis(75f, AttackTarget.up) * dir);
                                    Move(moveDir, _strafeSpeed);
                                    _strafeElapsedTime += Timer.Delta;
                                }
                            }
                            else
                            {
                                _isBackwards = true;
                            }
                        }
                    }

                    Rotate(Quaternion.LookRotation(dir), _strafeRotateSpeed);

                    Vector3 velocity = transform.InverseTransformVector(Velocity);
                    Animator.SetFloat(AnimatorHash.VelocityX, Mathf.Min(velocity.x, _strafeSpeed), 0.1f, Timer.Delta);
                    Animator.SetFloat(AnimatorHash.VelocityZ, Mathf.Min(velocity.z, _strafeSpeed), 0.1f, Timer.Delta);
                }
                break;

            case RangedAIState.Attack:
                {
                    if (_weapon.IsShootable)
                    {
                        Animator.SetTrigger("Attack");
                        _weapon.Fire();
                    }
                    else if (_weapon.CurrentAmmo == 0)
                    {
                        CurrentState = RangedAIState.Strafe;
                    }

                    Vector3 dir = AttackTarget.position - transform.position;
                    dir.y = 0f;
                    Rotate(Quaternion.LookRotation(dir), _attackRotateSpeed);

                    Animator.SetFloat(AnimatorHash.VelocityX, 0f, 0.1f, Timer.Delta);
                    Animator.SetFloat(AnimatorHash.VelocityZ, 0f, 0.1f, Timer.Delta);
                }
                break;
        }
    }

    protected override void OnStateExit(RangedAIState state)
    {
        switch (state)
        {
            case RangedAIState.Idle:
                {
                    Animator.SetTrigger("Combat");
                }
                break;

            case RangedAIState.Strafe:
                {
                    _strafeElapsedCooldown = 0f;
                    _strafeElapsedTime = 0f;
                    _attackRequestElapsedCooldown = 0f;
                }
                break;

            case RangedAIState.Attack:
                {
                    AttackTarget.SendMessage("CompleteAttack", gameObject);
                }
                break;

            case RangedAIState.Hit:
                {
                    RVOController.locked = false;
                }
                break;
        }
    }

    public override void TakeDamage(GameObject attacker, int damage, int? reactionId)
    {
        base.TakeDamage(attacker, damage, reactionId);
        CurrentState = RangedAIState.Hit;
    }

    protected override void OnReactionExit()
    {
        base.OnReactionExit();

        CurrentState = RangedAIState.Strafe;
    }

    private void AllowAttack()
    {
        GameObject icon = PoolManager.Instance.Spawn(_attackIcon, transform.position + new Vector3(0f, Collider.height + _attackIcon.transform.position.y, 0f));
        icon.transform.SetParent(transform, true);
        CurrentState = RangedAIState.Attack;
    }

    protected override RangedAIState InitState => RangedAIState.Idle;
}
