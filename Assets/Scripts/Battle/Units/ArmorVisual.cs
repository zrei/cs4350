using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorVisual : MonoBehaviour
{
    #region Animation
    public static readonly int DirXAnimParam = Animator.StringToHash("DirX");
    public static readonly int DirYAnimParam = Animator.StringToHash("DirY");
    private static readonly int IsMoveAnimParam = Animator.StringToHash("IsMove");

    private static readonly int SkillStartAnimParam = Animator.StringToHash("SkillStart");
    private static readonly int SkillExecuteAnimParam = Animator.StringToHash("SkillExecute");
    private static readonly int SkillCancelAnimParam = Animator.StringToHash("SkillCancel");
    private static readonly int SkillIDAnimParam = Animator.StringToHash("SkillID");

    private static readonly int PoseIDAnimParam = Animator.StringToHash("PoseID");

    public static readonly int HurtAnimParam = Animator.StringToHash("Hurt");
    private static readonly int DeathAnimParam = Animator.StringToHash("IsDead");

    private Animator m_Animator;
    private bool m_IsSkillAnimStarted;
    private AnimationEventHandler m_AnimationEventHandler;
    public AnimationEventHandler AnimationEventHandler => m_AnimationEventHandler;
    #endregion

    #region Model
    private GameObject m_Model;
    private List<WeaponModel> m_WeaponModels = new();
    public List<WeaponModel> WeaponModels => m_WeaponModels;
    #endregion

    private MeshFader m_MeshFader;

    /// <summary>
    /// Helps to instantiate the correct base model, and equip it with the class' equipment + weapons
    /// </summary>
    /// <param name="unitModelData"></param>
    /// <param name="weaponSO"></param>
    public void InstantiateModel(UnitModelData unitModelData, WeaponInstanceSO weaponSO, ClassSO classSO)
    {
        m_Model = Instantiate(unitModelData.m_Model, Vector3.zero, Quaternion.identity, transform);
        EquippingArmor equipArmor = m_Model.GetComponent<EquippingArmor>();
        equipArmor.Initialize(unitModelData.m_AttachItems);

        ChangeArmorMaterial(classSO.m_ArmorPlate, classSO.m_ArmorTrim, classSO.m_UnderArmor);

        foreach (var weaponModelPrefab in weaponSO.m_WeaponModels)
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

        m_Animator = m_Model.GetComponentInChildren<Animator>();
        if (m_Animator == null)
        {
            Logger.Log(this.GetType().Name, this.name, "No animator found!", this.gameObject, LogLevel.WARNING);
        }

        m_AnimationEventHandler = m_Animator.GetComponent<AnimationEventHandler>();
        m_Animator.SetInteger(PoseIDAnimParam, (int)classSO.WeaponAnimationType);
            
        m_MeshFader = gameObject.AddComponent<MeshFader>();
        m_MeshFader.SetRenderers(GetComponentsInChildren<Renderer>());
    }

    private void ChangeArmorMaterial(Color armorPlate, Color armorTrim, Color underArmor)
    {
        SkinnedMeshRenderer[] armorPieces = m_Model.GetComponentsInChildren<SkinnedMeshRenderer>();
        for (int i = 0; i < armorPieces.Length; i++) {
            Material[] newArmorMats = armorPieces[i].materials;
            for (int j = 0; j < newArmorMats.Length; j++) {
                if (newArmorMats[j].name == "ArmorPlate (Instance)") {
                    newArmorMats[j].color = armorPlate;
                } else if (newArmorMats[j].name == "ArmorTrim (Instance)") {
                    newArmorMats[j].color = armorTrim;
                } else if (newArmorMats[j].name == "UnderArmor (Instance)") {
                    newArmorMats[j].color = underArmor;
                }
            }
            armorPieces[i].materials = newArmorMats;
        }
    }

    #region Movement
    public void SetMoveAnimator(bool isMoving)
    {
        m_Animator.SetBool(IsMoveAnimParam, isMoving);
    }

    public void SetDirAnim(int directionAnimId, float value)
    {
        m_Animator.SetFloat(directionAnimId, value);
    }

    // for testing purposes
    public float GetDirAnim(int directionAnimId)
    {
        return m_Animator.GetFloat(directionAnimId);
    }
    #endregion

    #region Death Animations
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
            FadeMesh(0, 0.5f);
            yield return new WaitForSeconds(1f);
            onComplete?.Invoke();
        }
        StartCoroutine(DeathCoroutine());
    }

    /// <summary>
    /// Defeat animation without fade away
    /// </summary>
    /// <param name="onComplete"></param>
    public void Defeat(VoidEvent onComplete)
    {
        m_Animator.SetBool(DeathAnimParam, true);
        IEnumerator DefeatCoroutine()
        {
            yield return new WaitForSeconds(2f);
            onComplete?.Invoke();
        }
        StartCoroutine(DefeatCoroutine());
    }
    #endregion

    #region Attack Animations
    public void PlayAnimations(int triggerID)
    {
        m_Animator.SetTrigger(triggerID);
    }

    public void PlaySkillStartAnimation(int skillID)
    {
        m_Animator.SetInteger(SkillIDAnimParam, skillID);
        m_Animator.ResetTrigger(SkillCancelAnimParam);
        m_Animator.ResetTrigger(SkillExecuteAnimParam);
        m_Animator.SetTrigger(SkillStartAnimParam);
        m_IsSkillAnimStarted = true;

        m_WeaponModels.ForEach(x => x.PlaySkillStartAnimation(skillID));
    }

    public void PlaySkillExecuteAnimation()
    {
        if (!m_IsSkillAnimStarted) return;

        m_Animator.SetTrigger(SkillExecuteAnimParam);
        m_IsSkillAnimStarted = false;
        m_Animator.SetInteger(SkillIDAnimParam, 0);

        m_WeaponModels.ForEach(x => x.PlaySkillExecuteAnimation());
    }

    public void CancelSkillAnimation()
    {
        if (!m_IsSkillAnimStarted) return;

        m_Animator.SetTrigger(SkillCancelAnimParam);
        m_IsSkillAnimStarted = false;
        m_Animator.SetInteger(SkillIDAnimParam, 0);

        m_WeaponModels.ForEach(x => x.CancelSkillAnimation());
    }
    #endregion

    #region Fading
    public void FadeMesh(float targetOpacity, float duration)
    {
        m_MeshFader.Fade(targetOpacity, duration);
    }
    #endregion
}