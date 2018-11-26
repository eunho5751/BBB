using UnityEngine;

public class SceneObject : MonoBehaviour
{
    [SerializeField]
    private TextureType _textureType;

    public TextureType TextureType => _textureType;
}
