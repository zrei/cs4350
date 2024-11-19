using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public enum SkillButtonState
    {
        EMPTY,
        LOCKED,
        NORMAL
    }

    public class SkillButton : ActionButton
    {
        public Color defaultIconColor = Color.red;
        public Image fillImage;
        public FormattedTextDisplay statusText;
        public Image softGlow;
        public GraphicGroup graphicGroup;

        public Sprite SkillSprite {
            set {
                fillImage.sprite = value;
                icon.sprite = value;
            }
        }

        public void SetFill(float fill)
        {
            fillImage.fillAmount = fill;
            softGlow.gameObject.SetActive(fill >= 1f);
            graphicGroup.color = fill >= 1f ? defaultIconColor : Color.grey;
        }

        public void SetState(SkillButtonState skillButtonState)
        {
            interactable = skillButtonState != SkillButtonState.EMPTY;
            icon.color = skillButtonState == SkillButtonState.EMPTY ? Color.clear : Color.gray;
            fillImage.color = skillButtonState == SkillButtonState.EMPTY ? Color.clear : defaultIconColor;
            statusText.gameObject.SetActive(skillButtonState == SkillButtonState.LOCKED);
        }
    }
}
