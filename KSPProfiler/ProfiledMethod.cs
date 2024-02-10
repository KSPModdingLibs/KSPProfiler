using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

namespace KSPProfiler
{
    public class ProfiledMethod
    {
        private static Dictionary<MethodBase, ProfiledMethod> knownProfiledMethods = new Dictionary<MethodBase, ProfiledMethod>(1000);
        private static Dictionary<MethodBase, ProfiledMethod> knownProfiledCoroutines = new Dictionary<MethodBase, ProfiledMethod>(1000);
        private static Dictionary<MethodBase, ProfiledMethod> activeProfiledMethods = new Dictionary<MethodBase, ProfiledMethod>(1000);
        public static Dictionary<MethodBase, ProfiledMethod> calledProfiledMethods = new Dictionary<MethodBase, ProfiledMethod>(1000);

        private MethodBase target;
        public readonly string assemblyName;
        public readonly string name;
        private static Harmony harmony;
        private static string harmonyID;

        private long startTime;
        private List<long> lastTimes = new List<long>(1000);

        /// <summary>
        /// total amount of calls captured
        /// </summary>
        private int callCount;

        /// <summary>
        /// contiguous array of all call times, where the array is dynamically resized to be larger than callCount,
        /// and captured values are in the [0, callCount] range
        /// </summary>
        private double[] callTimes = new double[500];

        /// <summary>
        /// Amount of calls per frame. Array size is fixed to the max amount of frames captured
        /// </summary>
        private int[] callCountPerFrame = new int[300];


        private double[] callTimesStatsBuffer = new double[0];
        private double[] updateTimesStatsBuffer = new double[0];

        public void UpdateTimes()
        {
            bool hasReachedMaxCapture = MethodProfiler.capturedFrames == MethodProfiler.maxCapturedFrames;
            int callCountInFrame = lastTimes.Count;
            double[] updatedPerUpdateCallTimes;
            if (callCountInFrame > 0)
            {
                int callCountInFirstFrame = callCountPerFrame[0];
                int callCountRemaining = hasReachedMaxCapture ? callCount - callCountInFirstFrame : callCount;
                int callCountAfterUpdate = callCountRemaining + callCountInFrame;
                
                if (callTimes.Length < callCountAfterUpdate)
                    updatedPerUpdateCallTimes = new double[Math.Max(callCountPerFrame.Length, callCountAfterUpdate * 2)];
                else
                    updatedPerUpdateCallTimes = callTimes;

                if (hasReachedMaxCapture && callCountInFirstFrame > 0)
                    Array.Copy(callTimes, callCountInFirstFrame, updatedPerUpdateCallTimes, 0, callCountRemaining);

                int i = callCountInFrame;
                while (i-- > 0)
                    updatedPerUpdateCallTimes[callCountRemaining + i] = lastTimes[i] / millisecondsFrequency;

                callTimes = updatedPerUpdateCallTimes;
                callCount = callCountAfterUpdate;
                lastTimes.Clear();
            }

            if (hasReachedMaxCapture)
                Array.Copy(callCountPerFrame, 1, callCountPerFrame, 0, MethodProfiler.capturedFrames - 1);

            callCountPerFrame[MethodProfiler.capturedFrames - 1] = callCountInFrame;
        }

        public void GetCallStats(out int callCount, out double mean, out double median, out double worstPercent)
        {
            callCount = this.callCount;

            if (callTimesStatsBuffer.Length != callCount)
                callTimesStatsBuffer = new double[callCount];

            Array.Copy(callTimes, 0, callTimesStatsBuffer, 0, callCount);
            mean = ArrayStatistics.Mean(callTimesStatsBuffer);
            median = ArrayStatistics.MedianInplace(callTimesStatsBuffer);
            worstPercent = ArrayStatistics.QuantileInplace(callTimesStatsBuffer, 0.99);
        }

        public void GetFrameStats(out double meanCallsPerFrame, out double meanTimePerFrame)
        {
            if (updateTimesStatsBuffer.Length != MethodProfiler.capturedFrames)
                updateTimesStatsBuffer = new double[MethodProfiler.capturedFrames];

            int i = MethodProfiler.capturedFrames;
            int nextCall = callCount - 1;
            int nextFrameIndex = callCount - 1;
            while (i-- > 0)
            {
                double perFrameTime = 0.0;
                nextFrameIndex -= callCountPerFrame[i];
                while (nextCall > nextFrameIndex)
                {
                    perFrameTime += callTimes[nextCall];
                    nextCall--;
                }

                updateTimesStatsBuffer[i] = perFrameTime;
            }

            meanTimePerFrame = ArrayStatistics.Mean(updateTimesStatsBuffer);
            meanCallsPerFrame = (double)callCount / MethodProfiler.capturedFrames;
        }


        private static readonly double millisecondsFrequency = Stopwatch.Frequency / 1000.0;

        private static MethodInfo genericPrefix = AccessTools.Method(typeof(ProfiledMethod), nameof(GenericPrefix));
        private static MethodInfo genericPostfix = AccessTools.Method(typeof(ProfiledMethod), nameof(GenericPostfix));

        public delegate void OnCoroutineFound(Assembly assembly, ProfiledMethod profiledMethod);
        public static OnCoroutineFound onCoroutineFound;


        public static ProfiledMethod Get(MethodBase target)
        {
            if (!knownProfiledMethods.TryGetValue(target, out ProfiledMethod profiledMethod))
                profiledMethod = new ProfiledMethod(target);

            return profiledMethod;
        }

        private ProfiledMethod(MethodBase target)
        {
            this.target = target;
            assemblyName = target.DeclaringType.Assembly.GetName().Name;
            name = Lib.GetMethodName(target);
            knownProfiledMethods.Add(target, this);
        }

        public void Patch()
        {
            if (!activeProfiledMethods.TryAdd(target, this))
                return;

            harmony.Patch(target, new HarmonyMethod(genericPrefix), new HarmonyMethod(genericPostfix));
        }

        public void UnPatch()
        {
            if (!activeProfiledMethods.Remove(target))
                return;

            calledProfiledMethods.Remove(target);
            harmony.Unpatch(target, HarmonyPatchType.All, harmonyID);
        }

        private static ProfiledMethod BeginProfiling(MethodBase originalMethod)
        {
            if (!calledProfiledMethods.TryGetValue(originalMethod, out ProfiledMethod profiledMethod))
            {
                profiledMethod = activeProfiledMethods[originalMethod];
                calledProfiledMethods.Add(originalMethod, profiledMethod);
            }

            profiledMethod.startTime = Stopwatch.GetTimestamp();
            return profiledMethod;
        }

        private void EndProfiling()
        {
            lastTimes.Add(Stopwatch.GetTimestamp() - startTime);
        }

        private static void GenericPrefix(MethodBase __originalMethod, out ProfiledMethod __state)
        {
            __state = BeginProfiling(__originalMethod);
        }

        private static void GenericPostfix(ProfiledMethod __state)
        {
            __state.EndProfiling();
        }

        public static IEnumerable<ProfiledMethod> AllCoroutinesInAssembly(Assembly assembly)
        {
            foreach (ProfiledMethod method in knownProfiledCoroutines.Values)
            {
                if (method.target.DeclaringType.Assembly != assembly)
                    continue;

                yield return method;
            }
        }

        public static void InitOnGameStart()
        {
            harmonyID = "KSPProfilerMethodPatches";
            harmony = new Harmony(harmonyID);

            Harmony coroutinePatches = new Harmony("KSPProfilerCoroutinePatches");

            MethodInfo m_StartCoroutine1 = AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new[] { typeof(IEnumerator) });
            MethodInfo m_StartCoroutine2 = AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new[] { typeof(string) });
            MethodInfo m_StartCoroutine3 = AccessTools.Method(typeof(MonoBehaviour), nameof(MonoBehaviour.StartCoroutine), new[] { typeof(string), typeof(object) });

            MethodInfo m_patch1 = AccessTools.Method(typeof(ProfiledMethod), nameof(ProfiledMethod.StartCoroutine_Prefix1));
            MethodInfo m_patch2 = AccessTools.Method(typeof(ProfiledMethod), nameof(ProfiledMethod.StartCoroutine_Prefix2));

            coroutinePatches.Patch(m_StartCoroutine1, new HarmonyMethod(m_patch1));
            coroutinePatches.Patch(m_StartCoroutine2, new HarmonyMethod(m_patch2));
            coroutinePatches.Patch(m_StartCoroutine3, new HarmonyMethod(m_patch2));
        }

        private static HashSet<Type> patchedCoroutines = new HashSet<Type>();

        private static void StartCoroutine_Prefix1(IEnumerator routine)
        {
            Type routineType = routine.GetType();
            if (!patchedCoroutines.Add(routineType))
                return;

            MethodInfo moveNext = AccessTools.Method(routine.GetType(), "MoveNext");
            if (moveNext != null)
            {
                ProfiledMethod method = new ProfiledMethod(moveNext);
                knownProfiledCoroutines.Add(moveNext, method);
                if (onCoroutineFound != null)
                    onCoroutineFound(moveNext.DeclaringType.Assembly, method);
            }    
        }

        private static Dictionary<Type, HashSet<string>> patchedCoroutinesNames = new Dictionary<Type, HashSet<string>>();

        private static void StartCoroutine_Prefix2(MonoBehaviour __instance, string methodName)
        {
            Type instanceType = __instance.GetType();
            if (patchedCoroutinesNames.TryGetValue(instanceType, out HashSet<string> names))
            {
                if (names.Contains(methodName))
                    return;
                else
                    names.Add(methodName);
            }
            else
            {
                patchedCoroutinesNames.Add(instanceType, new HashSet<string>() { methodName });
            }

            MethodInfo moveNext = AccessTools.EnumeratorMoveNext(AccessTools.Method(instanceType, methodName));

            if (moveNext != null)
            {
                ProfiledMethod method = new ProfiledMethod(moveNext);
                knownProfiledCoroutines.Add(moveNext, method);
                if (onCoroutineFound != null)
                    onCoroutineFound(moveNext.DeclaringType.Assembly, method);
            }
        }
    }
}
