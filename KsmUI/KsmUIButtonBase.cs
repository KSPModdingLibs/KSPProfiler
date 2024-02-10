using UnityEngine.Events;
using UnityEngine.UI;

namespace KsmUI
{
    public abstract class KsmUIButtonBase : KsmUIBase
    {
        public Button ButtonComponent { get; private set; }
        private UnityAction onClick;

        internal void SetButtonOnClickImpl(UnityAction action)
        {
            if (onClick != null)
                ButtonComponent.onClick.RemoveListener(onClick);
            onClick = action;

            if (action != null)
                ButtonComponent.onClick.AddListener(onClick);
        }

        public KsmUIButtonBase(KsmUIBase parent) : base(parent) 
        {
            ButtonComponent = TopObject.AddComponent<Button>();
        }
    }

    public static class KsmUIButtonExtensions
    {
        public static T SetButtonOnClick<T>(this T instance, UnityAction onClick) where T : KsmUIButtonBase
        {
            instance.SetButtonOnClickImpl(onClick);
            return instance;
        }
    }
}
