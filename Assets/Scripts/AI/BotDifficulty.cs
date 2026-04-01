using UnityEngine;

/// <summary>
/// ScriptableObject that describes the difficulty profile of a single bot.
/// Create instances via Assets > Create > BlackHole > BotDifficulty
/// </summary>
[CreateAssetMenu(fileName = "BotDifficulty", menuName = "BlackHole/BotDifficulty")]
public class BotDifficulty : ScriptableObject
{
    [Header("Identity")]
    public string difficultyName = "Normal";

    [Header("Behaviour Weights (0 = never, 1 = always prefers)")]
    [Range(0f, 1f)] public float aggressionFactor  = 0.5f;  // tendency to Hunt
    [Range(0f, 1f)] public float territorialFactor = 0.5f;  // tendency to Capture
    [Range(0f, 1f)] public float cowardFactor      = 0.3f;  // tendency to Flee early

    [Header("Reaction")]
    [Range(0.1f, 2f)] public float reactionDelay   = 0.5f;  // seconds before state change
    [Range(0.5f, 2f)] public float speedMultiplier = 1f;    // movement speed multiplier

    [Header("Hunt / Flee Thresholds")]
    [Tooltip("Bot hunts if target mass ≤ own mass × this value")]
    public float huntMassThreshold   = 0.9f;
    [Tooltip("Bot flees if threat mass ≥ own mass × this value")]
    public float fleeMassThreshold   = 1.1f;
}
