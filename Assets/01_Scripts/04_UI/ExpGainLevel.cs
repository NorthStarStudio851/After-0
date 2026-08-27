using UnityEngine;
using TMPro;

/// <summary>
/// The little "+2 Exp" that pops up when Stone earns something, then fades away.
/// It reports the GAIN, not the total - the bar underneath already carries the total.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class ExpGainLabel : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private PlayerLevel level;

    [Header("Timing")]
    [SerializeField] private float holdSeconds = 1.2f;
    [SerializeField] private float fadeSeconds = 0.6f;

    [Tooltip("How far it drifts upward over its whole life, in canvas pixels")]
    [SerializeField] private float riseDistance = 40f;

    private TMP_Text label;
    private RectTransform rect;
    private Vector2 home;

    private float timer = -1f;
    private float pending;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
        rect = (RectTransform)transform;
        home = rect.anchoredPosition;
    }

    private void OnEnable()
    {
        if (level == null) level = FindFirstObjectByType<PlayerLevel>();
        if (level != null) level.Gained += Show;

        Hide();
    }

    private void OnDisable()
    {
        if (level != null) level.Gained -= Show;
    }

    private void Show(float amount)
    {
        // Chopping three trees quickly should read as one number climbing, not three flickers
        pending = timer > 0f ? pending + amount : amount;

        timer = holdSeconds + fadeSeconds;
        label.text = "+" + Mathf.RoundToInt(pending) + " Exp";
    }

    private void Update()
    {
        if (timer < 0f) return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Hide();
            return;
        }

        // Full brightness while it holds, then a straight fade over the last stretch
        label.alpha = timer > fadeSeconds ? 1f : timer / fadeSeconds;

        float life = 1f - Mathf.Clamp01(timer / (holdSeconds + fadeSeconds));
        rect.anchoredPosition = home + Vector2.up * riseDistance * life;
    }

    private void Hide()
    {
        timer = -1f;
        pending = 0f;

        if (label != null) label.alpha = 0f;
        if (rect != null) rect.anchoredPosition = home;
    }
}