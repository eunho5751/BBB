using UnityEngine;

public static class TextureEffectEmitter
{
    public static void Emit(TextureEffectMap effectMap, TextureType key, Vector3 position, Quaternion rotation)
    {
        TextureEffectInfo info;
        if (effectMap.TryGetEffectInfo(key, out info))
        {
            Emit(info, position, rotation);
        }
    }

    public static void Emit(TextureEffectInfo effectInfo, Vector3 position, Quaternion rotation)
    {
        if (effectInfo.VisualEffect != null)
            PoolManager.Instance.Spawn(effectInfo.VisualEffect, position, rotation);
        if (!string.IsNullOrEmpty(effectInfo.SoundEffect))
            FMODUnity.RuntimeManager.PlayOneShot(effectInfo.SoundEffect, position);
    }
}