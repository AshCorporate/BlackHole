using UnityEngine;

/// <summary>
/// ScriptableObject that holds all game configuration values.
/// Create via Assets > Create > BlackHole > GameConfig
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "BlackHole/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Match Settings")]
    [Tooltip("Total match duration in seconds (10 minutes = 600)")]
    public float matchDuration = 600f;

    [Tooltip("Number of bots in the match")]
    [Range(1, 7)]
    public int botCount = 6;

    [Header("Map Settings")]
    [Tooltip("Radius of the circular city map in world units")]
    public float mapRadius = 50f;

    [Tooltip("Minimum distance between spawned objects")]
    public float minObjectSpacing = 1.5f;

    [Header("Player / Black Hole")]
    [Tooltip("Starting mass of every black hole")]
    public float startMass = 5f;

    [Tooltip("Minimum scale of the black hole sprite")]
    public float minScale = 0.4f;

    [Tooltip("Scale multiplier relative to mass (scale = minScale + mass * scaleFactor)")]
    public float scaleFactor = 0.02f;

    [Tooltip("Base movement speed")]
    public float baseSpeed = 6f;

    [Tooltip("Speed penalty per unit of mass (bigger = slower)")]
    public float speedMassPenalty = 0.03f;

    [Tooltip("Minimum movement speed regardless of mass")]
    public float minSpeed = 2f;

    [Tooltip("Absorption pull force multiplier")]
    public float absorptionForce = 8f;

    [Tooltip("Range at which objects start being attracted (relative to hole radius)")]
    public float absorptionRangeMultiplier = 3f;

    [Header("Territory")]
    [Tooltip("Width of the trail line drawn behind the player")]
    public float trailWidth = 0.2f;

    [Tooltip("Seconds before a killed player respawns")]
    public float respawnDelay = 3f;

    [Tooltip("Mass fraction retained after death (0-1)")]
    [Range(0f, 1f)]
    public float respawnMassFraction = 0.3f;

    [Header("Power-ups / Buffs")]
    [Tooltip("How many buffs exist on the map at the same time")]
    public int maxActiveBuffs = 5;

    [Tooltip("Seconds between buff spawns")]
    public float buffSpawnInterval = 20f;

    [Tooltip("Speed Boost multiplier")]
    public float speedBoostMultiplier = 2f;

    [Tooltip("Speed Boost duration in seconds")]
    public float speedBoostDuration = 5f;

    [Tooltip("Magnet buff duration in seconds")]
    public float magnetDuration = 6f;

    [Tooltip("Magnet pull radius in world units")]
    public float magnetRadius = 8f;

    [Tooltip("Double Mass buff duration in seconds")]
    public float doubleMassDuration = 8f;

    [Tooltip("Shield buff duration in seconds")]
    public float shieldDuration = 5f;

    [Tooltip("Gravity Pulse buff radius")]
    public float gravityPulseRadius = 10f;

    [Tooltip("Gravity Pulse force")]
    public float gravityPulseForce = 20f;

    [Header("AI Bot Settings")]
    [Tooltip("Detection radius for bots to notice players / objects")]
    public float botDetectionRadius = 15f;

    [Tooltip("How often (seconds) bots re-evaluate their state")]
    public float botStateUpdateInterval = 0.5f;

    [Tooltip("Mass ratio at which a bot starts hunting (targetMass / ownMass)")]
    public float huntMassRatio = 0.8f;

    [Tooltip("Mass ratio at which a bot starts fleeing (ownMass / threatMass)")]
    public float fleeMassRatio = 1.2f;
}
