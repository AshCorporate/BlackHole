using UnityEngine;
using System.Collections;

/// <summary>
/// Handles the stun state. When Stun() is called, the player's movement is blocked
/// for StunDuration seconds (default 1 s per GDD §7).
/// TerritoryTrail calls Stun() when the tail timer expires.
/// </summary>
[RequireComponent(typeof(BlackHolePhysics))]
public class StunSystem : MonoBehaviour
{
    [Tooltip("Duration of the stun in seconds (GDD §7: 1 s)")]
    public float StunDuration = 1f;

    /// <summary>Returns true while the player is currently stunned.</summary>
    public bool IsStunned { get; private set; }

    private BlackHolePhysics _physics;

    private void Awake()
    {
        _physics = GetComponent<BlackHolePhysics>();
    }

    /// <summary>Stuns the player for StunDuration seconds.</summary>
    public void Stun()
    {
        if (IsStunned) return;
        StartCoroutine(StunCoroutine());
    }

    private IEnumerator StunCoroutine()
    {
        IsStunned = true;
        // Force zero input while stunned
        _physics.IsStunned = true;
        yield return new WaitForSeconds(StunDuration);
        _physics.IsStunned = false;
        IsStunned = false;
    }
}
