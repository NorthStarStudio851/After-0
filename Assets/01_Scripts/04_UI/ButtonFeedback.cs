using UnityEngine;

/// <summary>
/// The little squash a button does while it is held. ActionButton carries only the logic,
/// so this hangs off its state and never needs to know what the button actually does.
/// </summary>
[RequireComponent(typeof(ActionButton))]
public class ButtonFeedback : MonoBehaviour
{
    [Tooltip("Leave empty to squash the button itself")]
    [SerializeField] private RectTransform target;

    [Range(0.7f, 1f)]
    [SerializeField] private float pressedScale = 0.9f;

    [SerializeField] private float speed = 16f;

    private ActionButton button;

    private void Awake()
    {
        button = GetComponent<ActionButton>();
        if (target == null) target = (RectTransform)transform;
    }

    private void Update()
    {
        Vector3 wanted = button.IsHeld ? Vector3.one * pressedScale : Vector3.one;

        // Unscaled, so buttons still answer while the game is paused behind an open inventory
        target.localScale = Vector3.Lerp(target.localScale, wanted, speed * Time.unscaledDeltaTime);
    }
}