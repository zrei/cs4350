using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class IndividualBattleObjectiveDisplay : MonoBehaviour
    {
        [SerializeField]
        private SelectableBase selectable;

        [SerializeField]
        private GraphicGroup graphicGroup;

        [SerializeField]
        private FormattedTextDisplay text;

        [SerializeField]
        private Image strikethrough;

        [SerializeField]
        private ProgressBar progressBar;

        public IObjective TrackedObjective
        {
            set
            {
                if (trackedObjective == value) return;

                if (trackedObjective != null)
                {
                    trackedObjective.OnUpdate -= OnObjectiveUpdate;
                }

                trackedObjective = value;
                if (trackedObjective != null)
                {
                    trackedObjective.OnUpdate += OnObjectiveUpdate;
                    OnObjectiveUpdate(trackedObjective);
                }
            }
        }
        private IObjective trackedObjective;

        private void Start()
        {
            strikethrough?.CrossFadeAlpha(0, 0, true);
            selectable.onSelect.AddListener(() => trackedObjective?.Show(true));
            selectable.onDeselect.AddListener(() => trackedObjective?.Show(false));
        }

        private void OnDestroy()
        {
            TrackedObjective = null;
        }

        private void OnObjectiveUpdate(IObjective objective)
        {
            if (objective == null) return;

            graphicGroup.color = objective.Color;

            text.SetValue(objective.ToString());
            
            strikethrough.CrossFadeAlpha(
                objective.CompletionStatus == ObjectiveState.Completed || objective.CompletionStatus == ObjectiveState.Failed
                    ? 1 
                    : 0,
                0.5f,
                false);

            progressBar.gameObject.SetActive(objective.UseProgressBar);
            if (objective.UseProgressBar)
            {
                progressBar.SetValue(objective.DisplayedProgress, 1);
            }
        }
    }
}
