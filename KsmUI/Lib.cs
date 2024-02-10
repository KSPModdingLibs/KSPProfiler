using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace KsmUI
{
    public static class Lib
    {
        private const string modName = "KsmUI";

        public enum LogLevel
        {
            Message,
            Warning,
            Error
        }

        ///<summary>write a message to the log</summary>
        public static void Log(string message, LogLevel level = LogLevel.Message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
        {
            switch (level)
            {
                default:
                    UnityEngine.Debug.Log($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
            }
        }

        ///<summary>write a message and the call stack to the log</summary>
        public static void LogStack(string message, LogLevel level = LogLevel.Message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            StackTrace trace;

            switch (level)
            {
                default:
                    trace = new StackTrace();
                    UnityEngine.Debug.Log($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}\r\n\t{trace.ToString().Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Warning:
                    trace = new StackTrace();
                    UnityEngine.Debug.LogWarning($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}\r\n\t{trace.ToString().Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Error:
                    // KSP will already log the stacktrace if the log level is error
                    UnityEngine.Debug.LogError($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}\r\n");
                    return;
            }
        }

        ///<summary>write a message to the log, only on DEBUG and DEVBUILD builds</summary>
        [Conditional("DEBUG"), Conditional("DEVBUILD")]
        public static void LogDebug(string message, LogLevel level = LogLevel.Message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
        {
            switch (level)
            {
                default:
                    UnityEngine.Debug.Log($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
            }
        }

        ///<summary>write a message and the full call stack to the log, only on DEBUG and DEVBUILD builds</summary>
        [Conditional("DEBUG"), Conditional("DEVBUILD")]
        public static void LogDebugStack(string message, LogLevel level = LogLevel.Message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "")
        {
            StackTrace trace;

            switch (level)
            {
                default:
                    trace = new StackTrace();
                    UnityEngine.Debug.Log($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}\r\n\t{trace.ToString().Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Warning:
                    trace = new StackTrace();
                    UnityEngine.Debug.LogWarning($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}\r\n\t{trace.ToString().Replace("\n", "\r\n\t").TrimEnd()}");
                    return;
                case LogLevel.Error:
                    // KSP will already log the stacktrace if the log level is error
                    UnityEngine.Debug.LogError($"[{modName}:{Path.GetFileNameWithoutExtension(sourceFilePath)}.{memberName}] {message.Replace("\n", "\r\n\t").TrimEnd()}\r\n");
                    return;
            }
        }

        // store the random number generator
        static System.Random rng = new System.Random();

        ///<summary>return random float [0..1]</summary>
        public static float RandomFloat()
        {
            return (float)rng.NextDouble();
        }

        ///<summary>return random float [min, max]</summary>
        public static float RandomFloat(float min, float max)
        {
            return (max - min) * RandomFloat() / 1f + min;
        }
    }
}
