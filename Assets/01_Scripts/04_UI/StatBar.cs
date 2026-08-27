using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// One bar tied to one number. Put it on the Slider, pick which vital it shows, done.
/// The slider has to run from 0 to 1 - the fraction coming out of Vitals is already scaled.
/// </summary>
public class StatBar : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Vitals vitals;
    [SerializeField] private VitalKind kind = VitalKind.Health;

    [Header("Parts")]
    [SerializeField] private Slider slider;

    [Tooltip("Optional. Shows the percentage next to the bar.")]
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        // Vitals sits on the player and the bar sits on the canvas, so a dragged reference
        // dies the moment the player is spawned instead of placed
        if (vitals == null) vitals = FindFirstObjectByType<Vitals>();
        if (slider == null) slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (vitals != null) vitals.Changed += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (vitals != null) vitals.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (vitals == null) return;

        float fraction = vitals.Fraction(kind);

        if (slider != null) slider.value = fraction;

        // Ceil, so a bar with anything at all left in it never reads zero
        if (label != null) label.text = Mathf.CeilToInt(fraction * 100f).ToString();
    }
}