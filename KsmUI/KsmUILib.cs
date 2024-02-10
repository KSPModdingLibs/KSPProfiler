using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PQSCity2;
using static TMPro.TMP_Text;

namespace KsmUI
{
	public static class KsmUILib
	{
		public enum Orientation { Vertical, Horizontal }

		public static T SetParentFixScale<T>(this T child, Transform parent) where T : Transform
        {
			child.SetParent(parent, false);
			return child;
		}

		public static RectTransform ForceOneScale(RectTransform transform)
		{
			transform.localScale = Vector3.one;

			foreach (RectTransform rt in transform.GetComponentsInChildren<RectTransform>())
				rt.localScale = Vector3.one;

			return transform;
        }

		public static RectTransform NewUIGameObject(string gameObjectName, RectTransform parent, bool addCanvasRenderer)
		{
            GameObject gameObject = new GameObject(gameObjectName);
			RectTransform transform = gameObject.AddComponent<RectTransform>();
			transform.SetParentFixScale(parent);
			if (addCanvasRenderer)
				gameObject.AddComponent<CanvasRenderer>();

			return transform;
		}

		/// <summary>
		/// Set the anchorMin, anchorMax, pivot and anchoredPosition values of the RectTransform in a simplified way.
		/// Note : the values will be overridden if the parent is a horizontal/vertical layout and/or if SetLayout values have been defined.
		/// </summary>
		/// <param name="originInParent"> Point on the parent transform that will be used as origin for deltaX/Y values. Sets anchorMin & anchorMax </param>
		/// <param name="destinationPivot"> Point on this transform that will be used as destination for deltaX/Y values. Sets pivot</param>
		/// <param name="deltaX"> Distance in pixels between originInParent and destinationPivot</param>
		/// <param name="deltaY"> Distance in pixels between originInParent and destinationPivot</param>
		public static RectTransform SetAnchorsAndPosition(this RectTransform transform, TextAnchor originInParent, TextAnchor destinationPivot, int deltaX = 0, int deltaY = 0)
		{
			// set the anchor (origin point) on the parent (screen) that will be used as reference in anchoredPosition
			switch (originInParent)
			{
				case TextAnchor.UpperLeft:    transform.anchorMin = new Vector2(0.0f, 1.0f); transform.anchorMax = new Vector2(0.0f, 1.0f); break;
				case TextAnchor.UpperCenter:  transform.anchorMin = new Vector2(0.5f, 1.0f); transform.anchorMax = new Vector2(0.5f, 1.0f); break;
				case TextAnchor.UpperRight:   transform.anchorMin = new Vector2(1.0f, 1.0f); transform.anchorMax = new Vector2(1.0f, 1.0f); break;
				case TextAnchor.MiddleLeft:   transform.anchorMin = new Vector2(0.0f, 0.5f); transform.anchorMax = new Vector2(0.0f, 0.5f); break;
				case TextAnchor.MiddleCenter: transform.anchorMin = new Vector2(0.5f, 0.5f); transform.anchorMax = new Vector2(0.5f, 0.5f); break;
				case TextAnchor.MiddleRight:  transform.anchorMin = new Vector2(1.0f, 0.5f); transform.anchorMax = new Vector2(1.0f, 0.5f); break;
				case TextAnchor.LowerLeft:    transform.anchorMin = new Vector2(0.0f, 0.0f); transform.anchorMax = new Vector2(0.0f, 0.0f); break;
				case TextAnchor.LowerCenter:  transform.anchorMin = new Vector2(0.5f, 0.0f); transform.anchorMax = new Vector2(0.5f, 0.0f); break;
				case TextAnchor.LowerRight:   transform.anchorMin = new Vector2(1.0f, 0.0f); transform.anchorMax = new Vector2(1.0f, 0.0f); break;
			}

			// set the pivot (destination point) on the window that will be used as reference in anchoredPosition
			switch (destinationPivot)
			{
				case TextAnchor.UpperLeft:    transform.pivot = new Vector2(0.0f, 1.0f); break;
				case TextAnchor.UpperCenter:  transform.pivot = new Vector2(0.5f, 1.0f); break;
				case TextAnchor.UpperRight:   transform.pivot = new Vector2(1.0f, 1.0f); break;
				case TextAnchor.MiddleLeft:   transform.pivot = new Vector2(0.0f, 0.5f); break;
				case TextAnchor.MiddleCenter: transform.pivot = new Vector2(0.5f, 0.5f); break;
				case TextAnchor.MiddleRight:  transform.pivot = new Vector2(1.0f, 0.5f); break;
				case TextAnchor.LowerLeft:    transform.pivot = new Vector2(0.0f, 0.0f); break;
				case TextAnchor.LowerCenter:  transform.pivot = new Vector2(0.5f, 0.0f); break;
				case TextAnchor.LowerRight:   transform.pivot = new Vector2(1.0f, 0.0f); break;
			}

			// distance in pixels between the anchor and the pivot
			// TODO : compute delta as an offset depending on the origin/pivot, instead of having to provide coordinates
			// ex : if originInParent = UpperLeft & destinationPivot = LowerRight, and offset X / Y = -50
			// deltaX should be 50, deltaY should be -50
			// ----------
			// | object |
			// |        |
			// |    O---------  O => originInParent
			// |    |   |    |
			// ---------P    |  P => destinationPivot
			//      |        |
			//      | parent |
			//      ----------
			transform.anchoredPosition = new Vector2(deltaX, deltaY);
			return transform;
		}

		/// <summary>
		/// Set anchors, pivot, anchoredPosition and sizeDelta to dynamically stretch this transform in its parent
		/// </summary>
		/// <param name="inset">optional inset from the parent edges</param>
		public static RectTransform SetStretchInParent(this RectTransform transform, int inset = 0)
		{
			transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.pivot = Vector2.zero;
			transform.anchoredPosition = new Vector2(inset, inset);
            transform.sizeDelta = new Vector2(inset * -2, inset * -2);
            return transform;
        }

        /// <summary>
        /// Set anchors, pivot, anchoredPosition and sizeDelta to dynamically stretch this transform in its parent.
        /// </summary>
        public static RectTransform SetStretchInParent(this RectTransform transform, float insetTop, float insetBottom, float insetLeft, float insetRight)
        {
            transform.anchorMin = Vector2.zero;
            transform.anchorMax = Vector2.one;
            transform.pivot = Vector2.zero;
            transform.anchoredPosition = new Vector2(insetLeft, insetBottom);
            transform.sizeDelta = new Vector2((insetLeft + insetRight) * -1, (insetTop + insetBottom) * -1);
			return transform;
        }

        /// <summary>
        /// Sets the sizeDelta values of the rectTransform. Note : setting sizeDelta needs a non-stretch anchors setting.
        /// Also, the values will be overridden if the parent is a horizontal/vertical layout and/or if SetLayout values have been defined.
        /// </summary>
        /// <param name="transform"></param>
        /// <param name="sizeX"></param>
        /// <param name="sizeY"></param>
        public static RectTransform SetSizeDelta(this RectTransform transform, int sizeX, int sizeY)
		{
			transform.sizeDelta = new Vector2(sizeX, sizeY);
			return transform;
        }

		public static HorizontalLayoutGroup AddHorizontalLayoutGroup(this GameObject gameObject)
		{
            HorizontalLayoutGroup layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 0f;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
			return layoutGroup;
        }

        public static VerticalLayoutGroup AddVerticalLayoutGroup(this GameObject gameObject)
        {
            VerticalLayoutGroup layoutGroup = gameObject.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 0f;
            layoutGroup.padding = new RectOffset(0, 0, 0, 0);
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = false;
            return layoutGroup;
        }
    }
}
