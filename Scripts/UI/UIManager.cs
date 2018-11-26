using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField]
    private GameObject _controlGuide;
    [SerializeField]
    private UI_Progress _progress;
    [SerializeField]
    private GameObject _go;
    [SerializeField]
    private GameObject _combo;
    [SerializeField]
    private GameObject _gameOver;

    public static bool _newGame;

    private void Start()
    {
        if (!_newGame)
        {
            _controlGuide.SetActive(true);
            _newGame = true;
        }
    }

    private void Update()
    {
        if (_controlGuide.activeSelf && Input.anyKeyDown)
            _controlGuide.SetActive(false);
    }

    public void SetProgressLine(ProgressLineType type, float value)
    {
        _progress.SetProgressLine(type, value);
    }

    public void SetGoActive(bool active)
    {
        _go.SetActive(active);
    }

    public void SetComboActive(bool active)
    {
        _combo.SetActive(active);
    }

    public void SetComboNumber(int comboNumber)
    {
        _combo.GetComponent<UI_Combo>().SetCombo(comboNumber);
    }

    public void GameOver()
    {
        _gameOver.SetActive(true);
    }
}