using System;
using UnityEngine;

public enum VitalKind
{
    Health = 0,
    Hunger = 1,
    Thirst = 2,
    Cold = 3,
    Toxicity = 4
}

/// <summary>
/// The five numbers Stone lives by. Hunger and thirst fall towards zero; cold and toxicity
/// climb towards a hundred. Either way, reaching the end costs health every second, and two
/// ends at once cost twice as fast.
/// Nothing on screen reads the fields directly - the bars listen to Changed and ask for a
/// fraction, so adding a new bar costs no work in here.
/// </summary>
public class Vitals : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Real minutes from full to empty")]
    [Tooltip("A day in game is 24 real minutes. 96 here means one full day between meals.")]
    [SerializeField] private float hungerMinutes = 96f;

    [Tooltip("Thirst is meant to bite first, so keep it under hunger")]
    [SerializeField] private float thirstMinutes = 64f;

    [Header("Damage")]
    [Tooltip("Per second, for each stat sitting at its end. Two at once hurt twice as fast.")]
    [SerializeField] private float damagePerEmptyStat = 2f;

    public float Health { get; private set; }
    public float Hunger { get; private set; }
    public float Thirst { get; private set; }
    public float Cold { get; private set; }
    public float Toxicity { get; private set; }

    public float MaxHealth => maxHealth;
    public bool IsDead => Health <= 0f;

    /// <summary>Fires every time any of the five moves. Every bar hangs off this.</summary>
    public event Action Changed;

    private void Awake()
    {
        Health = maxHealth;
        Hunger = 100f;
        Thirst = 100f;
        Cold = 0f;
        Toxicity = 0f;
    }

    private void Update()
    {
        if (IsDead) return;

        // The drain is written in minutes because that is how it gets tuned; seconds here
        // would mean editing tiny decimals in the Inspector
        float minutes = Time.deltaTime / 60f;

        Hunger = Mathf.Max(0f, Hunger - 100f * minutes / Mathf.Max(0.01f, hungerMinutes));
        Thirst = Mathf.Max(0f, Thirst - 100f * minutes / Mathf.Max(0.01f, thirstMinutes));

        int atTheEnd = 0;
        if (Hunger <= 0f) atTheEnd++;
        if (Thirst <= 0f) atTheEnd++;
        if (Cold >= 100f) atTheEnd++;
        if (Toxicity >= 100f) atTheEnd++;

        if (atTheEnd > 0)
        {
            Health = Mathf.Max(0f, Health - atTheEnd * damagePerEmptyStat * Time.deltaTime);
        }

        Changed?.Invoke();
    }

    /// <summary>Always 0 to 1, and 1 always means healthy - even for the two that climb.</summary>
    public float Fraction(VitalKind kind)
    {
        switch (kind)
        {
            case VitalKind.Health: return Health / Mathf.Max(1f, maxHealth);
            case VitalKind.Hunger: return Hunger / 100f;
            case VitalKind.Thirst: return Thirst / 100f;

            // Flipped on purpose: a full cold bar would otherwise read as good news
            case VitalKind.Cold: return 1f - Cold / 100f;
            case VitalKind.Toxicity: return 1f - Toxicity / 100f;
        }

        return 1f;
    }

    public void Eat(float amount) => Hunger = Mathf.Clamp(Hunger + amount, 0f, 100f);
    public void Drink(float amount) => Thirst = Mathf.Clamp(Thirst + amount, 0f, 100f);
    public void AddCold(float amount) => Cold = Mathf.Clamp(Cold + amount, 0f, 100f);
    public void AddToxicity(float amount) => Toxicity = Mathf.Clamp(Toxicity + amount, 0f, 100f);

    public void Damage(float amount) => Health = Mathf.Max(0f, Health - Mathf.Max(0f, amount));
    public void Heal(float amount) => Health = Mathf.Min(maxHealth, Health + Mathf.Max(0f, amount));

#if UNITY_EDITOR
    // Right click the component header while playing. Editor only, so none of it ships.
    [ContextMenu("Test: hit for 20")]
    private void TestHit() => Damage(20f);

    [ContextMenu("Test: empty the stomach")]
    private void TestStarve() => Hunger = 0f;

    [ContextMenu("Test: empty the canteen")]
    private void TestParch() => Thirst = 0f;

    [ContextMenu("Test: freezing")]
    private void TestFreeze() => Cold = 100f;

    [ContextMenu("Test: refill everything")]
    private void TestRefill()
    {
        Health = maxHealth;
        Hunger = 100f;
        Thirst = 100f;
        Cold = 0f;
        Toxicity = 0f;
    }
#endif
}