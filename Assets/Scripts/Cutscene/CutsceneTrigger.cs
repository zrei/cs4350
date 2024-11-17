using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

public enum CutsceneTriggerEnum
{
    // follow the format: {PRE/POST}_LEVELNUM_NAME
    PRE_1_START,
    PRE_1_ATTACK
}

public class CutsceneTrigger : TriggerBase
{
    public void Trigger()
    {
        GlobalEvents.CutsceneEvents.CutsceneTriggerEvent?.Invoke(m_CutsceneTrigger);
    }
}

public abstract class TriggerBase : MonoBehaviour
{
    [SerializeField] protected CutsceneTriggerEnum m_CutsceneTrigger;

#if UNITY_EDITOR
    public void SetCutsceneTrigger(CutsceneTriggerEnum cutsceneTrigger)
    {
        m_CutsceneTrigger = cutsceneTrigger;
        EditorUtility.SetDirty(this.gameObject);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(TriggerBase), true)]
public class TriggerBaseEditor : Editor
{
    private TriggerBase m_TriggerBase;
    private bool m_PreCutsceneTrigger = true;
    private int m_LevelNum = 1;
    private int m_CutsceneTriggerIndex = 0;

    private void OnEnable()
    {
        m_TriggerBase = (TriggerBase) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(30);
        GUILayout.Label("Editor");
        m_PreCutsceneTrigger = EditorGUILayout.Toggle("Pre-Cutscene Trigger", m_PreCutsceneTrigger);
        m_LevelNum = EditorGUILayout.IntField("Level number", m_LevelNum);

        IEnumerable<CutsceneTriggerEnum> cutsceneTriggerEnums = Enum.GetValues(typeof(CutsceneTriggerEnum)).OfType<CutsceneTriggerEnum>().Where(x => Filter(x));
        string[] options = cutsceneTriggerEnums.Select(x => x.ToString()).ToArray();

        if (cutsceneTriggerEnums.Count() <= 0)
        {
            GUILayout.Label("No available triggers");
        }
        else
        {
            m_CutsceneTriggerIndex = EditorGUILayout.Popup(m_CutsceneTriggerIndex, options);

            if (GUILayout.Button("Set cutscene trigger"))
            {
                m_TriggerBase.SetCutsceneTrigger(cutsceneTriggerEnums.ElementAt(m_CutsceneTriggerIndex));
            }
        }

        bool Filter(CutsceneTriggerEnum cutsceneTriggerEnum)
        {
            string[] components = cutsceneTriggerEnum.ToString().Split("_");
            if (components.Count() < 2)
                return false;

            if (!components[0].ToUpper().Equals(m_PreCutsceneTrigger ? "PRE" : "POST"))
                return false;

            if (!components[1].Equals(m_LevelNum.ToString()))
                return false;

            return true;
        }
    }   
}
#endif
