using UnityEngine;

[CreateAssetMenu(fileName = "EnemyClassSO", menuName = "ScriptableObject/Battle/Enemy/EnemyClassSO")]
public class EnemyClassSO : ClassSO
{
    [Header("Enemy Actions")]
    public BehaviourTree m_EnemyBehaviourTree;
}
