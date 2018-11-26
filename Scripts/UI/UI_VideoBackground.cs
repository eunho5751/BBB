using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class UI_VideoBackground : MonoBehaviour
{
    [SerializeField]
    private RawImage _image;
    [SerializeField]
    private VideoPlayer _video;

    private void Start()
    {
        StartCoroutine(Play());
    }

    private IEnumerator Play()
    {
        _video.Prepare();
        while (!_video.isPrepared)        
            yield return null;

        _image.color = Color.white;
        _image.texture = _video.texture;
        _video.Play();
    }
}
