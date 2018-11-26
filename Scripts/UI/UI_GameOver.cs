using UnityEngine;
using UnityEngine.UI;

public class UI_GameOver : MonoBehaviour
{
    [SerializeField]
    private SceneGroup _mainMenu;

    private void GoToMainMenu()
    {
        UIManager._newGame = false;
        GameManager.Instance.LoadScene(_mainMenu);
    }
}