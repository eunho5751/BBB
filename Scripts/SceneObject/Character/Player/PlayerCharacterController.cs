using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using Cinemachine;
using Sirenix.OdinInspector;
using Kino;

[RequireComponent(typeof(AfterImage))]
[RequireComponent(typeof(TimeRigidbody))]
public class PlayerCharacterController : CharacterControllerBase
{
    private static readonly int MaxCorrectionCandidates = 5; 

    private static class AnimatorHash
    {
        public static readonly int Speed = Animator.StringToHash("Speed");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Dash = Animator.StringToHash("Dash");
        public static readonly int DashWeight = Animator.StringToHash("DashWeight");
        public static readonly int Slow = Animator.StringToHash("Slow");
    }

    private enum MoveState
    {
        None,
        Walking,
        Running
    }
    
    [TitleGroup("Move")]
    [SerializeField, Tooltip("걷기 속도")]
    private float _walkSpeed;
    [SerializeField, Tooltip("달리기 속도")]
    private float _runSpeed;
    [SerializeField, Tooltip("달리기 회전 속도")]
    private float _moveRotateSpeed;
    [TitleGroup("Attack")]
    [SerializeField, Tooltip("적 레이어")]
    private int _enemyLayer;
    [SerializeField, Tooltip("타겟 보정 이동 속도")]
    private float _correctionMoveSpeed;
    [SerializeField, Tooltip("타겟 보정 회전 속도")]
    private float _correctionRotateSpeed;
    [SerializeField, Tooltip("타겟 보정 최대 거리")]
    private float _correctionMaxDistance;
    [SerializeField, Tooltip("타겟 보정 최대 각")]
    private float _correctionMaxAngle;
    [SerializeField, Tooltip("콤보 지속시간")]
    private float _comboDuration;
    [TitleGroup("Dash")]
    [SerializeField, Tooltip("대시 속도")]
    private float _dashSpeed;
    [SerializeField, Tooltip("대시 쿨타임")]
    private float _dashCooldown;
    [SerializeField, Tooltip("대시 회전 속도")]
    private float _dashRotateSpeed;
    [TitleGroup("Slow")]
    [SerializeField, Required, Tooltip("슬로우 타이머 키")]
    private string _slowTimerKey;
    [SerializeField, Tooltip("적용할 슬로우 값 (기본 값 = 1)")]
    private float _slowAmount;
    [SerializeField, Tooltip("슬로우 게이지 초당 증가율 (사용중이 아닐 때)")]
    private float _slowGaugeIncrement;
    [SerializeField, Tooltip("슬로우 게이지 초당 감소율 (사용중 일 때)")]
    private float _slowGaugeDecrement;
    [TitleGroup("Etc")]
    [SerializeField, Tooltip("플레이어를 동시에 공격가능한 적의 수"), PropertyOrder(1000)]
    private int _maxAttackers;
    [SerializeField, Tooltip("키 입력 잠금"), PropertyOrder(1000)]
    private bool _lockInput;
    [SerializeField]
    private GameObject _popup;

    private CinemachineImpulseSource _impulseSource;
    private AfterImage _afterImage;
    private AnalogGlitch _glitch;
    private PostProcessingProfile _ppprofile;

    private MoveState _moveState = MoveState.None;
    private Vector2 _moveAxes;
    private bool _lockRun;
    private bool _lockMovement;

    private bool _attackPressed;
    private bool _lockAttack;
    private bool _isAttacking;
    private bool _attackEnabled;
    private bool _attackSaved;
    private Collider[] _correctionCandidates = new Collider[MaxCorrectionCandidates];
    private Transform _targetEnemy;
    private Vector3 _attackDir;
    private bool _isReacting;
    private List<GameObject> _currentAttackers = new List<GameObject>();

    private bool _dashPressed;
    private float _dashElapsedCooldown;
    private bool _lockDash;

    private float _slowGauge;
    private bool _slowPressed;
    private bool _preSlow;
    private bool _usingSlow;
    private bool _lockSlow;
    private float _lastAbberation;

    private Vector3 _deltaPosition;
    private bool _hasMoved;

    private int _currentCombo;

    private void OnApplicationQuit()
    {
        if (GameManager.CurrentSceneGroup != null)
            PlayerPrefs.SetInt("SceneGroup", GameManager.CurrentSceneGroup._id);
    }

    protected override void Awake()
    {
        base.Awake();

        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _afterImage = GetComponent<AfterImage>();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        var abberation = _ppprofile.chromaticAberration.settings;
        abberation.intensity = _lastAbberation;
        _ppprofile.chromaticAberration.settings = abberation;
    }

    protected override void Start()
    {
        base.Start();
        
        Rigid.isKinematic = false;

        //_dashElapsedCooldown = _dashCooldown;
        //SlowGauge = 100f;
    }

    private void ResolveReferences()
    {
        _glitch = Camera.main.gameObject.AddComponent<AnalogGlitch>();
        _ppprofile = Camera.main.GetComponent<PostProcessingBehaviour>().profile;
    }

    protected override void FixedUpdate()
    {
        if (_hasMoved)
        {
            Vector3 gravity = new Vector3(0f, Rigid.velocity.y, 0f);
            Rigid.velocity = _deltaPosition / MainTimer.RawFixedDelta + gravity;

            _deltaPosition = Vector3.zero;
            _hasMoved = false;
        }

        base.FixedUpdate();

    }

    protected override void Update()
    {
        base.Update();
        UpdateVelocity();

        ReadInput();
        CheckMovement();
        CheckAttack();
        CheckDash();
        CheckSlow();
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        float maxSpeed = _moveState == MoveState.None ? 0f : (_moveState == MoveState.Walking ? _walkSpeed : _runSpeed);
        Animator.SetFloat(AnimatorHash.Speed, Mathf.Min(Rigid.velocity.magnitude, maxSpeed), 0.1f, Timer.Delta);
        if (Animator.GetFloat(AnimatorHash.Speed) < 0.01f)
            Animator.SetFloat(AnimatorHash.Speed, 0f);
    }

    private void ReadInput()
    {
        if (!LockInput)
        {
            _moveAxes.x = Input.GetAxisRaw("MoveHorizontal");
            _moveAxes.y = Input.GetAxisRaw("MoveVertical");
            _attackPressed = Input.GetButtonDown("Attack");
            _dashPressed = Input.GetButtonDown("Dash");
            _slowPressed = Input.GetButtonDown("Slow");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0f;
            _popup.SetActive(true);
        }
    }

    private void CheckMovement()
    {
        MoveState moveState = !LockMovement && _moveAxes.sqrMagnitude > 0f ? (!LockRun ? MoveState.Running : MoveState.Walking) : MoveState.None;

        if (moveState != MoveState.None)
        {
            float speed = _moveState == MoveState.Walking ? _walkSpeed : _runSpeed;
            Vector3 dir = Vector3.forward * _moveAxes.y + Vector3.right * _moveAxes.x;
            Move(dir, speed);
            Rotate(Quaternion.LookRotation(dir), _moveRotateSpeed);
        }
        else if (_moveState != MoveState.None)
        {
            ResetMovement();
        }

        _moveState = moveState;
    }
    
    private void ResetMovement()
    {
        Rigid.velocity = new Vector3(0f, 0f, 0f);
        _deltaPosition = Vector3.zero;
        _targetRotation = transform.rotation;
    }

    private void CheckAttack()
    {
        if (!LockAttack && _attackPressed)
        {
            if (!_isAttacking)
            {
                StartCoroutine(AdvanceAttack());
                _isAttacking = true;
            }
            else if (_attackEnabled)
            {
                _attackEnabled = false;
                _attackSaved = true;
            }
        }
    }

    private void EnableAttack()
    {
        _attackEnabled = true;
    }
    
    private void CheckAttackTransition()
    {
        if (_attackSaved)
        {
            StartCoroutine(AdvanceAttack());
        }
        
        _attackEnabled = false;
    }

    protected override void OnAttackEnter(AttackInfo attackInfo)
    {
        base.OnAttackEnter(attackInfo);

        if (_usingSlow)
            _afterImage.Play(0.2f, new Color(1f, 1f, 1f, 0.1f));

        _attackSaved = false;
        LockMovement = true;
        LockSlow = true;
        ApplyRootMotion = true;
    }

    protected override void OnAttackExit()
    {
        base.OnAttackExit();

        if (!_attackSaved || _isReacting)
        {
            _isAttacking = false;
            _attackEnabled = false;
            LockMovement = false;
            LockSlow = false;
            ApplyRootMotion = false;
            ResetMovement();

            // Stop Correction
            StopCoroutine("MoveCorrection");
            StopCoroutine("RotateCorrection");
        }
    }

    private IEnumerator AdvanceAttack()
    {
        // Stop Correction
        StopCoroutine("MoveCorrection");
        StopCoroutine("RotateCorrection");

        // Get Control Direction
        _attackDir = _moveAxes.sqrMagnitude > 0f ? Vector3.forward * _moveAxes.y + Vector3.right * _moveAxes.x : transform.forward;
        _attackDir.y = 0f;

        // Find Closest Enemy
        _targetEnemy = null;
        _targetEnemy = GetClosestEnemy(transform.position, _attackDir, _correctionMaxDistance, _correctionMaxAngle);

        if (_targetEnemy != null)
        {
            StartCoroutine("RotateCorrection", _targetEnemy);
            //if (!_isAttacking)
            //    yield return StartCoroutine("MoveCorrection", _targetEnemy);
        }
        else
        {
            StartCoroutine("RotateCorrection", _attackDir);
        }

        Animator.SetTrigger(AnimatorHash.Attack);
        yield break;
    }

    private IEnumerator MoveCorrection(Transform target)
    {
        _afterImage.Play(0.03f, 0.2f, new Color(1f, 1f, 1f, 0.05f));
        
        RaycastHit hitInfo;
        while (true)
        {
            Vector3 dir = target.position - transform.position;
            dir.Normalize();

            Vector3 center = Collider.bounds.center - dir * 0.01f;
            Vector3 h = new Vector3(0f, Collider.height * 0.5f - Collider.radius, 0f);
            Vector3 p1 = center + h;
            Vector3 p2 = center - h;
            float dist = _correctionMoveSpeed * Timer.FixedDelta;

            if (Physics.CapsuleCast(p1, p2, Collider.radius, dir, out hitInfo, dist, 1 << _enemyLayer))
            {
                Move(dir, hitInfo.distance / Timer.FixedDelta);
                break;
            }
            else
            {
                Move(dir, _correctionMoveSpeed);
                yield return null;
            }
        }

        _afterImage.Stop();
    }

    private IEnumerator RotateCorrection(Transform target)
    {
        while (true)
        {
            Vector3 diff = target.position - transform.position;
            diff.y = 0f;

            if (Vector3.Angle(transform.forward, diff) > 0.01f)
            {
                Rotate(Quaternion.LookRotation(diff), _correctionRotateSpeed);
                yield return CoroutineManager.WaitForFixedUpdate;
            }
            else
            {
                yield break;
            }
        }
    }

    private IEnumerator RotateCorrection(Vector3 direction)
    {
        while (Vector3.Angle(transform.forward, direction) > 0.01f)
        {
            Rotate(Quaternion.LookRotation(direction), _correctionRotateSpeed);
            yield return CoroutineManager.WaitForFixedUpdate;
        }
    }

    private Transform GetClosestEnemy(Vector3 position, Vector3 direction, float maxDistance, float maxAngle)
    {
        Transform target = null;
        
        int count = 0;
        if ((count = Physics.OverlapSphereNonAlloc(position, maxDistance, _correctionCandidates, 1 << _enemyLayer)) > 0)
        {
            float closestDist = maxDistance;
            float closestAngle = maxAngle;
            for (int i = 0; i < count; i++)
            {
                Vector3 diff = _correctionCandidates[i].transform.position - position;
                diff.y = 0F;

                float dist = Vector3.Distance(position, _correctionCandidates[i].transform.position);
                float angle = Mathf.Acos(Vector3.Dot(direction.normalized, diff.normalized)) * Mathf.Rad2Deg;
                
                if (dist < closestDist && Mathf.Abs(angle) < closestAngle)
                {
                    closestDist = dist;
                    closestAngle = angle;
                    target = _correctionCandidates[i].transform;
                }
            }
        }

        return target;
    }

    private void RequestAttack(GameObject attacker)
    {
        if (_currentAttackers.Count < _maxAttackers)
        {
            attacker.SendMessage("AllowAttack");
            _currentAttackers.Add(attacker);
        }
    }

    private void CompleteAttack(GameObject attacker)
    {
        _currentAttackers.Remove(attacker);
    }

    private void OnAttackHit(GameObject target)
    {
        //var character = target.GetComponent<CharacterControllerBase>();
        //if (character != null && character.IsDead)
        //{
        //    StopCoroutine("KillEffect");
        //    StartCoroutine("KillEffect");
        //}

        _impulseSource.GenerateImpulse();
        
        _currentCombo++;
        StopCoroutine("ComboTimer");
        StartCoroutine("ComboTimer");
    }

    private IEnumerator KillEffect()
    {
        //MainTimer.Instance.LocalScale = 0.1f;
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.1f * 0.02f;
        Time.maximumDeltaTime /= 0.1f;

        float time = 0f;
        while (time < 0.5f)
        {
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        //MainTimer.Instance.LocalScale = 1f;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        Time.maximumDeltaTime *= 0.1f;
    }

    private IEnumerator ComboTimer()
    {
        UIManager.Instance.SetComboActive(true);
        UIManager.Instance.SetComboNumber(CurrentCombo);

        float elapsedTime = 0f;
        while (elapsedTime < _comboDuration)
        {
            elapsedTime += MainTimer.RawDelta;
            yield return null;
        }

        UIManager.Instance.SetComboActive(false);
        _currentCombo = 0;
    }

    private void CheckDash()
    {
        _dashElapsedCooldown += MainTimer.RawDelta;
        if (!LockDash && _dashPressed && DashElapsedCooldown >= DashCooldown)
        {
            _dashElapsedCooldown = 0f;
            Animator.SetTrigger(AnimatorHash.Dash);
        }
    }

    private IEnumerator Dash()
    {
        Vector3 dashDir = _moveAxes.sqrMagnitude > 0F ? Vector3.forward * _moveAxes.y + Vector3.right * _moveAxes.x : transform.forward;
        dashDir.y = 0F;
        dashDir.Normalize();

        while (true)
        {
            Move(dashDir, _dashSpeed * Animator.GetFloat(AnimatorHash.DashWeight));
            Rotate(Quaternion.LookRotation(dashDir), _dashRotateSpeed);

            Vector3 vel = Rigid.velocity;
            vel.y = 0f;
            Rigid.velocity = vel;

            yield return CoroutineManager.WaitForFixedUpdate;
        }
    }

    private void OnDashEnter()
    {
        StartCoroutine("Dash");

        _afterImage.Play(0.03f, 0.2f, new Color(1f, 1f, 1f, 0.1f));
        Physics.IgnoreLayerCollision(gameObject.layer, _enemyLayer, true);
        LockInput = true;
    }

    private void OnDashExit()
    {
        StopCoroutine("Dash");

        _afterImage.Stop();
        Physics.IgnoreLayerCollision(gameObject.layer, _enemyLayer, false);
        LockInput = false;
        ResetMovement();
    }

    private void CheckSlow()
    {
        SlowGauge += MainTimer.RawDelta * (_usingSlow ? -_slowGaugeDecrement : _slowGaugeIncrement * (1 + CurrentCombo / 10f));
        if (!LockSlow && _slowPressed && SlowGauge >= 100f)
        {
            Animator.SetTrigger(AnimatorHash.Slow);
        }
    }

    private void OnSlowEnter()
    {
        StartCoroutine("PreSlow");
        LockInput = true;
    }

    private void OnSlowExit()
    {
        if (!_usingSlow)
            _preSlow = false;
        LockInput = false;
    }

    private void InvokeSlow()
    {
        _preSlow = false;
        StopCoroutine("PreSlow");
        StartCoroutine("Slow");
    }

    private IEnumerator PreSlow()
    {
        _preSlow = true;

        TimerBase timer = MainTimer.Instance.GetTimer(_slowTimerKey);
        if (timer != null)
        {
            while (_preSlow)
            {
                timer.LocalScale = Mathf.Lerp(timer.LocalScale, _slowAmount, MainTimer.RawDelta * 2f);
                yield return null;
            }

            timer.LocalScale = 1f;
        }

        _preSlow = false;
    }

    private IEnumerator Slow()
    {
        _impulseSource.GenerateImpulse();

        float speed = 10f;
        while (_glitch.colorDrift < 2f)
        {
            _glitch.colorDrift += speed * MainTimer.RawDelta;
            yield return null;
        }
        _glitch.colorDrift = 0f;

        var abberationSettings = _ppprofile.chromaticAberration.settings;
        _lastAbberation = abberationSettings.intensity;

        abberationSettings.intensity = 1f;
        _ppprofile.chromaticAberration.settings = abberationSettings;

        _usingSlow = true;
        TimerBase timer = MainTimer.Instance.GetTimer(_slowTimerKey);
        if (timer != null)
        {
            timer.LocalScale = _slowAmount;
            while (SlowGauge > Mathf.Epsilon)
                yield return null;
            timer.LocalScale = 1f;
        }
        _usingSlow = false;

        abberationSettings.intensity = _lastAbberation;
        _ppprofile.chromaticAberration.settings = abberationSettings;
    }

    public override void TakeDamage(GameObject attacker, int damage, int? reactionId)
    {
        if (Animator.GetFloat(AnimatorHash.DashWeight) > 0f)
            return;

        // Check whether this attack is back-attack.
        float angle = Mathf.Abs(Vector3.Angle(transform.forward, attacker.transform.position - transform.position));
        if (angle > 90f)
            reactionId *= -1;

        base.TakeDamage(attacker, damage, reactionId);
    }

    protected override void OnReactionEnter()
    {
        base.OnReactionEnter();

        LockInput = true;
        _isReacting = true;
    }

    protected override void OnReactionExit()
    {
        base.OnReactionExit();

        LockInput = false;
        _isReacting = false;
        ResetMovement();
    }

    protected override void OnDead()
    {
        base.OnDead();

        UIManager.Instance.GameOver();
    }

    private void Deactivate()
    {
        LockInput = true;
        ResetMovement();
        this.enabled = false;
    }

    private void Activate()
    {
        LockInput = false;
        this.enabled = true;
    }

    private void RecoverHealth()
    {
        CurrentHealth += (int)(MaxHealth * 0.25f);
    }

    public override void Move(Vector3 dir, float speed)
    {
        _deltaPosition += dir.normalized * (speed * Timer.FixedDelta);
        _hasMoved = true;
    }

    public float SlowGauge
    {
        get { return _slowGauge; }
        set { _slowGauge = Mathf.Max(0f, Mathf.Min(100f, value)); }
    }

    public bool LockMovement
    {
        get
        {
            return _lockMovement;
        }

        set
        {
            _lockMovement = value;
            if (value)
            {
                _moveAxes = Vector3.zero;
                Animator.SetFloat(AnimatorHash.Speed, 0f);
            }
        }
    }

    public bool LockRun
    {
        get { return _lockRun; }
        set { _lockRun = value; }
    }
    
    public bool LockAttack
    {
        get
        {
            return _lockAttack;
        }

        set
        {
            _lockAttack = value;
            if (value)
            {
                _attackEnabled = _attackPressed = _attackSaved = _isAttacking = false;
                Animator.ResetTrigger(AnimatorHash.Attack);
            }
        }
    }
    
    public bool LockDash
    {
        get
        {
            return _lockDash;
        }

        set
        {
            _lockDash = value;
            if (value)
            {
                _dashPressed = false;
                Animator.ResetTrigger(AnimatorHash.Dash);
            }
        }
    }

    public bool LockSlow
    {
        get
        {
            return _lockSlow;
        }

        set
        {
            _lockSlow = value;
            if (value)
            {
                _slowPressed = false;
                Animator.ResetTrigger(AnimatorHash.Slow);
            }
        }
    }

    public bool LockInput
    {
        get
        {
            return _lockInput;
        }

        set
        {
            _lockInput = value;
            LockMovement = value;
            LockAttack = value;
            LockDash = value;
            LockSlow = value;
        }
    }

    public int CurrentCombo => _currentCombo;
    public float DashCooldown => _dashCooldown;
    public float DashElapsedCooldown => Mathf.Min(_dashElapsedCooldown, DashCooldown);
}