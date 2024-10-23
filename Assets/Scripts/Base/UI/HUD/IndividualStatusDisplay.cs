using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Pool;
using UnityEngine.UI;

public static class TokenUtil
{
    public static string NumToRomanNumeral(int num)
    {
        return num switch
        {
            -1 => string.Empty,
            0 => string.Empty,
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            6 => "VI",
            7 => "VII",
            8 => "VIII",
            9 => "IX",
            10 => "X",
            11 => "XI",
            12 => "XII",
            13 => "XIII",
            14 => "XIV",
            15 => "XV",
            16 => "XVI",
            17 => "XVII",
            18 => "XVIII",
            19 => "XIX",
            20 => "XX",
            _ => string.Empty,
        };
    }
}

public delegate void StatusEvent(IStatus status);

public interface IStatus
{
    Sprite Icon { get; }
    Color Color { get; }
    string DisplayTier { get; }
    string DisplayStacks { get; }
    string Name { get; }
    string Description { get; }
    List<int> NumStacksPerTier { get; }
    int CurrentHighestTier { get; }
}

namespace Game.UI
{
    public class IndividualStatusDisplay : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        private const int MaxDisplays = 20;

        [SerializeField]
        private TokenStackDisplay tokenStackDisplayPrefab;
        private Dictionary<int, TokenStackDisplay> activeDisplays = new();
        private ObjectPool<TokenStackDisplay> displayPool;

        [SerializeField]
        private GraphicGroup graphicGroup;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TextMeshProUGUI tier;

        [SerializeField]
        private TextMeshProUGUI stacks;

        [SerializeField]
        private LayoutGroup tokenStackLayout;

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
                    NumStacksPerTier = trackedStatus.NumStacksPerTier;
                }
            }
        }
        private IStatus trackedStatus;

        private List<int> NumStacksPerTier
        {
            get => numStacksPerTier;
            set
            {
                if (numStacksPerTier != null)
                {
                    if (activeDisplays.Count > 0)
                    {
                        var displays = new List<TokenStackDisplay>(activeDisplays.Values);
                        activeDisplays.Clear();
                        displays.ForEach(x => displayPool.Release(x));
                    }
                }

                numStacksPerTier = value;

                if (numStacksPerTier != null)
                {
                    // start from second highest tier
                    for (int tier = trackedStatus.CurrentHighestTier - 1; tier > 0; tier--)
                    {
                        var tierCount = numStacksPerTier[tier - 1];
                        TokenStackDisplay display = null;
                        if (!activeDisplays.TryGetValue(tier, out display) && tierCount > 0)
                        {
                            display = displayPool.Get();
                            activeDisplays.Add(tier, display);
                            display.graphicGroup.color = trackedStatus.Color;
                        }
                        if (display != null)
                        {
                            if (tierCount <= 0)
                            {
                                displayPool.Release(display);
                                activeDisplays.Remove(tier);
                            }
                            else
                            {
                                display.tier.text = TokenUtil.NumToRomanNumeral(tier);
                                display.stacks.SetValue(tierCount);
                            }
                        }
                    }
                }
            }
        }
        private List<int> numStacksPerTier;

        private void Awake()
        {
            displayPool = new(
                createFunc: () => Instantiate(tokenStackDisplayPrefab, tokenStackLayout.transform),
                actionOnGet: display => { display.gameObject.SetActive(true); display.transform.SetAsFirstSibling(); },
                actionOnRelease: display => display.gameObject.SetActive(false),
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 0,
                maxSize: MaxDisplays
            );
        }

        public void OnChange()
        {
            tier.text = trackedStatus.DisplayTier;
            stacks.text = trackedStatus.DisplayStacks;
            NumStacksPerTier = numStacksPerTier;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        public void OnPointerExit(PointerEventData eventData)
        {
        }
    }
}
