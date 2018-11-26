using UnityEngine;

public class UI_ControlGuide : MonoBehaviour
{
    [SerializeField]
    private GameObject _mainMenu;
    [SerializeField]
    private GameObject _rain;

    private void OnEnable()
    {
        _rain.SetActive(false);
    }
    
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            gameObject.SetActive(false);
            _mainMenu.SetActive(true);
            _rain.SetActive(true);
        }
    }
}