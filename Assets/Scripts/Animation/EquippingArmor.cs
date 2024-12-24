using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EquippingArmor : MonoBehaviour
{
    [Header("Bones")]
    [SerializeField] private List<Transform> m_UnitBonesArray;
    [SerializeField] private Transform m_BonesParent;
    [SerializeField] private Transform m_RootBone;
    [SerializeField] private Transform m_RightArmBone;
    [SerializeField] private Transform m_LeftArmBone;
    [SerializeField] private Transform m_BodyCenter;

    public Transform BonesParent => m_BonesParent; 
    public Transform RightArmBone => m_RightArmBone;
    public Transform LeftArmBone => m_LeftArmBone;
    public Transform BodyCenter => m_BodyCenter;

    private Dictionary<string, Transform> m_PlayerBonesDict;

    public void Initialize(SkinnedMeshRenderer[] itemMeshes)
    {
        InitializeBoneDictionary();
        foreach (SkinnedMeshRenderer itemMesh in itemMeshes) {
            AttachItemToPlayer(itemMesh); // Attach item to Player
        }
    }

    private void InitializeBoneDictionary() {
        // build bones dictionary
        m_PlayerBonesDict = new Dictionary<string, Transform>();

        foreach (Transform bone in m_UnitBonesArray) {
            m_PlayerBonesDict.Add(bone.name, bone);
        }
    }

    private void AttachItemToPlayer(SkinnedMeshRenderer itemMesh) {
        SkinnedMeshRenderer newMesh = Instantiate<SkinnedMeshRenderer>(itemMesh);

        Transform[] newBones = new Transform[m_UnitBonesArray.Count];

        for (int i = 0; i < itemMesh.bones.Length; i++) {
            if (m_PlayerBonesDict.ContainsKey(itemMesh.bones[i].name)) {
                newBones[i] = m_PlayerBonesDict[itemMesh.bones[i].name];
            } else {
                Debug.LogError("Player bones dictionary does not contain bone: " + itemMesh.bones[i].name);
            }
        }

        newMesh.bones = newBones;
        newMesh.rootBone = m_RootBone;
        newMesh.transform.SetParent(m_RootBone.parent);
        newMesh.gameObject.layer = LayerMask.NameToLayer("Objects");
    }

    #if UNITY_EDITOR
    public void PopulateBones()
    {
        if (m_BonesParent == null)
        {
            Logger.LogEditor(this.GetType().Name, "No bone parent set!", LogLevel.ERROR);
        }
        List<Transform> transformArray = m_UnitBonesArray;
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
    #endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(EquippingArmor))]
public class EquippingArmorEditor : Editor
{
    private EquippingArmor m_EquippingArmor;

    private void OnEnable()
    {
        m_EquippingArmor = (EquippingArmor) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10f);

        if (GUILayout.Button("Populate bones array"))
        {
            m_EquippingArmor.PopulateBones();
            Logger.LogEditor(this.GetType().Name, "Successfully populated bones for " + target.name, LogLevel.LOG);
        }
    }
}
#endif
