using UnityEngine;

/// <summary>
/// Renders the player (or bot) black hole as a clearly visible filled circle
/// with a thin white outline ring.
///
/// Uses ProceduralSprites to generate the circle and ring textures at runtime
/// — no external art assets required.
///
/// sortingOrder = 10 ensures the circle renders on top of all map layers.
/// Scale is overridden each LateUpdate to enforce a minimum visible radius of
/// MIN_RADIUS world units, regardless of mass.
/// </summary>
[RequireComponent(typeof(MassSystem))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Color circleColor = new Color(0f, 0.67f, 1f); // #00AAFF bright blue

    // ── Private ────────────────────────────────────────────────────────────────
    private SpriteRenderer _spriteRenderer;
    private Transform      _outlineTransform;
    private MassSystem     _massSystem;

    /// <summary>Minimum visual radius enforced by LateUpdate, independent of physics mass.</summary>
    private const float MIN_VISIBLE_RADIUS = 1.5f;
    /// <summary>Outline ring is rendered at this multiple of the circle radius.</summary>
    private const float OUTLINE_SCALE   = 1.15f;
    private const int   SORTING_ORDER   = 10;
    private const int   TEX_SIZE        = 128;

    // ── Lifecycle ──────────────────────────────────────────────────────────────

    private void Awake()
    {
        _massSystem = GetComponent<MassSystem>();
        SetupCircleSprite();
        SetupOutlineRing();
    }

    private void LateUpdate()
    {
        UpdateVisualScale();
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Sets the fill color of the player circle (e.g. set from GameManager).</summary>
    public void SetColor(Color color)
    {
        circleColor = color;
        if (_spriteRenderer != null)
            _spriteRenderer.color = color;
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void SetupCircleSprite()
    {
        // Reuse any SpriteRenderer already on the GameObject (e.g. from CreateBlackHolePrefab)
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        // Create the sprite in white so the SpriteRenderer.color tint controls the final hue
        // at runtime without needing to regenerate the texture when the color changes.
        _spriteRenderer.sprite       = ProceduralSprites.CreateCircleSprite(TEX_SIZE, Color.white);
        _spriteRenderer.color        = circleColor;
        _spriteRenderer.sortingOrder = SORTING_ORDER;
    }

    private void SetupOutlineRing()
    {
        GameObject outlineGo = new GameObject("Outline");
        outlineGo.transform.SetParent(transform);
        outlineGo.transform.localPosition = Vector3.zero;
        // OUTLINE_SCALE > 1 ensures the ring is always slightly outside the filled circle.
        // The parent's world scale is set every LateUpdate; the child inherits it and then
        // multiplies by this local scale.
        outlineGo.transform.localScale = Vector3.one * OUTLINE_SCALE;

        SpriteRenderer outlineSr = outlineGo.AddComponent<SpriteRenderer>();
        outlineSr.sprite       = ProceduralSprites.CreateRingSprite(TEX_SIZE, Color.white);
        outlineSr.color        = new Color(1f, 1f, 1f, 0.85f);
        outlineSr.sortingOrder = SORTING_ORDER - 1;

        _outlineTransform = outlineGo.transform;
    }

    private void UpdateVisualScale()
    {
        if (_massSystem == null) return;

        // Enforce a minimum visible size regardless of mass value
        float radius   = Mathf.Max(MIN_VISIBLE_RADIUS, _massSystem.Radius);
        float diameter = radius * 2f;

        // Override the transform scale so the sprite and the physics collider agree.
        // (MassSystem also sets localScale, but LateUpdate always runs last.)
        transform.localScale = Vector3.one * diameter;
        // Outline child keeps its own localScale = OUTLINE_SCALE, so its world size = diameter * OUTLINE_SCALE
    }
}
