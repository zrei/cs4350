using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
    public class BattleObjectiveDisplay : MonoBehaviour
    {
        private const int MaxDisplays = 5;

        [SerializeField]
        private IndividualBattleObjectiveDisplay objectiveDisplayPrefab;

        private ObjectPool<IndividualBattleObjectiveDisplay> displayPool;
        private HashSet<IndividualBattleObjectiveDisplay> activeDisplays = new();

        [SerializeField]
        private LayoutGroup layout;

        private UIAnimator uiAnimator;

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

            uiAnimator = GetComponent<UIAnimator>();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.BattleInitializedEvent -= Initialize;
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

            Show(false);

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

        private void OnAttackAnimation()
        {
            Hide();
        }

        private void Show(bool _)
        {
            uiAnimator.Show();
        }

        private void Hide()
        {
            uiAnimator.Hide();
        }
    }
}
