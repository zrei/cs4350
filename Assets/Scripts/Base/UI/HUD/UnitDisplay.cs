using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class UnitDisplay : MonoBehaviour
    {
        [SerializeField]
        private Color playerColor = new Color32(0, 64, 106, 255);
        [SerializeField]
        private Color enemyColor = new Color32(106, 0, 0, 255);

        [SerializeField]
        private bool isCurrentUnitDisplay = true;

        [SerializeField]
        private bool isSubDisplay;

        #region Component References
        [SerializeField]
        private FormattedTextDisplay nameDisplay;

        [SerializeField]
        private Image characterArt;

        [SerializeField]
        private Graphic background;

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
        private FormattedTextDisplay movDisplay;

        [SerializeField]
        private ProgressBar hpBar;

        [SerializeField]
        private ProgressBar mpBar;

        [SerializeField]
        private StatusDisplay statusDisplay;
        #endregion

        private UIAnimator uiAnimator;

        public Unit TrackedUnit
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
                statusDisplay.TrackedUnit = value;

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
            if (!isSubDisplay)
            {
                uiAnimator = GetComponent<UIAnimator>();

                GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;
            }
        }

        private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
        {
            if (toScene != SceneEnum.BATTLE)
                return;

            if (isCurrentUnitDisplay)
            {
                GlobalEvents.Battle.PreviewCurrentUnitEvent += OnPreviewUnit;
            }
            else
            {
                GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
            }

            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
            GlobalEvents.Scene.OnBeginSceneChange += OnSceneChange;
            GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
        }

        private void OnBattleEnd(UnitAllegiance _, int _2)
        {
            HandleQuit();
        }

        private void OnSceneChange(SceneEnum _, SceneEnum _2)
        {
            HandleQuit();
        }

        private void OnAttackAnimation()
        {
            Hide();
        }

        private void HandleQuit()
        {
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;
            GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;

            Hide();
        }

        private void OnDestroy()
        {
            GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
            GlobalEvents.Battle.PreviewCurrentUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Scene.OnBeginSceneChange -= OnSceneChange;
            GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
        }

        public void OnPreviewUnit(Unit currentUnit)
        {
            if (currentUnit == null)
            {
                Hide();
                return;
            }

            Show();

            var backgroundColor = currentUnit.UnitAllegiance switch
            {
                UnitAllegiance.PLAYER => playerColor,
                UnitAllegiance.ENEMY => enemyColor,
                _ => playerColor
            };
            backgroundColor.a = background.color.a;
            background.color = backgroundColor;

            nameDisplay.SetValue(currentUnit.DisplayName);

            characterArt.sprite = currentUnit.Sprite;
            var color = characterArt.color;
            color.a = currentUnit.Sprite != null ? 0.6f : 0;
            characterArt.color = color;

            var totalStats = currentUnit.GetTotalStats();
            phyAtkDisplay?.SetValue(totalStats.m_PhysicalAttack);
            mgcAtkDisplay?.SetValue(totalStats.m_MagicAttack);
            phyDefDisplay?.SetValue(totalStats.m_PhysicalDefence);
            mgcDefDisplay?.SetValue(totalStats.m_MagicDefence);
            spdDisplay?.SetValue(totalStats.m_Speed);
            movDisplay?.SetValue(totalStats.m_MovementRange);

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
            if (uiAnimator == null) return;

            uiAnimator.Show();
        }

        private void Hide()
        {
            if (uiAnimator == null) return;

            TrackedUnit = null;

            uiAnimator.Hide();
        }
    }
}
