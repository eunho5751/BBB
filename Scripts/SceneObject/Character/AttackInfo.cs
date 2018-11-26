using System.Collections.Generic;
using UnityEngine;

public class AttackInfo : IEventParameter
{
    [SerializeField]
    private int[] _hitBoxIndices;
    [SerializeField]
    private float _damageMultiplier;
    [SerializeField]
    private int _reactionId;

    public IReadOnlyList<int> HitBoxIndices => _hitBoxIndices;
    public float DamageMultiplier => _damageMultiplier;
    public int ReactionId => _reactionId;
}