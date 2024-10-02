using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum LevelResultType
{
    SUCCESS,
    DEFEAT,
    OUT_OF_TIME
}

// Temporary UI for level result, to be integrated with UI_Manager System
public class UI_LevelResult : MonoBehaviour
{
    [SerializeField] GameObject m_LevelResultPanel;
    [SerializeField] TextMeshProUGUI m_ResultText;
    [SerializeField] Button m_ReturnButton;

    private void Awake()
    {
        m_LevelResultPanel.SetActive(false);
        GlobalEvents.Level.LevelEndEvent += OnLevelEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Level.LevelEndEvent -= OnLevelEnd;
    }

    private void OnLevelEnd(LevelResultType result)
    {
        m_ResultText.text = result switch
        {
            LevelResultType.SUCCESS => "Level Completed!",
            LevelResultType.DEFEAT => "Defeat...",
            LevelResultType.OUT_OF_TIME => "Out of time...",
            _ => "???"
        };
        m_LevelResultPanel.SetActive(true);
        m_ReturnButton.onClick.AddListener(ReturnFromLevel);
    }
    
    public void ReturnFromLevel()
    {
        m_LevelResultPanel.SetActive(false);
        GlobalEvents.Level.ReturnFromLevelEvent?.Invoke();
        m_ReturnButton.onClick.RemoveListener(ReturnFromLevel);
    }
}