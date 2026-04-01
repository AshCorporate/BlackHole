using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// In-game leaderboard widget — displays top players by score (mass + territory).
/// Can be embedded inside GameHUD or shown as a standalone panel.
/// </summary>
public class Leaderboard : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private int             displayCount = 5;

    // ── Private ────────────────────────────────────────────────────────────────
    private ScoreManager _scoreManager;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Start()
    {
        _scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Update()
    {
        Refresh();
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void Refresh()
    {
        if (contentText == null || _scoreManager == null) return;

        var entries = _scoreManager.GetTopEntries(displayCount);
        var sb = new System.Text.StringBuilder();

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            // Highlight the human player
            bool isHuman = e.IsHuman;
            string prefix = isHuman ? "<color=yellow>" : "";
            string suffix = isHuman ? "</color>" : "";
            sb.AppendLine($"{prefix}{i + 1}. {e.Name}  {e.Mass:F0}{suffix}");
        }

        contentText.text = sb.ToString();
    }
}
