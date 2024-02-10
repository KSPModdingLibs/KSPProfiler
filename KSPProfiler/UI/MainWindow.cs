using KsmUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace KSPProfiler.UI
{
    public class MainWindow
    {
        public static KsmUIWindow window;

        private static GameLoopProfilerUI gameLoopProfilerUI;
        private static MethodProfilerUI methodProfilerUI;
        private static MethodProfilerResultsUI methodProfilerResultsUI;

        public static void Toggle()
        {
            if (window == null)
                Create();
            else
                Close();
        }

        private static void Close()
        {
            window.Close();
            window = null;
        }

        private static void Create()
        {
            window = new KsmUIWindow(KsmUILib.Orientation.Vertical)
                .SetDraggable(true, -250)
                .SetOpacity(1f)
                .SetLayout(false, false, 600);

            KsmUIHeader topHeader = new KsmUIHeader(window, "KSPProfiler");
            topHeader.AddButton(Textures.KsmUITexHeaderClose, Close);

            KsmUIToggleList<KsmUIBase> tabs = new KsmUIToggleList<KsmUIBase>(window, KsmUILib.Orientation.Horizontal, OnTabSelected);
            tabs.SetLayout(true, false, -1, 18);

            gameLoopProfilerUI = new GameLoopProfilerUI(window);
            methodProfilerUI = new MethodProfilerUI(window);
            methodProfilerUI.Enabled = false;
            methodProfilerResultsUI = new MethodProfilerResultsUI(window);
            methodProfilerResultsUI.Enabled = false;

            new KsmUIToggleListElement<KsmUIBase>(tabs, gameLoopProfilerUI)
                .SetText("GAMELOOP PROFILER")
                .SetTextAlignment(TextAlignmentOptions.Center)
                .SetStyle(FontStyles.Bold);
            new KsmUIToggleListElement<KsmUIBase>(tabs, methodProfilerUI)
                .SetText("METHOD PROFILER SETUP")
                .SetTextAlignment(TextAlignmentOptions.Center)
                .SetStyle(FontStyles.Bold);
            new KsmUIToggleListElement<KsmUIBase>(tabs, methodProfilerResultsUI)
                .SetText("METHOD PROFILER RESULTS")
                .SetTextAlignment(TextAlignmentOptions.Center)
                .SetStyle(FontStyles.Bold);

            window.LayoutOptimizer.SetDirty();
            window.LayoutOptimizer.RebuildLayout();
        }

        private static void OnTabSelected(KsmUIBase tabContent, bool selected)
        {
            tabContent.Enabled = selected;
            window.LayoutOptimizer.SetDirty();
            window.LayoutOptimizer.RebuildLayout();
        }

    }
}
