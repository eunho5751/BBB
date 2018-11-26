using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class UI_Dash : MonoBehaviour
{
    [SerializeField]
    private Image _bar;
    
    private PlayerCharacterController _player;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void Update()
    {
        if (_player == null)
            _player = FindObjectOfType<PlayerCharacterController>();

        _bar.fillAmount = Mathf.Clamp01(_player.DashElapsedCooldown / _player.DashCooldown);
        if (_bar.fillAmount >= 1f)
        {
            if (!_animator.GetBool("Complete"))
                _animator.SetBool("Complete", true);
        }
        else
        {
            if (_animator.GetBool("Complete"))
                _animator.SetBool("Complete", false);
        }
    }
}
