using UnityEngine;
using Sirenix.OdinInspector;

public class WeaponSocket : MonoBehaviour
{
    [SerializeField, Required]
    private string _socketName;

    public string SocketName => _socketName;
}