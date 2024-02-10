using System;
using System.Collections;
using System.Collections.Generic;
using Smooth.Compare.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static KsmUI.KsmUIBase;
using Color = UnityEngine.Color;

/*

LAYOUT SYSTEM :

By default, we don't use layout and can set stuff position / size with rectransform stuff
The ctors for the various components will set a default RectTransform based size.

If the user want to use the layout system, it must :
- Call SetupAutoLayout() on all objects from top to bottom
  The implementation of this add the various LayoutElement or Groups needed on the top object and
  sub-objects, initialized at default sizes and to be "self-expanding".
- Call SetAutoLayout() to override the Layout properties
 */


namespace KsmUI
{
    public static class KsmUIBaseExtensions
    {
        /// <inheritdoc cref="KsmUIBase.SetParentImpl" />
        public static T SetParent<T>(this T instance, KsmUIBase parent) where T : KsmUIBase
        {
			instance.SetParentImpl(parent);
			return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetUpdateActionImpl" />
        public static T SetUpdateAction<T>(this T instance, Action action, float updateFrequency = -1f) where T : KsmUIBase
        {
            instance.SetUpdateActionImpl(action, updateFrequency);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetUpdateCoroutineImpl" />
        public static T SetUpdateCoroutine<T>(this T instance, KsmUIUpdateCoroutine coroutineFactory) where T : KsmUIBase
        {
            instance.SetUpdateCoroutineImpl(coroutineFactory);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetTooltipImpl(string, TextAlignmentOptions, int)" />
        public static T SetTooltip<T>(this T instance, string text, TextAlignmentOptions textAlignement = TextAlignmentOptions.TopLeft, int maxWidth = 300) where T : KsmUIBase
        {
            instance.SetTooltipImpl(text, textAlignement, maxWidth);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetTooltipImpl(Func{string}, TextAlignmentOptions, int)" />
        public static T SetTooltip<T>(this T instance, Func<string> textFunc, TextAlignmentOptions textAlignement = TextAlignmentOptions.TopLeft, int maxWidth = 300) where T : KsmUIBase
        {
            instance.SetTooltipImpl(textFunc, textAlignement, maxWidth);
            return instance;
        }

		/// <inheritdoc cref="KsmUIBase.SetTooltipImpl(Func{KsmUIBase})" />
		public static T SetTooltip<T>(this T instance, Func<KsmUIBase> contentBuilder) where T : KsmUIBase
        {
            instance.SetTooltipImpl(contentBuilder);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetLayoutImpl" />
        public static T SetLayout<T>(this T instance, bool flexibleWidth = false, bool flexibleHeight = false, int preferredWidth = -1, int preferredHeight = -1, int minWidth = -1, int minHeight = -1) where T : KsmUIBase
        {
            instance.SetLayoutImpl(flexibleWidth ? 1f : -1f, flexibleHeight ? 1f : -1f, preferredWidth, preferredHeight, minWidth, minHeight);
            return instance;
        }

        public static T SetLayout<T>(this T instance, int preferredWidth = -1, int preferredHeight = -1) where T : KsmUIBase
        {
            instance.SetLayoutImpl(-1f, -1f, preferredWidth, preferredHeight, -1, -1);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetStaticSizeAndPositionImpl" />
        public static T SetStaticSizeAndPosition<T>(this T instance, int width, int height, int horizontalOffset = 0, int verticalOffset = 0, HorizontalEdge horizontalEdge = HorizontalEdge.Left, VerticalEdge verticalEdge = VerticalEdge.Top) where T : KsmUIBase
        {
            instance.SetStaticSizeAndPositionImpl(width, height, horizontalOffset, verticalOffset, horizontalEdge, verticalEdge);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetStretchInParentImpl" />
        public static T SetStretchInParent<T>(this T instance) where T : KsmUIBase
        {
            instance.SetStretchInParentImpl();
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.MoveAsFirstChildImpl" />
        public static T MoveAsFirstChild<T>(this T instance) where T : KsmUIBase
        {
            instance.MoveAsFirstChildImpl();
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.MoveAsLastChild" />
        public static T MoveAsLastChild<T>(this T instance) where T : KsmUIBase
        {
            instance.MoveAsLastChild();
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.MoveToSiblingIndex" />
        public static T MoveToSiblingIndex<T>(this T instance, int index) where T : KsmUIBase
        {
            instance.MoveToSiblingIndex(index);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.MoveAfter" />
        public static T MoveAfter<T>(this T instance, KsmUIBase afterThis) where T : KsmUIBase
        {
            instance.MoveAfter(afterThis);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetDestroyCallback(Action)" />
        public static T SetDestroyCallback<T>(this T instance, Action onDestroy) where T : KsmUIBase
        {
            instance.SetDestroyCallback(onDestroy);
            return instance;
        }

        /// <inheritdoc cref="KsmUIBase.SetBackgroundColorImpl(Color)" />
        public static T SetBackgroundColor<T>(this T instance, Color color) where T : KsmUIBase
        {
            instance.SetBackgroundColorImpl(color);
            return instance;
        }

    }

    public class KsmUIBase
	{
        public RectTransform TopTransform { get; private set; }
		public GameObject TopObject { get; private set; }
		public LayoutElement LayoutElement { get; private set; }
		public KsmUIUpdateHandler UpdateHandler { get; private set; }
		public KsmUILayoutOptimizer LayoutOptimizer { get; private set; }
		private KsmUITooltipBase tooltip;

		private Image colorComponent;

		/// <summary>
		/// transform that will be used as parent for child KsmUI objects.
		/// override this if you have an internal object hierarchy where child
		/// objects must be parented to a specific transform (ex : scroll view)
		/// </summary>
		public virtual RectTransform ParentTransformForChilds => TopTransform;

		internal KsmUIBase() { }

		internal void WindowSetup(GameObject contentTopObject, LayoutElement contentLayoutElement = null)
		{
			TopObject = contentTopObject;
			TopTransform = (RectTransform)contentTopObject.transform;
			LayoutElement = contentLayoutElement;
			LayoutOptimizer = TopObject.AddComponent<KsmUILayoutOptimizer>();
			TopObject.SetLayerRecursive(5);
		}

		public KsmUIBase(KsmUIBase parent)
		{
			TopObject = new GameObject(Name);
			TopTransform = TopObject.AddComponent<RectTransform>();
			TopObject.AddComponent<CanvasRenderer>();

			if (parent != null)
			{
				LayoutOptimizer = parent.LayoutOptimizer;
				LayoutOptimizer.SetDirty();
				LayoutOptimizer.RebuildLayout();
				TopTransform.SetParentFixScale(parent.ParentTransformForChilds);
			}
			else
			{
				LayoutOptimizer = TopObject.AddComponent<KsmUILayoutOptimizer>();
			}

			TopObject.SetLayerRecursive(5);
		}

        internal void SetParentImpl(KsmUIBase parent)
		{
			LayoutOptimizer = parent.LayoutOptimizer;
			LayoutOptimizer.SetDirty();
			LayoutOptimizer.RebuildLayout();
			TopTransform.SetParentFixScale(parent.ParentTransformForChilds);
		}

		public virtual string Name => GetType().Name;

		public virtual bool Enabled
		{
			get => TopObject.activeSelf;
			set
			{
				if (value == TopObject.activeSelf)
					return;

				TopObject.SetActive(value);

				// enabling/disabling an object almost always require a layout rebuild
				LayoutOptimizer.RebuildLayout();

				// if enabling and update frequency is more than every update, update immediately
				if (value && UpdateHandler != null)
				{
					UpdateHandler.UpdateASAP();
				}
			}
		}

        /// <summary> callback that will be called on this object Update(). Won't be called if Enabled = false </summary>
        /// <param name="updateFrequency">seconds between updates, or set to 0f to update every frame</param>
        internal void SetUpdateActionImpl(Action action, float updateFrequency = -1f)
		{
			if (UpdateHandler == null)
				UpdateHandler = TopObject.AddComponent<KsmUIUpdateHandler>();

			UpdateHandler.updateAction = action;
			UpdateHandler.updateFrequency = updateFrequency == -1f ? Lib.RandomFloat(0.18f, 0.22f) : updateFrequency;
			UpdateHandler.UpdateASAP();
		}

        /// <summary> coroutine-like (IEnumerable) method that will be called repeatedly as long as Enabled = true </summary>
        internal void SetUpdateCoroutineImpl(KsmUIUpdateCoroutine coroutineFactory)
		{
			if (UpdateHandler == null)
				UpdateHandler = TopObject.AddComponent<KsmUIUpdateHandler>();

			UpdateHandler.coroutineFactory = coroutineFactory;
		}

        public void ForceExecuteCoroutine(bool fromStart = false)
		{
			if (UpdateHandler != null)
				UpdateHandler.ForceExecuteCoroutine(fromStart);
		}

		/// <summary>
		/// Set a static text tooltip when hovering on this element
		/// </summary>
		/// <param name="text">tooltip text</param>
		/// <param name="textAlignement">text alignement</param>
		/// <param name="maxWidth">tooltip max width</param>
        internal void SetTooltipImpl(string text, TextAlignmentOptions textAlignement = TextAlignmentOptions.Top, int maxWidth = 300)
		{
			if (ReferenceEquals(tooltip, null))
			{
				tooltip = TopObject.AddComponent<KsmUITooltipStaticText>();
				((KsmUITooltipStaticText)tooltip).Setup(text, textAlignement, maxWidth);
			}
			else if (tooltip is KsmUITooltipStaticText staticTextTooltip)
			{
				staticTextTooltip.SetText(text);
			}
			else
			{
				Lib.Log($"Can't set tooltip text : tooltip isn't defined or isn't a static text tooltip", Lib.LogLevel.Error);
			}
		}

        /// <summary>
        /// Set a dynamic text tooltip when hovering on this element
        /// </summary>
        /// <param name="textFunc">delegate returning the tooltip text</param>
        /// <param name="textAlignement">text alignement</param>
        /// <param name="maxWidth">tooltip max width</param>
        internal void SetTooltipImpl(Func<string> textFunc, TextAlignmentOptions textAlignement = TextAlignmentOptions.Top, int maxWidth = 300)
		{
			if (ReferenceEquals(tooltip, null))
				tooltip = TopObject.AddComponent<KsmUITooltipDynamicText>();

			((KsmUITooltipDynamicText)tooltip).Setup(textFunc, textAlignement, maxWidth);
		}

        /// <summary>
        /// Set a dynamic tooltip that can contain KsmUI elements when hovering on this element
        /// </summary>
        /// <param name="contentBuilder">delegate returning the KsmUI element to show in the tooltip</param>
        internal void SetTooltipImpl(Func<KsmUIBase> contentBuilder)
		{
			if (ReferenceEquals(tooltip, null))
				tooltip = TopObject.AddComponent<KsmUITooltipDynamicContent>();

			((KsmUITooltipDynamicContent)tooltip).Setup(contentBuilder);
		}

        internal void SetTooltipEnabledImpl(bool enabled)
		{
			if (!ReferenceEquals(tooltip, null))
			{
				tooltip.TooltipEnabled = enabled;
				if (!enabled && KsmUITooltipController.Instance.CurrentTooltip == tooltip)
				{
					KsmUITooltipController.Instance.HideTooltip();
				}
			}
		}

        /// <summary> 
		/// Add a LayoutElement component to implement dynamic sizing constraints
		/// </summary>
        internal virtual void SetLayoutImpl(float flexibleWidth = -1f, float flexibleHeight = -1f, int preferredWidth = -1, int preferredHeight = -1, int minWidth = -1, int minHeight = -1)
		{
			if (LayoutElement == null)
				LayoutElement = TopObject.AddComponent<LayoutElement>();

			LayoutElement.flexibleWidth = flexibleWidth;
            LayoutElement.flexibleHeight = flexibleHeight;
			LayoutElement.preferredWidth = preferredWidth;
			LayoutElement.preferredHeight = preferredHeight;
			LayoutElement.minWidth = minWidth;
			LayoutElement.minHeight = minHeight;
		}

		public enum HorizontalEdge { Left, Right }
		public enum VerticalEdge { Top, Bottom }

        /// <summary>
        /// Set size and relative position from parent edges. Only works for dimensions not already controlled by a LayoutElement.
        /// </summary>
        internal void SetStaticSizeAndPositionImpl(int width, int height, int horizontalOffset = 0, int verticalOffset = 0, HorizontalEdge horizontalEdge = HorizontalEdge.Left, VerticalEdge verticalEdge = VerticalEdge.Top)
		{
			TopTransform.anchorMin = new Vector2(horizontalEdge == HorizontalEdge.Left ? 0f : 1f, verticalEdge == VerticalEdge.Top ? 1f : 0f);
			TopTransform.anchorMax = TopTransform.anchorMin;

			TopTransform.sizeDelta = new Vector2(width, height);
			TopTransform.anchoredPosition = new Vector2(
				horizontalEdge == HorizontalEdge.Left ? horizontalOffset + width * TopTransform.pivot.x : horizontalOffset - width * (1f - TopTransform.pivot.x),
				verticalEdge == VerticalEdge.Top ? -verticalOffset - height * TopTransform.pivot.y : -verticalOffset + height * (1f - TopTransform.pivot.y));
		}

        /// <summary>
        /// Stretch the object transform to match its parent size and position. Only works if the parent has no layout component
        /// </summary>
        internal void SetStretchInParentImpl()
		{
			TopTransform.anchorMin = Vector2.zero;
			TopTransform.anchorMax = Vector2.one;
			TopTransform.sizeDelta = Vector2.zero;
		}

		public void RebuildLayout() => LayoutOptimizer.RebuildLayout();



        internal void MoveAsFirstChildImpl()
		{
			TopTransform.SetAsFirstSibling();
		}

        internal void MoveAsLastChild()
		{
			TopTransform.SetAsLastSibling();
		}

        internal void MoveToSiblingIndex(int index)
		{
			TopTransform.SetSiblingIndex(index);
		}

        public bool MoveAfter(KsmUIBase afterThis)
		{
			for (int i = 0; i < TopTransform.childCount; i++)
			{
				if (TopTransform.GetChild(i).transform == afterThis.TopTransform)
				{
					TopTransform.SetSiblingIndex(i + 1);
					return true;
				}
			}

			return false;
		}

        internal void SetDestroyCallback(Action callback)
		{
			KsmUIDestroyCallback destroyCallback = TopObject.AddComponent<KsmUIDestroyCallback>();
			destroyCallback.SetCallback(callback);
		}

		public void Destroy()
		{
			TopObject.DestroyGameObject();
			RebuildLayout();
		}




        /// <summary>
        /// Add a Color component with the specified color to the top GameObject, or change the existing color of the component.
        /// The GameObject can't already have a graphic component (image, text...).
        /// </summary>
        /// <param name="color">If set to default, will add a black color with 20% transparency</param>
        internal void SetBackgroundColorImpl(Color color)
		{
			if (colorComponent == null)
			{
				colorComponent = TopObject.GetComponent<Image>();

				if (colorComponent == null)
				{
					try
					{
						colorComponent = TopObject.AddComponent<Image>();
					}
					catch (Exception)
					{
						Lib.LogDebugStack($"Can't set background color on {this}, it already has a graphic component", Lib.LogLevel.Warning);
						return;
					}
				}
			}

            colorComponent.color = color;
		}

		public void SetBoxColor() => SetBackgroundColorImpl(KsmUIStyle.boxColor);
	}
}
