using UnityEngine;

[ExecuteInEditMode]
public class AxisTracker : MonoBehaviour
{
    private Transform _target;

    private void Start()
    {
        _target = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        Vector3 pos = _target.position;
        transform.position = new Vector3(pos.x, pos.y, transform.position.z);
    }

    private void OnDrawGizmos()
    {
        Color color = Gizmos.color;
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _target.position);
        Gizmos.DrawWireSphere(transform.position, 0.15f);
        Gizmos.color = color;
    }
}
