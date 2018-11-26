using UnityEngine;

public class UI_Title : MonoBehaviour
{
    [SerializeField]
    private Animator _animator;
    [SerializeField, FMODUnity.EventRef]
    private string _sound;

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            FMODUnity.RuntimeManager.PlayOneShot(_sound);
            _animator.SetTrigger("PressAnyKey");
        }
    }

    private void Shut()
    {
        _animator.enabled = false;
    }
}
