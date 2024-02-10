using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIHeader : KsmUIHorizontalLayout, IKsmUIText
	{
		public KsmUIText TextObject { get; private set; }

		public KsmUIHeader(KsmUIBase parent, string title, Color backgroundColor = default, int textPreferredWidth = -1) : base(parent)
		{
            LayoutGroup.spacing = 2f;
            SetBackgroundColorImpl(backgroundColor == default ? Color.black : backgroundColor);

			TextObject = new KsmUIText(this)
				.SetText(title)
				.SetTextAlignment(TextAlignmentOptions.Center)
				.SetStyle(FontStyles.UpperCase)
				.SetLayout(true, false, textPreferredWidth, -1, -1, 16);
		}

		public string Text
		{
			get => TextObject.Text;
			set => TextObject.Text = value;
		}

        public TextMeshProUGUI TextComponent => TextObject.TextComponent;

        public KsmUIIconButton AddButton(Texture2D texture, UnityAction onClick = null, string tooltip = null, bool leftSide = false)
		{
			KsmUIIconButton button = new KsmUIIconButton(this, texture, onClick, 16, 16);
			button.SetLayoutImpl(-1f, -1f, 16, 16);

			if (leftSide)
				button.MoveAsFirstChildImpl();

			if (tooltip != null)
				button.SetTooltipImpl(tooltip);

			return button;
		}

		public KsmUIIconToggle AddToggle(Texture2D whenTrueTexture, Texture2D whenFalseTexture, bool initalValue = false, Action<bool> valueChangedAction = null, bool leftSide = false)
		{
			KsmUIIconToggle toggle = new KsmUIIconToggle(this, whenTrueTexture, whenFalseTexture, initalValue, valueChangedAction, 16, 16);
			toggle.SetLayoutImpl(-1f, -1f, 16, 16);
			if (leftSide)
				toggle.MoveAsFirstChildImpl();

			return toggle;
		}

		public KsmUIIconToggle AddToggle(Texture2D texture, Kolor whenTrue, Kolor whenFalse, bool initalValue = false, Action<bool> valueChangedAction = null, bool leftSide = false)
		{
			KsmUIIconToggle toggle = new KsmUIIconToggle(this, texture, whenTrue, whenFalse, initalValue, valueChangedAction, 16, 16);
			toggle.SetLayoutImpl(-1f, -1f, 16, 16);
			if (leftSide)
				toggle.MoveAsFirstChildImpl();
			return toggle;
		}
	}
}
