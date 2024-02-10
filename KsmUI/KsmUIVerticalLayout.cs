using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIVerticalLayout : KsmUIBase, IKsmUILayoutGroup
    {
		public VerticalLayoutGroup LayoutGroup { get; private set; }

        HorizontalOrVerticalLayoutGroup IKsmUILayoutGroup.LayoutGroup => LayoutGroup;

        public KsmUIVerticalLayout(KsmUIBase parent) : base(parent)
		{
			LayoutGroup = TopObject.AddComponent<VerticalLayoutGroup>();
			LayoutGroup.spacing = 0f;
			LayoutGroup.padding = new RectOffset(0, 0 , 0, 0);
			LayoutGroup.childAlignment = TextAnchor.UpperLeft;
			LayoutGroup.childControlHeight = true;
			LayoutGroup.childControlWidth = true;
			LayoutGroup.childForceExpandHeight = false;
			LayoutGroup.childForceExpandWidth = false;
		}
	}
}
