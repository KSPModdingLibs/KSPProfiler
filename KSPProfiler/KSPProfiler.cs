using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.LowLevel;
using Debug = UnityEngine.Debug;

namespace KSPProfiler
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KSPProfilerAddon : MonoBehaviour
    {
        public static bool profilingInProgress;
        private bool startProfiling;
        private bool stopProfiling;
        private int frameCount;

        void Awake()
        {
            ProfiledMethod.InitOnGameStart();
        }

        void Start()
        {
            DontDestroyOnLoad(this);
            new Launcher();
        }

        //void Update()
        //{
        //    if (startProfiling)
        //    {
        //        startProfiling = false;
        //        MethodStats.ClearStats();
        //        frameCount = 0;
        //        profilingInProgress = true;
        //    }

        //    if (profilingInProgress)
        //    {
        //        MethodStats.ProcessStatsOnUpdate();
        //        frameCount++;
        //    }

        //    if (stopProfiling)
        //    {
        //        stopProfiling = false;
        //        profilingInProgress = false;
        //        MethodStats.WriteResults();
        //    }
        //}
    }
}
