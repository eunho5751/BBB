using System;
using System.Collections;
using UnityEngine;
using Sirenix.OdinInspector;

[DefaultExecutionOrder(-32000)]
public abstract class TimerBase : MonoBehaviour, ITimer
{
    [SerializeField, DisableInPlayMode]
    private string _parentKey;
    [SerializeField, OnValueChanged("Compute")]
    private float _localScale = 1F;
    [ShowInInspector, HideInEditorMode, DisableInPlayMode]
    private float _totalScale;

    private GlobalTimer _parentTimer;
    private float _fixedDelta, _delta;
    private bool _deltaUpdated;
    private event Action<float> _onTimeScaleChanged;

    private void OnEnable()
    {
        RegisterInParent();
    }

    private void OnDisable()
    {
        UnregisterFromParent();
    }

    protected virtual void Start()
    {
        if (_parentTimer == null)
        {
            _parentTimer = GetParentTimer();
            if (_parentTimer._parentTimer == null)
                _parentTimer.Start();
            RegisterInParent();
        }
    }

    private void FixedUpdate()
    {
        UpdateFixedDelta();
    }

    private void Update()
    {
        UpdateDelta();
    }

    private GlobalTimer GetParentTimer()
    {
        GlobalTimer timer = null;
        if (string.IsNullOrEmpty(_parentKey))
        {
            timer = MainTimer.Instance;
        }
        else
        {
            timer = MainTimer.Instance.GetTimer(_parentKey);
            if (timer == null)
                timer = MainTimer.Instance;
        }

        return timer;
    }

    private void RegisterInParent()
    {
        if (_parentTimer != null)
        {
            _parentTimer.RegisterChild(this);
            Compute();
        }
    }

    private void UnregisterFromParent()
    {
        if (_parentTimer != null)
        {
            _parentTimer.UnregisterChild(this);
            Compute();
        }
    }

    public void UpdateFixedDelta()
    {
        _fixedDelta = Time.fixedDeltaTime * _totalScale;
    }

    public void UpdateDelta()
    {
        _delta = Time.deltaTime * _totalScale;
    }

    public float Compute()
    {
        return TotalScale = ComputeTotal();
    }

    protected virtual float ComputeTotal()
    {
        return _localScale * (_parentTimer != null ? _parentTimer.TotalScale : 1F);
    }

    public float LocalScale
    {
        get
        {
            return _localScale;
        }

        set
        {
            _localScale = value;
            Compute();
        }
    }

    public virtual float TotalScale
    {
        get
        {
            return _totalScale;
        }

        protected set
        {
            _totalScale = value;
            _onTimeScaleChanged?.Invoke(TotalScale);
            _deltaUpdated = false;
        }
    }

    public float FixedDelta
    {
        get
        {
            if (!_deltaUpdated)
            {
                UpdateFixedDelta();
                _deltaUpdated = true;
            }

            return _fixedDelta;
        }
    }
    
    public float Delta
    {
        get
        {
            if (!_deltaUpdated)
            {
                UpdateDelta();
                _deltaUpdated = true;
            }

            return _delta;
        }
    }

    public event Action<float> OnTimeScaleChanged
    {
        add { _onTimeScaleChanged += value; }
        remove { _onTimeScaleChanged -= value; }
    }

    public GlobalTimer ParentTimer => _parentTimer;
}