using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem m_Particles;

    RectTransform m_RectT;

    private void Start()
    {
        m_RectT = transform as RectTransform;
    }

    private void Update()
    {
        Cursor.visible = false;

        m_RectT.anchoredPosition = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {
            m_Particles.Play();
        }
    }
}
