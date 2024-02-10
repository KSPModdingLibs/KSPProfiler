using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KsmUI
{
	public class KsmUIBox : KsmUIBase
	{
		public KsmUIBox(KsmUIBase parent, Kolor backgroundColor = null) : base(parent)
		{
			RawImage image = TopObject.AddComponent<RawImage>();

			if (backgroundColor == null)
				image.color = Color.clear;
			else
				image.color = KsmUIStyle.boxColor;

		}
	}
}
