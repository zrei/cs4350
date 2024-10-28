using System.Collections;
using UnityEngine;

namespace Level
{
    public abstract class LevelCharacterToken : BaseCharacterToken
    {
        public void SetPositionToNode(NodeVisual nodeVisual)
        {
            transform.position = nodeVisual.transform.position + GridYOffset + nodeVisual.TokenOffset;
        }

        public void MoveToPosition(Vector3 destPos, VoidEvent onCompleteMovement, float moveTime)
        {
            m_ArmorVisual.SetMoveAnimator(true);
            
            // Base rotation is facing opposite direction, so we need to negate the direction vector
            Quaternion targetRot = Quaternion.LookRotation(-(destPos - transform.position).normalized, Vector3.up);
            StartCoroutine(Rotate(targetRot, FinishRotation, ROTATION_TIME));
            
            return;

            void FinishRotation()
            {
                StartCoroutine(Move(destPos, FinishMovement, moveTime));
            }
            
            void FinishMovement()
            {
                m_ArmorVisual.SetMoveAnimator(false);
                onCompleteMovement?.Invoke();
            }
        }
        
        /// <summary>
        /// MoveToPosition but with a target rotation to face after moving.
        /// </summary>
        /// <param name="destPos"></param>
        /// <param name="targetRotation">Target rotation to face after moving</param>
        /// <param name="onCompleteMovement"></param>
        /// <param name="moveTime"></param>
        public void MoveToPosition(Vector3 destPos, Quaternion targetRotation, VoidEvent onCompleteMovement, float moveTime)
        {
            MoveToPosition(destPos, FinishMovement, moveTime);
            
            void FinishMovement()
            {
                m_ArmorVisual.SetMoveAnimator(false);
                StartCoroutine(Rotate(targetRotation, onCompleteMovement, ROTATION_TIME));
            }
        }
        
        private IEnumerator Move(Vector3 destPos, VoidEvent onCompleteMovement, float moveTime)
        {
            float time = 0f;
            Vector3 currPos = transform.position;
            
            // Always play forward animation
            m_ArmorVisual.SetDirAnim(ArmorVisual.DirXAnimParam, 0f);
            m_ArmorVisual.SetDirAnim(ArmorVisual.DirYAnimParam, 1f);
            
            while (time < moveTime)
            {
                time += Time.deltaTime;
                float l = time / moveTime;
                Vector3 newPos = Vector3.Lerp(currPos, destPos, l);
                
                transform.position = newPos;
                yield return null;
            }
            transform.position = destPos;
            
            onCompleteMovement?.Invoke();
        }
        
        /// <summary>
        /// Defeat animation with death (fade away)
        /// </summary>
        /// <param name="onComplete"></param>
        public void Die(VoidEvent onComplete)
        {
            m_ArmorVisual.Die(onComplete);
        }
        
        /// <summary>
        /// Defeat animation without fade away
        /// </summary>
        /// <param name="onComplete"></param>
        public void Defeat(VoidEvent onComplete)
        {
            m_ArmorVisual.Defeat(onComplete);
        }
    }
}