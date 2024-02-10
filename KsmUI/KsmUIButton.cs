using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KsmUI
{
    public class KsmUIButton : KsmUIButtonBase, IKsmUIText, IKsmUIInteractable
    {
        public TextMeshProUGUI TextComponent { get; private set; }

        public KsmUIButton(KsmUIBase parent) : base(parent)
        {
            SetBackgroundColorImpl(Color.white);
            ButtonComponent.transition = Selectable.Transition.ColorTint;
            ButtonComponent.colors = KsmUIStyle.buttonTransitionColorBlock;
            ButtonComponent.navigation = new Navigation() { mode = Navigation.Mode.None };

            RectTransform background = KsmUILib.NewUIGameObject("Background", TopTransform, true);
            Image backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f);
            ButtonComponent.targetGraphic = backgroundImage;
            background.SetStretchInParent(1);

            RectTransform text = KsmUILib.NewUIGameObject("Text", background, true);
            TextComponent = text.gameObject.AddComponent<TextMeshProUGUI>();
            TextComponent.color = KsmUIStyle.textColor;
            TextComponent.font = KsmUIStyle.textFont;
            TextComponent.fontSize = KsmUIStyle.textSize;
            TextComponent.alignment = TextAlignmentOptions.Center;
            TextComponent.enableWordWrapping = true;
            TextComponent.overflowMode = TextOverflowModes.Overflow;
            text.SetStretchInParent();
        }

        public string Text
        {
            get => TextComponent.text;
            set
            {
                if (value == null)
                    value = string.Empty;

                TextComponent.text = value;
            }
        }

        bool IKsmUIInteractable.Interactable
        {
            get => ButtonComponent.interactable;
            set => ButtonComponent.interactable = value;
        }
    }


}
