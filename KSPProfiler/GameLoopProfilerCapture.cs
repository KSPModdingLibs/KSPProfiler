using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace KSPProfiler
{
    public abstract class GameLoopProfilerCaptureBase
    {
        public static List<GameLoopProfilerCaptureBase> captures = new List<GameLoopProfilerCaptureBase>();

        protected static double[] buffer = new double[GameLoopProfiler.maxCapturedFrames];

        public static void CheckBufferSize(int capturedFramesCount)
        {
            if (buffer.Length != capturedFramesCount)
                buffer = new double[capturedFramesCount];
        }

        public double mean;
        public double median;
        public double worst25;
        public double worst1;

        private static double millisecondsFrequency = Stopwatch.Frequency / 1000.0;

        private long startTime;
        private long elapsed;
        private bool isRunning;

        public List<double> lastTimes = new List<double>(500);
        public double[] times = new double[GameLoopProfiler.maxCapturedFrames];

        public GameLoopProfilerCaptureBase()
        {
            captures.Add(this);
        }

        public abstract void UpdateTimes();

        public abstract void UpdateStats();

        public void ClearLastTimes()
        {
            lastTimes.Clear();
        }

        public virtual void ClearTimes()
        {
            Array.Clear(times, 0, times.Length);
        }

        protected void ResizeArrayCopyToEnd(ref double[] array, int newSize)
        {
            double[] newArray = new double[newSize];
            int oldSize = array.Length;
            int entriesToCopy = Math.Min(oldSize, newSize);
            Array.Copy(array, oldSize - entriesToCopy, newArray, newSize - entriesToCopy, entriesToCopy);
            array = newArray;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start(long timestamp = -1L)
        {
            if (!isRunning)
            {
                startTime = timestamp > 0 ? timestamp : Stopwatch.GetTimestamp();
                isRunning = true;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Stop(long timestamp = -1L)
        {
            if (isRunning)
            {
                if (timestamp < 0)
                    timestamp = Stopwatch.GetTimestamp();

                elapsed += timestamp - startTime;
                isRunning = false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Restart(long timestamp = -1L)
        {
            elapsed = 0L;
            startTime = timestamp > 0 ? timestamp : Stopwatch.GetTimestamp();
            isRunning = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            elapsed = 0L;
            isRunning = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double ElapsedMilliseconds(long timestamp = -1L)
        {
            if (isRunning)
            {
                if (timestamp < 0)
                    timestamp = Stopwatch.GetTimestamp();

                elapsed += timestamp - startTime;
                startTime = timestamp;
            }

            return elapsed / millisecondsFrequency;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CaptureElapsed(long timestamp = -1L)
        {
            if (isRunning)
            {
                if (timestamp < 0)
                    timestamp = Stopwatch.GetTimestamp();

                elapsed += timestamp - startTime;
                startTime = timestamp;
            }

            lastTimes.Add(elapsed / millisecondsFrequency);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CaptureElapsedAndRestart(long timestamp = -1L)
        {
            if (timestamp < 0L)
                timestamp = Stopwatch.GetTimestamp();

            if (isRunning)
                elapsed += timestamp - startTime;

            lastTimes.Add(elapsed / millisecondsFrequency);

            elapsed = 0L;
            startTime = timestamp;
            isRunning = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UpdateElapsed(long timestamp = -1L)
        {
            if (isRunning)
            {
                if (timestamp < 0)
                    timestamp = Stopwatch.GetTimestamp();

                elapsed += timestamp - startTime;
                startTime = timestamp;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CapturePreviousElapsed()
        {
            lastTimes.Add(elapsed / millisecondsFrequency);
        }
    }

    public class GameLoopProfilerFixedCapture : GameLoopProfilerCaptureBase
    {
        private double[] perCallTimes = new double[GameLoopProfiler.maxCapturedFrames];

        public double meanPerCall;
        public double medianPerCall;
        public double worst25PerCall;
        public double worst1PerCall;

        public override void UpdateTimes()
        {
            int capturedFrames = GameLoopProfiler.fixedCountsInCapturedFrames.Count;
            if (capturedFrames == 0)
                return;

            int maxFrames = GameLoopProfiler.maxCapturedFrames;
            if (times.Length != maxFrames)
                ResizeArrayCopyToEnd(ref times, maxFrames);

            int framesToKeep = maxFrames - capturedFrames;

            if (framesToKeep > 0)
                Array.Copy(times, capturedFrames, times, 0, framesToKeep);

            int capturedCalls = lastTimes.Count;

            int maxCalls = GameLoopProfiler.maxCapturedFrames;
            if (perCallTimes.Length != maxCalls)
                ResizeArrayCopyToEnd(ref perCallTimes, maxCalls);

            if (capturedCalls == 0)
                return;

            int callsToKeep = maxCalls - capturedCalls;

            if (callsToKeep > 0)
                Array.Copy(perCallTimes, capturedCalls, perCallTimes, 0, callsToKeep);

            int i = capturedCalls;
            int j = maxCalls - 1;
            int k = maxFrames - 1;
            int l = capturedFrames - 1;
            int callsInFrame = GameLoopProfiler.fixedCountsInCapturedFrames[l];
            double perFrameTime = 0.0;
            while (i-- > 0)
            {
                double callTime = lastTimes[i];
                perCallTimes[j] = callTime;
                j--;

                if (callsInFrame > 0)
                    perFrameTime += callTime;

                callsInFrame--;

                if (callsInFrame <= 0)
                {
                    times[k] = perFrameTime;
                    perFrameTime = 0.0;
                    k--;
                    if (l-- > 0)
                        callsInFrame = GameLoopProfiler.fixedCountsInCapturedFrames[l];
                }
            }

            lastTimes.Clear();
        }

        public override void UpdateStats()
        {
            Array.Copy(times, times.Length - GameLoopProfiler.capturedFrames, buffer, 0, GameLoopProfiler.capturedFrames);
            mean = ArrayStatistics.Mean(buffer);
            median = ArrayStatistics.MedianInplace(buffer);
            worst25 = ArrayStatistics.QuantileInplace(buffer, 0.75);
            worst1 = ArrayStatistics.QuantileInplace(buffer, 0.99);

            Array.Copy(perCallTimes, perCallTimes.Length - GameLoopProfiler.capturedFrames, buffer, 0, GameLoopProfiler.capturedFrames);
            meanPerCall = ArrayStatistics.Mean(buffer);
            medianPerCall = ArrayStatistics.MedianInplace(buffer);
            worst25PerCall = ArrayStatistics.QuantileInplace(buffer, 0.75);
            worst1PerCall = ArrayStatistics.QuantileInplace(buffer, 0.99);
        }

        public override void ClearTimes()
        {
            Array.Clear(times, 0, times.Length);
            Array.Clear(perCallTimes, 0, perCallTimes.Length);
        }
    }

    public class GameLoopProfilerUpdateCapture : GameLoopProfilerCaptureBase
    {
        public override void UpdateTimes()
        {
            int capturedFrames = lastTimes.Count;
            if (capturedFrames == 0)
                return;

            int maxFrames = GameLoopProfiler.maxCapturedFrames;
            if (times.Length != maxFrames)
                ResizeArrayCopyToEnd(ref times, maxFrames);

            int framesToKeep = maxFrames - capturedFrames;

            if (framesToKeep > 0)
            {
                Array.Copy(times, capturedFrames, times, 0, framesToKeep);
            }

            int i = capturedFrames;
            int j = maxFrames - 1;
            while (i-- > 0)
            {
                times[j] = lastTimes[i];
                j--;
            }

            lastTimes.Clear();
        }

        public override void UpdateStats()
        {
            Array.Copy(times, times.Length - GameLoopProfiler.capturedFrames, buffer, 0, GameLoopProfiler.capturedFrames);
            mean = ArrayStatistics.Mean(buffer);
            median = ArrayStatistics.MedianInplace(buffer);
            worst25 = ArrayStatistics.QuantileInplace(buffer, 0.75);
            worst1 = ArrayStatistics.QuantileInplace(buffer, 0.99);
        }
    }

    public class GameLoopProfilerFixedInFrameCapture : GameLoopProfilerCaptureBase
    {
        public override void UpdateTimes()
        {
            int capturedFrames = GameLoopProfiler.fixedCountsInCapturedFrames.Count;
            if (capturedFrames == 0)
                return;

            int maxFrames = GameLoopProfiler.maxCapturedFrames;
            if (times.Length != maxFrames)
                ResizeArrayCopyToEnd(ref times, maxFrames);

            int framesToKeep = maxFrames - capturedFrames;

            if (framesToKeep > 0)
            {
                Array.Copy(times, capturedFrames, times, 0, framesToKeep);
            }

            int i = capturedFrames;
            int j = maxFrames - 1;
            while (i-- > 0 && j >= 0)
            {
                times[j] = GameLoopProfiler.fixedCountsInCapturedFrames[i];
                j--;
            }
        }

        public override void UpdateStats()
        {
            Array.Copy(times, times.Length - GameLoopProfiler.capturedFrames, buffer, 0, GameLoopProfiler.capturedFrames);
            mean = ArrayStatistics.Mean(buffer);
            median = ArrayStatistics.MedianInplace(buffer);
            worst25 = ArrayStatistics.QuantileInplace(buffer, 0.75);
            worst1 = ArrayStatistics.QuantileInplace(buffer, 0.99);
        }
    }
}
