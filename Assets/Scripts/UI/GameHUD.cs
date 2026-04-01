using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// In-game HUD: shows the countdown timer, top-3 leaderboard, and a minimap dot.
///
/// Scene setup (inside Canvas – Screen Space Overlay):
///   HUD
///     ├── TimerText (TextMeshProUGUI – top-centre)
///     ├── LeaderboardPanel
///     │     └── LeaderboardText (TextMeshProUGUI)
///     └── MinimapPanel (RawImage)
/// </summary>
public class GameHUD : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI leaderboardText;
    [SerializeField] private RawImage        minimapImage;   // optional minimap render texture

    // ── Private ────────────────────────────────────────────────────────────────
    private MatchTimer   _matchTimer;
    private ScoreManager _scoreManager;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Start()
    {
        _matchTimer   = FindObjectOfType<MatchTimer>();
        _scoreManager = FindObjectOfType<ScoreManager>();
    }

    private void Update()
    {
        UpdateTimer();
        UpdateLeaderboard();
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void UpdateTimer()
    {
        if (timerText == null || _matchTimer == null) return;
        timerText.text = MathHelpers.FormatTime(_matchTimer.TimeRemaining);

        // Flash red in the last 30 seconds
        timerText.color = _matchTimer.TimeRemaining < 30f ? Color.red : Color.white;
    }

    private void UpdateLeaderboard()
    {
        if (leaderboardText == null || _scoreManager == null) return;

        var entries = _scoreManager.GetTopEntries(5);
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>TOP PLAYERS</b>");
        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            sb.AppendLine($"{i + 1}. {e.Name}  {e.Mass:F0} kg  {e.Area:F0} m²");
        }
        leaderboardText.text = sb.ToString();
    }
}
