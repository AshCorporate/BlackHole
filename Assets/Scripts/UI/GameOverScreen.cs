using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Game Over screen — displayed when the match ends.
/// Shows the final leaderboard and win/loss result.
///
/// Scene setup (inside Canvas):
///   GameOverPanel
///     ├── ResultText (TextMeshProUGUI – "YOU WIN!" / "GAME OVER")
///     ├── RankingText (TextMeshProUGUI – full leaderboard)
///     ├── PlayAgainButton
///     └── MainMenuButton
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameObject      gameOverPanel;
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI rankingText;
    [SerializeField] private Button          playAgainButton;
    [SerializeField] private Button          mainMenuButton;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        // Panel visibility is managed here so it hides even in hand-crafted scenes.
        gameOverPanel?.SetActive(false);
    }

    private void Start()
    {
        // Wire listeners in Start so that fields injected via reflection (by
        // AutoSceneSetup) are guaranteed to be set before binding.
        playAgainButton?.onClick.AddListener(PlayAgain);
        mainMenuButton?.onClick.AddListener(GoToMainMenu);
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Shows the game over screen with the final results.
    /// </summary>
    /// <param name="playerWon">True if the human player won.</param>
    /// <param name="entries">Sorted list of all score entries.</param>
    public void Show(bool playerWon, List<ScoreManager.ScoreEntry> entries)
    {
        gameOverPanel?.SetActive(true);
        Time.timeScale = 0f;

        if (resultText != null)
        {
            resultText.text = playerWon ? "🎉 YOU WIN!" : "💀 GAME OVER";
            resultText.color = playerWon ? Color.yellow : Color.red;
        }

        if (rankingText != null)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>FINAL RANKINGS</b>");
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                string marker = (i == 0) ? "🏆 " : $"{i + 1}. ";
                sb.AppendLine($"{marker}{e.Name}  Mass: {e.Mass:F0}  Area: {e.Area:F0} m²");
            }
            rankingText.text = sb.ToString();
        }
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void PlayAgain()
    {
        Time.timeScale = 1f;
        if (GameBootstrapper.Instance != null)
            GameBootstrapper.Instance.RestartGame();
        else
            SceneManager.LoadScene("Game");
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        if (GameBootstrapper.Instance != null)
            GameBootstrapper.Instance.GoToMainMenu();
        else
            SceneManager.LoadScene("MainMenu");
    }
}
