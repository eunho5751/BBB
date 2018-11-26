using UnityEngine;

public class CastBox : HitBox
{
    private Vector3 _prevPosition;
    private Quaternion _prevRotation;
    
    private void OnEnable()
    {
        _prevPosition = transform.position;
        _prevRotation = transform.rotation;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 diff = currentPosition - _prevPosition;

        //int count = Physics.BoxCastNonAlloc(_prevPosition, HalfExtents, diff.normalized, _results, _prevRotation, diff.magnitude, HitLayer);
        //for (int i = 0; i < count; i++)
        //    InvokeHit(_results[i].transform.gameObject, _results[i].point, _results[i].normal);
        RaycastHit hitInfo;
        if (Physics.BoxCast(_prevPosition, HalfExtents, diff.normalized, out hitInfo, _prevRotation, diff.magnitude, HitLayer))
        {
            InvokeHit(hitInfo.transform.gameObject, hitInfo.point, hitInfo.normal);
        }

        _prevPosition = currentPosition;
        _prevRotation = transform.rotation;
    }
}