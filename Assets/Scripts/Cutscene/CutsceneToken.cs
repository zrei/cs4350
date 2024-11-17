using UnityEditor;
using UnityEngine;

public abstract class CutsceneToken : BaseCharacterToken
{
    [SerializeField] protected bool m_SpawnWeapon;

    private void Start()
    {
        Initialise();
    }

    protected virtual void Initialise()
    {
        if (transform.childCount > 0)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(0).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }

#if UNITY_EDITOR
    public void SceneInitialise()
    {
        Initialise();
    }
#endif
}


#if UNITY_EDITOR
[CustomEditor(typeof(CutsceneToken), true)]
public class CutsceneTokenEditor : Editor
{
    CutsceneToken cutsceneToken;

    void OnEnable()
    {
        cutsceneToken = (CutsceneToken) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(30f);

        if (GUILayout.Button("Spawn Visual"))
            cutsceneToken.SceneInitialise();

        GUILayout.Space(10f);

        GUILayout.Label("Feel free to leave the model there, it'll be cleared at runtime");
    }
}
#endif
