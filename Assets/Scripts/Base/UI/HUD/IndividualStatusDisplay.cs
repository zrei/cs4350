using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void StatusEvent(IStatus status);

public interface IStatus
{
    Sprite Icon { get; }
    Color Color { get; }
    string DisplayTier { get; }
    string DisplayStacks { get; }
    string Name { get; }
    string Description { get; }
}

namespace Game.UI
{
    public class IndividualStatusDisplay : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField]
        private GraphicGroup graphicGroup;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI tier;

        [SerializeField]
        private TextMeshProUGUI stacks;

        public IStatus TrackedStatus
        {
            get => trackedStatus;
            set
            {
                trackedStatus = value;
                if (trackedStatus != null)
                {
                    icon.sprite = trackedStatus.Icon;
                    graphicGroup.color = trackedStatus.Color;
                    tier.text = trackedStatus.DisplayTier;
                    stacks.text = trackedStatus.DisplayStacks;
                }
            }
        }
        private IStatus trackedStatus;

        private void Awake()
        {
        }

        public void OnChange()
        {
            stacks.text = trackedStatus.DisplayStacks;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
