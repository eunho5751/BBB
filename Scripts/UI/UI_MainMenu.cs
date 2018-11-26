using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_MainMenu : MonoBehaviour
{
    [SerializeField]
    private GameObject _firstSelected;
    [SerializeField]
    private SceneGroup _newGameSceneGroup;
    [SerializeField]
    private GameObject _controlGuide;
    [SerializeField]
    private Button _continue;

    private GameObject _lastSelected;
    
    private void OnEnable()
    {
        EventSystem.current.SetSelectedGameObject(_firstSelected);
    }

    private void Start()
    {
        int id = PlayerPrefs.GetInt("SceneGroup", -1);
        _continue.interactable = id != -1;
        Debug.Log(id);
    }

    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(_lastSelected);

        _lastSelected = EventSystem.current.currentSelectedGameObject;
    }

    public void NewGame()
    {
        GameManager.Instance.LoadScene(_newGameSceneGroup);
    }

    public void Continue()
    {
        GameManager.Instance.LoadScene(PlayerPrefs.GetInt("SceneGroup"));
    }

    public void ControlGuide()
    {
        gameObject.SetActive(false);    
        _controlGuide.SetActive(true);
    }

    public void Exit()
    {
        GameManager.Instance.Exit();
    }
}
