using UnityEngine;

namespace KsmUI
{
    public static class Textures
    {
        private static string gameDataDirectory = "KsmUI";

        public static Texture2D empty { get; private set; }

        public static Sprite KsmUISpriteBackground { get; private set; }

        public static Sprite KsmUISpriteBtnNormal { get; private set; }
        public static Sprite KsmUISpriteBtnHighlight { get; private set; }
        public static Sprite KsmUISpriteBtnDisabled { get; private set; }

        public static Texture2D KsmUIColorPickerBackground { get; private set; }
        public static Texture2D KsmUIColorPickerSelector { get; private set; }

        public static Texture2D KsmUITexCheckmark { get; private set; }

        public static Texture2D KsmUITexHeaderArrowsLeft { get; private set; }
        public static Texture2D KsmUITexHeaderArrowsRight { get; private set; }
        public static Texture2D KsmUITexHeaderArrowsUp { get; private set; }
        public static Texture2D KsmUITexHeaderArrowsDown { get; private set; }

        public static Texture2D KsmUITexHeaderClose { get; private set; }
        public static Texture2D KsmUITexHeaderInfo { get; private set; }
        public static Texture2D KsmUITexHeaderRnD { get; private set; }

        public static Texture2D GetTexture(string path)
        {
            return GameDatabase.Instance.GetTexture(gameDataDirectory + "/Icons/" + path, false);
        }

        public static Sprite GetSprite(string path, int width, int height)
        {
            Texture2D tex = GetTexture(path);
            return Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f));
        }

        public static Sprite Get9SlicesSprite(string path, int width, int height, int borderSize)
        {
            // 9 slice sprites are self extending, they don't need to get scaled manually (I think...)
            Texture2D tex = GetTexture(path);
            return Sprite.Create(tex, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 100f, 0u, SpriteMeshType.FullRect, new Vector4(borderSize, borderSize, borderSize, borderSize));
        }

        internal static void Init()
        {
            empty = GetTexture("empty");

            KsmUISpriteBackground = Get9SlicesSprite("background-64-5", 64, 64, 5);
            KsmUISpriteBtnNormal = Get9SlicesSprite("btn-black-64-5", 64, 64, 5);
            KsmUISpriteBtnHighlight = Get9SlicesSprite("btn-black-highlight-64-5", 64, 64, 5);
            KsmUISpriteBtnDisabled = Get9SlicesSprite("btn-black-disabled-64-5", 64, 64, 5);

            KsmUIColorPickerBackground = GetTexture("ColorPicker");
            KsmUIColorPickerSelector = GetTexture("Selector");

            KsmUITexCheckmark = GetTexture("checkmark-20");

            KsmUITexHeaderClose = GetTexture("i8-header-close-32");
            KsmUITexHeaderArrowsLeft = GetTexture("arrows-left-32");
            KsmUITexHeaderArrowsRight = GetTexture("arrows-right-32");
            KsmUITexHeaderArrowsUp = GetTexture("arrows-up-32");
            KsmUITexHeaderArrowsDown = GetTexture("arrows-down-32");

            KsmUITexHeaderClose = GetTexture("i8-header-close-32");
            KsmUITexHeaderInfo = GetTexture("info-32");
            KsmUITexHeaderRnD = GetTexture("i8-rnd-32");
        }


    }
}
