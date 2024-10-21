using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level
{
    public class CharacterToken : MonoBehaviour
    {
        #region Animation
        private static readonly int DirXAnimParam = Animator.StringToHash("DirX");
        private static readonly int DirYAnimParam = Animator.StringToHash("DirY");
        private static readonly int IsMoveAnimParam = Animator.StringToHash("IsMove");

        private static readonly int PoseIDAnimParam = Animator.StringToHash("PoseID");

        public static readonly int HurtAnimParam = Animator.StringToHash("Hurt");
        private static readonly int DeathAnimParam = Animator.StringToHash("IsDead");

        private Animator m_Animator;
        private bool m_IsSkillAnimStarted;
        private AnimationEventHandler m_AnimationEventHandler;

        public AnimationEventHandler AnimationEventHandler => m_AnimationEventHandler;
        #endregion
        
        #region Static Data
        public WeaponAnimationType WeaponAnimationType { get; private set; }

        protected ClassSO m_Class;
        public string ClassName => m_Class.m_ClassName;

        public Sprite Sprite { get; private set; }

        public Vector3 GridYOffset { get; private set; }

        private const float ROTATION_TIME = 0.2f;

        public virtual UnitAllegiance UnitAllegiance => UnitAllegiance.NONE;
        #endregion

        private GameObject m_TokenModel;
        private WeaponInstanceSO m_EquippedWeapon;
        private List<WeaponModel> m_WeaponModels = new();
        
        private MeshFader m_MeshFader;

        private LevelNodeVisual m_CurrentNode;

        public void Initialise(PlayerCharacterBattleData characterBattleData)
        {
            var classSo = characterBattleData.m_ClassSO;
            var unitModelData = characterBattleData.GetUnitModelData();
            var weaponInstanceSo = characterBattleData.m_CurrEquippedWeapon;
            
            InstantiateModel(unitModelData, weaponInstanceSo, classSo);
        }
        
        public void Initialise(EnemyCharacterSO enemyCharacterSO)
        {
            var classSo = enemyCharacterSO.m_EnemyClass;
            var unitModelData = enemyCharacterSO.GetUnitModelData();
            var weaponInstanceSo = enemyCharacterSO.m_EquippedWeapon;
            
            InstantiateModel(unitModelData, weaponInstanceSo, classSo);
        }


        /// <summary>
        /// Helps to instantiate the correct base model, and equip it with the class' equipment + weapons
        /// </summary>
        /// <param name="unitModelData"></param>
        /// <param name="weaponSO"></param>
        private void InstantiateModel(UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
        {
            m_TokenModel = Instantiate(unitModelData.m_Model, Vector3.zero, Quaternion.identity, transform);
            EquippingArmor equipArmor = m_TokenModel.GetComponent<EquippingArmor>();
            equipArmor.Initialize(unitModelData.m_AttachItems);

            m_EquippedWeapon = weaponSO;
            foreach (var weaponModelPrefab in m_EquippedWeapon.m_WeaponModels)
            {
                if (weaponModelPrefab != null)
                {
                    var weaponModel = Instantiate(weaponModelPrefab);
                    var attachPoint = weaponModel.attachmentType switch
                    {
                        WeaponModelAttachmentType.RIGHT_HAND => equipArmor.RightArmBone,
                        WeaponModelAttachmentType.LEFT_HAND => equipArmor.LeftArmBone,
                        _ => null,
                    };
                    weaponModel.transform.SetParent(attachPoint, false);
                    m_WeaponModels.Add(weaponModel);
                }
            }

            m_Animator = m_TokenModel.GetComponentInChildren<Animator>();
            if (m_Animator == null)
            {
                Logger.Log(this.GetType().Name, this.name, "No animator found!", this.gameObject, LogLevel.WARNING);
            }
            m_AnimationEventHandler = m_Animator.GetComponent<AnimationEventHandler>();

            WeaponAnimationType = classSO.WeaponAnimationType;
            m_Animator.SetInteger(PoseIDAnimParam, (int)WeaponAnimationType);

            GridYOffset = new Vector3(0f, unitModelData.m_GridYOffset, 0f);
            
            m_MeshFader = gameObject.AddComponent<MeshFader>();
            m_MeshFader.SetRenderers(GetComponentsInChildren<Renderer>());
        }
        
        public void SetPositionToNode(LevelNodeVisual nodeVisual)
        {
            transform.position = nodeVisual.transform.position + GridYOffset + LevelNodeVisual.TOKEN_OFFSET;
        }

        public void MoveToPosition(Vector3 destPos, VoidEvent onCompleteMovement, float moveTime)
        {
            m_Animator.SetBool(IsMoveAnimParam, true);
            
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
                m_Animator.SetBool(IsMoveAnimParam, false);
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
                m_Animator.SetBool(IsMoveAnimParam, false);
                StartCoroutine(Rotate(targetRotation, onCompleteMovement, ROTATION_TIME));
            }
        }
        
        private IEnumerator Move(Vector3 destPos, VoidEvent onCompleteMovement, float moveTime)
        {
            float time = 0f;
            Vector3 currPos = transform.position;
            
            // Always play forward animation
            m_Animator.SetFloat(DirXAnimParam, 0);
            m_Animator.SetFloat(DirYAnimParam, 1);
            
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
        
        private IEnumerator Rotate(Quaternion targetRot, VoidEvent onCompleteRotation, float rotateTime)
        {
            float time = 0f;
            Quaternion currRot = transform.rotation;
            
            while (time < rotateTime)
            {
                time += Time.deltaTime;
                float l = time / rotateTime;
                Quaternion newRot = Quaternion.Lerp(currRot, targetRot, l);

                transform.rotation = newRot;
                yield return null;
            }
            transform.rotation = targetRot;
            
            onCompleteRotation?.Invoke();
        }
        
        /// <summary>
        /// Defeat animation with death (fade away)
        /// </summary>
        /// <param name="onComplete"></param>
        public void Die(VoidEvent onComplete)
        {
            m_Animator.SetBool(DeathAnimParam, true);

            IEnumerator DeathCoroutine()
            {
                yield return new WaitForSeconds(1f);
                m_MeshFader.Fade(0, 0.5f);
                yield return new WaitForSeconds(1f);
                onComplete?.Invoke();
            }
            StartCoroutine(DeathCoroutine());
        }
        
        /// <summary>
        /// Defeat animation without death, revives afterwards
        /// </summary>
        /// <param name="onComplete"></param>
        public void Defeat(VoidEvent onComplete)
        {
            m_Animator.SetBool(DeathAnimParam, true);
            IEnumerator DefeatCoroutine()
            {
                yield return new WaitForSeconds(2f);
                m_Animator.SetBool(DeathAnimParam, false);
                yield return new WaitForSeconds(1f);
                onComplete?.Invoke();
            }
            StartCoroutine(DefeatCoroutine());
        }
    }
}