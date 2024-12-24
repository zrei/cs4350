using UnityEngine;

public enum NodePuckType
{
    CURRENT,
    CLEARED,
    BATTLE,
    REWARD,
    DIALOGUE,
}

// ScriptableObject that holds the materials for the different node states
[CreateAssetMenu(fileName="NodeColorSO", menuName="ScriptableObject/Level/NodeColorSO")]
public class NodeColorSO : ScriptableObject
{
    public Material m_DefaultMaterial;
    public Material m_CurrentMaterial;
    public Material m_ClearedMaterial;
    public Material m_EnemyMaterial;
    public Material m_RewardMaterial;
    public Material m_DialogueMaterial;
    
    public Material GetMaterial(NodePuckType puckType)
    {
        switch (puckType)
        {
            case NodePuckType.CURRENT:
                return m_CurrentMaterial;
            case NodePuckType.CLEARED:
                return m_ClearedMaterial;
            case NodePuckType.BATTLE:
                return m_EnemyMaterial;
            case NodePuckType.REWARD:
                return m_RewardMaterial;
            case NodePuckType.DIALOGUE:
                return m_DialogueMaterial;
            default:
                return m_DefaultMaterial;
        }
    }
}
