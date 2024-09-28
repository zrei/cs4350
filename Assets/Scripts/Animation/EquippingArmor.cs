using System.Collections.Generic;
using UnityEngine;

public class EquippingArmor : MonoBehaviour
{
    [Header("PlayerBones")]
    public List<Transform> m_UnitBonesArray;
    [SerializeField] Transform m_RootBone;
    
    private Dictionary<string, Transform> m_PlayerBonesDict;

    [Header("AttachItem")]
    public SkinnedMeshRenderer[] itemMeshes;

    

    public void Initialize(SkinnedMeshRenderer[] itemMeshes)
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        InitializeBoneDictionary(); // Build the bone dictionary

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
    }
}
