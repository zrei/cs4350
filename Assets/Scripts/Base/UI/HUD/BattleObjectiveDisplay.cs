using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI
{
    public class BattleObjectiveDisplay : MonoBehaviour
    {
        private const int MaxDisplays = 5;

        [SerializeField]
        private IndividualBattleObjectiveDisplay objectiveDisplayPrefab;

        private ObjectPool<IndividualBattleObjectiveDisplay> displayPool;
        private HashSet<IndividualBattleObjectiveDisplay> activeDisplays = new();

        [SerializeField]
        private LayoutGroup layout;

        private Animator animator;
        private CanvasGroup canvasGroup;
        private bool isHidden;

        private void Awake()
        {
            displayPool = new(
                createFunc: () => { var display = Instantiate(objectiveDisplayPrefab, layout.transform); display.gameObject.SetActive(false); return display; },
                actionOnGet: display => { display.gameObject.SetActive(true); display.transform.SetAsLastSibling(); activeDisplays.Add(display); },
                actionOnRelease: display => { display.gameObject.SetActive(false); activeDisplays.Remove(display); },
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 3,
                maxSize: MaxDisplays
            );

            GlobalEvents.Battle.BattleInitializedEvent += Initialize;
            GlobalEvents.Level.ReturnFromLevelEvent += Clear;

            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            isHidden = true;
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.BattleInitializedEvent -= Initialize;
            GlobalEvents.Level.ReturnFromLevelEvent -= Clear;
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
            GlobalEvents.Battle.CompleteAttackAnimationEvent -= Show;
        }

        private void Initialize()
        {
            Clear();

            var objectives = BattleManager.Instance.Objectives;
            foreach (var objective in objectives )
            {
                var display = displayPool.Get();
                display.TrackedObjective = objective;
            }

            Show();

            GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
            GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
            GlobalEvents.Battle.CompleteAttackAnimationEvent += Show;
    }

        private void Clear()
        {
            activeDisplays.ToList().ForEach(x => { x.TrackedObjective = null; displayPool.Release(x); });
        }

        private void OnBattleEnd(UnitAllegiance _, int numTurns)
        {
            GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
            GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
            GlobalEvents.Battle.CompleteAttackAnimationEvent -= Show;
            Hide();
        }

        private void OnAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> target)
        {
            Hide();
        }

        private void Show()
        {
            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
        }

        private void Hide()
        {
            isHidden = true;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            canvasGroup.interactable = !isHidden;
            canvasGroup.blocksRaycasts = !isHidden;
        }
    }
}
