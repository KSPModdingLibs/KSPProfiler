using System.Diagnostics;
using UnityEngine;


/*
# InitializationBegin
- Initialization
  - PlayerUpdateTime --> WaitForTargetFPS when a target framerate is set
  # PlayerUpdateEnd
  - DirectorSampleTime
  - AsyncUploadTimeSlicedUpdate
  - SynchronizeInputs
  - SynchronizeState
  - XREarlyUpdate
# EarlyUpdateBegin
- EarlyUpdate
  - PollPlayerConnection
  - ProfilerStartFrame
  - GpuTimestamp
  - AnalyticsCoreStatsUpdate
  - UnityWebRequestUpdate
  - ExecuteMainThreadJobs
  - ProcessMouseInWindow
  - ClearIntermediateRenderers
  - ClearLines
  - PresentBeforeUpdate
  - ResetFrameStatsAfterPresent
  - UpdateAsyncReadbackManager
  - UpdateStreamingManager
  - UpdateTextureStreamingManager
  - UpdatePreloading
  - RendererNotifyInvisible
  - PlayerCleanupCachedData
  - UpdateMainGameViewRect
  - UpdateCanvasRectTransform
  - XRUpdate
  - UpdateInputManager
  - ProcessRemoteInput
  - ScriptRunDelayedStartupFrame
  - UpdateKinect
  - DeliverIosPlatformEvents
  - TangoUpdate
  - DispatchEventQueueEvents
  - PhysicsResetInterpolatedTransformPosition
  - SpriteAtlasManagerUpdate
  - PerformanceAnalyticsUpdate
# EarlyUpdateEnd
- FixedUpdate
  # FixedUpdateStart
  - ClearLines
  - NewInputFixedUpdate
  - DirectorFixedSampleTime
  - AudioFixedUpdate
  # FixedUpdateScriptsBegin
  - ScriptRunBehaviourFixedUpdate
  # FixedUpdateScriptsEnd
  - DirectorFixedUpdate
  - LegacyFixedAnimationUpdate
  - XRFixedUpdate
  # FixedUpdatePhysicsBegin
  - PhysicsFixedUpdate
  - Physics2DFixedUpdate
  - PhysicsClothFixedUpdate
  # FixedUpdatePhysicsEnd
  - DirectorFixedUpdatePostPhysics
  # FixedUpdateCoroutinesBegin
  - ScriptRunDelayedFixedFrameRate
  # FixedUpdateEnd
# PreUpdateBegin
- PreUpdate
  - PhysicsUpdate
  - Physics2DUpdate
  - CheckTexFieldInput
  - IMGUISendQueuedEvents
  - NewInputUpdate
  # OnMouseBegin
  - SendMouseEvents
  # OnMouseEnd
  - AIUpdate
  - WindUpdate
  - UpdateVideo
# UpdateBegin
- Update
  - ScriptRunBehaviourUpdate
  # UpdateCoroutinesBegin
  - ScriptRunDelayedDynamicFrameRate
  # UpdateCoroutinesEnd
  - ScriptRunDelayedTasks
  - DirectorUpdate
# PreLateUpdateBegin
- PreLateUpdate
  - AIUpdatePostScript
  - DirectorUpdateAnimationBegin
  - LegacyAnimationUpdate
  - DirectorUpdateAnimationEnd
  - DirectorDeferredEvaluate
  - EndGraphicsJobsAfterScriptUpdate
  - ConstraintManagerUpdate
  - ParticleSystemBeginUpdateAll
  # ScriptLateUpdateBegin
  - ScriptRunBehaviourLateUpdate
  # ScriptLateUpdateEnd
# PostLateUpdateBegin
- PostLateUpdate
  - PlayerSendFrameStarted
  - DirectorLateUpdate
  - ScriptRunDelayedDynamicFrameRate
  - PhysicsSkinnedClothBeginUpdate
  # UGUIUpdateStart
  - UpdateRectTransform
  - PlayerUpdateCanvases
  # AudioUpdateStart
  - UpdateAudio
  # AudioUpdateEnd
  - VFXUpdate
  - ParticleSystemEndUpdateAll
  - EndGraphicsJobsAfterScriptLateUpdate
  - UpdateCustomRenderTextures
  # RenderersUpdateStart
  - UpdateAllRenderers
  # RenderersUpdateEnd
  - EnlightenRuntimeUpdate
  # MeshSkinningStart
  - UpdateAllSkinnedMeshes
  # MeshSkinningEnd
  - ProcessWebSendMessages
  - SortingGroupsUpdate
  - UpdateVideoTextures
  - UpdateVideo
  - DirectorRenderImage
  # UGUIGeometryStart
  - PlayerEmitCanvasGeometry
  # UGUIGeometryEnd
  - PhysicsSkinnedClothFinishUpdate
  # RenderStart
  - FinishFrameRendering -> include VSync, Camera renders, OnGUI
    ### To get Vsync, we can measure the time between #RenderStart and a Camera.OnPreCull static callback. Its not perfect, there is a call to Camera.FindStacks (0.03 ms) before OnPreCull
    ### To get all cameras and scripts render time, we can measure the time between a Camera.OnPreCull static callback and a late MonoBehaviour.OnRenderObject()
    ### To get OnGUI code, use OnGUI(), but we have to handle the fact that it is called multiple time and we will miss a few early stuff
  # RenderEnd
  - BatchModeUpdate
  # EndOfFrameCoroutinesStart
  - PlayerSendFrameComplete
  # EndOfFrameCoroutinesEnd
  - UpdateCaptureScreenshot
  - PresentAfterDraw
  - ClearImmediateRenderers
  - PlayerSendFramePostPresent
  - UpdateResolution
  - InputEndFrame
  - TriggerEndOfFrameCallbacks
  - GUIClearEvents
  - ShaderHandleErrors
  - ResetInputAxis
  - ThreadedLoadingDebug
  - ProfilerSynchronizeStats
  - MemoryFrameMaintenance
  - ExecuteGameCenterCallbacks
  - ProfilerEndFrame
*/

namespace KSPProfiler
{
    [DefaultExecutionOrder(-10000)]
    public class GameLoopProfilerEarlyDaemon : MonoBehaviour
    {
        private void OnGUI()
        {
            if (!GameLoopProfiler.onGUICaptureInProgress)
            {
                GameLoopProfiler.renderCameraRenderCapture.CapturePreviousElapsed();
                GameLoopProfiler.onGUICaptureInProgress = true;
                GameLoopProfiler.renderOnGUICapture.Restart();
            }
        }
    }

    [DefaultExecutionOrder(10000)]
    public class GameLoopProfilerLateDaemon : MonoBehaviour
    {
        private void Awake()
        {
            Camera.onPreCull += OnPreCullCallback;
        }

        private void OnDestroy()
        {
            Camera.onPreCull -= OnPreCullCallback;
        }

        private void OnPreCullCallback(Camera cam)
        {
            if (GameLoopProfiler.preCullCalled)
                return;

            GameLoopProfiler.preCullCalled = true;
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.renderVsyncCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.renderCameraRenderCapture.Restart(timestamp);
        }

        // this is called multiple times, so just update elapsed, then
        // sometimes latter when we are sure this won't get called anymore,
        // capture the last elapsed
        private void OnRenderObject()
        {
            GameLoopProfiler.renderCameraRenderCapture.UpdateElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "Initialization")]
    public class InitializationBegin
    {
        public static void Capture()
        {
            long timestamp1 = Stopwatch.GetTimestamp();
            GameLoopProfiler.postLateUpdateCapture.CaptureElapsed(timestamp1);
            GameLoopProfiler.frameTotalCapture.CaptureElapsedAndRestart(timestamp1);
            GameLoopProfiler.profilerOverheadCapture.Restart(timestamp1);
            // Profiler processing
            GameLoopProfiler.Update();

            long timestamp2 = Stopwatch.GetTimestamp();
            GameLoopProfiler.profilerOverheadCapture.CaptureElapsed(timestamp2);
            GameLoopProfiler.initializationCapture.Restart(timestamp2);
            GameLoopProfiler.targetFramerateCapture.Restart(timestamp2);
        }
    }

    [UnitySystem(InsertType.After, "PlayerUpdateTime")]
    public class PlayerUpdateEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.targetFramerateCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "EarlyUpdate")]
    public class EarlyUpdateBegin
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.initializationCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.earlyUpdateCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.After, "EarlyUpdate")]
    public class EarlyUpdateEnd
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.earlyUpdateCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.fixedUpdateCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.Before, "NewInputFixedUpdate")]
    public class FixedUpdateStart
    {
        public static void Capture()
        {
            GameLoopProfiler.fixedCountInCurrentFrame++;
            GameLoopProfiler.fixedCallCapture.Restart();
        }
    }

    [UnitySystem(InsertType.Before, "ScriptRunBehaviourFixedUpdate")]
    public class FixedUpdateScriptsBegin
    {
        public static void Capture()
        {
            GameLoopProfiler.fixedCallScriptsCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "ScriptRunBehaviourFixedUpdate")]
    public class FixedUpdateScriptsEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.fixedCallScriptsCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "PhysicsFixedUpdate")]
    public class FixedUpdatePhysicsBegin
    {
        public static void Capture()
        {
            GameLoopProfiler.fixedCallPhysicsCapture.Restart();
        }
    }

    [UnitySystem(InsertType.Before, "DirectorFixedUpdatePostPhysics")]
    public class FixedUpdatePhysicsEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.fixedCallPhysicsCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "ScriptRunDelayedFixedFrameRate")]
    public class FixedUpdateCoroutinesBegin
    {
        public static void Capture()
        {
            GameLoopProfiler.fixedCallCoroutinesCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "ScriptRunDelayedFixedFrameRate")]
    public class FixedUpdateEnd
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.fixedCallCoroutinesCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.fixedCallCapture.CaptureElapsed(timestamp);
        }
    }

    [UnitySystem(InsertType.Before, "PreUpdate")]
    public class PreUpdateBegin
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.fixedUpdateCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.fixedCountsInCapturedFrames.Add(GameLoopProfiler.fixedCountInCurrentFrame);
            GameLoopProfiler.fixedCountInCurrentFrame = 0;
            GameLoopProfiler.preUpdateCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.Before, "SendMouseEvents")]
    public class OnMouseBegin
    {
        public static void Capture()
        {
            GameLoopProfiler.preUpdateOnMouseCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "SendMouseEvents")]
    public class OnMouseEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.preUpdateOnMouseCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "Update")]
    public class UpdateBegin
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.preUpdateCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.updateCapture.Restart(timestamp);
            GameLoopProfiler.updateScriptsCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.After, "ScriptRunBehaviourUpdate")]
    public class UpdateCoroutinesBegin
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.updateScriptsCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.updateCoroutinesCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.After, "ScriptRunDelayedDynamicFrameRate")]
    public class UpdateCoroutinesEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.updateCoroutinesCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "PreLateUpdate")]
    public class PreLateUpdateBegin
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.updateCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.preLateUpdateCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.Before, "ScriptRunBehaviourLateUpdate")]
    public class ScriptLateUpdateBegin
    {
        public static void Capture()
        {
            GameLoopProfiler.preLateUpdateScriptsCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "ScriptRunBehaviourLateUpdate")]
    public class ScriptLateUpdateEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.preLateUpdateScriptsCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "PostLateUpdate")]
    public class PostLateUpdateBegin
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.preLateUpdateCapture.CaptureElapsed(timestamp);
            GameLoopProfiler.postLateUpdateCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.Before, "UpdateRectTransform")]
    public class UGUIUpdateStart
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateUGUICapture.Restart();
        }
    }

    [UnitySystem(InsertType.Before, "UpdateAudio")]
    public class AudioUpdateStart
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.postLateUpdateUGUICapture.CaptureElapsed(timestamp);
            GameLoopProfiler.postLateUpdateAudioCapture.Restart(timestamp);
        }
    }

    [UnitySystem(InsertType.After, "UpdateAudio")]
    public class AudioUpdateEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateAudioCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "UpdateAllRenderers")]
    public class RenderersUpdateStart
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateRendererCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "UpdateAllRenderers")]
    public class RenderersUpdateEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateRendererCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "UpdateAllSkinnedMeshes")]
    public class MeshSkinningStart
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateMeshSkinningCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "UpdateAllSkinnedMeshes")]
    public class MeshSkinningEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateMeshSkinningCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "PlayerEmitCanvasGeometry")]
    public class UGUIGeometryStart
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateUGUIEmitGeometryCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "PlayerEmitCanvasGeometry")]
    public class UGUIGeometryEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateUGUIEmitGeometryCapture.CaptureElapsed();
        }
    }

    [UnitySystem(InsertType.Before, "FinishFrameRendering")]
    public class RenderStart
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.postLateUpdateRenderCapture.Restart(timestamp);
            GameLoopProfiler.renderVsyncCapture.Restart(timestamp);
            GameLoopProfiler.preCullCalled = false;
        }
    }

    [UnitySystem(InsertType.After, "FinishFrameRendering")]
    public class RenderEnd
    {
        public static void Capture()
        {
            long timestamp = Stopwatch.GetTimestamp();
            GameLoopProfiler.onGUICaptureInProgress = false;
            GameLoopProfiler.renderOnGUICapture.CaptureElapsed(timestamp);
            GameLoopProfiler.postLateUpdateRenderCapture.CaptureElapsed(timestamp);
        }
    }

    [UnitySystem(InsertType.Before, "PlayerSendFrameComplete")]
    public class EndOfFrameCoroutinesStart
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateEndOfFrameCoroutinesCapture.Restart();
        }
    }

    [UnitySystem(InsertType.After, "PlayerSendFrameComplete")]
    public class EndOfFrameCoroutinesEnd
    {
        public static void Capture()
        {
            GameLoopProfiler.postLateUpdateEndOfFrameCoroutinesCapture.CaptureElapsed();
        }
    }
}
