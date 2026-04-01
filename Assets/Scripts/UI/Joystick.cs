using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Virtual joystick for mobile touch input.
/// The joystick handle appears where the player first touches the screen
/// and provides a normalised Direction vector for movement.
/// </summary>
public class Joystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // ── Inspector ──────────────────────────────────────────────────────────────
    [SerializeField] private RectTransform background;  // outer circle
    [SerializeField] private RectTransform handle;       // inner circle (knob)

    [Tooltip("Maximum handle travel distance in pixels")]
    [SerializeField] private float handleRange = 80f;

    [Tooltip("Dead zone — inputs below this magnitude are ignored")]
    [SerializeField] private float deadZone = 0.1f;

    // ── Public State ───────────────────────────────────────────────────────────
    /// <summary>Normalised direction vector in the range [-1, 1] per axis.</summary>
    public Vector2 Direction { get; private set; }

    /// <summary>True while the joystick is being held.</summary>
    public bool IsHeld { get; private set; }

    // ── Private ────────────────────────────────────────────────────────────────
    private Canvas   _canvas;
    private Vector2  _startPos;

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (background == null)
            background = GetComponent<RectTransform>();

        // Hide until touched
        SetVisible(false);
    }

    // ── IPointer/IDrag Handlers ────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        IsHeld = true;

        // Move background to touch position
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.GetComponent<RectTransform>(),
            eventData.position,
            _canvas.worldCamera,
            out localPoint);

        background.anchoredPosition = localPoint;
        SetVisible(true);

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!IsHeld) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            _canvas.worldCamera,
            out localPoint);

        // Clamp to the handle range
        Vector2 clamped = Vector2.ClampMagnitude(localPoint, handleRange);
        handle.anchoredPosition = clamped;

        Direction = clamped / handleRange;
        if (Direction.magnitude < deadZone)
            Direction = Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsHeld = false;
        Direction = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        SetVisible(false);
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void SetVisible(bool visible)
    {
        if (background != null)
            background.gameObject.SetActive(visible);
    }
}
