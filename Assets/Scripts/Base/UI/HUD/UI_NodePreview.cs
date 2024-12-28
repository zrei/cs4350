using Game;
using Game.UI;
using System.Collections;
using System.Collections.Generic;
using Level.Nodes;
using TMPro;
using UnityEngine;

public class UI_NodePreview : MonoBehaviour
{
    public float horizontalOffset = 60f;
    public float verticalOffset = 30f;

    [SerializeField] TextMeshProUGUI m_NodeNameText;
    [SerializeField] TextMeshProUGUI m_DescriptionText;
    [SerializeField] GameObject m_BattleDataSection;
    [SerializeField] TextDisplayPooler m_ObjectivesTextDisplay;
    [SerializeField] TextDisplayPooler m_EnemiesTextDisplay;
    [SerializeField] GameObject m_UnlockConditionSection;
    [SerializeField] TextMeshProUGUI m_UnlockConditionText;

    private CanvasGroup m_CanvasGroup;
    private UIFader m_UIFader;
    private RectTransform m_RectTransform;
    private Vector3 m_CurrPreviewedNodePosition;

    private void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_UIFader = new(m_CanvasGroup, true, false, false);
        m_RectTransform = GetComponent<RectTransform>();

        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;
    }

    public void Show()
    {
        m_UIFader.Show();
    }

    public void Hide()
    {
        m_UIFader.Hide();

        StopAllCoroutines();
    }

    public void SetUpPreview(LevelNode node)
    {
        var previewData = node.NodeData.GetNodePreviewData();

        m_NodeNameText.text = previewData.NodeName;
        m_DescriptionText.text = previewData.NodeDescription;

        if (previewData is BattleNodePreviewData battleNodePreviewData)
        {
            SetUpBattleDisplay(battleNodePreviewData);
        }
        else
        {
            m_BattleDataSection.SetActive(false);
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
        m_BattleDataSection.SetActive(true);

        // Get number of enemies by class
        Dictionary<string, int> enemyClassCount = new Dictionary<string, int>();
        foreach (var enemy in battleData.EnemyUnits)
        {
            if (!enemyClassCount.TryAdd(enemy.m_EnemyCharacterData.m_EnemyClass.m_ClassName, 1))
                enemyClassCount[enemy.m_EnemyCharacterData.m_EnemyClass.m_ClassName]++;
        }

        // List enemies by their classes
        m_EnemiesTextDisplay.Clear();

        foreach (var enemyClass in enemyClassCount)
        {
            var enemiesDisplay = m_EnemiesTextDisplay.Get();
            enemiesDisplay.PrimaryText.SetValue(enemyClass.Key);
            enemiesDisplay.SecondaryText.SetValue(enemyClass.Value);
        }

        // List objectives
        m_ObjectivesTextDisplay.Clear();

        foreach (var objective in battleData.Objectives)
        {
            var objectiveDisplay = m_ObjectivesTextDisplay.Get();
            objectiveDisplay.PrimaryText.SetValue(objective.ToString());
            objectiveDisplay.PrimaryText.SetColor(objective.m_Color);
        }
    }

    private void SetUpUnlockConditionsDisplay(NodePreviewData previewData)
    {
        int thresholdValue = previewData.MoralityCondition.threshold;
        bool isSatisfied = previewData.MoralityCondition.Evaluate();

        m_UnlockConditionSection.SetActive(true);
        m_UnlockConditionText.text = $"Morality<sprite name=\"Morality\" tint>: ";
        m_UnlockConditionText.text += previewData.MoralityCondition.mode switch
        {
            MoralityCondition.Mode.GreaterThan => $">{thresholdValue}",
            MoralityCondition.Mode.GreaterThanOrEqual => $"≥{thresholdValue}",
            MoralityCondition.Mode.LessThan => $"<{thresholdValue}",
            MoralityCondition.Mode.LessThanOrEqual => $"≤{thresholdValue}",
            _ => "null"
        };
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
            offset += Vector3.up * (m_RectTransform.rect.height / 2 + verticalOffset);
        }
        else if (viewportPosition.y > 0.8f)
        {
            offset += Vector3.down * (m_RectTransform.rect.height / 2 + verticalOffset);
        }

        m_RectTransform.anchoredPosition = screenPosition + offset;
    }
}