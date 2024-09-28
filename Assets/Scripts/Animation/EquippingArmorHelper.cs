
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(EquippingArmorHelper))]
public class EquippingArmorHelperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Populate bones array"))
        {
            EquippingArmorHelper helper = (EquippingArmorHelper) target;
            helper.PopulateBones();
            Logger.LogEditor(this.GetType().Name, "Successfully populated bones for " + target.name, LogLevel.LOG);
        }
    }
}

[RequireComponent(typeof(EquippingArmor))]
public class EquippingArmorHelper : MonoBehaviour
{
    [Header("Helper - for attaching bones")]
    [SerializeField] Transform m_BonesParent;

    private EquippingArmor component;

    public void PopulateBones()
    {
        component = GetComponent<EquippingArmor>();

        if (m_BonesParent == null)
        {
            Logger.LogEditor(this.GetType().Name, "No bone parent set!", LogLevel.ERROR);
        }
        List<Transform> transformArray = component.m_UnitBonesArray;
        transformArray.Clear();
        LookIntoChildren(transformArray, m_BonesParent, false);
    }

    private void LookIntoChildren(List<Transform> transformArray, Transform parent, bool include = true)
    {
        if (include)
        {
            transformArray.Add(parent);
        }

        foreach (Transform child in parent)
        {
            LookIntoChildren(transformArray, child);
        }
    }
}
#endif
