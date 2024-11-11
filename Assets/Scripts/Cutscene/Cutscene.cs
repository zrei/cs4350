using Cinemachine;
using UnityEngine;

public class Cutscene : MonoBehaviour
{
    public GameObject m_CutsceneObj;
    public Transform m_CutsceneTransform;
    public Transform m_VCamTransform;
    public Dialogue m_Dialogue;

    private GameObject m_CutsceneInstance = null;

    public void InstantiateCutscene(CinemachineVirtualCamera virtualCamera)
    {
        m_CutsceneInstance = Instantiate(m_CutsceneObj, m_CutsceneTransform.position, m_CutsceneTransform.rotation, m_CutsceneTransform);
        virtualCamera.transform.position = m_VCamTransform.position;
        virtualCamera.transform.rotation = m_VCamTransform.rotation;
    }

    public void EndCutscene()
    {
        Destroy(m_CutsceneInstance);
        m_CutsceneInstance = null;
    }
}
