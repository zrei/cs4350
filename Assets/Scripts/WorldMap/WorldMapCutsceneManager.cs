using System.Collections;
using UnityEngine;

public class WorldMapCutsceneManager : MonoBehaviour
{    
    [SerializeField] CutsceneSpawner m_TestSpawner;
    
    private CutsceneSpawner m_CurrCutscene = null;
    private int m_InitialCullingMask = -1;

    private void Start()
    {
        StartCoroutine(StartingCutscene());
    }

    private IEnumerator StartingCutscene()
    {
        yield return null;
        ShowCutscene(m_TestSpawner, null);
    }

    public void ShowCutscene(CutsceneSpawner cutscene, VoidEvent postCutscene)
    {
        m_CurrCutscene = cutscene;
        m_CurrCutscene.BeginCutscene(() => PostCutscene(postCutscene));

        m_InitialCullingMask = Camera.main.cullingMask;
        Camera.main.cullingMask = ~LayerMask.GetMask("WorldMap");
    }

    private void PostCutscene(VoidEvent additionalCallback)
    {
        Camera.main.cullingMask = m_InitialCullingMask;

        additionalCallback?.Invoke();
    }
}
