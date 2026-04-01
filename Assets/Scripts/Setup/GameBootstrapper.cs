using UnityEngine;

/// <summary>
/// Application entry point.
/// Runs before the scene loads (<see cref="RuntimeInitializeLoadType.BeforeSceneLoad"/>)
/// and creates a persistent DontDestroyOnLoad manager that survives scene transitions.
///
/// Flow:
///   1. <see cref="GameBootstrapper"/> (BeforeSceneLoad) — registers the persistent manager.
///   2. <see cref="AutoSceneSetup"/>   (AfterSceneLoad)  — builds the game or menu scene.
///
/// To start from the main menu set <see cref="ShowMenuOnBoot"/> = true before
/// pressing Play (or toggle it from another script).
/// By default an empty Unity scene opens straight into the game.
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────────
    public static GameBootstrapper Instance { get; private set; }

    // ── Configuration (set before Play or from other scripts) ─────────────────
    /// <summary>
    /// When <c>true</c> the first boot will show the main menu instead of jumping
    /// straight into the game.  Defaults to <c>false</c> so an empty scene
    /// immediately starts the game.
    /// </summary>
    public static bool ShowMenuOnBoot = false;

    // ── Entry point ───────────────────────────────────────────────────────────

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnBeforeSceneLoad()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("[GameBootstrapper]");
        Object.DontDestroyOnLoad(go);
        Instance = go.AddComponent<GameBootstrapper>();

        Debug.Log("[GameBootstrapper] Persistent manager created.");
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Guard against duplicate instances created by scene loading
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Tears down the current game scene and rebuilds it from scratch.
    /// Useful for "Play Again" without an actual scene file.
    /// </summary>
    public void RestartGame()
    {
        ShowMenuOnBoot = false;
        StartCoroutine(TransitionCoroutine(() => AutoSceneSetup.BuildGameScene()));
    }

    /// <summary>
    /// Returns to the main menu without loading a scene file.
    /// </summary>
    public void GoToMainMenu()
    {
        ShowMenuOnBoot = true;
        StartCoroutine(TransitionCoroutine(() => MainMenuSetup.Build()));
    }

    // ── Private ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Destroys all current scene objects then waits one frame before calling
    /// <paramref name="buildAction"/> so that Unity can finish the Destroy calls
    /// and clear singleton Instance references before new ones are created.
    /// </summary>
    private System.Collections.IEnumerator TransitionCoroutine(System.Action buildAction)
    {
        DestroyCurrentScene();
        yield return null;      // let Unity process all pending Destroy() calls
        buildAction?.Invoke();
    }

    private static void DestroyCurrentScene()
    {
        // Destroy all root GameObjects in the active scene except the bootstrapper
        foreach (GameObject root in UnityEngine.SceneManagement.SceneManager
                                               .GetActiveScene().GetRootGameObjects())
        {
            if (Instance != null && root == Instance.gameObject) continue;
            Destroy(root);
        }
    }
}
