using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIPopup : KsmUIVerticalLayout
	{
		private class KsmUIPopupArea : MonoBehaviour, IPointerExitHandler
		{
			public KsmUIPopup popup;

			public void OnPointerExit(PointerEventData pointerEventData)
			{
				popup.OnPointerExit();
			}
		}

		public override RectTransform ParentTransformForChilds => contentParent?.TopTransform ?? TopTransform;

		private readonly KsmUIVerticalLayout contentParent;

		public KsmUIPopup(KsmUIBase parent, TextAnchor originInParent = TextAnchor.UpperLeft, TextAnchor pivot = TextAnchor.LowerRight) : base(parent)
		{
			TopTransform.SetAnchorsAndPosition(originInParent, pivot, 50, -50);
			this.SetPadding(50);

			KsmUIPopupArea popupArea = TopObject.AddComponent<KsmUIPopupArea>();
			popupArea.popup = this;

			RawImage mouseAreaImg = TopObject.AddComponent<RawImage>();
			mouseAreaImg.color = new Color(0f, 0f, 0f, 0f);

			ContentSizeFitter topFitter = TopObject.AddComponent<ContentSizeFitter>();
			topFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize; //.Unconstrained;
			topFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;


			// first child : 1px white border
			KsmUIVerticalLayout whiteBorder = new KsmUIVerticalLayout(this).SetPadding(1);
			Image borderImage = whiteBorder.TopObject.AddComponent<Image>();
			borderImage.color = KsmUIStyle.tooltipBorderColor;

			// 2nd child : black background
			contentParent = new KsmUIVerticalLayout(whiteBorder).SetPadding(5, 5, 2, 2);
			Image backgroundImage = contentParent.TopObject.AddComponent<Image>();
			backgroundImage.color = KsmUIStyle.tooltipBackgroundColor;
		}

		private void OnPointerExit()
		{
			Destroy();
		}

		public KsmUITextButton AddButton(string label, Action callback)
		{
			void CloseCallback()
			{
				callback();
				Destroy();
			}

			return new KsmUITextButton(contentParent, label, CloseCallback);
		}
	}
}
