using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Experience and the level it buys. Each level costs more than the one before, and a
/// single big haul can cross several at once.
/// The floating "+N Exp" is not built in here - ExpGainLabel listens to Gained, so the
/// number on screen and the number in the save file stay separate concerns.
/// </summary>
public class PlayerLevel : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private int level = 1;
    [SerializeField] private float experience;

    [Header("Curve")]
    [Tooltip("What level two costs. Every level after that multiplies by Growth again.")]
    [SerializeField] private float baseCost = 100f;

    [Range(1.05f, 2f)]
    [SerializeField] private float growth = 1.35f;

    [Header("Display")]
    [SerializeField] private Slider bar;

    [Tooltip("The level number beside the bar. Leave empty until that text exists.")]
    [SerializeField] private TMP_Text levelLabel;

    public int Level => level;
    public float Experience => experience;
    public float NeededForNext => baseCost * Mathf.Pow(growth, level - 1);

    /// <summary>Carries how much was just earned, not the total. ExpGainLabel hangs off this.</summary>
    public event Action<float> Gained;

    /// <summary>Fires on the level itself changing, so a sound or a flash can hook in later.</summary>
    public event Action<int> LevelledUp;

    private void OnEnable() => Refresh();

    public void Add(float amount)
    {
        if (amount <= 0f) return;

        experience += amount;

        // A while, not an if: clearing a big location can be worth three levels at once
        while (experience >= NeededForNext)
        {
            experience -= NeededForNext;
            level++;
            LevelledUp?.Invoke(level);
        }

        Gained?.Invoke(amount);
        Refresh();
    }

    private void Refresh()
    {
        if (bar != null) bar.value = Mathf.Clamp01(experience / NeededForNext);
        if (levelLabel != null) levelLabel.text = level.ToString();
    }

#if UNITY_EDITOR
    // Right click the component header while playing. Editor only, so none of it ships.
    [ContextMenu("Test: +25 exp")]
    private void TestSmall() => Add(25f);

    [ContextMenu("Test: +200 exp (level up)")]
    private void TestBig() => Add(200f);

    [ContextMenu("Test: back to level 1")]
    private void TestReset()
    {
        level = 1;
        experience = 0f;
        Refresh();
    }
#endif
}