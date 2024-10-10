using System.Collections.Generic;
using UnityEngine;

public class EquippingArmor : MonoBehaviour
{
    [Header("Bones")]
    public List<Transform> m_UnitBonesArray;
    [SerializeField] Transform m_RootBone;
    [SerializeField] Transform m_RightArmBone;
    [SerializeField] Transform m_LeftArmBone;

    public Transform RightArmBone => m_RightArmBone;
    public Transform LeftArmBone => m_LeftArmBone;
    
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
}
