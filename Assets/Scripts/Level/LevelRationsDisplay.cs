using Game.UI;
using System.Collections;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UIAnimator))]
public class LevelRationsDisplay : MonoBehaviour
{
    private const float DisplayMaxRations = 100;

    [SerializeField] private TextMeshProUGUI m_CurrRationsText;
    [SerializeField] private Image m_CurrRationsFill;
    [SerializeField] private Image m_Outline;

    [Tooltip("Gradient for the display color as rations drop further below zero.")]
    [SerializeField] private Gradient m_HungerGradient;

    [Tooltip("Rations value for the maximum hunger color change on the gradient.")]
    [SerializeField] private float m_RationsForMaxHungerColor = -15;

    [Tooltip("List of thresholds that, once rations falls below, will have an effect.")]
    public HungerDisplayThreshold[] m_HungerDisplayThresholds;

    private LevelRationsManager m_LevelRationsManager;

    private float m_StartingRations;
    private float m_CurrRations;
    public float CurrRations
    {
        get => m_CurrRations;
        set
        {
            IEnumerator Animate()
            {
                var startRations = m_CurrRations;
                var targetRations = value;

                var t = 0f;
                var duration = 0.25f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    m_CurrRations = Mathf.Lerp(startRations, targetRations, t / duration);
                    UpdateCurrRationsDisplay();
                    yield return null;
                }

                m_CurrRations = targetRations;
                UpdateCurrRationsDisplay();
            }

            StopAllCoroutines();
            StartCoroutine(Animate());
        }
    }

    private UIAnimator m_UIAnimator;

    #region Initialisation

    private void Awake()
    {
        m_UIAnimator = GetComponent<UIAnimator>();

        GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;
    }

    public void Initialise(LevelRationsManager levelRationsManager)
    {
        m_StartingRations = levelRationsManager.StartingRations;
        m_CurrRations = levelRationsManager.CurrRations;
        UpdateCurrRationsDisplay();
    }

    #endregion

    #region Callbacks

    private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
    {
        if (toScene != SceneEnum.LEVEL)
            return;

        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;

        m_LevelRationsManager = FindObjectOfType<LevelRationsManager>();

        if (m_LevelRationsManager == null)
        {
            Debug.LogError("LevelRationsDisplay: LevelRationsManager not found in scene!");
            return;
        }

        Initialise(m_LevelRationsManager);

        GlobalEvents.Rations.RationsSetEvent += OnRationsSet;
        GlobalEvents.Rations.RationsChangeEvent += OnRationsChange;
        GlobalEvents.Scene.OnBeginSceneChange += OnBeginSceneChange;

        Show();
    }

    private void OnBeginSceneChange(SceneEnum fromScene, SceneEnum toScene)
    {
        GlobalEvents.Rations.RationsSetEvent -= OnRationsSet;
        GlobalEvents.Rations.RationsChangeEvent -= OnRationsChange;
        GlobalEvents.Scene.OnBeginSceneChange -= OnBeginSceneChange;

        GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
        GlobalEvents.Rations.RationsSetEvent -= OnRationsSet;
        GlobalEvents.Rations.RationsChangeEvent -= OnRationsChange;
        GlobalEvents.Scene.OnBeginSceneChange -= OnBeginSceneChange;
    }

    private void OnRationsSet(float newRations)
    {
        StopAllCoroutines();
        m_CurrRations = m_LevelRationsManager.CurrRations;
        UpdateCurrRationsDisplay();
    }

    private void OnRationsChange(float changeAmount)
    {
        CurrRations = m_LevelRationsManager.CurrRations;
    }

    #endregion

    #region Graphics

    private void UpdateCurrRationsDisplay()
    {
        SetCurrRationsText(m_CurrRations);
        m_CurrRationsFill.fillAmount = m_CurrRations / DisplayMaxRations;

        var hungerValue = math.clamp(m_CurrRations, m_RationsForMaxHungerColor, 0);
        hungerValue /= m_RationsForMaxHungerColor;
        m_CurrRationsText.color = m_HungerGradient.Evaluate(hungerValue);
        m_Outline.color = m_HungerGradient.Evaluate(hungerValue);

        foreach (HungerDisplayThreshold threshold in m_HungerDisplayThresholds)
        {
            threshold.m_HungerIcon.enabled = threshold.IsThresholdMet(m_CurrRations, true);
        }
    }

    private void SetCurrRationsText(float currRations)
    {
        m_CurrRationsText.text = $"{currRations:F0}<sprite name=\"Rations\" tint>";
    }

    private void Hide()
    {
        m_UIAnimator.Hide();
    }

    private void Show()
    {
        m_UIAnimator.Show();
    }
    #endregion
}

[System.Serializable]
public struct HungerDisplayThreshold
{
    public float m_Threshold;

    public Image m_HungerIcon;

    /// <summary>
    /// Checks whether the threshold has been met
    /// </summary>
    /// <param name="currRations"></param>
    /// <param name="lessThan">Whether to check for less than the threshold or greater than the threshold</param>
    /// <returns></returns>
    public bool IsThresholdMet(float currRations, bool lessThan)
    {
        if (lessThan && currRations <= m_Threshold)
            return true;
        else if (!lessThan && currRations >= m_Threshold)
            return true;
        return false;
    }
}
