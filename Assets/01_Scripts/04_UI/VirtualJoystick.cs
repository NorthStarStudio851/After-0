using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// The movement stick. Fixed in place while playing, but the player gets to choose where it
/// sits and how visible it is, and those choices survive between sessions.
/// Sprint has no button: push the stick past the outer threshold and it locks on, the way
/// PUBG does it. Tapping the stick again releases it.
/// </summary>
public class VirtualJoystick : MonoBehaviour,
    IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private const string PrefX = "joystick_x";
    private const string PrefY = "joystick_y";
    private const string PrefAlpha = "joystick_alpha";

    [Header("Parts")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;
    [SerializeField] private CanvasGroup group;

    [Header("Feel")]
    [Tooltip("Below this fraction of the radius the stick reads as zero")]
    [Range(0f, 0.5f)]
    [SerializeField] private float deadZone = 0.15f;

    [Tooltip("Push past this fraction of the radius and sprint locks on")]
    [Range(0.5f, 1f)]
    [SerializeField] private float sprintThreshold = 0.85f;

    [Tooltip("How fast the knob catches up when it is only showing keyboard input")]
    [SerializeField] private float displaySpeed = 14f;

    [Header("Player settings")]
    [Tooltip("Never fully invisible: a player who slides it to zero can no longer steer")]
    [Range(0.15f, 1f)]
    [SerializeField] private float defaultAlpha = 0.85f;

    /// <summary>Read by PlayerMover. Nobody writes it from outside.</summary>
    public Vector2 InputVector { get; private set; }

    /// <summary>True while the stick is pushed past the threshold, or locked there.</summary>
    public bool IsSprinting { get; private set; }

    public bool IsHeld { get; private set; }

    private Camera uiCamera;
    private Vector2 shownDirection;
    private Vector2 externalTarget;
    private bool gotExternalThisFrame;

    private Vector2 lockedDirection;
    private bool sprintLocked;

    // The finger that owns the stick. Any other finger is somebody else's business.
    private int activePointerId = -1;

    private float Radius => background != null ? background.rect.width * 0.5f : 1f;

    private void Awake()
    {
        Canvas canvas = GetComponentInParent<Canvas>();

        // Overlay canvases take a null camera; anything else needs the real one
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        LoadPlayerSettings();
    }

    // --- What the player chose in settings ---

    private void LoadPlayerSettings()
    {
        if (background != null && PlayerPrefs.HasKey(PrefX))
        {
            background.anchoredPosition = new Vector2(
                PlayerPrefs.GetFloat(PrefX), PlayerPrefs.GetFloat(PrefY));
        }

        if (group != null)
        {
            group.alpha = Mathf.Clamp(PlayerPrefs.GetFloat(PrefAlpha, defaultAlpha), 0.15f, 1f);
        }
    }

    /// <summary>Called by the settings screen once it exists.</summary>
    public void SavePlacement(Vector2 anchoredPosition, float alpha)
    {
        alpha = Mathf.Clamp(alpha, 0.15f, 1f);

        if (background != null) background.anchoredPosition = anchoredPosition;
        if (group != null) group.alpha = alpha;

        PlayerPrefs.SetFloat(PrefX, anchoredPosition.x);
        PlayerPrefs.SetFloat(PrefY, anchoredPosition.y);
        PlayerPrefs.SetFloat(PrefAlpha, alpha);
        PlayerPrefs.Save();
    }

    // --- Touch and mouse ---

    public void OnPointerDown(PointerEventData eventData)
    {
        // A tap while auto-running is how you stop, so it never starts a new drag
        if (sprintLocked)
        {
            ReleaseLock();
            return;
        }

        if (IsHeld) return;                       // already driven by another finger

        activePointerId = eventData.pointerId;
        IsHeld = true;

        Drag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        Drag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        IsHeld = false;
        activePointerId = -1;

        // Letting go while pushed all the way out leaves the stick running on its own
        if (sprintLocked) return;

        InputVector = Vector2.zero;
        IsSprinting = false;
        externalTarget = Vector2.zero;
    }

    private void Drag(PointerEventData eventData)
    {
        if (background == null) return;
        if (!ScreenToLocal(eventData, out Vector2 local)) return;

        Vector2 offset = local - background.anchoredPosition;
        Vector2 raw = Vector2.ClampMagnitude(offset / Radius, 1f);

        InputVector = ApplyDeadZone(raw);
        shownDirection = raw;

        if (handle != null) handle.anchoredPosition = raw * Radius;

        if (raw.magnitude >= sprintThreshold)
        {
            sprintLocked = true;
            lockedDirection = raw.normalized;
            IsSprinting = true;
        }
        else if (!sprintLocked)
        {
            IsSprinting = false;
        }
    }

    private void ReleaseLock()
    {
        sprintLocked = false;
        IsSprinting = false;
        InputVector = Vector2.zero;
        lockedDirection = Vector2.zero;
        externalTarget = Vector2.zero;
    }

    // Converts a screen point into this rect's own coordinates. Without the right camera
    // argument the numbers look plausible and are quietly wrong on some canvases.
    private bool ScreenToLocal(PointerEventData eventData, out Vector2 local)
    {
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, eventData.position, uiCamera, out local);
    }

    // Rescales the live part so the stick starts at zero right after the dead zone
    // instead of snapping to full speed
    private Vector2 ApplyDeadZone(Vector2 raw)
    {
        float magnitude = raw.magnitude;

        if (magnitude < deadZone) return Vector2.zero;
        if (deadZone >= 1f) return raw;

        float scaled = (magnitude - deadZone) / (1f - deadZone);
        return raw.normalized * Mathf.Clamp01(scaled);
    }

    /// <summary>Show a direction that came from somewhere else. Ignored while a finger holds it.</summary>
    public void ShowExternal(Vector2 direction)
    {
        if (IsHeld || sprintLocked) return;

        externalTarget = Vector2.ClampMagnitude(direction, 1f);
        gotExternalThisFrame = true;
    }

    // The knob settles itself. Nothing outside has to call anything for it to come back
    // to the middle.
    private void LateUpdate()
    {
        if (sprintLocked)
        {
            InputVector = lockedDirection;
            shownDirection = lockedDirection;

            if (handle != null) handle.anchoredPosition = lockedDirection * Radius;
            return;
        }

        if (IsHeld) return;

        Vector2 target = gotExternalThisFrame ? externalTarget : Vector2.zero;
        gotExternalThisFrame = false;

        if (shownDirection == target) return;

        shownDirection = Vector2.MoveTowards(shownDirection, target, displaySpeed * Time.deltaTime);

        if (handle != null) handle.anchoredPosition = shownDirection * Radius;
    }

    private void OnValidate()
    {
        displaySpeed = Mathf.Max(0.1f, displaySpeed);
    }
}