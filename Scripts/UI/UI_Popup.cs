using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Popup : MonoBehaviour
{
    [SerializeField]
    private Button _firstSelected;
    [SerializeField]
    private SceneGroup _mainMenu;
    

    private void OnEnable()
    {
        _firstSelected.Select();
        _firstSelected.OnSelect(null);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Continue();
    }

    public void Continue()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);
    }

    public void Replay()
    {
        Time.timeScale = 1f;
        if (GameManager.CurrentSceneGroup != null)
            GameManager.Instance.LoadScene(GameManager.CurrentSceneGroup);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        UIManager._newGame = false;
        if (GameManager.CurrentSceneGroup != null)
            PlayerPrefs.SetInt("SceneGroup", GameManager.CurrentSceneGroup._id);
        GameManager.Instance.LoadScene(_mainMenu);
    }
}