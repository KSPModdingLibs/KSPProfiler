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

	public class KsmUIToggleList<T> : KsmUIBase
	{
		public ToggleGroup ToggleGroupComponent { get; private set; }
		public UnityAction<T, bool> OnToggleSelectedChange { get; set; }
		public List<KsmUIToggleListElement<T>> ChildToggles { get; private set; } = new List<KsmUIToggleListElement<T>>();
		public HorizontalOrVerticalLayoutGroup LayoutGroup { get; private set; }

		public KsmUIToggleList(KsmUIBase parent, KsmUILib.Orientation orientation, UnityAction<T, bool> onToggleSelectedChange) : base(parent)
		{
			switch (orientation)
			{
				case KsmUILib.Orientation.Vertical: LayoutGroup = TopObject.AddComponent<VerticalLayoutGroup>(); break;
				case KsmUILib.Orientation.Horizontal: LayoutGroup = TopObject.AddComponent<HorizontalLayoutGroup>(); break;
			}

			LayoutGroup.spacing = 2f;
			LayoutGroup.padding = new RectOffset(0, 0, 0, 0);
			LayoutGroup.childControlHeight = true;
			LayoutGroup.childControlWidth = true;
			LayoutGroup.childForceExpandHeight = false;
			LayoutGroup.childForceExpandWidth = false;
			LayoutGroup.childAlignment = TextAnchor.UpperLeft;

			ToggleGroupComponent = TopObject.AddComponent<ToggleGroup>();
			OnToggleSelectedChange = onToggleSelectedChange;
		}
	}

	public class KsmUIToggleListElement<T> : KsmUIHorizontalLayout, IKsmUIInteractable, IKsmUIText, IKsmUIToggle
	{
		public KsmUIText TextObject { get; private set; }
		public Toggle ToggleComponent { get; private set; }
		public T ToggleId { get; private set; }
		private KsmUIToggleList<T> parent;

		public KsmUIToggleListElement(KsmUIToggleList<T> parent, T toggleId) : base(parent)
		{
			ToggleComponent = TopObject.AddComponent<Toggle>();
			ToggleComponent.transition = Selectable.Transition.None;
			ToggleComponent.navigation = new Navigation() { mode = Navigation.Mode.None };
			ToggleComponent.isOn = false;
			ToggleComponent.toggleTransition = Toggle.ToggleTransition.Fade;
			ToggleComponent.group = parent.ToggleGroupComponent;

			this.parent = parent;
			parent.ChildToggles.Add(this);
			ToggleId = toggleId;
			ToggleComponent.onValueChanged.AddListener(NotifyParent);

			Image image = TopObject.AddComponent<Image>();
			image.color = KsmUIStyle.boxColor;

			SetLayoutImpl(-1f, -1f, -1, -1, -1, 14);

			KsmUIHorizontalLayout highlightImage = new KsmUIHorizontalLayout(this);
			Image bgImage = highlightImage.TopObject.AddComponent<Image>();
			bgImage.color = KsmUIStyle.selectedBoxColor;
			bgImage.raycastTarget = false;
			ToggleComponent.graphic = bgImage;

			TextObject = new KsmUIText(highlightImage);
			TextObject.SetLayoutImpl(1f);
		}

		private void NotifyParent(bool selected)
		{
			if (parent.OnToggleSelectedChange != null)
			{
				parent.OnToggleSelectedChange(ToggleId, selected);
			}
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

        public void SetToggleOnChange(UnityAction<bool> action)
		{
			ToggleComponent.onValueChanged.AddListener(action);
		}
	}
}
