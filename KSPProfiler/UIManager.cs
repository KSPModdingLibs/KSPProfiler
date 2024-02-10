using KsmUI;
using TMPro;

namespace KSPProfiler
{
    public static class UIManager
    {
        public static KsmUIWindow window;

        public static void Toggle()
        {
            if (window == null)
                Create();
            else
                Close();
        }

        private static void Close()
        {
            KSPProfilerAddon.AppButton.SetFalse(false);
            window.Close();
            window = null;
        }

        private static void Create()
        {
            window = new KsmUIWindow(KsmUILib.Orientation.Vertical)
                .SetDraggable(true, -250)
                .SetOpacity(1f)
                .SetLayout(false, false, 600)
                .SetSpacing(5)
                .SetDestroyCallback(() => window = null);

            KsmUIHeader topHeader = new KsmUIHeader(window, "KSP PROFILER");
            topHeader.AddButton(Textures.KsmUITexHeaderClose, Close);

            new GameLoopProfilerUI(window);

            window.LayoutOptimizer.SetDirty();
            window.LayoutOptimizer.RebuildLayout();
        }
    }
}
