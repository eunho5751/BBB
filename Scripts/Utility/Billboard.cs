using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform _camTr;

    private void Start()
    {
        _camTr = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _camTr.rotation * Vector3.forward, _camTr.rotation * Vector3.up);
        //transform.rotation = Quaternion.Euler(0f, _camTr.eulerAngles.y, 0f);
    }
}