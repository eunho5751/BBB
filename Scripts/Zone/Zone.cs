using UnityEngine;
using UnityEngine.Playables;
using Sirenix.OdinInspector;

[RequireComponent(typeof(PlayableDirector))]
public class Zone : SerializedMonoBehaviour
{
    [SerializeField]
    private bool _activeOnAwake = true;
    [SerializeField, Required]
    private Restrictor _restrictor;
    [SerializeField, DisableContextMenu]
    private CharacterControllerBase[] _enemies;
    [SerializeField]
    private IGameEvent _enterEvent;
    [SerializeField]
    private IGameEvent _clearEvent;

    private GameObject _player;
    private PlayableDirector _director;
    private int _deadCount;

    private void Awake()
    {
        _director = GetComponent<PlayableDirector>();
        _director.stopped += OnDirectingStopped;

        _restrictor.OnRestrictorPassed += OnRestrictorPassed;
        foreach (var enemy in _enemies)
            enemy.OnCharacterDead += OnCharacterDead;

        SetZoneActive(_activeOnAwake);
    }

    private void Start()
    {
        _player = GameObject.FindWithTag("Player");
        CheckClear();
    }

    private void OnDestroy()
    {
        _director.stopped -= OnDirectingStopped;

        _restrictor.OnRestrictorPassed -= OnRestrictorPassed;
        foreach (var enemy in _enemies)
            enemy.OnCharacterDead -= OnCharacterDead;
    }
    
    private void OnDirectingStopped(PlayableDirector director)
    {
        _player.SendMessage("Activate");
        foreach (var enemy in _enemies)
            enemy.SendMessage("Activate");
    }

    private void OnCharacterDead()
    {
        _deadCount++;

        CheckClear();
    }

    private void OnRestrictorPassed()
    {
        _player.SendMessage("Deactivate");
        _director.Play();

        _enterEvent?.Trigger();
        SetZoneActive(true);
        UIManager.Instance.SetGoActive(false);

        CheckClear();
    }
    
    private void CheckClear()
    {
        if (_deadCount == _enemies.Length)
        {
            _player.SendMessage("RecoverHealth");
            IsCleared = true;
            UIManager.Instance.SetGoActive(true);
            _clearEvent?.Trigger();
        }
    }

    public void SetZoneActive(bool active)
    {
        _restrictor.SetRestrictorActive(active);
    }

    public bool IsCleared { get; private set; }
}