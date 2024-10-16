using Game;
using Game.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceUnitDisplay : MonoBehaviour
{
    #region Asset References
    [SerializeField]
    private ProgressBar hpBar;
    [SerializeField]
    private ProgressBar mpBar;
    #endregion

    [SerializeField]
    private Vector3 worldOffset;

    private Unit TrackedUnit
    {
        get => trackedUnit;
        set
        {
            if (trackedUnit == value) return;

            if (trackedUnit != null)
            {
                trackedUnit.OnDeath -= OnDeath;
            }

            trackedUnit = value;
            if (trackedUnit != null)
            {
                trackedUnit.OnDeath += OnDeath;
                trackedUnit.OnHealthChange += OnHealthChange;
                trackedUnit.OnManaChange += OnManaChange;

                if (hpBar != null)
                {
                    hpBar.SetValue(trackedUnit.CurrentHealth, trackedUnit.MaxHealth, 0);
                }

                if (mpBar != null)
                {
                    var maxMana = trackedUnit.MaxMana;
                    if (maxMana == 0)
                    {
                        mpBar.gameObject.SetActive(false);
                    }
                    else
                    {
                        mpBar.gameObject.SetActive(true);
                        mpBar.SetValue(trackedUnit.CurrentMana, maxMana, 0);
                    }
                }

                BeginFollow();
            }
        }
    }
    private Unit trackedUnit;
    private void OnHealthChange(float change, float value, float max) { hpBar?.SetValue(value, max); }
    private void OnManaChange(float change, float value, float max) { mpBar?.SetValue(value, max); }

    private Animator animator;
    private CanvasGroup canvasGroup;

    private bool isHidden;

    private Coroutine followCoroutine;

    private event Action onAnimationFinishEvent;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        isHidden = true;

        GlobalEvents.Scene.BattleSceneLoadedEvent += OnSceneLoad;
        GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
        GlobalEvents.Battle.CompleteAttackAnimationEvent += OnCompleteAttackAnimation;
    }

    private void OnAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target)
    {
        Hide();
    }

    private void OnCompleteAttackAnimation()
    {
        Show();
    }

    private void OnSceneLoad()
    {
        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance _, int _2)
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
        GlobalEvents.Battle.CompleteAttackAnimationEvent -= OnCompleteAttackAnimation;

        TrackedUnit = null;
        Hide();
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.BattleSceneLoadedEvent -= OnSceneLoad;
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
        GlobalEvents.Battle.CompleteAttackAnimationEvent -= OnCompleteAttackAnimation;
    }

    private void Start()
    {
        var parentUnit = transform.parent.GetComponent<Unit>();
        if (parentUnit == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(parentUnit.transform.parent);
        TrackedUnit = parentUnit;
        Show();
    }

    private void BeginFollow()
    {
        if (trackedUnit == null || !trackedUnit) return;

        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
        followCoroutine = StartCoroutine(FollowPosition(trackedUnit.transform));
    }

    private IEnumerator FollowPosition(Transform t)
    {
        while (t && t != null)
        {
            //transform.localPosition = WorldHUDManager.Instance.WorldToHUDSpace(t.position + worldOffset);
            transform.position = t.position + worldOffset;
            var rot = CameraManager.Instance.MainCamera.transform.rotation;
            rot.x = 0;
            rot.z = 0;
            transform.rotation = rot;
            yield return null;
        }

        // if this is reached, the tracked transform was destroyed
        trackedUnit = null;
        Hide();
    }

    private void OnDeath()
    {
        void Dispose()
        {
            onAnimationFinishEvent -= Dispose;

            Destroy(gameObject);
        }
        onAnimationFinishEvent += Dispose;
        TrackedUnit = null;
        Hide();
    }

    public void Show()
    {
        if (!isHidden) return;

        isHidden = false;
        animator.enabled = true;
        animator.Play(UIConstants.ShowAnimHash);
    }

    public void Hide()
    {
        if (isHidden) return;

        isHidden = true;
        animator.enabled = true;
        animator.CrossFade(UIConstants.HideAnimHash, 0.1f);
    }

    private void OnAnimationFinish()
    {
        animator.enabled = !isHidden;
        canvasGroup.interactable = !isHidden;
        canvasGroup.blocksRaycasts = !isHidden;

        onAnimationFinishEvent?.Invoke();
    }
}
