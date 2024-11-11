using Cinemachine;
using Game.UI;
using UnityEngine;

public class WorldMapCutsceneManager : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera m_CutsceneVCam;
    
    private Cutscene m_CurrCutscene = null;
    private VoidEvent m_PostCutsceneCallback = null;
    private int m_InitialCullingMask = -1;

    public void ShowCutscene(Cutscene cutscene, VoidEvent postCutscene)
    {
        m_CurrCutscene = cutscene;
        m_CurrCutscene.InstantiateCutscene(m_CutsceneVCam);

        m_InitialCullingMask = Camera.main.cullingMask;
        Camera.main.cullingMask = ~LayerMask.GetMask("WorldMap");

        m_CutsceneVCam.enabled = true;
        m_PostCutsceneCallback = postCutscene;

        GlobalEvents.Dialogue.DialogueEndEvent += EndCutscene;
        DialogueDisplay.Instance.StartDialogue(cutscene.m_Dialogue);
    }

    private void EndCutscene()
    {
        GlobalEvents.Dialogue.DialogueEndEvent -= EndCutscene;

        m_CutsceneVCam.enabled = false;
        Camera.main.cullingMask = m_InitialCullingMask;
        
        m_CurrCutscene.EndCutscene();
        m_CurrCutscene = null;

        m_PostCutsceneCallback?.Invoke();
        m_PostCutsceneCallback = null;
    }
}
