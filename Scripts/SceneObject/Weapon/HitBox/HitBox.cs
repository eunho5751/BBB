using System;
using UnityEngine;

public abstract class HitBox : MonoBehaviour
{
    [SerializeField]
    private LayerMask _hitLayer;
    [SerializeField]
    private Vector3 _halfExtents;

    private event Action<GameObject, Vector3, Vector3> _onHit;

    protected void InvokeHit(GameObject gameObject, Vector3 point, Vector3 normal)
    {
        _onHit?.Invoke(gameObject, point, normal);
    }

    private void OnDrawGizmosSelected()
    {
        Matrix4x4 matrix = Gizmos.matrix;
        Color color = Gizmos.color;

        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, _halfExtents);

        Gizmos.matrix = matrix;
        Gizmos.color = color;
    }

    public event Action<GameObject, Vector3, Vector3> OnHit
    {
        add { _onHit += value; }
        remove { _onHit -= value; }
    }
    
    protected LayerMask HitLayer => _hitLayer;
    protected Vector3 HalfExtents => _halfExtents;
}