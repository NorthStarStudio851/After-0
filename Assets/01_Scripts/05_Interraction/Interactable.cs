using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Anything Stone can walk up to and use: a pole to light, a tree to chop, a crate to open.
/// Every one of these carries its own small circle. When it touches the circle Stone carries,
/// the interact button wakes up.
/// Inherit from this and fill in Interact().
/// </summary>
public abstract class Interactable : MonoBehaviour
{
    // Same arrangement LightSource has with LightMap: the objects announce themselves instead
    // of somebody sweeping the whole scene looking for them every frame
    private static readonly List<Interactable> active = new List<Interactable>();
    public static IReadOnlyList<Interactable> Active => active;

    [Header("Reach")]
    [Tooltip("Own circle in metres. Touches the circle Stone carries to open the button.")]
    [SerializeField] private float radius = 0.6f;

    [Header("Reward")]
    [Tooltip("Experience for one successful interaction. Zero means this one pays nothing.")]
    [SerializeField] private float experience = 5f;

    public float Radius => radius;
    public float Experience => experience;

    /// <summary>
    /// False takes it out of reach without removing it from the world - an already burning
    /// pole is still there, it is just not worth walking up to any more.
    /// </summary>
    public virtual bool IsAvailable => true;

    /// <summary>
    /// Runs once, when the player presses the interact button.
    /// Return false when nothing actually happened, so no experience gets paid for it -
    /// otherwise tapping a spent object over and over would be free levels.
    /// </summary>
    public abstract bool Interact();

    // Virtual so children can add their own setup without forgetting to register
    protected virtual void OnEnable() => active.Add(this);

    protected virtual void OnDisable() => active.Remove(this);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}