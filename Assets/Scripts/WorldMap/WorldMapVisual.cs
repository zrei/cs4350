using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections;

public class WorldMapVisual : BaseNodeVisual
{
    [SerializeField] private Vector3 m_NormalScale = new Vector3(5f, 5f, 5f);
    [SerializeField] private Vector3 m_MinScale = new Vector3(3f, 3f, 3f);

    public override float NodeRadiusOffset => 0.7f;

    public VoidEvent OnSelected;
    public VoidEvent OnDeselected;  

    private Coroutine m_CurrLevelCoroutine = null;
    private bool m_IsCurrent = false;

    private const float HALF_CYCLE_TIME = 1.0f;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        OnSelected?.Invoke();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        OnDeselected?.Invoke();
    }

    public override void Initialise()
    {
        SetToNormalScale();
    }

    public override void UpdateNodeVisualState()
    {
        // do nothing
    }

    private void SetToNormalScale()
    {
        this.transform.localScale = m_NormalScale;
    }

    private void SetToMinScale()
    {
        this.transform.localScale = m_MinScale;
    }

    public void ToggleCurrLevel(bool isCurrent)
    {
        if (m_IsCurrent == isCurrent)
            return;

        if (m_CurrLevelCoroutine != null)
        {
            StopCoroutine(m_CurrLevelCoroutine);
            m_CurrLevelCoroutine = null;
            SetToNormalScale();
        }

        if (isCurrent)
        {
            m_CurrLevelCoroutine = StartCoroutine(CurrLevel_Coroutine());
        }

        m_IsCurrent = isCurrent;
    }

    private IEnumerator CurrLevel_Coroutine()
    {
        float t = 0f;
        bool shrink = true;

        while (true)
        {
            if (t >= HALF_CYCLE_TIME)
            {
                t = 0f;
                if (shrink)
                    SetToMinScale();
                else
                    SetToNormalScale();
                shrink = !shrink;
            }
            else
            {
                yield return null;
                t += Time.deltaTime;
                float finalLerpValue = shrink ? 1 - (t / HALF_CYCLE_TIME) : t / HALF_CYCLE_TIME;
                float x = Mathf.Lerp(m_MinScale.x, m_NormalScale.x, finalLerpValue);
                float y = Mathf.Lerp(m_MinScale.y, m_NormalScale.y, finalLerpValue);
                float z = Mathf.Lerp(m_MinScale.z, m_NormalScale.z, finalLerpValue);
                this.transform.localScale = new Vector3(x, y, z);
            }
        }
    }

    
}
