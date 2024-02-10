using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static KsmUI.KsmUIInputField;
using static TMPro.TMP_InputField;

namespace KsmUI
{
    public static class KsmUIInputFieldExtensions
    {


        public static T SetOnInputCallback<T>(this T instance, UnityAction<string> onEndEdit) where T : KsmUIInputField
        {
            instance.SetOnEndEditImpl(onEndEdit);
            return instance;
        }

        public static T SetOnInputCallback<T>(this T instance, OnExit onEndEdit) where T : KsmUIInputField
        {
            instance.SetOnEndEditChangeTextImpl(onEndEdit);
            return instance;
        }

        public static T SetPlaceholderText<T>(this T instance, string text) where T : KsmUIInputField
        {
            instance.SetPlaceholderTextImpl(text);
            return instance;
        }

        public static T SetText<T>(this T instance, string text) where T : KsmUIInputField
        {
            instance.InputField.text = text;
            return instance;
        }

        public static T SetCharLimit<T>(this T instance, int charLimit = 0) where T : KsmUIInputField
        {
            instance.InputField.characterLimit = charLimit;
            return instance;
        }

        public static T SetLineType<T>(this T instance, LineType lineType) where T : KsmUIInputField
        {
            instance.InputField.lineType = lineType;
            return instance;
        }

        public static T SetContentType<T>(this T instance, ContentType contentType) where T : KsmUIInputField
        {
            instance.InputField.contentType = contentType;
            return instance;
        }
    }

    public class KsmUIInputField : KsmUIBase
    {
        public TMP_InputField InputField { get; private set; }
        private RectTransform textArea;
        private TextMeshProUGUI textComponent;
        private TextMeshProUGUI placeholder;
        private UnityAction<string> onEndEditCallback;
        private OnExit onEndEditChangeTextCallback;
        private string keyboardLockId;

        public delegate string OnExit(string input);

        public KsmUIInputField(KsmUIBase parent) : base(parent)
        {
            SetBackgroundColorImpl(KsmUIStyle.selectedBoxColor);
            InputField = TopObject.AddComponent<TMP_InputField>();

            textArea = KsmUILib.NewUIGameObject("Text Area", TopTransform, true);
            textArea.SetStretchInParent(1, 1, 4, 4);
            RectMask2D rectMask = textArea.gameObject.AddComponent<RectMask2D>();
            rectMask.padding = new Vector4(-4, -1, -4, -1); // 2 px left/right, 1px top/bottom

            RectTransform text = KsmUILib.NewUIGameObject("Text", textArea, true);
            textComponent = text.gameObject.AddComponent<TextMeshProUGUI>();
            text.SetStretchInParent();
            
            textComponent.color = KsmUIStyle.textColor;
            textComponent.font = KsmUIStyle.textFont;
            textComponent.fontSize = KsmUIStyle.textSize;
            textComponent.alignment = TextAlignmentOptions.TopLeft;
            textComponent.enableWordWrapping = true;
            textComponent.overflowMode = TextOverflowModes.Overflow;

            InputField.textViewport = textArea;
            InputField.textComponent = textComponent;
            InputField.navigation = new Navigation() { mode = Navigation.Mode.None };
            InputField.transition = Selectable.Transition.None;
            InputField.contentType = ContentType.Standard;
            InputField.lineType = LineType.SingleLine;
            InputField.fontAsset = KsmUIStyle.textFont;
            InputField.pointSize = KsmUIStyle.textSize;
            InputField.richText = false;
            InputField.customCaretColor = true;
            InputField.caretColor = Color.white;
            InputField.selectionColor = new Color(0.3f, 0.6f, 1f, 0.5f);

            InputField.enabled = false;
            InputField.enabled = true;


            // called when the input field gets focus
            InputField.onSelect.AddListener(SetInputLock);
            InputField.onEndEdit.AddListener(RemoveInputLock);

            keyboardLockId = TopObject.GetInstanceID().ToString();
        }

        public string Text
        {
            get => InputField.text;
            set => InputField.text = value;
        }

        private void SetInputLock(string s)
        {
            InputLockManager.SetControlLock(ControlTypes.KEYBOARDINPUT, keyboardLockId);
        }

        private void RemoveInputLock(string s)
        {
            InputLockManager.RemoveControlLock(keyboardLockId);
        }

        internal void SetCharLimitImpl(int charLimit = 0)
        {
            InputField.characterLimit = charLimit;
        }

        internal void SetOnEndEditImpl(UnityAction<string> onEndEdit)
        {
            if (onEndEditCallback != null)
                InputField.onEndEdit.RemoveListener(onEndEditCallback);

            onEndEditCallback = onEndEdit;
            if (onEndEdit != null)
                InputField.onEndEdit.AddListener(onEndEditCallback);
        }

        internal void SetOnEndEditChangeTextImpl(OnExit onEndEdit)
        {
            if (onEndEditCallback != null)
                InputField.onEndEdit.RemoveListener(onEndEditCallback);

            if (onEndEdit != null)
            {
                onEndEditCallback = OnEndEditChangeTextCallback;
                InputField.onEndEdit.AddListener(onEndEditCallback);
            }
            else
            {
                onEndEditCallback = null;
            }

            onEndEditChangeTextCallback = onEndEdit;
        }

        private void OnEndEditChangeTextCallback(string input)
        {
            string newText = onEndEditChangeTextCallback(input);
            InputField.text = newText;
        }

        internal void SetPlaceholderTextImpl(string text)
        {
            if (placeholder == null)
            {
                RectTransform placeholderTransform = KsmUILib.NewUIGameObject("Placeholder", textArea, true);
                placeholder = placeholderTransform.gameObject.AddComponent<TextMeshProUGUI>();
                placeholder.color = Kolor.LightGrey;
                placeholder.font = KsmUIStyle.textFont;
                placeholder.fontSize = KsmUIStyle.textSize;
                placeholder.fontStyle = FontStyles.Italic;
                placeholder.alignment = TextAlignmentOptions.TopLeft;
                placeholder.enableWordWrapping = false;
                placeholder.overflowMode = TextOverflowModes.Overflow;
                

                LayoutElement noLayout = placeholderTransform.gameObject.AddComponent<LayoutElement>();
                noLayout.ignoreLayout = true;
                noLayout.layoutPriority = 1;

                InputField.placeholder = placeholder;
                placeholderTransform.SetStretchInParent();
            }

            placeholder.text = text;
        }

        internal void SetLineTypeImpl(LineType lineType)
        {
            InputField.lineType = lineType;
        }
    }
}
