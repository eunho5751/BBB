using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI_Ending : MonoBehaviour
{
    [SerializeField]
    private string[] _messages;
    [SerializeField]
    private Text _text;
    [SerializeField]
    private float _fadeDuration;
    [SerializeField]
    private SceneGroup _mainMenu;
    [SerializeField]
    private VideoPlayer _video;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1f);
        StartCoroutine(Ending(0));
    }

    private IEnumerator Ending(int index)
    {
        _text.text = string.Empty;
        _text.color = new Color(0f, 0f, 0f, 1f);

        int charIdx = 0;
        char[] chars = _messages[index].ToCharArray();
        while (charIdx < chars.Length)
        {
            char c = chars[charIdx++];
            if (c == '\\')
            {
                c = '\n';
                yield return new WaitForSeconds(0.35f);
            }

            _text.text += c;

            yield return new WaitForSeconds(Random.Range(0.1f, 0.15f));
        }

        yield return new WaitForSeconds(1f);
        //if (index++ >= _messages.Length)
        //    yield return new WaitForSeconds(2f);

        float elapsedTime = 0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += MainTimer.RawDelta;
            float a = _text.color.a;
            a = Mathf.Lerp(1f, 0f, elapsedTime / _fadeDuration);
            _text.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        index++;
        if (index < _messages.Length)
            StartCoroutine(Ending(index));
        else
        {
            _video.Play();
            while (!_video.isPlaying)
                yield return null;

            while (_video.isPlaying)
            {
                if (Input.anyKeyDown)
                    break;

                yield return null;
            }

            UIManager._newGame = false;
            GameManager.Instance.LoadScene(_mainMenu);
        }

    }
}