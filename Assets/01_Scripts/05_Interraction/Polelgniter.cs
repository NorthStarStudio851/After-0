using UnityEngine;

/// <summary>
/// A pole that starts dark. Stone walks up with his torch, presses interact, and it stays
/// lit for good.
/// Dark and lit are the same object in two states: the only real difference is whether
/// LightSource is enabled.
/// The LightMap picks up its poles once and then only redraws when something asks it to,
/// so switching one on has to say so - otherwise the light appears and the fog does not move.
/// Place these by hand in mission one: they teach the player how far apart poles belong,
/// long before he gets to place any himself.
/// </summary>
[RequireComponent(typeof(LightSource))]
public class PoleIgniter : Interactable
{
    [Header("Flame")]
    [Tooltip("Switched on together with the light. Leave empty until the prefab has one.")]
    [SerializeField] private GameObject flame;

    private LightSource source;

    public bool IsLit => source != null && source.enabled;

    // A pole already burning is not worth walking up to, so the button stays hidden
    public override bool IsAvailable => !IsLit;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (source == null) source = GetComponent<LightSource>();

        // Whether a pole starts dark is decided in the scene by unticking LightSource,
        // not by a field in here. This only makes the flame agree with it.
        ApplyState();
    }

    public override bool Interact()
    {
        if (IsLit) return false;

        source.enabled = true;

        // The pole was already on the list while it was dark - Stamp just skipped it. Coming
        // back on changes nothing the map notices by itself, so ask it to redraw.
        if (LightMap.Instance != null) LightMap.Instance.MarkDirty();

        ApplyState();

        return true;
    }

    private void ApplyState()
    {
        if (flame != null) flame.SetActive(IsLit);
    }
}