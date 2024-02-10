using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KsmUI
{
	public interface IKsmUIText
	{
		string Text { get; set; }
        TextMeshProUGUI TextComponent { get; }
    }

	public interface IKsmUIInteractable
	{
		bool Interactable { get; set; }
	}

    public interface IKsmUIToggle
	{
		void SetToggleOnChange(UnityAction<bool> action);
	}

    public interface IKsmUILayoutGroup
    {
        HorizontalOrVerticalLayoutGroup LayoutGroup { get; }
    }

    public static class IKsmUILayoutGroupExtensions
    {
        public static T SetSpacing<T>(this T instance, int spacing) where T : IKsmUILayoutGroup
        {
            instance.LayoutGroup.spacing = spacing;
            return instance;
        }

        public static T SetPadding<T>(this T instance, int paddingLeft = 0, int paddingRight = 0, int paddingTop = 0, int paddingBottom = 0) where T : IKsmUILayoutGroup
        {
            instance.LayoutGroup.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            return instance;
        }

        public static T SetPadding<T>(this T instance, int padding) where T : IKsmUILayoutGroup
        {
            instance.LayoutGroup.padding = new RectOffset(padding, padding, padding, padding);
            return instance;
        }

        public static T SetChildAlignment<T>(this T instance, TextAnchor childAlignement) where T : IKsmUILayoutGroup
        {
            instance.LayoutGroup.childAlignment = childAlignement;
            return instance;
        }
    }

    public static class IKsmUITextExtensions
    {
        public static T SetText<T>(this T instance, string text) where T : IKsmUIText
        {
            instance.Text = text;
            return instance;
        }

        public static T SetText<T>(this T instance, Func<string> textFunc) where T : KsmUIBase, IKsmUIText
        {
            instance.SetUpdateActionImpl(() => instance.Text = textFunc());
            return instance;
        }

        public static T SetStyle<T>(this T instance, FontStyles style) where T : IKsmUIText
        {
            instance.TextComponent.fontStyle = style;
			return instance;
		}

        public static T SetTextColor<T>(this T instance, Color color) where T : IKsmUIText
        {
            instance.TextComponent.color = color;
            return instance;
        }

        public static T SetTextAlignment<T>(this T instance, TextAlignmentOptions alignment) where T : IKsmUIText
        {
            instance.TextComponent.alignment = alignment;
            return instance;
        }

        public static T SetWordWarp<T>(this T instance, bool wrap) where T : IKsmUIText
        {
            instance.TextComponent.enableWordWrapping = wrap;
            return instance;
        }

        public static T SetOverflowMode<T>(this T instance, TextOverflowModes overflowMode) where T : IKsmUIText
        {
            instance.TextComponent.overflowMode = overflowMode;
            return instance;
        }
    }
}
