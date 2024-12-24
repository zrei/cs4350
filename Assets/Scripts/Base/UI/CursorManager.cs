using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [SerializeField]
    private Canvas m_Canvas;

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

        if (!Application.isEditor)
        {
            Cursor.lockState = CursorLockMode.Confined;
        }

        Vector2 mousePos;

        // Convert screen point to local point in canvas space
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_Canvas.transform as RectTransform,
            Input.mousePosition,
            m_Canvas.worldCamera,
            out mousePos
        );

        m_RectT.localPosition = mousePos;

        if (Input.GetMouseButtonDown(0))
        {
            m_Particles.Play();
        }
    }
}
