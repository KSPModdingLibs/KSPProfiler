using SoftMasking.Samples;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static PQSCity2;

namespace KsmUI
{
	public static class KsmUIWindowExtensions
	{
		public static T SetDestroyOnClose<T>(this T instance, bool destroyOnClose) where T : KsmUIWindow
        {
			instance.destroyOnClose = destroyOnClose;
			return instance;
        }

        public static T SetOpacity<T>(this T instance, float opacity) where T : KsmUIWindow
        {
            instance.Background.color = new Color(1.0f, 1.0f, 1.0f, opacity);
            return instance;
        }

        public static T SetDraggable<T>(this T instance, bool draggable, int screenBorderOffset = 0) where T : KsmUIWindow
		{
			instance.SetDraggableImpl(draggable, screenBorderOffset);
            return instance;
        }
    }



    public class KsmUIWindow : KsmUIBase, IKsmUILayoutGroup
    {
		private KsmUIInputLock inputLockManager;
        public bool IsDraggable { get; protected set; }
        protected DragPanel DragPanel { get; set; }
        protected ContentSizeFitter SizeFitter { get; set; }
        public Action OnClose { get; set; }
        internal HorizontalOrVerticalLayoutGroup LayoutGroup { get; private set; }
		public Image Background { get; private set; }
        internal bool destroyOnClose;

        HorizontalOrVerticalLayoutGroup IKsmUILayoutGroup.LayoutGroup => LayoutGroup;

        public KsmUIWindow
			(
				KsmUILib.Orientation layoutOrientation,
				TextAnchor screenAnchor = TextAnchor.MiddleCenter,
				TextAnchor windowPivot = TextAnchor.MiddleCenter,
				int posX = 0, int posY = 0
			) : base(null)
		{

			destroyOnClose = true;
			IsDraggable = false;

            TopTransform.SetAnchorsAndPosition(screenAnchor, windowPivot, posX, posY);
			TopTransform.SetParentFixScale(KsmUIMasterController.Instance.KsmUITransform);
			TopTransform.localScale = Vector3.one;

			// our custom lock manager
			inputLockManager = TopObject.AddComponent<KsmUIInputLock>();
			inputLockManager.rectTransform = TopTransform;

            Background = TopObject.AddComponent<Image>();
            Background.sprite = Textures.KsmUISpriteBackground;
            Background.type = Image.Type.Sliced;
            Background.color = new Color(1.0f, 1.0f, 1.0f, 0.8f);

			SizeFitter = TopObject.AddComponent<ContentSizeFitter>();
			SizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			SizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

			switch (layoutOrientation)
			{
				case KsmUILib.Orientation.Vertical: LayoutGroup = TopObject.AddComponent<VerticalLayoutGroup>(); break;
				case KsmUILib.Orientation.Horizontal: LayoutGroup = TopObject.AddComponent<HorizontalLayoutGroup>(); break;
			}

			LayoutGroup.spacing = 0f;
			LayoutGroup.padding = new RectOffset(5, 5, 5, 5);
			LayoutGroup.childControlHeight = true;
			LayoutGroup.childControlWidth = true;
			LayoutGroup.childForceExpandHeight = false;
			LayoutGroup.childForceExpandWidth = false;
			LayoutGroup.childAlignment = TextAnchor.UpperLeft;

			// close on scene changes
			GameEvents.onGameSceneLoadRequested.Add(OnSceneChange);
        }

		public virtual void OnSceneChange(GameScenes data) => Close();

		public void Close()
		{
			if (OnClose != null)
				OnClose();

			if (destroyOnClose)
				TopObject.DestroyGameObject();
			else
				Enabled = false;
		}

		private void OnDestroy()
		{
			GameEvents.onGameSceneLoadRequested.Remove(OnSceneChange);
		}

        internal void SetDraggableImpl(bool draggable, int screenBorderOffset = 0)
        {
            if (!draggable)
            {
                IsDraggable = false;

                if (DragPanel != null)
                    UnityEngine.Object.Destroy(DragPanel);
            }
            else
            {
                IsDraggable = true;
                if (DragPanel == null)
                    DragPanel = TopObject.AddComponent<DragPanel>();

                DragPanel.edgeOffset = screenBorderOffset;
            }
        }

        public bool IsHovering => inputLockManager.IsHovering;

		public void SetOnPointerEnterAction(Action action) => inputLockManager.onPointerEnterAction = action;

		public void SetOnPointerExitAction(Action action) => inputLockManager.onPointerExitAction = action;

		public void StartCoroutine(IEnumerator routine) => LayoutGroup.StartCoroutine(routine);
	}
}
