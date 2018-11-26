using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;
using Pathfinding.RVO;
using Pathfinding;

[RequireComponent(typeof(TimeRichAI))]
[RequireComponent(typeof(RichAI))]
[RequireComponent(typeof(NavmeshClamp))]
[RequireComponent(typeof(RVOController))]
public abstract class AICharacterControllerBase<TState> : CharacterControllerBase
{
    protected static class AnimatorHash
    {
        public static readonly int VelocityX = Animator.StringToHash("VelocityX");
        public static readonly int VelocityZ = Animator.StringToHash("VelocityZ");
        public static readonly int Attack = Animator.StringToHash("Attack");
    }

    [TitleGroup("Etc", order: 1000)]
    [SerializeField, DisableContextMenu, PropertyOrder(1)]
    private GameObject[] _renderers = new GameObject[0];
    [TitleGroup("Debug", order: int.MaxValue)]
    [ShowInInspector, HideInEditorMode, DisableInPlayMode, PropertyOrder(2)]
    private TState _currentState;
    
    private RaycastHit _hitInfo;

    protected override void Awake()
    {
        base.Awake();

        AIAgent = GetComponent<RichAI>();
        RVOController = GetComponent<RVOController>();
    }

    protected override void Start()
    {
        base.Start();

        AttackTarget = GameObject.FindWithTag("Player").transform;
        AttackTarget.GetComponent<CharacterControllerBase>().OnCharacterDead += OnTargetDead;

        Rigid.isKinematic = true;
        AIAgent.updateRotation = false;
        
        _currentState = InitState;
        OnStateEnter(_currentState);
    }

    protected abstract void OnStateEnter(TState state);
    protected abstract void OnStateUpdate(TState state);
    protected abstract void OnStateExit(TState state);

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        base.Update();

        UpdateVelocity();
        OnStateUpdate(_currentState);
    }

    private void OnTargetDead()
    {
        Animator.SetFloat("VelocityX", 0f);
        Animator.SetFloat("VelocityZ", 0f);
        this.enabled = false;
    }

    protected override void OnDead ()
    {
        base.OnDead();
        
        RVOController.enabled = false;
        StartCoroutine(BlinkDead());
    }

    private IEnumerator BlinkDead()
    {
        yield return new WaitForSeconds(2f);

        var blinkDelay = new WaitForSeconds(0.025f);
        int blinkMax = 10;
        int blinkCount = 0;
        while (true)
        {
            blinkCount++;
            
            foreach (var renderer in _renderers)
                renderer.SetActive(false);
            if (blinkCount >= blinkMax)
                break;

            yield return blinkDelay;
            foreach (var renderer in _renderers)
                renderer.SetActive(true);
            yield return blinkDelay;
        }
    }

    public override void TakeDamage ( GameObject attacker, int damage, int? reactionId )
    {
        base.TakeDamage(attacker, damage, reactionId);

        Vector3 dir = attacker.transform.position - transform.position;
        dir.y = 0f;
        Rigid.rotation = Quaternion.LookRotation(dir);

        attacker.SendMessage("OnAttackHit", gameObject);
    }

    public override void Move(Vector3 dir, float speed)
    {
        AIAgent.maxSpeed = speed * Timer.TotalScale;
        AIAgent.Move(dir.normalized * (speed * Timer.FixedDelta));
    }

    protected TState CurrentState
    {
        get
        {
            return _currentState;
        }

        set
        {
            OnStateExit(_currentState);
            _currentState = value;
            OnStateEnter(_currentState);
        }
    }

    protected RVOController RVOController { get; private set; }
    protected RichAI AIAgent { get; private set; }
    protected abstract TState InitState { get; }
    protected Transform AttackTarget { get; private set; }
}

