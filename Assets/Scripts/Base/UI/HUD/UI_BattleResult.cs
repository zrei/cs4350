using Game.UI;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UI_BattleResult : MonoBehaviour
{
    [SerializeField] GraphicGroup graphicGroup;
    [SerializeField] TextMeshProUGUI m_ResultText;
    [SerializeField] SelectableBase m_ReturnButton;

    private CanvasGroup m_CanvasGroup;

    private void Awake()
    {
        m_CanvasGroup = GetComponent<CanvasGroup>();
        m_CanvasGroup.alpha = 0f;
        m_CanvasGroup.interactable = false;
        m_CanvasGroup.blocksRaycasts = false;

        m_ReturnButton.onSubmit.AddListener(ReturnFromBattle);

        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance victor, int numTurns)
    {
        graphicGroup.color = victor switch
        {
            UnitAllegiance.PLAYER => UIConstants.VictoryColor,
            UnitAllegiance.ENEMY => UIConstants.DefeatColor,
            _ => Color.white,
        };
        m_ResultText.text = victor switch
        {
            UnitAllegiance.PLAYER => "Victory!",
            UnitAllegiance.ENEMY => "Defeat!",
            _ => "???"
        };

        SetActive(true);
    }
    
    public void ReturnFromBattle()
    {
        SetActive(false);
        GameSceneManager.Instance.UnloadBattleScene();
    }

    private void SetActive(bool active)
    {
        IEnumerator Transition()
        {
            var targetA = active ? 1f : 0f;
            var currentA = m_CanvasGroup.alpha;

            var t = 0f;
            var duration = 0.5f;
            while (t < duration)
            {
                t += Time.deltaTime;
                m_CanvasGroup.alpha = Mathf.Lerp(currentA, targetA, t / duration);
                yield return null;
            }
            m_CanvasGroup.alpha = targetA;
            m_CanvasGroup.interactable = active;
            m_CanvasGroup.blocksRaycasts = active;
        }
        StopAllCoroutines();
        StartCoroutine(Transition());
    }
}