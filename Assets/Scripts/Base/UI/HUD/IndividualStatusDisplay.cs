using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public delegate void StatusEvent(IStatus status);

public interface IStatus
{
    Sprite Icon { get; }
    Color Color { get; }
    string DisplayAmount { get; }
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
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI amount;

        [SerializeField]
        private GameObject displayNameRoot;

        [SerializeField]
        private TextMeshProUGUI displayName;

        [SerializeField]
        private List<Graphic> tintable;

        public IStatus TrackedStatus
        {
            get => trackedStatus;
            set
            {
                trackedStatus = value;
                if (trackedStatus != null)
                {
                    icon.sprite = trackedStatus.Icon;
                    amount.text = trackedStatus.DisplayAmount;
                    displayName.text = trackedStatus.Name;
                    tintable.ForEach(t => t.color = trackedStatus.Color);
                }
            }
        }
        private IStatus trackedStatus;

        private void Awake()
        {
            displayNameRoot.gameObject.SetActive(false);
        }

        public void OnChange()
        {
            amount.text = trackedStatus.DisplayAmount;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            displayNameRoot.gameObject.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            displayNameRoot.gameObject.SetActive(false);
        }
    }
}
