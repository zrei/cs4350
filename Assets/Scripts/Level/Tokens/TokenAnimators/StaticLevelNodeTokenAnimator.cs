using UnityEngine;
using UnityEngine.Serialization;

namespace Level.Tokens
{
    public class StaticLevelNodeTokenAnimator : LevelNodeTokenAnimator
    {
        private GameObject m_StaticObjectToken;
        private LevelNodeVisual m_LevelNodeVisual;
        
        public float nodeEntryTime = 0.15f;
        
        public void Initialise(GameObject tokenPrefab, LevelNodeVisual levelNodeVisual)
        {
            m_LevelNodeVisual = levelNodeVisual;
            m_StaticObjectToken = Instantiate(tokenPrefab, transform, false);
        }

        public override void ShowToken()
        {
            m_StaticObjectToken.SetActive(true);
        }

        public override void HideToken()
        {
            m_StaticObjectToken.SetActive(false);
        }

        public override void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            Vector3 destPos = m_LevelNodeVisual.GetPlayerTargetPosition();
            Quaternion finalRotation = playerToken.transform.rotation;
            playerToken.MoveToPosition(destPos, finalRotation, onComplete, nodeEntryTime);
            HideToken();
        }
    }
}