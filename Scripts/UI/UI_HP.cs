using UnityEngine;
using UnityEngine.UI;

public class UI_HP : MonoBehaviour
{
    [SerializeField]
    private Image _bar;

    private PlayerCharacterController _player;

    private void Update()
    {
        if (_player == null)
            _player = FindObjectOfType<PlayerCharacterController>();

        _bar.fillAmount = Mathf.Clamp01((float)_player.CurrentHealth / _player.MaxHealth);
    }
}