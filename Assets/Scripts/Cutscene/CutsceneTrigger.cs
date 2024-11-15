using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;

public enum CutsceneTriggerEnum
{
    PRE_1_EXAMPLE,
    POST_1_HELLO,
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
    [Header("Editor")]
    [Tooltip("Is this for the pre or post cutscene")]
    [SerializeField] private bool m_IsPre;
    [SerializeField] private int m_LevelNum = 1;

    public bool IsPre => m_IsPre;
    public int LevelNum => m_LevelNum;
    
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

    private void OnEnable()
    {
        m_TriggerBase = (TriggerBase) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        bool isPre = m_TriggerBase.IsPre;
        int levelNum = m_TriggerBase.LevelNum;

        IEnumerable<CutsceneTriggerEnum> cutsceneTriggerEnums = Enum.GetValues(typeof(CutsceneTriggerEnum)).OfType<CutsceneTriggerEnum>().Where(x => Filter(x));
        string[] options = cutsceneTriggerEnums.Select(x => x.ToString()).ToArray();

        if (cutsceneTriggerEnums.Count() <= 0)
        {
            GUILayout.Label("No available triggers");
        }
        else
        {
            int index = EditorGUILayout.Popup(0, options);

            if (GUILayout.Button("Set cutscene trigger"))
            {
                m_TriggerBase.SetCutsceneTrigger(cutsceneTriggerEnums.ElementAt(index));
            }
        }

        bool Filter(CutsceneTriggerEnum cutsceneTriggerEnum)
        {
            string[] components = cutsceneTriggerEnum.ToString().Split("_");
            if (components.Count() < 2)
                return false;

            if (!components[0].ToUpper().Equals(isPre ? "PRE" : "POST"))
                return false;

            if (!components[1].Equals(levelNum.ToString()))
                return false;

            return true;
        }
    }   
}
#endif
