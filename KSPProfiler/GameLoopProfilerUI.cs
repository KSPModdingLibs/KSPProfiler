using KsmUI;
using System.Collections.Generic;
using System.IO;
using System.Text;
using TMPro;

namespace KSPProfiler.UI
{
    public class GameLoopProfilerUI : KsmUIVerticalLayout
    {
        public static GameLoopProfilerUI Instance { get; private set; }

        public List<StatsLineCapture> frameStatsToUpdate = new List<StatsLineCapture>();

        private StatsLineCapture frameTotalStats;
        private StatsLine fpsStats;

        private KsmUIButton autoUpdateToggle;
        private KsmUIButton autoCaptureToggle;
        private KsmUIButton manualCaptureButton;

        private KsmUIInputField exportDirectory;
        private KsmUIInputField exportFileName;

        public GameLoopProfilerUI(KsmUIBase parent) : base(parent)
        {
            this.SetSpacing(5);
            Instance = this;
            this.SetDestroyCallback(OnDestroy);

            GameLoopProfiler.InjectProfiler(TopObject);

            KsmUIHorizontalLayout uiOptions = new KsmUIHorizontalLayout(this)
                .SetLayout(true, false, -1, 18)
                .SetSpacing(10);

            new KsmUIText(uiOptions, "UI update rate (s)")
                .SetLayout(100, 18)
                .SetTextAlignment(TextAlignmentOptions.TopRight);

            new KsmUIInputField(uiOptions)
                .SetLayout(50, 18)
                .SetContentType(TMP_InputField.ContentType.DecimalNumber)
                .SetOnInputCallback(OnUpdateIntervalChanged)
                .SetText(GameLoopProfiler.uiUpdateInterval.ToString());

            autoUpdateToggle = new KsmUIButton(uiOptions)
                .SetLayout(100, 18)
                .SetText(GetAutoUpdateToggleLabel())
                .SetButtonOnClick(OnSwitchAutoUpdate)
                .SetTooltip("Disable UI auto-updates to prevent (most of) the profiler overhead in the captured results");

            new KsmUIButton(uiOptions)
                .SetLayout(100, 18)
                .SetText("Update now")
                .SetButtonOnClick(GameLoopProfiler.RequestUIUpdate);

            KsmUIHorizontalLayout frameOptions = new KsmUIHorizontalLayout(this)
                .SetLayout(true, false, -1, 18)
                .SetSpacing(10);

            new KsmUIText(frameOptions, "Frames to capture")
                .SetLayout(100, 18)
                .SetTextAlignment(TextAlignmentOptions.TopRight);

            new KsmUIInputField(frameOptions)
                .SetLayout(50, 18)
                .SetContentType(TMP_InputField.ContentType.IntegerNumber)
                .SetOnInputCallback(OnCapturedFramesChanged)
                .SetText(GameLoopProfiler.maxCapturedFrames.ToString())
                .SetTooltip("Amount of frames to capture. Set this a high value (1000+) to get more accurate stats, " +
                "set it to a low value (100-200) to have the stats reacting more quickly to variations");

            autoCaptureToggle = new KsmUIButton(frameOptions)
                .SetLayout(100, 18)
                .SetText(GetAutoCaptureToggleLabel())
                .SetButtonOnClick(OnSwitchAutoCapture)
                .SetTooltip("Set to ON for continously collecting frame stats, resulting in moving average/mean values." +
                "\nSet to OFF to manually collect a limited amount of frames");

            manualCaptureButton = new KsmUIButton(frameOptions)
                .SetLayout(100, 18)
                .SetText(GetManualCaptureButtonLabel())
                .SetButtonOnClick(OnManualCapture);

            new KsmUIText(frameOptions)
                .SetLayout(true, false, -1, 18)
                .SetText(CapturedFramesLabel)
                .SetTooltip("Amount of frames collected and rolling amount");

            KsmUIHorizontalLayout csvExport = new KsmUIHorizontalLayout(this)
                .SetLayout(true, false, -1, 18)
                .SetSpacing(10);

            exportDirectory = new KsmUIInputField(csvExport)
                .SetLayout(true, false, -1, 18)
                .SetOnInputCallback(CheckIfFolderExists)
                .SetText(KSPRootPath);

            exportFileName = new KsmUIInputField(csvExport)
                .SetLayout(200, 18)
                .SetText("KSPProfiler_GameLoop.csv");

            new KsmUIButton(csvExport)
                .SetLayout(100, 18)
                .SetText("Export to CSV")
                .SetButtonOnClick(ExportToCSV);

            KsmUIVerticalLayout table = new KsmUIVerticalLayout(this)
                .SetLayout(true)
                .SetSpacing(2)
                .SetBackgroundColor(KsmUIStyle.boxColor);

            new TableHeader(table, "General stats", null, false);

            fpsStats = new StatsLine(table, "FPS", "F1", 0, false)
                .SetTooltip("Frames per second");

            frameTotalStats = new StatsLineCapture(table, GameLoopProfiler.frameTotalCapture, "Frame time", "F2", 0, false)
                .SetTooltip("(ms) Total frame time");

            new TableHeader(table, "Frame time", null, true);

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.profilerOverheadCapture, "Profiler overhead", "F2", 0)
                .SetTooltip("(ms) Approximate overhead caused by the present profiler"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.initializationCapture, "Frame init", "F2", 0)
                .SetTooltip("(ms) Internal Unity subsystems"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.targetFramerateCapture, "Target framerate", "F2", 1)
                .SetTooltip("(ms) Loop synchronization to wait for target framerate"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.earlyUpdateCapture, "EarlyUpdate", "F2", 0)
                .SetTooltip("(ms) Internal Unity subsystems"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.fixedUpdateCapture, "FixedUpdate", "F2", 0)
                .SetTooltip("(ms) FixedUpdate is a framerate-independant fixed 0.02s timestep loop " +
                "(it tries to run at 50 calls/second, no matter the actual framerate), " +
                "mainly used for physics processing. Since it is desynchonized from the main loop, it may run zero, " +
                "one or more times per frame"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.fixedCallScriptsCapture, "FixedUpdate usercode", "F2", 1)
                .SetTooltip("(ms) MonoBehaviour.FixedUpdate() usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.fixedCallPhysicsCapture, "Physics", "F2", 1)
                .SetTooltip("(ms) PhysX integration (joints, forces, collisions...) and OnTrigger*/OnCollision* usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.fixedCallCoroutinesCapture, "Coroutines", "F2", 1)
                .SetTooltip("(ms) Yield WaitForFixedUpdate coroutines"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.preUpdateCapture, "PreUpdate", "F2", 0)
                .SetTooltip("(ms) Internal Unity subsystems"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.preUpdateOnMouseCapture, "OnMouse events", "F2", 1)
                .SetTooltip("(ms) MonoBehaviour.OnMouse* usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.updateCapture, "Update", "F2", 0)
                .SetTooltip("(ms) MonoBehaviour.Update() and Coroutine usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.updateScriptsCapture, "Update", "F2", 1)
                .SetTooltip("(ms) MonoBehaviour.Update() usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.updateCoroutinesCapture, "Coroutines", "F2", 1)
                .SetTooltip("(ms) Coroutines usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.preLateUpdateCapture, "LateUpdate", "F2", 0)
                .SetTooltip("(ms) LateUpdate contains the animation components update, followed by the LateUpdate() usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.preLateUpdateScriptsCapture, "LateUpdate", "F2", 1)
                .SetTooltip("(ms) MonoBehaviour.LateUpdate() usercode"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateCapture, "PostLateUpdate", "F2", 0)
                .SetTooltip("(ms) Scene and UI rendering"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateUGUICapture, "UGUI", "F2", 1)
                .SetTooltip("(ms) RectTransfom and Layout update, TextMeshPro SDF text rendering"));
            
            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateAudioCapture, "Audio", "F2", 1)
                .SetTooltip("(ms) Main audio update"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateRendererCapture, "Renderers", "F2", 1)
                .SetTooltip("(ms) Renderer components update"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateMeshSkinningCapture, "Mesh skinning", "F2", 1)
                .SetTooltip("(ms) Skinned meshs update"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateUGUIEmitGeometryCapture, "UGUI geometry", "F2", 1)
                .SetTooltip("(ms) UGUI geometry committing"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateRenderCapture, "Render", "F2", 1)
                .SetTooltip("(ms) Main rendering operations"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.renderVsyncCapture, "VSync", "F2", 2)
                .SetTooltip("(ms) Loop synchronization to wait for VSync"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.renderCameraRenderCapture, "Cameras render", "F2", 2)
                .SetTooltip("(ms) Cameras render operations, this is the main rendering step"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.renderOnGUICapture, "OnGUI", "F2", 2)
                .SetTooltip("(ms) MonoBehaviour.OnGUI() user code. This is the legacy IMGUI system used by many mods"));

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.postLateUpdateEndOfFrameCoroutinesCapture, "EndOfFrame Coroutines", "F2", 1)
                .SetTooltip("(ms) Yield WaitForEndOfFrame coroutines"));

            new TableHeader(table, "FixedUpdate per call stats", null, true);

            frameStatsToUpdate.Add(new StatsLineCapture(table, GameLoopProfiler.fixedInFrameCapture, "Fixed calls / frame", "F2", 0, false)
                .SetTooltip("FixedUpdate calls per frame\nThe maximum value is directly controlled by the \"Max Physics Delta-Time Per Frame\" KSP Setting"));

            frameStatsToUpdate.Add(new StatsLineFixedCapture(table, GameLoopProfiler.fixedCallCapture, "FixedUpdate loop", "F2", 0)
                .SetTooltip("(ms) FixedUpdate is a framerate-independant fixed 0.02s timestep loop " +
                "(it tries to run at 50 calls/second, no matter the actual framerate), " +
                "mainly used for physics processing. Since it is desynchonized from the main loop, it may run zero, " +
                "one or more times per frame"));

            frameStatsToUpdate.Add(new StatsLineFixedCapture(table, GameLoopProfiler.fixedCallScriptsCapture, "FixedUpdate usercode", "F2", 1)
                .SetTooltip("(ms) MonoBehaviour.FixedUpdate() usercode"));

            frameStatsToUpdate.Add(new StatsLineFixedCapture(table, GameLoopProfiler.fixedCallPhysicsCapture, "Physics", "F2", 1)
                .SetTooltip("(ms) PhysX integration (joints, forces, collisions...) and OnTrigger*/OnCollision* usercode"));

            frameStatsToUpdate.Add(new StatsLineFixedCapture(table, GameLoopProfiler.fixedCallCoroutinesCapture, "Coroutines", "F2", 1)
                .SetTooltip("(ms) Yield WaitForFixedUpdate coroutines"));
        }

        private string KSPRootPath => Path.GetFullPath(KSPUtil.ApplicationRootPath);


        private string CheckIfFolderExists(string input)
        {
            if (!Directory.Exists(input))
                return KSPRootPath;

            return input;
        }

        private void ExportToCSV()
        {
            if (!Directory.Exists(exportDirectory.Text))
                return;

            string path = Path.Combine(exportDirectory.Text, exportFileName.Text);

            List<string> text = new List<string>();

            text.Add("Statistic;Mean;Median;Worst 25%;Worst 1%");
            text.Add(fpsStats.CSVExport());
            text.Add(frameTotalStats.CSVExport());

            foreach (StatsLine line in frameStatsToUpdate)
                text.Add(line.CSVExport());

            File.WriteAllLines(path, text.ToArray());
        }

        private void OnSwitchAutoUpdate()
        {
            GameLoopProfiler.uiAutoUpdate = !GameLoopProfiler.uiAutoUpdate;
            autoUpdateToggle.Text = GetAutoUpdateToggleLabel();
        }

        private string GetAutoUpdateToggleLabel()
        {
            return GameLoopProfiler.uiAutoUpdate ? "AutoUpdate : ON" : "AutoUpdate : OFF";
        }

        private void OnSwitchAutoCapture()
        {
            if (GameLoopProfiler.autoCapture)
            {
                GameLoopProfiler.autoCapture = false;
                GameLoopProfiler.captureEnabled = false;
                GameLoopProfiler.ResetCaptures();
            }
            else
            {
                GameLoopProfiler.autoCapture = true;
                GameLoopProfiler.captureEnabled = true;
            }

            autoCaptureToggle.Text = GetAutoCaptureToggleLabel();
            manualCaptureButton.Text = GetManualCaptureButtonLabel();
        }

        private string GetAutoCaptureToggleLabel()
        {
            return GameLoopProfiler.autoCapture ? "AutoCapture : ON" : "AutoCapture : OFF";
        }

        private void OnManualCapture()
        {
            if (GameLoopProfiler.autoCapture)
            {
                GameLoopProfiler.ResetCaptures();
            }
            else
            {
                if (GameLoopProfiler.captureEnabled)
                {
                    GameLoopProfiler.captureEnabled = false;
                    GameLoopProfiler.RequestUIUpdate();
                }
                else
                {
                    GameLoopProfiler.captureEnabled = true;
                    GameLoopProfiler.ResetCaptures();
                }
            }

            manualCaptureButton.Text = GetManualCaptureButtonLabel();
        }

        private string GetManualCaptureButtonLabel()
        {
            if (GameLoopProfiler.autoCapture)
                return "Reset capture";

            return GameLoopProfiler.captureEnabled ? "Stop capture" : "Start capture";
        }

        private string CapturedFramesLabel()
        {
            if (GameLoopProfiler.autoCapture)
                return $"Captured frames : {GameLoopProfiler.capturedFrames} ({GameLoopProfiler.rollingCapturedFrames})";
            else
                return $"Captured frames : {GameLoopProfiler.capturedFrames}";
        }

        public void OnManualCaptureEnd()
        {
            manualCaptureButton.Text = "Start capture";
        }

        private string OnUpdateIntervalChanged(string input)
        {
            if (!float.TryParse(input, out float interval))
                return GameLoopProfiler.uiUpdateInterval.ToString();
            else if (interval < 0.1f)
                interval = 0.1f;
            else if (interval > 30f)
                interval = 30f;

            GameLoopProfiler.uiUpdateInterval = interval;
            return interval.ToString();
        }

        private string OnCapturedFramesChanged(string input)
        {
            if (!int.TryParse(input, out int frames))
                return GameLoopProfiler.maxCapturedFrames.ToString();
            else if (frames < 25)
                frames = 25;
            else if (frames > 10000)
                frames = 10000;

            GameLoopProfiler.maxCapturedFrames = frames;
            return frames.ToString();
        }

        public void Update()
        {
            frameTotalStats.UpdateFromCapture(-1.0);
            double frameMean = frameTotalStats.Capture.mean;

            foreach (StatsLineCapture capture in frameStatsToUpdate)
            {
                capture.UpdateFromCapture(frameMean);
            }

            fpsStats.SetValues(
                1000.0 / frameTotalStats.Capture.mean,
                1000.0 / frameTotalStats.Capture.median,
                1000.0 / frameTotalStats.Capture.worst25,
                1000.0 / frameTotalStats.Capture.worst1);
;
        }

        private void OnDestroy()
        {
            Instance = null;
            GameLoopProfiler.RemoveProfiler();
        }

        public class TableHeader : KsmUIBase
        {
            public TableHeader(KsmUIBase parent, string title, string titleTooltip, bool showFramePercent) : base(parent)
            {
                this.SetLayout(true, false, -1, 15);
                this.SetBackgroundColor(KsmUIStyle.headerColor);

                KsmUIText titleText = new KsmUIText(this, title)
                    .SetStaticSizeAndPosition(160, 15);

                if (!string.IsNullOrEmpty(titleTooltip))
                    titleText.SetTooltip(titleTooltip);

                int hPos = 160;

                if (showFramePercent)
                {
                    new KsmUIText(this, "Frame %").SetStaticSizeAndPosition(60, 15, hPos)
                        .SetTextAlignment(TextAlignmentOptions.Right)
                        .SetTooltip("% of frame time taken by each subsystem");
                }

                new KsmUIText(this, "Mean").SetStaticSizeAndPosition(60, 15, hPos += 60)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetTooltip("Mean (average)");

                new KsmUIText(this, "Median").SetStaticSizeAndPosition(60, 15, hPos += 60)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetTooltip("Median is the most typical value. It is usually a better representation than the " +
                    "mean of the value you're getting most of the time");

                new KsmUIText(this, "Worst 25%").SetStaticSizeAndPosition(70, 15, hPos += 60)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetTooltip("Upper/lower quartile. This is the typical value you're getting 25% of the time.");

                new KsmUIText(this, "Worst 1%").SetStaticSizeAndPosition(70, 15, hPos += 60)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetTooltip("Upper/lower 99th centile. This is the typical value you're getting 1% of the time. " +
                    "The further it is from the median, the less stable the update rate is.");
            }
        }

        public class StatsLine : KsmUIBase
        {
            protected string format;
            protected bool hasFrameProportion;
            private string name;

            protected KsmUIText framePercentText;
            protected KsmUIText meanText;
            protected KsmUIText medianText;
            protected KsmUIText worst25Text;
            protected KsmUIText worst1Text;

            private StringBuilder sb = new StringBuilder();

            public StatsLine(KsmUIBase parent, string name, string format, int level = 0, bool hasFrameProportion = true)
                : base(parent)
            {
                this.name = name;
                this.hasFrameProportion = hasFrameProportion;

                if (level == 0)
                    this.SetBackgroundColor(KsmUIStyle.boxColor);
                else
                    this.SetBackgroundColor(KsmUIStyle.selectedBoxColor);

                this.format = format;

                this.SetLayout(true, false, -1, 15);

                int indent = level * 15;
                new KsmUIText(this, name)
                    .SetStaticSizeAndPosition(160 - indent, 15, indent);

                int hPos = 160;

                if (hasFrameProportion)
                {
                    framePercentText = new KsmUIText(this)
                        .SetTextAlignment(TextAlignmentOptions.Right)
                        .SetStaticSizeAndPosition(60, 15, hPos);
                }
                meanText = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                medianText = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                worst25Text = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
                worst1Text = new KsmUIText(this)
                    .SetTextAlignment(TextAlignmentOptions.Right)
                    .SetStaticSizeAndPosition(60, 15, hPos += 60);
            }

            public void SetValues(double mean, double median, double worst25, double worst1)
            {
                meanText.Text = mean.ToString(format);
                medianText.Text = median.ToString(format);
                worst25Text.Text = worst25.ToString(format);
                worst1Text.Text = worst1.ToString(format);
            }

            public string CSVExport()
            {
                return $"{name};{meanText.Text};{medianText.Text};{worst25Text.Text};{worst1Text.Text}";
            }
        }

        public class StatsLineCapture : StatsLine
        {
            protected GameLoopProfilerCaptureBase capture;
            public GameLoopProfilerCaptureBase Capture => capture;

            public StatsLineCapture(KsmUIBase parent, GameLoopProfilerCaptureBase capture, string name, string format, int level = 0, bool hasFrameProportion = true)
                : base(parent, name, format, level, hasFrameProportion)
            {
                this.capture = capture;
            }

            public virtual void UpdateFromCapture(double frameMeanTime = -1.0)
            {
                capture.UpdateStats();

                if (hasFrameProportion)
                {
                    if (frameMeanTime >= 0.0)
                    {
                        double frameProportion = frameMeanTime > 0.0 ? capture.mean / frameMeanTime : 0.0;
                        framePercentText.Text = frameProportion.ToString("P1");
                    }
                    else
                    {
                        framePercentText.Text = 1.0.ToString("P1");
                    }
                }

                meanText.Text = capture.mean.ToString(format);
                medianText.Text = capture.median.ToString(format);
                worst25Text.Text = capture.worst25.ToString(format);
                worst1Text.Text = capture.worst1.ToString(format);
            }
        }

        public class StatsLineFixedCapture : StatsLineCapture
        {
            public StatsLineFixedCapture(KsmUIBase parent, GameLoopProfilerFixedCapture capture, string name, string format, int level = 0)
                : base(parent, capture, name, format, level, false)
            { }

            public override void UpdateFromCapture(double frameMeanTime = -1.0)
            {
                capture.UpdateStats();

                if (hasFrameProportion)
                {
                    if (frameMeanTime >= 0.0)
                    {
                        double frameProportion = frameMeanTime > 0.0 ? capture.mean / frameMeanTime : 0.0;
                        framePercentText.Text = frameProportion > 0.0 ? frameProportion.ToString("P1") : "--";
                    }
                    else
                    {
                        framePercentText.Text = 1.0.ToString("P1");
                    }
                }

                GameLoopProfilerFixedCapture fixedCapture = (GameLoopProfilerFixedCapture)this.capture;

                meanText.Text = fixedCapture.meanPerCall.ToString(format);
                medianText.Text = fixedCapture.medianPerCall.ToString(format);
                worst25Text.Text = fixedCapture.worst25PerCall.ToString(format);
                worst1Text.Text = fixedCapture.worst1PerCall.ToString(format);
            }
        }


    }
}
