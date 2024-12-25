using UnityEngine;
using UnityEngine.Serialization;

namespace Level.Tokens
{
    public class EmptyLevelNodeTokenAnimator : LevelNodeTokenAnimator
    {
        private LevelNodeVisual m_LevelNodeVisual;
        
        public float nodeEntryTime = 0.15f;
        
        public void Initialise(LevelNodeVisual levelNodeVisual)
        {
            m_LevelNodeVisual = levelNodeVisual;
        }

        public override void ShowToken()
        {
        }

        public override void HideToken()
        {
        }

        public override void PlayEntryAnimation(PlayerToken playerToken, VoidEvent onComplete)
        {
            Vector3 destPos = m_LevelNodeVisual.GetPlayerTargetPosition();
            Quaternion finalRotation = playerToken.transform.rotation;
            playerToken.MoveToPosition(destPos, finalRotation, onComplete, nodeEntryTime);
        }
    }
}