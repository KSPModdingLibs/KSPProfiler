using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIColorPicker : KsmUIImage
	{

		private DragPanel dragPanel;
		private KsmUIImage selector;
		private Picker colorPicker;
		private Color[] Data;

		public Color Color { get; private set; }

		public static void PickerPopup(ref Color color, int colorViewerHeight = 30, Texture2D textureToTestColorOn = null, int textureWidth = -1, int textureHeight = -1)
		{
			KsmUIWindow window = new KsmUIWindow(KsmUILib.Orientation.Vertical).SetDraggable(true).SetSpacing(5);
			KsmUIHeader header = new KsmUIHeader(window, "Color picker");
			header.AddButton(Textures.KsmUITexHeaderClose, window.Close);
			KsmUIColorPicker picker = new KsmUIColorPicker(window);
			picker.SetLayoutImpl(-1f, -1f, -1, -1, 125, 100);

			if (textureToTestColorOn != null && textureWidth <= 0 && textureHeight <= 0)
			{
				textureWidth = colorViewerHeight;
				textureHeight = colorViewerHeight;
			}

			KsmUIImage test = new KsmUIImage(window, textureToTestColorOn, textureWidth, textureHeight);
			test.SetLayoutImpl(1f, -1f, -1, colorViewerHeight);
			test.SetUpdateActionImpl(() => test.SetIconColor(picker.Color));
		}

		public KsmUIColorPicker(KsmUIBase parent, Action onColorPicked = null, bool preventWindowDrag = true) : base(parent, Textures.KsmUIColorPickerBackground)
		{
			colorPicker = TopObject.AddComponent<Picker>();
			colorPicker.picker = this;

			TopObject.AddComponent<RectMask2D>();

			selector = new KsmUIImage(this, Textures.KsmUIColorPickerSelector, 16, 16);
			selector.Enabled = false;

			Data = Textures.KsmUIColorPickerBackground.GetPixels();

			SetOnColorPickedAction(onColorPicked);

			if (preventWindowDrag)
			{
				dragPanel = TopObject.GetComponentInParent<DragPanel>();
			}
		}

		public void SetOnColorPickedAction(Action action)
		{
			colorPicker.onColorPicked = action;
		}

		private class Picker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
		{
			private const int textureWidth = 500;
			private const int textureHeight = 400;

			private Camera camera;
			private bool mouse_over = false;
			public KsmUIColorPicker picker;
			public Action onColorPicked;

			public Color color;

			private void Update()
			{
				if (Input.GetMouseButton(0) && mouse_over)
				{
					RectTransformUtility.ScreenPointToLocalPointInRectangle(picker.IconTransform, Input.mousePosition, camera, out Vector2 localpoint);
					Vector2 normalizedPoint = Rect.PointToNormalized(picker.IconTransform.rect, localpoint);
					int x = (int)(textureWidth * normalizedPoint.x);
					int y = (int)(textureHeight * normalizedPoint.y);
					int index = y * textureWidth + x;

					if (index < picker.Data.Length)
						picker.Color = picker.Data[index];
					else
						picker.Color = Color.white;

					color = picker.Color;

					picker.selector.TopTransform.anchorMin = normalizedPoint;
					picker.selector.TopTransform.anchorMax = normalizedPoint;
					picker.selector.Enabled = true;

					if (onColorPicked != null)
					{
						onColorPicked();
					}
				}
			}

			public void OnPointerEnter(PointerEventData eventData)
			{
				mouse_over = true;
				camera = eventData.enterEventCamera;

				if (picker.dragPanel != null)
				{
					picker.dragPanel.enabled = false;
				}
			}

			public void OnPointerExit(PointerEventData eventData)
			{
				mouse_over = false;
				camera = eventData.enterEventCamera;

				if (picker.dragPanel != null)
				{
					picker.dragPanel.enabled = true;
				}
			}

			private void OnDestroy()
			{
				if (picker.dragPanel != null)
				{
					picker.dragPanel.enabled = true;
				}
			}
		}
	}
}
