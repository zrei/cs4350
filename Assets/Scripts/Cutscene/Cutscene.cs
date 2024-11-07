using UnityEngine;

public class Cutscene : MonoBehaviour
{
    [SerializeField] private GameObject m_CutsceneObj;
    [SerializeField] private Transform m_CutsceneTransform;
    public Dialogue m_Dialogue;

    private GameObject m_CutsceneInstance = null;

    public void InstantiateCutscene()
    {
        m_CutsceneInstance = Instantiate(m_CutsceneObj, m_CutsceneTransform.position, m_CutsceneTransform.rotation, m_CutsceneTransform);
    }

    public void EndCutscene()
    {
        Destroy(m_CutsceneInstance);
        m_CutsceneInstance = null;
    }
}
