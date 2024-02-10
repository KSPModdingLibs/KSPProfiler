using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KsmUI
{
	public static class KsmUITextExtensions
	{
        public static T UseEllipsisWithTooltip<T>(this T instance) where T : KsmUIText
		{
			instance.UseEllipsisWithTooltip();
			return instance;
        }
    }

    public class KsmUIText : KsmUIBase, IKsmUIText
	{
		public TextMeshProUGUI TextComponent { get; private set; }

        private TextAlignmentOptions savedAlignement;
		private bool useEllipsisWithTooltip = false;

		public KsmUIText(KsmUIBase parent) : base(parent)
		{
			savedAlignement = TextAlignmentOptions.TopLeft;
			TextComponent = TopObject.AddComponent<TextMeshProUGUI>();
            TextComponent.color = KsmUIStyle.textColor;
			TextComponent.font = KsmUIStyle.textFont;
			TextComponent.fontSize = KsmUIStyle.textSize;
			TextComponent.alignment = TextAlignmentOptions.TopLeft;
			TextComponent.enableWordWrapping = true;
			TextComponent.overflowMode = TextOverflowModes.Overflow;
		}

        public KsmUIText(KsmUIBase parent, string text) : base(parent)
        {
            savedAlignement = TextAlignmentOptions.TopLeft;
            TextComponent = TopObject.AddComponent<TextMeshProUGUI>();
            TextComponent.color = KsmUIStyle.textColor;
            TextComponent.font = KsmUIStyle.textFont;
            TextComponent.fontSize = KsmUIStyle.textSize;
            TextComponent.alignment = TextAlignmentOptions.TopLeft;
            TextComponent.enableWordWrapping = true;
            TextComponent.overflowMode = TextOverflowModes.Overflow;
			TextComponent.text = text;
        }

        // note : this only works reliably with the ellipsis mode, not with the truncate mode...
        internal void UseEllipsisWithTooltip()
		{
			TextComponent.overflowMode = TextOverflowModes.Ellipsis;
			TextComponent.enableWordWrapping = true;
			useEllipsisWithTooltip = true;
			SetTooltipImpl(string.Empty);
		}

		public string Text
		{
			get => TextComponent.text;
			set
			{
				if (value == null)
					value = string.Empty;

                // DON'T USE SetText() !!!!
                // SetText() won't check :
                // - if the text has actually changed
                // - if the changed text is actually causing a size change
                // This reduce occurences of the canvas being marked dirty by 99+%,
                // For complex windows, the savings can be several ms per update !
                TextComponent.text = value;


                if (useEllipsisWithTooltip)
				{
					TextComponent.ForceMeshUpdate();
					if (TextComponent.isTextTruncated)
					{
						SetTooltipImpl(value);
					}
					else
					{
						SetTooltipImpl(string.Empty);
					}
				}
			}
		}

		// workaround for a textmeshpro bug :
		// https://forum.unity.com/threads/textmeshprougui-alignment-resets-when-enabling-disabling-gameobject.549784/#post-3901597
		public override bool Enabled
		{
			get => base.Enabled;
			set
			{
				base.Enabled = value;
				TextComponent.alignment = savedAlignement;
			}
		}

	}
}
