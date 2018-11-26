using UnityEngine;
using Sirenix.OdinInspector;

public class RandomInteger : StateMachineBehaviour
{
    [System.Serializable]
    private struct ValueInfo
    {
        private enum ValueType
        {
            Constant,
            Parameter
        }

        [SerializeField]
        private ValueType _type;
        [SerializeField, ShowIf("_type", optionalValue:ValueType.Constant)]
        private int _value;
        [SerializeField, ShowIf("_type", optionalValue: ValueType.Parameter)]
        private string _parameter;

        public int GetValue(Animator animator)
        {
            switch (_type)
            {
                case ValueType.Constant:
                    return _value;
                case ValueType.Parameter:
                    return animator.GetInteger(_parameter);
                default:
                    return 0;
            }
        }
    }

    [SerializeField]
    private string _parameter;
    [SerializeField]
    private ValueInfo _minValue;
    [SerializeField]
    private ValueInfo _maxValue;

    public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        animator.SetInteger(_parameter, Random.Range(_minValue.GetValue(animator), _maxValue.GetValue(animator) + 1));
    }
}
