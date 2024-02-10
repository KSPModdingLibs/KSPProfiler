using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KsmUI
{
	public class KsmUIDestroyCallback : MonoBehaviour
	{
		private Action callback;

		public void SetCallback(Action callback)
		{
			this.callback = callback;
		}

		private void OnDestroy()
		{
			callback();
		}
	}
}
