using UnityEngine;
using UnityEngine.UI;

public class UI_Combo : MonoBehaviour
{
    [SerializeField]
    private Text _comboNumber;

    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void SetCombo(int comboNumber)
    {
        _comboNumber.text = comboNumber.ToString();
        _animator.Play(0);
    }
}