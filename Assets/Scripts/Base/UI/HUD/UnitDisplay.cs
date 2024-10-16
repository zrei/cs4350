using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class UnitDisplay : MonoBehaviour
    {
        [SerializeField]
        private bool isCurrentUnitDisplay = true;

        [SerializeField]
        private UnitAllegiance displayType = UnitAllegiance.PLAYER;

        #region Component References
        [SerializeField]
        private FormattedTextDisplay nameDisplay;

        [SerializeField]
        private Image characterArt;

        [SerializeField]
        private FormattedTextDisplay phyAtkDisplay;

        [SerializeField]
        private FormattedTextDisplay mgcAtkDisplay;

        [SerializeField]
        private FormattedTextDisplay phyDefDisplay;

        [SerializeField]
        private FormattedTextDisplay mgcDefDisplay;

        [SerializeField]
        private FormattedTextDisplay spdDisplay;

        [SerializeField]
        private ProgressBar hpBar;

        [SerializeField]
        private ProgressBar mpBar;
        #endregion

        private Animator animator;
        private bool isHidden;

        private Unit TrackedUnit
        {
            get => trackedUnit;
            set
            {
                if (trackedUnit == value) return;

                if (trackedUnit != null)
                {
                    trackedUnit.OnHealthChange -= OnHealthChange;
                    trackedUnit.OnManaChange -= OnManaChange;
                }

                trackedUnit = value;
                if (trackedUnit != null)
                {
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
                }
            }
        }
        private Unit trackedUnit;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            GetComponent<CanvasGroup>().alpha = 0;
            isHidden = true;

            GlobalEvents.Scene.BattleSceneLoadedEvent += OnSceneLoad;
        }

        private void OnSceneLoad()
        {
            if (isCurrentUnitDisplay)
            {
                GlobalEvents.Battle.PreviewCurrentUnitEvent += OnPreviewUnit;
            }
            else
            {
                GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
            }

            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
        }

        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

            Hide();
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.BattleSceneLoadedEvent -= OnSceneLoad;
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
        }

        private void OnPreviewUnit(Unit currentUnit)
        {
            if (currentUnit == null || currentUnit.UnitAllegiance != displayType)
            {
                if (!isHidden) Hide();
                return;
            }

            if (isHidden) Show();

            nameDisplay.SetValue(currentUnit.DisplayName);

            characterArt.sprite = currentUnit.Sprite;
            var color = characterArt.color;
            color.a = currentUnit.Sprite != null ? 1 : 0;
            characterArt.color = color;

            var totalStats = currentUnit.GetTotalStats();
            phyAtkDisplay?.SetValue(totalStats.m_PhysicalAttack);
            mgcAtkDisplay?.SetValue(totalStats.m_MagicAttack);
            phyDefDisplay?.SetValue(totalStats.m_PhysicalDefence);
            mgcDefDisplay?.SetValue(totalStats.m_MagicDefence);
            spdDisplay?.SetValue(totalStats.m_Speed);

            TrackedUnit = currentUnit;
        }

        private void OnHealthChange(float change, float value, float max)
        {
            hpBar?.SetValue(value, max);
        }

        private void OnManaChange(float change, float value, float max)
        {
            mpBar?.SetValue(value, max);
        }

        private void Show()
        {
            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            TrackedUnit = null;

            isHidden = true;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
        }
    }
}
