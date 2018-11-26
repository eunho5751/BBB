using UnityEngine;
using UnityEngine.UI;

public class UI_Fader : MonoBehaviour
{
    [SerializeField]
    private Image _fadeImage;
    [SerializeField]
    private float _duration;

    private float _elapsedTime;

    private void Update()
    {
        _elapsedTime += MainTimer.RawDelta;
        float a = _fadeImage.color.a;
        a = Mathf.Lerp(a, 1f, _elapsedTime / _duration);
        _fadeImage.color = new Color(0f, 0f, 0f, a);
    }
}