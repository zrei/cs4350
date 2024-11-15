using Game.UI;
using UnityEngine;

public class CutsceneSpawner : MonoBehaviour
{
    [SerializeField] private Cutscene m_StartingCutscene;
    [SerializeField] private Dialogue m_Dialogue;

    private Cutscene m_CurrCutsceneInstance = null;

    private VoidEvent m_PostCutsceneAction = null;
    public void BeginCutscene(VoidEvent postCutsceneAction = null)
    {
        m_PostCutsceneAction = postCutsceneAction;
        GlobalEvents.Dialogue.DialogueEndEvent += EndCutscene;
        GlobalEvents.CutsceneEvents.StartCutsceneEvent += SwitchToCutscene;
        CreateCutscene(m_StartingCutscene);
        DialogueDisplay.Instance.StartDialogue(m_Dialogue);
    }

    private void EndCutscene()
    {
        GlobalEvents.Dialogue.DialogueEndEvent -= EndCutscene;
        GlobalEvents.CutsceneEvents.StartCutsceneEvent -= SwitchToCutscene;
        DestroyCurrCutscene();
        m_PostCutsceneAction?.Invoke();
        m_PostCutsceneAction = null;
    }

    public void SwitchToCutscene(Cutscene cutscene)
    {
        GameSceneManager.Instance.PlayTransition(MidTransictionAction, null);

        void MidTransictionAction()
        {
            DestroyCurrCutscene();
            CreateCutscene(cutscene);
        }
    }

    private void CreateCutscene(Cutscene cutscene)
    {
        m_CurrCutsceneInstance = Instantiate(cutscene, this.transform);
    }

    private void DestroyCurrCutscene()
    {
        Destroy(m_CurrCutsceneInstance.gameObject);
        m_CurrCutsceneInstance = null;
    }
}
