using System.Collections;
using UnityEngine;

public class WorldMapPathNode : MonoBehaviour
{
    [SerializeField] private Vector3 m_MaxSize = new Vector3(0.4f, 0.4f, 0.4f);
    
    public void SetToMaxSize()
    {
        this.transform.localScale = m_MaxSize;
    }

    public void Expand(float expandTime)
    {
        StartCoroutine(ExpandCoroutine(expandTime));
    }

    private IEnumerator ExpandCoroutine(float expandTime)
    {
        float t = 0f;

        while (t < expandTime)
        {
            yield return null;
            t += Time.deltaTime;
            float x = Mathf.Lerp(0f, m_MaxSize.x, t / expandTime);
            float y = Mathf.Lerp(0f, m_MaxSize.y, t / expandTime);
            float z = Mathf.Lerp(0f, m_MaxSize.z, t / expandTime);
            this.transform.localScale = new Vector3(x, y, z);
        }

        SetToMaxSize();
    }
}
