using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Counts down the match duration and fires events on completion.
/// </summary>
public class MatchTimer : MonoBehaviour
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private GameConfig config;

    // ── Events ─────────────────────────────────────────────────────────────────
    public UnityEvent OnTimeUp;

    // ── Public State ───────────────────────────────────────────────────────────
    public float TimeRemaining { get; private set; }
    public bool  IsRunning     { get; private set; }

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        if (config == null)
            config = Resources.Load<GameConfig>("GameConfig");

        TimeRemaining = config != null ? config.matchDuration : 600f;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    public void StartTimer()  => IsRunning = true;
    public void PauseTimer()  => IsRunning = false;
    public void ResumeTimer() => IsRunning = true;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Update()
    {
        if (!IsRunning) return;

        TimeRemaining -= Time.deltaTime;
        if (TimeRemaining <= 0f)
        {
            TimeRemaining = 0f;
            IsRunning = false;
            OnTimeUp?.Invoke();
        }
    }
}
