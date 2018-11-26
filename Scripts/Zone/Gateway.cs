using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider))]
public class Gateway : MonoBehaviour
{
    [SerializeField, FMODUnity.EventRef]
    private string _sound;
    [SerializeField]
    private SceneGroup _sceneGroup;
    [SerializeField]
    private Image _fader;
    [SerializeField]
    private float _duration;
    [SerializeField]
    private bool _black = true;

    private BoxCollider _collider;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FMODUnity.RuntimeManager.PlayOneShot(_sound, transform.position);
            other.GetComponent<PlayerCharacterController>().LockInput = true;
            other.SendMessage("ResetMovement");
            StartCoroutine(Fade());
        }
    }

    private IEnumerator Fade()
    {
        float time = 0f;
        while (time < _duration)
        {
            time += MainTimer.RawDelta;
            float a = _fader.color.a;
            a = Mathf.Lerp(0f, 1f, time / _duration);
            _fader.color = _black ? new Color(0f, 0f, 0f, a) : new Color(1f, 1f, 1f, a);
            yield return null;
        }

        GameManager.Instance.LoadScene(_sceneGroup);
    }
}