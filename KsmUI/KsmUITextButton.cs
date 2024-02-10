using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KsmUI
{
	public static class KsmUITextButtonExtensions
	{
		public static T SetOnClick<T>(this T instance, UnityAction action) where T : KsmUITextButton
        {
			instance.SetOnClickImpl(action);
			return instance;
		}
    }

    public class KsmUITextButton : KsmUIText, IKsmUIInteractable
    {
		public Button ButtonComponent { get; private set; }
		private UnityAction onClick;

        public KsmUITextButton(KsmUIBase parent, string text, UnityAction onClick) : base(parent, text)
        {
            ButtonComponent = TopObject.AddComponent<Button>();
            ButtonComponent.interactable = true;
            ButtonComponent.navigation = new Navigation() { mode = Navigation.Mode.None }; // fix the transitions getting stuck
            this.onClick = onClick;
            ButtonComponent.onClick.AddListener(onClick);
            UnderscoreOnHover hoverComponent = TopObject.AddComponent<UnderscoreOnHover>();
            hoverComponent.textComponent = TextComponent;
        }

        bool IKsmUIInteractable.Interactable
		{
			get => ButtonComponent.interactable;
			set => ButtonComponent.interactable = value;
		}

		internal void SetOnClickImpl(UnityAction action)
		{
			if (onClick != null)
				ButtonComponent.onClick.RemoveListener(onClick);

			onClick = action;

			if (action != null)
				ButtonComponent.onClick.AddListener(onClick);
		}
	}

	public class UnderscoreOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public TextMeshProUGUI textComponent;

		public void OnPointerEnter(PointerEventData eventData)
		{
			textComponent.fontStyle |= FontStyles.Underline;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			textComponent.fontStyle ^= FontStyles.Underline;
		}
	}
}
