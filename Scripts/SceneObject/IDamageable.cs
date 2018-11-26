using UnityEngine;

public interface IDamageable
{
    void TakeDamage(GameObject attacker, int damage, int? reactionId = null);
}
