using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// One button, two ways to press it: tap the thing on screen, or hit the key.
/// Both paths end in the same event, and holding one while releasing the other
/// does not fire twice.
/// The key badge in the corner shows up wherever a keyboard exists - including a tablet
/// with one plugged in, which is why the check is only for the keyboard itself.
/// </summary>
public class ActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Keyboard")]
    [SerializeField] private Key key = Key.Space;

    [Tooltip("Small badge in the upper right corner. Hidden where there is no keyboard")]
    [SerializeField] private GameObject keyBadge;
    [SerializeField] private TMP_Text keyLabel;

    [Header("Events")]
    public UnityEvent OnPressed;
    public UnityEvent OnReleased;

    /// <summary>True while either the finger or the key is down.</summary>
    public bool IsHeld { get; private set; }

    private bool pointerDown;
    private bool keyDown;
    private int activePointerId = -1;

    private void OnEnable()
    {
        RefreshBadge();

        // A keyboard can be plugged in or pulled out mid-session, so the badge listens
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;

        pointerDown = false;
        keyDown = false;
        activePointerId = -1;

        if (!IsHeld) return;

        IsHeld = false;
        OnReleased.Invoke();
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is Keyboard) RefreshBadge();
    }

    private void RefreshBadge()
    {
        bool hasKeyboard = Keyboard.current != null;

        if (keyBadge != null) keyBadge.SetActive(hasKeyboard);
        if (hasKeyboard && keyLabel != null) keyLabel.text = KeyText();
    }

    // Key.Digit1 reads badly on a badge; the number alone is what people look for
    private string KeyText()
    {
        string raw = key.ToString();

        if (raw.StartsWith("Digit")) return raw.Substring(5);
        if (raw.Length == 1) return raw;

        return raw.ToUpperInvariant();
    }

    private void Update()
    {
        bool nowDown = Keyboard.current != null && Keyboard.current[key].isPressed;

        if (nowDown == keyDown) return;

        keyDown = nowDown;
        Evaluate();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (pointerDown) return;

        activePointerId = eventData.pointerId;
        pointerDown = true;
        Evaluate();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.pointerId != activePointerId) return;

        activePointerId = -1;
        pointerDown = false;
        Evaluate();
    }

    // Held if EITHER source is down, so the event fires once on the way in and once on the
    // way out no matter how the two overlap
    private void Evaluate()
    {
        bool held = pointerDown || keyDown;
        if (held == IsHeld) return;

        IsHeld = held;

        if (held) OnPressed.Invoke();
        else OnReleased.Invoke();
    }
}