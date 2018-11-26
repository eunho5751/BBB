using UnityEngine;
using Sirenix.OdinInspector;

public class StateEvent : SerializedStateMachineBehaviour
{
    [SerializeField]
    private SMBEventInfo _enterEvent;
    [SerializeField]
    private SMBEventInfo _preExitEvent;
    [SerializeField]
    private SMBEventInfo _exitEvent;

    private bool _isTransitioningIn;
    private bool _isTransitioningOut;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!string.IsNullOrEmpty(_enterEvent.FunctionName))
            animator.SendMessage(_enterEvent.FunctionName, _enterEvent.Parameter);

        _isTransitioningIn = animator.IsInTransition(layerIndex);
        _isTransitioningOut = false;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isTransitioningOut)
        {
            if (animator.IsInTransition(layerIndex))
            {
                if (!_isTransitioningIn)
                {
                    if (!string.IsNullOrEmpty(_preExitEvent.FunctionName))
                        animator.SendMessage(_preExitEvent.FunctionName, _preExitEvent.Parameter);
                    _isTransitioningOut = true;
                }
            }
            else if (_isTransitioningIn)
            {
                _isTransitioningIn = false;
            }
        }
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!_isTransitioningOut)
        {
            if (!string.IsNullOrEmpty(_preExitEvent.FunctionName))
                animator.SendMessage(_preExitEvent.FunctionName, _preExitEvent.Parameter);
            _isTransitioningOut = true;
        }

        if (!string.IsNullOrEmpty(_exitEvent.FunctionName))
            animator.SendMessage(_exitEvent.FunctionName, _exitEvent.Parameter);
    }
}
