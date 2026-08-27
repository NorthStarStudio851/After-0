using UnityEngine;

/// <summary>
/// The side of the interaction that Stone carries. He has a small circle, about his own
/// width; every interactable carries one too. The moment the two touch, the interact button
/// appears. Pressing it runs whatever that object does and pays out the experience.
/// This is the small button on the left, not the big Attack one.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Reach")]
    [Tooltip("The circle Stone carries, in metres - roughly his shoulder width")]
    [SerializeField] private float radius = 0.5f;

    [Header("Button")]
    [Tooltip("Hidden while nothing is in reach")]
    [SerializeField] private ActionButton interactButton;

    [Header("Reward")]
    [Tooltip("Leave empty and it finds the one in the scene")]
    [SerializeField] private PlayerLevel level;

    /// <summary>What the button would act on right now. Null when nothing is in reach.</summary>
    public Interactable Current { get; private set; }

    private void Awake()
    {
        if (level == null) level = FindFirstObjectByType<PlayerLevel>();
    }

    private void OnEnable()
    {
        if (interactButton == null) return;

        interactButton.OnPressed.AddListener(TryInteract);
        interactButton.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (interactButton != null) interactButton.OnPressed.RemoveListener(TryInteract);
    }

    private void Update() => SetCurrent(FindNearest());

    private void SetCurrent(Interactable found)
    {
        if (found == Current) return;

        Current = found;

        // The button showing up is the whole signal that something is within reach
        if (interactButton != null) interactButton.gameObject.SetActive(Current != null);
    }

    private Interactable FindNearest()
    {
        Interactable best = null;
        float bestDistance = float.MaxValue;
        Vector3 me = transform.position;

        var candidates = Interactable.Active;

        for (int i = 0; i < candidates.Count; i++)
        {
            Interactable candidate = candidates[i];
            if (candidate == null || !candidate.IsAvailable) continue;

            // Height is dropped on purpose: standing at the foot of a three metre pole
            // has to count as standing next to it
            Vector3 delta = candidate.transform.position - me;
            delta.y = 0f;

            // Two circles touch when the gap is smaller than the two radii added up.
            // Squared on both sides so there is no square root in here.
            float reach = radius + candidate.Radius;
            float distance = delta.sqrMagnitude;

            if (distance > reach * reach) continue;

            // Two bushes side by side can both be in reach; the closer one wins
            if (distance >= bestDistance) continue;

            bestDistance = distance;
            best = candidate;
        }

        return best;
    }

    public void TryInteract()
    {
        if (Current == null) return;

        Interactable used = Current;

        // Only a real result pays. A spent object returning false keeps tapping worthless.
        if (used.Interact() && level != null)
        {
            level.Add(used.Experience);
        }

        // Using something usually changes whether it is still worth using, so look again
        // straight away instead of leaving a dead button on screen for a frame
        SetCurrent(FindNearest());
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0.2f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}