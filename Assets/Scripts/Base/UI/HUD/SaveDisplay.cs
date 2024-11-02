using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using System.Collections;

public class SaveDisplay : MonoBehaviour
{
    [SerializeField] CanvasGroup m_MainCanvasGroup;
    [SerializeField] List<CanvasGroup> m_ChildCanvasGroups;

    private const float INTERVAL = 0.5f;

    private void Awake()
    {
        GlobalEvents.Save.OnBeginSaveEvent += OnBeginSave;
        GlobalEvents.Save.OnCompleteSaveEvent += OnCompleteSave;
    }

    private void OnDestroy()
    {
        GlobalEvents.Save.OnBeginSaveEvent -= OnBeginSave;
        GlobalEvents.Save.OnCompleteSaveEvent -= OnCompleteSave;
    }

    private void OnBeginSave()
    {
        ToggleShown(true);
        StartCoroutine(DisplayCoroutine());
    }

    private void OnCompleteSave()
    {
        ToggleShown(false);
        StopAllCoroutines();
    }

    private void ToggleShown(bool shown)
    {
        m_MainCanvasGroup.alpha = shown ? 1f : 0f;
    }

    private void ResetDisplay()
    {
        foreach (CanvasGroup cg in m_ChildCanvasGroups)
        {
            cg.alpha = 0f;
        }
    }

    private IEnumerator DisplayCoroutine()
    {
        int index = 0;
        while (true)
        {
            yield return new WaitForSeconds(INTERVAL);
            if (index == 0)
            {
                ResetDisplay();
            }
            m_ChildCanvasGroups[index].alpha = 1f;
            index = (index + 1) % m_ChildCanvasGroups.Count;
        }
    }
}
