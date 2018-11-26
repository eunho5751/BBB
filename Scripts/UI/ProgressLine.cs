using UnityEngine;

public enum ProgressLineType
{
    Min,
    Max
}

public class ProgressLine : MonoBehaviour
{
    [SerializeField]
    private ProgressLineType _type;
    
    private void Start()
    {
        UIManager.Instance.SetProgressLine(_type, transform.position.x);
    }

    private void OnDrawGizmos()
    {
        Color color = Gizmos.color;
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 10f);
        Gizmos.color = color;
    }
}
