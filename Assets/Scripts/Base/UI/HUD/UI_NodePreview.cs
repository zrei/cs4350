using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game;
using TMPro;
using UnityEngine;

public class UI_NodePreview : MonoBehaviour
{
    public float horizontalOffset = 60f;
    public float verticalOffset = 30f;
    
    [SerializeField] TextMeshProUGUI m_NodeNameText;
    [SerializeField] TextMeshProUGUI m_DescriptionText;
    [SerializeField] GameObject m_EnemiesSection;
    [SerializeField] TextMeshProUGUI m_EnemiesText;
    [SerializeField] GameObject m_UnlockConditionSection;
    [SerializeField] TextMeshProUGUI m_UnlockConditionText;
    
    
    private CanvasGroup m_CanvasGroup;
    private RectTransform m_RectTransform;
    private Vector3 m_CurrPreviewedNodePosition;

    private void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_RectTransform = GetComponent<RectTransform>();
    }
    
    public void Show()
    {
        m_CanvasGroup.alpha = 1f;
        m_CanvasGroup.interactable = true;
        m_CanvasGroup.blocksRaycasts = true;
    }
    
    public void Hide()
    {
        m_CanvasGroup.alpha = 0f;
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
        
        StopAllCoroutines();
    }
    
    public void SetUpPreview(NodeInternal node)
    {
        var previewData = node.GetNodePreviewData();
        
        m_NodeNameText.text = previewData.NodeName;
        m_DescriptionText.text = previewData.NodeDescription;

        if (previewData is BattleNodePreviewData battleNodePreviewData)
        {
            SetUpBattleDisplay(battleNodePreviewData);
        }
        else
        {
            m_EnemiesSection.SetActive(false);
            m_EnemiesText.text = "";
        }

        if (previewData.IsMoralityLocked)
        {
            SetUpUnlockConditionsDisplay(previewData);
        }
        else
        {
            m_UnlockConditionSection.SetActive(false);
            m_UnlockConditionText.text = "";
        }
        
        m_CurrPreviewedNodePosition = node.transform.position;
        StartCoroutine(UpdatePosition_Coroutine());
    }
    
    private void SetUpBattleDisplay(BattleNodePreviewData battleData)
    {
        m_EnemiesSection.SetActive(true);

        // Get number of enemies by class
        Dictionary<string, int> enemyClassCount = new Dictionary<string, int>();
        foreach (var enemy in battleData.EnemyUnits)
        {
            if (!enemyClassCount.TryAdd(enemy.m_EnemyCharacterData.m_EnemyClass.m_ClassName, 1))
                enemyClassCount[enemy.m_EnemyCharacterData.m_EnemyClass.m_ClassName]++;
        }
        
        // List enemies by their classes
        StringBuilder builder = new StringBuilder();
        foreach (var enemyClass in enemyClassCount)
        {
            builder.Append($"{enemyClass.Key}\tx{enemyClass.Value}\n");
        }
        m_EnemiesText.text = builder.ToString();
    }
    
    private void SetUpUnlockConditionsDisplay(NodePreviewData previewData)
    {
        int thresholdValue = MoralityManager.Instance.GetMoralityValue(previewData.MoralityThreshold.m_Threshold);
        bool isSatisfied = previewData.MoralityThreshold.IsSatisfied(MoralityManager.Instance.CurrMoralityPercentage);

        m_UnlockConditionSection.SetActive(true);
        m_UnlockConditionText.text = $"Morality<sprite name=\"Morality\" tint>: ";
        m_UnlockConditionText.text += previewData.MoralityThreshold.m_GreaterThan ? $">{thresholdValue}" : $"<{thresholdValue}";
        m_UnlockConditionText.text += isSatisfied ? " (Satisfied)" : " (Not Satisfied)";
        m_UnlockConditionText.color = isSatisfied ? Color.green : Color.red;
    }
    
    private IEnumerator UpdatePosition_Coroutine()
    {
        while (true)
        {
            UpdatePosition();
            yield return null;
        }
    }

    private void UpdatePosition()
    {
        SetUpPreviewPanelLocation(m_CurrPreviewedNodePosition);
    }

    private void SetUpPreviewPanelLocation(Vector3 nodePosition)
    {
        // Set position of preview panel to be at the node's position
        var screenPosition = CameraManager.Instance.MainCamera.WorldToScreenPoint(nodePosition);
        var viewportPosition = CameraManager.Instance.MainCamera.ScreenToViewportPoint(screenPosition);
        
        Vector3 offset = Vector3.zero;
        
        offset = viewportPosition.x < 0.8f 
            ? Vector3.right * (m_RectTransform.rect.width / 2 + horizontalOffset)
            : Vector3.left * (m_RectTransform.rect.width / 2 + horizontalOffset);
        
        if (viewportPosition.y < 0.2f)
        {
            offset +=  Vector3.up * (m_RectTransform.rect.height / 2 + verticalOffset);
        }
        else if (viewportPosition.y > 0.8f)
        {
            offset += Vector3.down * (m_RectTransform.rect.height / 2 + verticalOffset);
        }
        
        m_RectTransform.anchoredPosition = screenPosition + offset;
    }
}