using UnityEngine;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    [SerializeField]
    private RawImage _image;
    [SerializeField]
    private Text _text;
    [SerializeField]
    private Texture[] _backgrounds;
    [SerializeField]
    private string[] _texts;

    private void Awake()
    {
        _image.texture = _backgrounds[Random.Range(0, _backgrounds.Length)];
        _text.text = _texts[Random.Range(0, _texts.Length)];
    }
}
