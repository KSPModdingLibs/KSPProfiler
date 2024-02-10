using KSPProfiler;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

namespace KSPProfiler
{


    public class MethodProfiler
    {
        [UnitySystem(InsertType.After, "PostLateUpdate")]
        public class MethodProfilerSystem
        {
            public static void Capture()
            {
                capturedFrames = Math.Min(capturedFrames + 1, maxCapturedFrames);

                foreach (ProfiledMethod method in ProfiledMethod.calledProfiledMethods.Values)
                {
                    method.UpdateTimes();
                }

                if (Time.realtimeSinceStartup > lastTime + 2.5f)
                {
                    lastTime = Time.realtimeSinceStartup;

                    foreach (ProfiledMethod method in ProfiledMethod.calledProfiledMethods.Values)
                    {
                        method.GetCallStats(out int callCount, out double mean, out double median, out double percent);
                        method.GetFrameStats(out double meanCallsPerFrame, out double meanTimePerFrame);
                    }

                }
            }
        }

        public static List<ProfiledMethodAssemblyGroup> allAssemblyGroups = new List<ProfiledMethodAssemblyGroup>();

        public static int maxCapturedFrames = 300;
        public static int capturedFrames;

        public static float lastTime;

        private static PlayerLoopSystem systemBackup;

        private static bool init;

        public static void Init()
        {
            if (init)
                return;

            init = true;

            foreach (AssemblyLoader.LoadedAssembly kspAssembly in AssemblyLoader.loadedAssemblies)
            {
                if (kspAssembly.assembly == null)
                    continue;

                ProfiledMethodAssemblyGroup assemblyGroup = new ProfiledMethodAssemblyGroup(kspAssembly.assembly);
                if (assemblyGroup.methods.Count == 0)
                    continue;

                allAssemblyGroups.Add(assemblyGroup);
            }
        }

        public static void InjectProfiler()
        {
            systemBackup = PlayerLoop.GetCurrentPlayerLoop();
            GameLoopProfilerInjector.InsertSystem(typeof(MethodProfilerSystem));

            foreach (ProfiledMethodAssemblyGroup group in allAssemblyGroups)
            {
                foreach (ProfiledMethod method in group.methods)
                    method.Patch();
            }
        }

        public static void RemoveProfiler()
        {
            foreach (ProfiledMethodAssemblyGroup group in allAssemblyGroups)
            {
                foreach (ProfiledMethod method in group.methods)
                    method.UnPatch();
            }

            PlayerLoop.SetPlayerLoop(systemBackup);
        }

        static void ScenarioDiscoverableObjects_UpdateSpaceObjects_Prefix(ScenarioDiscoverableObjects __instance)
        {
            __instance.discoveryUnlocked = false;
        }
    }
}
