using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquippingArmor : MonoBehaviour
{
    [Header("PlayerBones")]
    public Transform[] playerBonesArray;
    public Transform rootBone;
    public Dictionary<string, Transform> playerBonesDict;

    [Header("AttachItem")]
    public SkinnedMeshRenderer[] itemMeshes;

    // Start is called before the first frame update
    void Start()
    {
        InitializeBoneDictionary(); // Build the bone dictionary

        foreach (SkinnedMeshRenderer itemMesh in itemMeshes) {
            AttachItemToPlayer(itemMesh); // Attach item to Player
        }
    }

    public void InitializeBoneDictionary() {
        // build bones dictionary
        playerBonesDict = new Dictionary<string, Transform>();

        foreach (Transform bone in playerBonesArray) {
            playerBonesDict.Add(bone.name, bone);
        }
    }

    public void AttachItemToPlayer(SkinnedMeshRenderer itemMesh) {
        SkinnedMeshRenderer newMesh = Instantiate<SkinnedMeshRenderer>(itemMesh);

        Transform[] newBones = new Transform[playerBonesArray.Length];

        for (int i = 0; i < itemMesh.bones.Length; i++) {
            if (playerBonesDict.ContainsKey(itemMesh.bones[i].name)) {
                newBones[i] = playerBonesDict[itemMesh.bones[i].name];
            } else {
                Debug.LogError("Player bones dictionary does not contain bone: " + itemMesh.bones[i].name);
            }
        }

        newMesh.bones = newBones;
        newMesh.rootBone = rootBone;
        newMesh.transform.SetParent(rootBone.parent);
    }
}
