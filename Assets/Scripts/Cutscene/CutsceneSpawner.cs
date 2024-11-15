using Game.UI;
using UnityEngine;

public class CutsceneSpawner : MonoBehaviour
{
    [SerializeField] private GameObject m_StartingCutscene;
    [SerializeField] private Dialogue m_Dialogue;

    private GameObject m_CurrCutsceneInstance = null;

    private VoidEvent m_PostCutsceneAction = null;
    public void BeginCutscene(VoidEvent postCutsceneAction = null)
    {
        m_PostCutsceneAction = postCutsceneAction;
        GlobalEvents.Dialogue.DialogueEndEvent += EndCutscene;
        CreateCutscene(m_StartingCutscene);
        DialogueDisplay.Instance.StartDialogue(m_Dialogue);
    }

    private void EndCutscene()
    {
        GlobalEvents.Dialogue.DialogueEndEvent -= EndCutscene;
        DestroyCurrCutscene();
        GlobalEvents.CutsceneEvents.EndCutsceneEvent?.Invoke();
        m_PostCutsceneAction?.Invoke();
        m_PostCutsceneAction = null;
    }

    public void SwitchToCutscene(GameObject cutscene)
    {
        GameSceneManager.Instance.PlayTransition(MidTransictionAction, null);

        void MidTransictionAction()
        {
            DestroyCurrCutscene();
            CreateCutscene(cutscene);
        }
    }

    private void CreateCutscene(GameObject cutscene)
    {
        m_CurrCutsceneInstance = Instantiate(cutscene, this.transform);
    }

    private void DestroyCurrCutscene()
    {
        Destroy(m_CurrCutsceneInstance);
        m_CurrCutsceneInstance = null;
    }
}
