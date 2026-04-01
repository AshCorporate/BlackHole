using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Tracks scores (mass + captured territory area) for all players.
/// Used by the HUD leaderboard and the Game Over screen.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static ScoreManager Instance { get; private set; }

    // ── Score Entry ────────────────────────────────────────────────────────────
    public class ScoreEntry
    {
        public string  Name;
        public float   Mass;
        public float   Area;
        public bool    IsHuman;
        public bool    IsAlive;

        /// <summary>Combined score = mass + area (tunable weighting).</summary>
        public float Score => Mass + Area * 0.5f;
    }

    // ── Private ────────────────────────────────────────────────────────────────
    private readonly List<ScoreEntry> _entries = new List<ScoreEntry>();
    private TerritorySystem           _territory;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _territory = FindFirstObjectByType<TerritorySystem>();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Registers a player with the score manager.</summary>
    public void RegisterPlayer(string name, bool isHuman)
    {
        _entries.Add(new ScoreEntry { Name = name, IsHuman = isHuman, IsAlive = true });
    }

    /// <summary>Updates the score entry for a named player.</summary>
    public void UpdateScore(string playerName, float mass, float area, bool alive)
    {
        var entry = _entries.Find(e => e.Name == playerName);
        if (entry == null)
        {
            entry = new ScoreEntry { Name = playerName };
            _entries.Add(entry);
        }
        entry.Mass   = mass;
        entry.Area   = area;
        entry.IsAlive = alive;
    }

    /// <summary>Returns the top N entries sorted by combined score.</summary>
    public List<ScoreEntry> GetTopEntries(int count)
    {
        return _entries
            .OrderByDescending(e => e.Score)
            .Take(count)
            .ToList();
    }

    /// <summary>Returns all entries sorted by score (for Game Over screen).</summary>
    public List<ScoreEntry> GetAllEntries()
    {
        return _entries.OrderByDescending(e => e.Score).ToList();
    }

    /// <summary>Returns the human player's entry.</summary>
    public ScoreEntry GetHumanEntry()
    {
        return _entries.Find(e => e.IsHuman);
    }
}
