using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Animator))]
public class FootstepEffectEmitter : MonoBehaviour
{
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField, Required]
    private string _leftWeightParam, _rightWeightParam;
    [SerializeField, Required]
    private Transform _leftFoot, _rightFoot;
    [SerializeField, Required]
    private TextureEffectMap _footstepEffectMap;

    private Animator _animator;
    private int _lFootHash, _rFootHash;
    private float _lFootWeight, _rFootWeight;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _lFootHash = Animator.StringToHash(_leftWeightParam);
        _rFootHash = Animator.StringToHash(_rightWeightParam);
    }

    private void Update()
    {
        OnFootPlant(_lFootHash, _leftFoot.position, ref _lFootWeight);
        OnFootPlant(_rFootHash, _rightFoot.position, ref _rFootWeight);
    }

    private void OnFootPlant(int hash, Vector3 origin, ref float value)
    {
        float current = _animator.GetFloat(hash);
        if (value < 0f && current > 0f)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(origin + new Vector3(0F, 0.1F, 0F), Vector3.down, out hitInfo, 0.5F + 0.1F, _groundLayer))
            {
                var sceneObj = hitInfo.transform.GetComponent<SceneObject>();
                if (sceneObj != null)
                {
                    TextureEffectEmitter.Emit(_footstepEffectMap, sceneObj.TextureType, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
                }
            }
        }

        value = current;
    }
}