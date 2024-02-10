using System;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KsmUI
{
	public class KsmUITooltipBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public bool TooltipEnabled { get; set; } = true;

		public virtual void OnShowTooltip() {}

		public virtual void OnHideTooltip() {}

		public virtual void OnTooltipUpdate() {}

		public void OnPointerEnter(PointerEventData eventData)
		{
			KsmUITooltipController.Instance.ShowTooltip(this);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			KsmUITooltipController.Instance.HideTooltip();
		}

		private void OnDisable()
		{
			if (KsmUITooltipController.Instance.CurrentTooltip == this)
			{
				KsmUITooltipController.Instance.HideTooltip();
			}
		}

		private void OnDestroy()
		{
			if (KsmUITooltipController.Instance.CurrentTooltip == this)
			{
				KsmUITooltipController.Instance.HideTooltip();
			}
		}
	}

	public class KsmUITooltipStaticText : KsmUITooltipBase
	{
		private string tooltipText;
		private TextAlignmentOptions textAlignement;
		private int maxWidth;

		public void Setup(string text, TextAlignmentOptions textAlignement = TextAlignmentOptions.Top, int maxWidth = 300)
		{
			SetText(text);
			this.textAlignement = textAlignement;
			this.maxWidth = maxWidth;
		}

		public void SetText(string text)
		{
			if (string.IsNullOrEmpty(text))
			{
				TooltipEnabled = false;
			}
			else
			{
				TooltipEnabled = true;
				tooltipText = text;

				if (KsmUITooltipController.Instance.CurrentTooltip == this)
				{
					KsmUITooltipController.Instance.TextComponent.text = tooltipText;
				}
			}
		}

		public override void OnShowTooltip()
		{
			KsmUITooltipController controller = KsmUITooltipController.Instance;
			controller.TextComponent.enabled = true;
			controller.TextComponent.alignment = textAlignement;
			controller.TextComponent.text = tooltipText;
			controller.SetMaxWidth(maxWidth);
		}
	}

	public class KsmUITooltipDynamicText : KsmUITooltipBase
	{
		public Func<string> textFunc;
		private TextAlignmentOptions textAlignement;
		private int maxWidth;
		private string text;

		public void Setup(Func<string> textFunc, TextAlignmentOptions textAlignement = TextAlignmentOptions.Top, int maxWidth = 300)
		{
			this.textFunc = textFunc;
			this.textAlignement = textAlignement;
			this.maxWidth = maxWidth;
		}

		public override void OnShowTooltip()
		{
			KsmUITooltipController controller = KsmUITooltipController.Instance;
			controller.TextComponent.enabled = true;
			controller.TextComponent.alignment = textAlignement;
			controller.TextComponent.text = textFunc();
			controller.SetMaxWidth(maxWidth);
		}

		public override void OnTooltipUpdate()
		{
			text = textFunc();

			if (string.IsNullOrEmpty(text))
			{
				TooltipEnabled = false;
			}
			else
			{
				TooltipEnabled = true;
			}

			KsmUITooltipController.Instance.TextComponent.text = text;
		}
	}

	public class KsmUITooltipDynamicContent : KsmUITooltipBase
    {
		public Func<KsmUIBase> contentBuilder;
		public KsmUIBase content;

		public void Setup(Func<KsmUIBase> contentBuilder)
		{
			this.contentBuilder = contentBuilder;
		}

		public override void OnShowTooltip()
		{
			KsmUITooltipController controller = KsmUITooltipController.Instance;
			controller.TextComponent.enabled = false;
			controller.SetMaxWidth(-1);
			content = contentBuilder();
			content.LayoutOptimizer.enabled = false;
			content.TopTransform.SetParentFixScale(controller.ContentTransform);
		}

		public override void OnHideTooltip()
		{
			if (content != null)
			{
				content.TopObject.DestroyGameObject();
				content = null;
			}
		}
	}
}
