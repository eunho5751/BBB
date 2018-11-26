using UnityEngine;

public class OverlapBox : HitBox
{
    [SerializeField]
    private int _maxHit;
    //[SerializeField]
    //private Transform _hitPoint;

    private Collider[] _results;

    private void Awake()
    {
        _results = new Collider[_maxHit];
    }

    public void CheckOverlap()
    {
        Transform tr = /*_hitPoint ?? */transform;
        int count = Physics.OverlapBoxNonAlloc(transform.position, HalfExtents, _results, transform.rotation, HitLayer);
        for (int i = 0; i < count; i++)
            InvokeHit(_results[i].gameObject, tr.position, -tr.forward);
    }
}