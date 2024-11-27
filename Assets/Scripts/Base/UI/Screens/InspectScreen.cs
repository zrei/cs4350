using Game.UI;
using UnityEngine;

namespace Base.UI
{
    public class InspectScreen : BaseUIScreen
    {
        [SerializeField]
        private UnitDisplay unitDisplay;

        [SerializeField]
        private SkillsOverviewDisplay skillDisplay;

        public override void ScreenUpdate()
        {
        }

        public override void Show(params object[] args)
        {
            if (args.Length == 0) return;

            var unit = args[0] as Unit;
            if (unit == null) return;

            unitDisplay.OnPreviewUnit(unit);
            skillDisplay.DisplayUnitSkills(unit);
            skillDisplay.Show();

            base.Show();
        }

        protected override void HideDone()
        {
            base.HideDone();

            unitDisplay.TrackedUnit = null;
        }

        private void OnDestroy()
        {
            unitDisplay.TrackedUnit = null;
        }
    }
}
