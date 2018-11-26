using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class UI_Progress : MonoBehaviour
{
    [SerializeField]
    private Image _bar;
    [SerializeField]
    private Text _text;

    private Transform _player;
    private StringBuilder _sb = new StringBuilder();
    private float _minX, _maxX;
    private float _last;

    private void Update()
    {
        if (_player == null)
            _player = GameObject.FindWithTag("Player").transform;

        _sb.Clear();

        float progress = Mathf.Clamp01((_player.position.x - _minX )/ (_maxX - _minX));
        _bar.fillAmount = Mathf.Max(_last, progress);
        _sb.Append(Mathf.FloorToInt(progress * 100f));
        _sb.Append('%');
        _text.text = _sb.ToString();
        _last = progress;
    }

    public void SetProgressLine(ProgressLineType type, float value)
    {
        if (type == ProgressLineType.Min)
            _minX = value;
        else if (type == ProgressLineType.Max)
            _maxX = value;
    }
}
