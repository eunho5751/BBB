using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

public enum MeleeAIState
{
    Idle,
    Strafe,
    Rush,
    Attack,
    Hit,
    Dead,
}

public class MeleeAICharacterController : AICharacterControllerBase<MeleeAIState>
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
    [TitleGroup("Rush")]
    [SerializeField]
    private float _rushSpeed;
    [SerializeField]
    private float _rushRotateSpeed;
    [TitleGroup("Attack")]
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private float _attackRotateSpeed;
    [SerializeField]
    private float _attackRequestCooldown;
    [TitleGroup("Etc")]
    [SerializeField, AssetsOnly]
    private GameObject _attackIcon;

    private float _strafeDir;
    private float _strafeElapsedTime;
    private float _strafeElapsedCooldown;
    private float _attackRequestElapsedCooldown;
    private bool _isBackwards;

    private void Activate()
    {
        CurrentState = MeleeAIState.Strafe;
    }
    
    protected override void OnStateEnter(MeleeAIState state)
    {
        switch (state)
        {
            case MeleeAIState.Idle:
                {

                }
                break;

            case MeleeAIState.Strafe:
                {
                    _strafeDir = Random.value > 0.5f ? 1f : -1f;
                    //Animator.SetFloat(AnimatorHash.VelocityX, 0f);
                    //Animator.SetFloat(AnimatorHash.VelocityZ, 0f);
                }
                break;

            case MeleeAIState.Rush:
                {
                    
                }
                break;

            case MeleeAIState.Attack:
                {
                    Animator.SetTrigger(AnimatorHash.Attack);
                }
                break;

            case MeleeAIState.Hit:
                {
                    AttackTarget.SendMessage("CompleteAttack", gameObject);
                    RVOController.locked = true;

                    if (IsDead)
                        CurrentState = MeleeAIState.Dead;
                }
                break;
        }
    }

    protected override void OnStateUpdate(MeleeAIState state)
    {
        switch (state)
        {
            case MeleeAIState.Strafe:
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

            case MeleeAIState.Rush:
                {
                    Vector3 diff = AttackTarget.position - transform.position;
                    Vector3 dir = diff;
                    dir.y = 0f;

                    if (diff.sqrMagnitude > _attackRange * _attackRange)
                    {
                        Move(dir, _rushSpeed);
                    }
                    else
                    {
                        CurrentState = MeleeAIState.Attack;
                    }

                    Rotate(Quaternion.LookRotation(dir), _rushRotateSpeed);

                    Vector3 velocity = transform.InverseTransformVector(Velocity);
                    Animator.SetFloat(AnimatorHash.VelocityX, Mathf.Min(velocity.x, _rushSpeed), 0.1f, Timer.Delta);
                    Animator.SetFloat(AnimatorHash.VelocityZ, Mathf.Min(velocity.z, _rushSpeed), 0.1f, Timer.Delta);
                }
                break;
        }
    }

    protected override void OnStateExit(MeleeAIState state)
    {
        switch (state)
        {
            case MeleeAIState.Idle:
                {
                    Animator.SetTrigger("Combat");
                }
                break;

            case MeleeAIState.Strafe:
                {
                    _strafeElapsedCooldown = 0f;
                    _strafeElapsedTime = 0f;
                    _attackRequestElapsedCooldown = 0f;
                }
                break;

            case MeleeAIState.Rush:
                {

                }
                break;

            case MeleeAIState.Attack:
                {
                    AttackTarget.SendMessage("CompleteAttack", gameObject);
                    Animator.ResetTrigger(AnimatorHash.Attack);
                }
                break;

            case MeleeAIState.Hit:
                {
                    RVOController.locked = false;
                }
                break;
        }
    }
    
    public override void TakeDamage(GameObject attacker, int damage, int? reactionId)
    {
        base.TakeDamage(attacker, damage, reactionId);
        CurrentState = MeleeAIState.Hit;
    }

    protected override void OnReactionExit()
    {
        base.OnReactionExit();

        CurrentState = MeleeAIState.Strafe;
    }

    protected override void OnAttackEnter(AttackInfo attackInfo)
    {
        base.OnAttackEnter(attackInfo);

        ApplyRootMotion = true;

        Vector3 dir = AttackTarget.position - transform.position;
        dir.y = 0f;
        StartCoroutine("Correction", dir);
    }

    protected override void OnAttackExit()
    {
        base.OnAttackExit();

        ApplyRootMotion = false;
        StopCoroutine("Correction");

        CurrentState = MeleeAIState.Strafe;
    }

    private IEnumerator Correction(Vector3 direction)
    {
        while (Vector3.Angle(transform.forward, direction) > 0.01f)
        {
            Rotate(Quaternion.LookRotation(direction), _attackRotateSpeed);
            yield return CoroutineManager.WaitForFixedUpdate;
        }
    }

    private void AllowAttack()
    {
        GameObject icon = PoolManager.Instance.Spawn(_attackIcon, transform.position + new Vector3(0f, Collider.height + _attackIcon.transform.position.y, 0f));
        icon.transform.SetParent(transform, true);
        CurrentState = MeleeAIState.Rush;
    }

    protected override MeleeAIState InitState => MeleeAIState.Idle;
}