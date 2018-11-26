using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_Opening : MonoBehaviour
{
    [System.Serializable]
    private struct CutInfo
    {
        public Sprite Sprite;
        public Text Text;
        public string Message;
    }

    [SerializeField]
    private Image _image;
    [SerializeField]
    private float _fadeDuration;
    [SerializeField]
    private CutInfo[] _cuts;
    [SerializeField]
    private SceneGroup _group;

    private bool _cancelable;
    private bool _cancel;

    private void Start()
    {
        StartCoroutine(StartCut(0));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GameManager.Instance.LoadScene(_group);
        }
        else if (Input.anyKeyDown)
        {
            if (_cancelable)
                _cancel = true;
        }
    }

    private IEnumerator StartCut(int index)
    {
        float elapsedTime = 0f;
        CutInfo cut = _cuts[index];

        _image.sprite = cut.Sprite;
        cut.Text.text = string.Empty;

        _image.color = new Color(0f, 0f, 0f, 0f);
        cut.Text.color = new Color(1f, 1f, 1f, 0f);

        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += MainTimer.RawDelta;
            float a = _image.color.a;
            a = Mathf.Lerp(0f, 1f, elapsedTime / _fadeDuration);
            _image.color = new Color(1f, 1f, 1f, a);
            cut.Text.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }
        
        _cancelable = true;
        int charIdx = 0;
        char[] chars = cut.Message.ToCharArray();
        while (!_cancel && charIdx < chars.Length)
        {
            char c = chars[charIdx++];
            if (c == '\\')
            {
                c = '\n';
                yield return new WaitForSeconds(0.35f);
            }

            cut.Text.text += c;
            
            yield return new WaitForSeconds(Random.Range(0.08f, 0.15f));
        }

        if (_cancel)
        {
            while (charIdx < chars.Length)
            {
                char c = chars[charIdx++];
                if (c == '\\')
                    c = '\n';
                cut.Text.text += c;
            }
        }

        _cancel = false;
        _cancelable = false;

        yield return new WaitForSeconds(1.5f);
        
        index++;

        elapsedTime = 0f;
        while (elapsedTime < _fadeDuration)
        {
            elapsedTime += MainTimer.RawDelta;
            float a = _image.color.a;
            a = Mathf.Lerp(1f, 0f, elapsedTime / _fadeDuration);
            _image.color = new Color(1f, 1f, 1f, a);
            cut.Text.color = new Color(1f, 1f, 1f, a);
            yield return null;
        }

        if (index < _cuts.Length)
            StartCoroutine(StartCut(index));
        else
            GameManager.Instance.LoadScene(_group);
    }
}