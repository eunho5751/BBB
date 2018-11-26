using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BodyPart : MonoBehaviour
{
    [SerializeField]
    private SceneObject _root;

    public SceneObject Root => _root;
    public Collider Collider => GetComponent<Collider>();
}
