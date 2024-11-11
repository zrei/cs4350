using Game.UI;
using UnityEngine;

public class TooltipShower : MonoBehaviour
{
    [SerializeField]
    private string header;

    [SerializeField]
    [TextArea]
    private string description;

    [SerializeField]
    private Color color;

    [SerializeField]
    private SelectableBase selectable;

    private TooltipDisplay TooltipDisplay
    {
        set
        {
            if (tooltipDisplay == value) return;

            if (tooltipDisplay != null)
            {
                tooltipDisplay.Hide();
            }

            tooltipDisplay = value;
        }
    }
    private TooltipDisplay tooltipDisplay;

    private void Start()
    {
        selectable.onSelect.AddListener(() => ShowTooltip());
        selectable.onDeselect.AddListener(() => TooltipDisplay = null);
    }

    public void ShowTooltip()
    {
        if (!TooltipDisplayManager.IsReady) return;

        TooltipDisplay = TooltipDisplayManager.Instance.ShowTooltip(
            transform.position,
            header,
            description,
            color);
    }
}
