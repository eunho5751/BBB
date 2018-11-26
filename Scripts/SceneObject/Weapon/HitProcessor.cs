using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public struct HitProcessorInfo
{
    public HitProcessorInfo(GameObject attacker, IReadOnlyList<int> hitBoxIndices, int damage, int? reactionId)
    {
        Attacker = attacker;
        HitBoxIndices = hitBoxIndices;
        Damage = damage;
        ReactionId = reactionId;
    }

    public GameObject Attacker { get; private set; }
    public IReadOnlyList<int> HitBoxIndices { get; private set; }
    public int Damage { get; private set; }
    public int? ReactionId { get; private set; }
}

[DisallowMultipleComponent]
public class HitProcessor : MonoBehaviour
{
    [SerializeField]
    private TextureEffectMap _hitEffectMap;
    [SerializeField, DisableContextMenu]
    private CastBox[] _castBoxes = new CastBox[0];
    [SerializeField, DisableContextMenu]
    private OverlapBox[] _overlapBoxes = new OverlapBox[0];

    private HitProcessorInfo _processorInfo;
    private List<GameObject> _hitExcluders = new List<GameObject>();
    private event Action<GameObject> _onHit;

    private void OnEnable()
    {
        foreach (var castBox in _castBoxes)
        {
            castBox.OnHit += InvokeHit;
            castBox.enabled = false;
        }

        foreach (var overlapBox in _overlapBoxes)
        {
            overlapBox.OnHit += InvokeHit;
        }
    }

    private void OnDisable()
    {
        foreach (var castBox in _castBoxes)
        {
            castBox.OnHit -= InvokeHit;
            castBox.enabled = false;
        }

        foreach (var overlapBox in _overlapBoxes)
        {
            overlapBox.OnHit -= InvokeHit;
        }
    }

    private void InvokeHit(GameObject gameObject, Vector3 point, Vector3 normal)
    {
        var sceneObj = gameObject.GetComponent<SceneObject>() ?? gameObject.GetComponent<BodyPart>().Root;
        if (sceneObj != null && _processorInfo.Attacker.layer != sceneObj.gameObject.layer && !_hitExcluders.Contains(sceneObj.gameObject))
        {
            var damageable = sceneObj as IDamageable;
            damageable?.TakeDamage(_processorInfo.Attacker, _processorInfo.Damage, _processorInfo.ReactionId);

            if (_hitEffectMap != null)
                TextureEffectEmitter.Emit(_hitEffectMap, sceneObj.TextureType, point, Quaternion.LookRotation(normal));

            _onHit?.Invoke(sceneObj.gameObject);
            _hitExcluders.Add(sceneObj.gameObject);
        }
    }

    public void EnableCast(HitProcessorInfo processorInfo)
    {
        _processorInfo = processorInfo;

        if (processorInfo.HitBoxIndices != null)
        {
            foreach (var index in processorInfo.HitBoxIndices)
            {
                if (index < _castBoxes.Length)
                    _castBoxes[index].enabled = true;
            }
        }
        else
        {
            foreach (var castBox in _castBoxes)
                castBox.enabled = true;
        }
    }

    public void DisableCast()
    {
        foreach (var castBox in _castBoxes)
            castBox.enabled = false;

        _hitExcluders.Clear();
    }

    public void CheckOverlap(HitProcessorInfo processorInfo)
    {
        _processorInfo = processorInfo;

        if (processorInfo.HitBoxIndices != null)
        {
            foreach (var index in processorInfo.HitBoxIndices)
            {
                if (index < _overlapBoxes.Length)
                    _overlapBoxes[index].CheckOverlap();
            }
        }
        else
        {
            foreach (var overlapBox in _overlapBoxes)
                overlapBox.CheckOverlap();
        }

        _hitExcluders.Clear();
    }

    public event Action<GameObject> OnHit
    {
        add { _onHit += value; }
        remove { _onHit -= value; }
    }
}