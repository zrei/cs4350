using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.UI
{
    [System.Flags]
    public enum VisibilityTags
    {
        None = 0,
        World = 1 << 0,
        Level = 1 << 1,
        Battle = 1 << 2,
        //BattleSetUp,
        //BattlePlayerTurn,
        //BattleEnemyTurn,
        //BattleSkillAnimation,
        //BattleEnd,
    }

    public interface IToggleableUI
    {
        VisibilityTags VisibilityTags { get; }

        void Show();
        void Hide();
    }

    public class UIManager : Singleton<UIManager>
    {
        private VisibilityTags VisibilityTags
        {
            get => m_VisibilityTags;
            set
            {
                if (m_VisibilityTags == value) return;

                m_VisibilityTags = value;
                var newActiveUIs = m_ToggleableUIs.Where(x => x.VisibilityTags.HasFlag(m_VisibilityTags));
                foreach (var ui in m_ActiveUIs.ToList())
                {
                    if (!newActiveUIs.Contains(ui))
                    {
                        ui.Hide();
                        m_ActiveUIs.Remove(ui);
                    }
                }
                foreach (var ui in newActiveUIs)
                {
                    if (m_ActiveUIs.Add(ui)) ui.Show();
                }
            }
        }
        [SerializeProperty("VisibilityTags")]
        [SerializeField]
        private VisibilityTags m_VisibilityTags = VisibilityTags.None;

        private HashSet<IToggleableUI> m_ActiveUIs = new();

        private HashSet<IToggleableUI> m_ToggleableUIs = new();

        public void Add(IToggleableUI ui)
        {
            m_ToggleableUIs.Add(ui);
            if (ui.VisibilityTags.HasFlag(m_VisibilityTags)) ui.Show();
            else ui.Hide();
        }

        public void Remove(IToggleableUI ui)
        {
            m_ToggleableUIs.Remove(ui);
            ui.Hide();
        }

        protected override void HandleAwake()
        {
            base.HandleAwake();

            GlobalEvents.Level.ReturnFromLevelEvent += OnWorldSceneLoaded;

            GlobalEvents.Scene.LevelSceneLoadedEvent += OnLevelSceneLoaded;
            GlobalEvents.Battle.ReturnFromBattleEvent += OnLevelSceneLoaded;

            GlobalEvents.Scene.BattleSceneLoadedEvent += OnBattleSceneLoaded;
            //GlobalEvents.Battle.PlayerUnitSetupStartEvent += OnBattleSetUpStart;
            //GlobalEvents.Battle.PlayerTurnStartEvent += OnBattlePlayerTurnStart;
            //GlobalEvents.Battle.EnemyTurnStartEvent += OnBattleEnemyTurnStart;
            //GlobalEvents.Battle.AttackAnimationEvent += OnBattleSkillAnimationEvent;
            //GlobalEvents.Battle.BattleEndEvent += OnBattleEndEvent;

            OnWorldSceneLoaded();
        }

        private void OnDestroy()
        {
            GlobalEvents.Level.ReturnFromLevelEvent -= OnWorldSceneLoaded;

            GlobalEvents.Scene.LevelSceneLoadedEvent -= OnLevelSceneLoaded;
            GlobalEvents.Battle.ReturnFromBattleEvent -= OnLevelSceneLoaded;

            GlobalEvents.Scene.BattleSceneLoadedEvent += OnBattleSceneLoaded;
            //GlobalEvents.Battle.PlayerUnitSetupStartEvent += OnBattleSetUpStart;
            //GlobalEvents.Battle.PlayerTurnStartEvent += OnBattlePlayerTurnStart;
            //GlobalEvents.Battle.EnemyTurnStartEvent += OnBattleEnemyTurnStart;
            //GlobalEvents.Battle.AttackAnimationEvent += OnBattleSkillAnimationEvent;
            //GlobalEvents.Battle.BattleEndEvent += OnBattleEndEvent;
        }

        private void OnWorldSceneLoaded() { VisibilityTags = VisibilityTags.World; }
        private void OnLevelSceneLoaded() { VisibilityTags = VisibilityTags.Level; }
        private void OnBattleSceneLoaded() { VisibilityTags = VisibilityTags.Battle; }
        //private void OnBattleSetUpStart() { VisibilityTags = VisibilityTags.BattleSetUp; }
        //private void OnBattlePlayerTurnStart() { VisibilityTags = VisibilityTags.BattlePlayerTurn; }
        //private void OnBattleEnemyTurnStart() { VisibilityTags = VisibilityTags.BattleEnemyTurn; }
        //private void OnBattleSkillAnimationEvent(ActiveSkillSO _, Unit _, List<Unit> _) { VisibilityTags = VisibilityTags.BattleSkillAnimation; }
        //private void OnBattleEndEvent(UnitAllegiance _, int _) { VisibilityTags = VisibilityTags.BattleEnd; }
    }
}