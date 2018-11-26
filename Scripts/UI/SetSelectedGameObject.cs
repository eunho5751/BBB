using UnityEngine;
using UnityEngine.EventSystems;

public class SetSelectedGameObject : MonoBehaviour
{
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }
}