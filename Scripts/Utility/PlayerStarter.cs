using UnityEngine;

public class PlayerStarter : MonoBehaviour
{
    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        player.transform.SetPositionAndRotation(transform.position, transform.rotation);
        player.SendMessage("ResolveReferences");
    }

    private void OnDrawGizmos()
    {
        Color color = Gizmos.color;

        float height = 2f;
        Vector3 forward = transform.forward;
        Vector3 center = transform.position + Vector3.up * height / 2f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(center, new Vector3(1f, height, 1f));
        Gizmos.DrawRay(center + forward * 0.5f, forward);
        Gizmos.DrawRay(center + forward * 1.5f, (-forward + -transform.right) * 0.5f);
        Gizmos.DrawRay(center + forward * 1.5f, (-forward + transform.right) * 0.5f);

        Gizmos.color = color;
    }
}
