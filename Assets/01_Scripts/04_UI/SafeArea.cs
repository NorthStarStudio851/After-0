using UnityEngine;

/// <summary>
/// Keeps its children inside the part of the screen the phone actually shows.
/// In landscape the notch and the rounded corners eat into the sides, which is exactly
/// where the joystick and the action buttons live.
/// Put this on a full-screen RectTransform and parent the whole HUD to it.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    [Header("Which edges to respect")]
    [SerializeField] private bool left = true;
    [SerializeField] private bool right = true;
    [SerializeField] private bool top = true;
    [SerializeField] private bool bottom = true;

    private RectTransform rect;
    private Rect lastArea;
    private Vector2Int lastResolution;

    private void OnEnable()
    {
        rect = GetComponent<RectTransform>();
        Apply();
    }

    private void Update()
    {
        // The safe area changes when the device rotates or the keyboard opens, and there is
        // no event for it - polling a comparison is the standard way
        if (Screen.safeArea == lastArea &&
            Screen.width == lastResolution.x && Screen.height == lastResolution.y) return;

        Apply();
    }

    [ContextMenu("Apply now")]
    public void Apply()
    {
        if (rect == null) rect = GetComponent<RectTransform>();
        if (Screen.width <= 0 || Screen.height <= 0) return;

        Rect area = Screen.safeArea;

        lastArea = area;
        lastResolution = new Vector2Int(Screen.width, Screen.height);

        Vector2 min = area.position;
        Vector2 max = area.position + area.size;

        min.x /= Screen.width;
        min.y /= Screen.height;
        max.x /= Screen.width;
        max.y /= Screen.height;

        // An unticked edge goes back to the raw screen border
        if (!left) min.x = 0f;
        if (!bottom) min.y = 0f;
        if (!right) max.x = 1f;
        if (!top) max.y = 1f;

        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private void OnValidate()
    {
        if (isActiveAndEnabled) Apply();
    }
}