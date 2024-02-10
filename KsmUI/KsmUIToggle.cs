using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIToggle<T> : KsmUIHorizontalLayout, IKsmUIText, IKsmUIInteractable
	{
		public bool IsOn => ToggleComponent.isOn;
		public Toggle ToggleComponent { get; private set; }
		public KsmUIText TextObject { get; private set; }
		private UnityAction<bool> onClickAction;

		public KsmUIToggle(KsmUIBase parent, string toggleText, bool initialOnState, UnityAction<bool> onClick, string tooltipText = null, int width = -1, int height = 18)
			: base(parent)
		{
			onClickAction = onClick;
			this.SetSpacing(5);

			if (width <= 0)
				SetLayoutImpl(1f, -1f, -1, height);
			else
				SetLayoutImpl(-1f, -1f, width, height);

			ToggleComponent = TopObject.AddComponent<Toggle>();
			ToggleComponent.transition = Selectable.Transition.SpriteSwap;
			ToggleComponent.spriteState = KsmUIStyle.buttonSpriteSwap;
			ToggleComponent.navigation = new Navigation() { mode = Navigation.Mode.None }; // fix the transitions getting stuck
			ToggleComponent.toggleTransition = Toggle.ToggleTransition.Fade;
			ToggleComponent.isOn = initialOnState;
			ToggleComponent.onValueChanged.AddListener(onClickAction);

			GameObject background = new GameObject("Background");
			RectTransform backgroundTransform = background.AddComponent<RectTransform>();
			LayoutElement backgroundLayout = background.AddComponent<LayoutElement>();
			backgroundLayout.preferredHeight = 18f; // the toggle is always 18x18
			backgroundLayout.preferredWidth = 18f;
			backgroundTransform.SetParentFixScale(TopTransform);
			background.AddComponent<CanvasRenderer>();
			Image backgroundImage = background.AddComponent<Image>();
			backgroundImage.sprite = Textures.KsmUISpriteBtnNormal;
			backgroundImage.type = Image.Type.Sliced;
			backgroundImage.fillCenter = true;
			ToggleComponent.targetGraphic = backgroundImage;

			GameObject checkmark = new GameObject("Checkmark");
			checkmark.AddComponent<RectTransform>()
				.SetAnchorsAndPosition(TextAnchor.MiddleCenter, TextAnchor.MiddleCenter, 0, 0)
				.SetSizeDelta(10, 10) // a checkbox is always 10x10, centered in the toggle
				.SetParentFixScale(backgroundTransform);
			checkmark.AddComponent<CanvasRenderer>();
			RawImage checkmarkImage = checkmark.AddComponent<RawImage>();
			checkmarkImage.texture = Textures.KsmUITexCheckmark;
			ToggleComponent.graphic = checkmarkImage;

			TextObject = new KsmUIText(this)
				.SetText(toggleText)
				.SetTextAlignment(TextAlignmentOptions.Left)
				.SetWordWarp(false)
				.SetOverflowMode(TextOverflowModes.Ellipsis);

			if (width <= 0)
				SetLayoutImpl(1f, -1f, -1, height);
			else
				SetLayoutImpl(-1f, -1f, width-18-5, height);
			TextObject.TopTransform.SetParentFixScale(TopTransform);

			if (tooltipText != null) SetTooltipImpl(tooltipText);
		}

		public bool Interactable
		{
			get => ToggleComponent.interactable;
			set => ToggleComponent.interactable = value;
		}

		public string Text
		{
			get => TextObject.Text;
			set => TextObject.Text = value;
		}

        public TextMeshProUGUI TextComponent => TextObject.TextComponent;

        public void SetOnState(bool isOn, bool fireOnClick = false)
		{
			if (!fireOnClick)
				ToggleComponent.onValueChanged.RemoveListener(onClickAction);

			ToggleComponent.isOn = isOn;

			if (!fireOnClick)
				ToggleComponent.onValueChanged.AddListener(onClickAction);
		}
    }
}
