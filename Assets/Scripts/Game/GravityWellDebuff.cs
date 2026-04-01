using UnityEngine;

/// <summary>
/// Attached to the player. Detects when the player is inside an enemy territory
/// and applies a -30% speed modifier (Gravity Well debuff from the GDD).
/// TerritorySystem calls ApplyGravityWell() / RemoveGravityWell() each frame.
/// </summary>
[RequireComponent(typeof(BlackHolePhysics))]
public class GravityWellDebuff : MonoBehaviour
{
    public const float SpeedModifier = -0.30f; // -30 %

    private BlackHolePhysics _physics;
    private bool _wellActive;

    private void Awake()
    {
        _physics = GetComponent<BlackHolePhysics>();
    }

    /// <summary>Called by TerritorySystem every fixed frame when inside enemy territory.</summary>
    public void ApplyGravityWell()
    {
        if (_wellActive) return;
        _wellActive = true;
        _physics.AddSpeedModifier(SpeedModifier);
    }

    /// <summary>Called by TerritorySystem when the player leaves enemy territory.</summary>
    public void RemoveGravityWell()
    {
        if (!_wellActive) return;
        _wellActive = false;
        _physics.AddSpeedModifier(-SpeedModifier); // remove by reversing
    }

    /// <summary>Returns true if the Gravity Well debuff is currently active.</summary>
    public bool IsWellActive => _wellActive;
}
