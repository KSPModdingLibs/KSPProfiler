using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIHorizontalLayout : KsmUIBase, IKsmUILayoutGroup
    {

		public HorizontalLayoutGroup LayoutGroup { get; private set; }

		HorizontalOrVerticalLayoutGroup IKsmUILayoutGroup.LayoutGroup => LayoutGroup;

		public KsmUIHorizontalLayout(KsmUIBase parent) : base(parent)
		{
			LayoutGroup = TopObject.AddComponent<HorizontalLayoutGroup>();
            LayoutGroup.spacing = 0f;
            LayoutGroup.padding = new RectOffset(0, 0, 0, 0);
            LayoutGroup.childAlignment = TextAnchor.UpperLeft;
            LayoutGroup.childControlHeight = true;
            LayoutGroup.childControlWidth = true;
            LayoutGroup.childForceExpandHeight = false;
            LayoutGroup.childForceExpandWidth = false;
        }
	}
}
