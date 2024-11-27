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

                if (m_VisibilityTags == VisibilityTags.None)
                {
                    foreach (var ui in m_ActiveUIs.ToList())
                    {
                        ui.Hide();
                        m_ActiveUIs.Remove(ui);
                    }
                    return;
                }

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
        [SerializeField]
        private VisibilityTags m_VisibilityTags = VisibilityTags.None;

        private HashSet<IToggleableUI> m_ActiveUIs = new();

        private HashSet<IToggleableUI> m_ToggleableUIs = new();

        public void Add(IToggleableUI ui)
        {
            m_ToggleableUIs.Add(ui);

            if (m_VisibilityTags != VisibilityTags.None && ui.VisibilityTags.HasFlag(m_VisibilityTags))
            {
                m_ActiveUIs.Add(ui);
                ui.Show();
            }
            else
            {
                ui.Hide();
            }
        }

        public void Remove(IToggleableUI ui)
        {
            m_ToggleableUIs.Remove(ui);
            m_ActiveUIs.Remove(ui);
            ui.Hide();
        }

        protected override void HandleAwake()
        {
            base.HandleAwake();

            GlobalEvents.Scene.OnBeginSceneChange += OnBeginSceneChange;
            GlobalEvents.Scene.OnSceneTransitionCompleteEvent += OnSceneLoad;

            GlobalEvents.WorldMap.OnBeginLevelAnimationEvent += SetVisiblityNone;
            GlobalEvents.WorldMap.OnEndLevelAnimationEvent += SetVisibilityWorld;

            SetVisiblityNone();
        }
        
        protected override void AddDependencies()
        {
            AddDependency<CoroutineManager>();
        }

        protected override void HandleDestroy()
        {
            base.HandleDestroy();

            GlobalEvents.Scene.OnBeginSceneChange -= OnBeginSceneChange;
            GlobalEvents.Scene.OnSceneTransitionCompleteEvent -= OnSceneLoad;
            GlobalEvents.WorldMap.OnBeginLevelAnimationEvent -= SetVisiblityNone;
            GlobalEvents.WorldMap.OnEndLevelAnimationEvent -= SetVisibilityWorld;
        }

        private void OnBeginSceneChange(SceneEnum _, SceneEnum _2)
        {
            SetVisiblityNone();
        }

        private void OnSceneLoad(SceneEnum fromScene, SceneEnum toScene)
        {
            switch (toScene)
            {
                case SceneEnum.BATTLE:
                    SetVisibilityBattle();
                    break;
                case SceneEnum.LEVEL:
                    SetVisibilityLevel();
                    break;
                case SceneEnum.WORLD_MAP:
                    SetVisibilityWorld();
                    break;
                default:
                    SetVisiblityNone();
                    break;
            }
        }

        private void SetVisiblityNone() { VisibilityTags = VisibilityTags.None; }
        private void SetVisibilityWorld() { VisibilityTags = VisibilityTags.World; }
        private void SetVisibilityLevel() { VisibilityTags = VisibilityTags.Level; }
        private void SetVisibilityBattle() { VisibilityTags = VisibilityTags.Battle; }
        //private void OnBattleSetUpStart() { VisibilityTags = VisibilityTags.BattleSetUp; }
        //private void OnBattlePlayerTurnStart() { VisibilityTags = VisibilityTags.BattlePlayerTurn; }
        //private void OnBattleEnemyTurnStart() { VisibilityTags = VisibilityTags.BattleEnemyTurn; }
        //private void OnBattleSkillAnimationEvent(ActiveSkillSO _, Unit _, List<Unit> _) { VisibilityTags = VisibilityTags.BattleSkillAnimation; }
        //private void OnBattleEndEvent(UnitAllegiance _, int _) { VisibilityTags = VisibilityTags.BattleEnd; }
    }
}
