using System.Collections;
using UnityEngine;

namespace Level.Tokens
{
    public class BattleNodeTokenAnimator : LevelNodeTokenAnimator
    {
        [Header("Token")]
        [SerializeField] private EnemyToken m_EnemyCharacterTokenPrefab;
        
        [Header("Token Animation")]
        [SerializeField] Transform m_EnemyTokenTransform;
        [SerializeField] Transform m_PlayerTokenTransform;
    
        public float entryAnimTime = 0.3f;
        public float clearAnimTime = 0.3f;
        public float skipAnimTime = 0.1f;
        
        private LevelNodeVisual m_LevelNodeVisual;
        private EnemyToken m_EnemyCharacterToken;
        
        public void Initialise(EnemyCharacterSO enemyCharacterData, LevelNodeVisual levelNodeVisual)
        {
            m_EnemyCharacterToken = Instantiate(m_EnemyCharacterTokenPrefab, transform, true);
            m_EnemyCharacterToken.Initialise(enemyCharacterData);
            m_EnemyCharacterToken.SetPositionToNode(levelNodeVisual);
            m_LevelNodeVisual = levelNodeVisual;
        }

        public override void ShowToken()
        {
            m_EnemyCharacterToken.gameObject.SetActive(true);
        }

        public override void HideToken()
        {
            m_EnemyCharacterToken.gameObject.SetActive(false);
        }
        
        public override void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            playerToken.MoveToPosition(m_PlayerTokenTransform.position, 
                m_PlayerTokenTransform.rotation, null, entryAnimTime);
            m_EnemyCharacterToken.MoveToPosition(m_EnemyTokenTransform.position, 
                m_EnemyTokenTransform.rotation, onComplete, entryAnimTime);
        }
    
        public override void PlayClearAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            m_EnemyCharacterToken.Die(OnEnemyDeathComplete);
            return;
        
            void OnEnemyDeathComplete()
            {
                m_EnemyCharacterToken.gameObject.SetActive(false);
                playerToken.MoveToPosition(m_LevelNodeVisual.GetPlayerTargetPosition(), onComplete, clearAnimTime);
            }
        }
    
        public override void PlayFailureAnimation(PlayerToken playerToken, VoidEvent onComplete, bool resetOnComplete)
        {
            playerToken.Defeat(onComplete, resetOnComplete);
        }
        
        public void PlayBattleSkipAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            var enemyTokenPos = m_EnemyCharacterToken.transform.position;
            var enemyTargetPos = enemyTokenPos + (enemyTokenPos - m_PlayerTokenTransform.position).normalized * 0.01f;
            m_EnemyCharacterToken.MoveToPosition(enemyTargetPos, OnEnemyMovementComplete, skipAnimTime);
            return;
            
            void OnEnemyMovementComplete()
            {
                m_EnemyCharacterToken.FadeAway(OnEnemyFadeAwayComplete);
            }
        
            void OnEnemyFadeAwayComplete()
            {
                m_EnemyCharacterToken.gameObject.SetActive(false);
                playerToken.MoveToPosition(m_LevelNodeVisual.GetPlayerTargetPosition(), onComplete, clearAnimTime);
            }
        }
    }
}