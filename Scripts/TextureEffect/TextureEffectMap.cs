using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[Serializable]
public struct TextureEffectInfo
{
    [SerializeField, AssetsOnly]
    private GameObject _visualEffect;
    [SerializeField, FMODUnity.EventRef]
    private string _soundEffect;

    public GameObject VisualEffect => _visualEffect;
    public string SoundEffect => _soundEffect;
}

[CreateAssetMenu]
public class TextureEffectMap : SerializedScriptableObject
{
    [SerializeField, DisableContextMenu]
    private Dictionary<TextureType, TextureEffectInfo> _effectMap = new Dictionary<TextureType, TextureEffectInfo>();

    public bool TryGetEffectInfo(TextureType key, out TextureEffectInfo value)
    {
        return _effectMap.TryGetValue(key, out value);
    }
}
