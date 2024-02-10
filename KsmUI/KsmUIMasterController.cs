using System;
using System.Collections.Generic;
using KSP.UI;
using UnityEngine;
using UnityEngine.UI;

namespace KsmUI
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KsmUIBootstrapper : MonoBehaviour
	{
        private void Start()
		{
            UIMasterController.Instance.gameObject.AddOrGetComponent<KsmUIMasterController>();
			GameEvents.OnGameDatabaseLoaded.Add(OnGameDatabaseLoaded);
        }

		private void OnGameDatabaseLoaded()
		{
			Textures.Init();
            GameEvents.OnGameDatabaseLoaded.Remove(OnGameDatabaseLoaded);
            Destroy(this);
        }
    }

	public class KsmUIMasterController : MonoBehaviour
	{
		public static KsmUIMasterController Instance { get; private set; }

		public GameObject KsmUICanvas { get; private set; }
		public RectTransform KsmUITransform { get; private set; }
		public CanvasScaler CanvasScaler { get; private set; }
		public GraphicRaycaster GraphicRaycaster { get; private set; }
		public Canvas Canvas { get; private set; }

		private void Start()
		{
			Instance = this;

            // Add tooltip controller to the tooltip canvas
            UIMasterController.Instance.tooltipCanvas.gameObject.AddComponent<KsmUITooltipController>();
			UIMasterController.Instance.tooltipCanvas.gameObject.AddComponent<KsmUIContextMenu>();

			// create our own canvas as a child of the UIMaster object. this allow :
			// - using an independant scaling factor
			// - setting the render order as we need
			// - making our UI framework completely independant from any KSP code
			KsmUICanvas = new GameObject("KsmUICanvas");
			KsmUITransform = KsmUICanvas.AddComponent<RectTransform>();

			Canvas = KsmUICanvas.AddComponent<Canvas>();
			Canvas.renderMode = RenderMode.ScreenSpaceCamera;
			Canvas.pixelPerfect = false;
			Canvas.worldCamera = UIMasterController.Instance.uiCamera;
			Canvas.sortingLayerName = "ScreenMessages"; //"Dialogs"; // it seems this is actually handling the sorting, not the Z value...
			Canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent;

			// render order of the various UI canvases (lower value = on top)
			// maincanvas => Z 750
			// appCanvas => Z 625
			// actionCanvas => Z 500 (PAW)
			// screenMessageCanvas => Z 450
			// dialogCanvas => Z 400 (DialogGUI windows)
			// dragDropcanvas => Z 333
			// debugCanvas => Z 315
			// tooltipCanvas => Z 300

			// above the PAW but behind the stock dialogs
			Canvas.planeDistance = 475f;
			CanvasScaler = KsmUICanvas.AddComponent<CanvasScaler>();

			// note : we don't use app scale, but it might become necessary with fixed-position windows that are "attached" to the app launcher (toolbar buttons)
			CanvasScaler.scaleFactor = GameSettings.UI_SCALE;

			GraphicRaycaster = KsmUICanvas.AddComponent<GraphicRaycaster>();
			CanvasGroup test = KsmUICanvas.AddComponent<CanvasGroup>();
			test.alpha = 1f;
			test.blocksRaycasts = true;
			test.interactable = true;

			KsmUITransform.SetParentFixScale(UIMasterController.Instance.transform);

			// things not on layer 5 will not be rendered
			KsmUICanvas.SetLayerRecursive(5);
			Canvas.ForceUpdateCanvases();

			GameEvents.onUIScaleChange.Add(OnScaleChange);
		}

		private void OnScaleChange()
		{
			CanvasScaler.scaleFactor = GameSettings.UI_SCALE;
		}


	}
}
