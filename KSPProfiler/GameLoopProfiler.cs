using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.LowLevel;

namespace KSPProfiler
{
    public static class GameLoopProfiler
    {
        // total time of frame
        public static GameLoopProfilerUpdateCapture frameTotalCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture profilerOverheadCapture = new GameLoopProfilerUpdateCapture();

        // main frame systems
        public static GameLoopProfilerUpdateCapture initializationCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture earlyUpdateCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture fixedUpdateCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture preUpdateCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture updateCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture preLateUpdateCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateCapture = new GameLoopProfilerUpdateCapture();

        // initialization subsystems
        public static GameLoopProfilerUpdateCapture targetFramerateCapture = new GameLoopProfilerUpdateCapture();

        // FixedUpdate subsystems
        public static GameLoopProfilerFixedCapture fixedCallCapture = new GameLoopProfilerFixedCapture();
        public static GameLoopProfilerFixedCapture fixedCallScriptsCapture = new GameLoopProfilerFixedCapture();
        public static GameLoopProfilerFixedCapture fixedCallPhysicsCapture = new GameLoopProfilerFixedCapture();
        public static GameLoopProfilerFixedCapture fixedCallCoroutinesCapture = new GameLoopProfilerFixedCapture();

        // PreUpdate subsystems
        public static GameLoopProfilerUpdateCapture preUpdateOnMouseCapture = new GameLoopProfilerUpdateCapture();

        // Update subsystems
        public static GameLoopProfilerUpdateCapture updateScriptsCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture updateCoroutinesCapture = new GameLoopProfilerUpdateCapture();

        // PreLateUpdate subsystems
        public static GameLoopProfilerUpdateCapture preLateUpdateScriptsCapture = new GameLoopProfilerUpdateCapture();

        // PostLateUpdate subsystems
        public static GameLoopProfilerUpdateCapture postLateUpdateUGUICapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateAudioCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateRendererCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateMeshSkinningCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateUGUIEmitGeometryCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateRenderCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture postLateUpdateEndOfFrameCoroutinesCapture = new GameLoopProfilerUpdateCapture();

        // PostLateUpdate > FinishFrameRendering subsystems
        public static GameLoopProfilerUpdateCapture renderVsyncCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture renderCameraRenderCapture = new GameLoopProfilerUpdateCapture();
        public static GameLoopProfilerUpdateCapture renderOnGUICapture = new GameLoopProfilerUpdateCapture();


        public static List<int> fixedCountsInCapturedFrames = new List<int>(500);
        public static GameLoopProfilerFixedInFrameCapture fixedInFrameCapture = new GameLoopProfilerFixedInFrameCapture();

        public static bool preCullCalled;
        public static bool onGUICaptureInProgress;
        public static int fixedCountInCurrentFrame;
        public static int fixedCountPerSecond;

        public static float uiUpdateInterval = 1f;
        public static int maxCapturedFrames = 200;
        public static int capturedFrames;
        public static int rollingCapturedFrames;

        public static bool uiAutoUpdate = true;

        public static bool autoCapture = true;
        public static bool captureEnabled = true;

        private static bool uiUpdateRequested = false;
        private static bool resetCapturesRequested = false;
        private static bool delayCaptureToNextFrame = false;

        private static GameLoopProfilerEarlyDaemon earlyDaemon;
        private static GameLoopProfilerLateDaemon lateDaemon;

        private static PlayerLoopSystem systemBackup;

        private static float lastUpdateTime;

        private static List<Type> systems;

        public static void InjectProfiler(GameObject singleton)
        {
            systemBackup = PlayerLoop.GetCurrentPlayerLoop();

            if (systems == null)
            {
                systems = new List<Type>();

                foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
                {
                    if (type.GetCustomAttribute<UnitySystemAttribute>() != null)
                    {
                        systems.Add(type);
                    }
                }
            }

            foreach (Type type in systems)
                GameLoopProfilerInjector.InsertSystem(type);

            earlyDaemon = singleton.AddComponent<GameLoopProfilerEarlyDaemon>();
            lateDaemon = singleton.AddComponent<GameLoopProfilerLateDaemon>();

            capturedFrames = 0;
        }

        public static void RemoveProfiler()
        {
            UnityEngine.Object.Destroy(earlyDaemon);
            UnityEngine.Object.Destroy(lateDaemon);
            PlayerLoop.SetPlayerLoop(systemBackup);
            earlyDaemon = null;
            lateDaemon = null;
        }

        public static void Update()
        {
            List<GameLoopProfilerCaptureBase> captures = GameLoopProfilerCaptureBase.captures;
            int i = captures.Count;

            if (resetCapturesRequested)
            {
                resetCapturesRequested = false;
                uiUpdateRequested = true;
                delayCaptureToNextFrame = true;

                capturedFrames = 0;
                rollingCapturedFrames = 0;

                while (i-- > 0)
                {
                    captures[i].ClearTimes();
                    captures[i].ClearLastTimes();
                }
            }
            else
            {
                if (captureEnabled && !delayCaptureToNextFrame)
                {
                    if (rollingCapturedFrames > maxCapturedFrames)
                        rollingCapturedFrames = 0;

                    rollingCapturedFrames = rollingCapturedFrames + fixedCountsInCapturedFrames.Count;

                    capturedFrames = Math.Min(capturedFrames + fixedCountsInCapturedFrames.Count, maxCapturedFrames);
                    GameLoopProfilerCaptureBase.CheckBufferSize(capturedFrames);

                    while (i-- > 0)
                        captures[i].UpdateTimes();

                    if (!autoCapture && capturedFrames == maxCapturedFrames)
                    {
                        captureEnabled = false;
                        GameLoopProfilerUI.Instance.OnManualCaptureEnd();
                        GameLoopProfilerUI.Instance.Update();
                    }
                }
                else
                {
                    delayCaptureToNextFrame = false;
                    while (i-- > 0)
                        captures[i].ClearLastTimes();
                }
            }

            fixedCountsInCapturedFrames.Clear();

            if (uiAutoUpdate)
            {
                if (Time.realtimeSinceStartup < lastUpdateTime + uiUpdateInterval)
                    return;

                lastUpdateTime = Time.realtimeSinceStartup;

                GameLoopProfilerUI.Instance.Update();
            }
            else if (uiUpdateRequested)
            {
                uiUpdateRequested = false;
                lastUpdateTime = Time.realtimeSinceStartup;
                GameLoopProfilerUI.Instance.Update();
            }
        }

        public static void RequestUIUpdate()
        {
            uiUpdateRequested = true;
        }

        public static void ResetCaptures()
        {
            resetCapturesRequested = true;
        }
    }
}
