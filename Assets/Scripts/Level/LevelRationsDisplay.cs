using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CanvasGroup))]
public class LevelRationsDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI m_CurrRationsText;
    [SerializeField] private Image m_CurrRationsFill;
    
    private LevelRationsManager m_LevelRationsManager;
    
    private float m_StartingRations;
    private float m_CurrRations;
    public float CurrRations
    {
        get => m_CurrRations;
        set
        {
            m_CurrRations = value;
            SetCurrRationsText(m_CurrRations);
            m_CurrRationsFill.fillAmount = m_CurrRations / m_StartingRations;
        }
    }
    
    private Animator m_Animator;
    private CanvasGroup m_CanvasGroup;

    private bool isHidden;
    
    #region Initialisation

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_Animator.enabled = false;
        
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
        m_CanvasGroup.alpha = 0;
        
        isHidden = true;
        
        GlobalEvents.Scene.LevelSceneLoadedEvent += OnSceneLoad;
    }

    public void Initialise(LevelRationsManager levelRationsManager)
    {
        m_StartingRations = levelRationsManager.StartingRations;
        CurrRations = levelRationsManager.CurrRations;
    }
    
    #endregion
    
    #region Callbacks

    private void OnSceneLoad()
    {
        m_LevelRationsManager = FindObjectOfType<LevelRationsManager>();
        
        if (m_LevelRationsManager == null)
        {
            Debug.LogError("LevelRationsDisplay: LevelRationsManager not found in scene!");
            return;
        }
        
        Initialise(m_LevelRationsManager);
        
        GlobalEvents.Rations.RationsSetEvent += OnRationsSet;
        GlobalEvents.Rations.RationsChangeEvent += OnRationsChange;
        GlobalEvents.Level.BattleNodeStartEvent += OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent += OnBattleNodeEnd;
        GlobalEvents.Level.ReturnFromLevelEvent += Hide;
        
        Show();
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.LevelSceneLoadedEvent -= OnSceneLoad;
        GlobalEvents.Rations.RationsSetEvent -= OnRationsSet;
        GlobalEvents.Rations.RationsChangeEvent -= OnRationsChange;
        GlobalEvents.Level.BattleNodeStartEvent -= OnBattleNodeStart;
        GlobalEvents.Level.BattleNodeEndEvent -= OnBattleNodeEnd;
        GlobalEvents.Level.ReturnFromLevelEvent -= Hide;
    }
    
    private void OnRationsSet(float newRations)
    {
        CurrRations = m_LevelRationsManager.CurrRations;
    }

    private void OnRationsChange(float changeAmount)
    {
        CurrRations = m_LevelRationsManager.CurrRations;
    }

    private void OnBattleNodeStart(BattleNode _)
    {
        Hide();
    }

    private void OnBattleNodeEnd(BattleNode node, UnitAllegiance _1, int _2)
    {
        Show();
    }
    
    #endregion
    
    #region Graphics
    
    private void SetCurrRationsText(float currRations)
    {
        m_CurrRationsText.text = $"{currRations:F0}<sprite name=\"Rations\" tint>";
    }

    private void Hide()
    {
        isHidden = true;
        m_Animator.enabled = true;
        m_Animator.Play(UIConstants.HideAnimHash);
    }
    
    private void Show()
    {
        isHidden = false;
        m_Animator.enabled = true;
        m_Animator.Play(UIConstants.ShowAnimHash);
    }
    
    private void OnAnimationFinish()
    {
        m_Animator.enabled = false;
        m_CanvasGroup.interactable = !isHidden;
        m_CanvasGroup.blocksRaycasts = !isHidden;
    }
    
    #endregion
}
