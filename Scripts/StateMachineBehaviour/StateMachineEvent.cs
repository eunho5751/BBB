using UnityEngine;
using Sirenix.OdinInspector;

public class StateMachineEvent : SerializedStateMachineBehaviour
{
    [SerializeField]
    private SMBEventInfo _enterEvent;
    [SerializeField]
    private SMBEventInfo _exitEvent;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if (!string.IsNullOrEmpty(_enterEvent.FunctionName))
            animator.SendMessage(_enterEvent.FunctionName, _enterEvent.Parameter);
    }

    public override void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (!string.IsNullOrEmpty(_exitEvent.FunctionName))
            animator.SendMessage(_exitEvent.FunctionName, _exitEvent.Parameter);
    }
}