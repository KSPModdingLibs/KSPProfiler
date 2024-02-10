using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.LowLevel;

namespace KSPProfiler
{
    public enum InsertType
    {
        After, Before
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class UnitySystemAttribute : Attribute
    {
        public readonly InsertType insertType;
        public readonly string targetSystem;

        public UnitySystemAttribute(InsertType insertType, string targetSystem)
        {
            this.insertType = insertType;
            this.targetSystem = targetSystem;
        }
    }

    public static class GameLoopProfilerInjector
    {
        public static void InsertSystem(Type type)
        {
            PlayerLoopSystem.UpdateFunction del =
                AccessTools.MethodDelegate<PlayerLoopSystem.UpdateFunction>(
                AccessTools.Method(type, "Capture"));

            UnitySystemAttribute attribute = type.GetCustomAttribute<UnitySystemAttribute>();

            PlayerLoopSystem system = new PlayerLoopSystem()
            {
                type = type,
                updateDelegate = del
            };

            PlayerLoopSystem root = PlayerLoop.GetCurrentPlayerLoop();
            InsertSystemRecursive(ref root, system, attribute.targetSystem, attribute.insertType);
            PlayerLoop.SetPlayerLoop(root);
        }

        private static void InsertSystemRecursive(ref PlayerLoopSystem currentSystem, PlayerLoopSystem systemToInsert, string targetSystem, InsertType insertType)
        {
            if (currentSystem.subSystemList == null)
                return;

            int targetIndex = -1;
            for (int i = 0; i < currentSystem.subSystemList.Length; i++)
            {
                if (currentSystem.subSystemList[i].type.Name == targetSystem)
                {
                    targetIndex = i;
                    break;
                }
                else
                {
                    InsertSystemRecursive(ref currentSystem.subSystemList[i], systemToInsert, targetSystem, insertType);
                }
            }

            if (targetIndex > -1)
            {
                PlayerLoopSystem[] subSystemList = new PlayerLoopSystem[currentSystem.subSystemList.Length + 1];
                int j = 0;
                for (int i = 0; i < subSystemList.Length; i++)
                {
                    if (i == targetIndex)
                    {
                        if (insertType == InsertType.Before)
                        {
                            subSystemList[i] = systemToInsert;
                        }
                        else
                        {
                            subSystemList[i] = currentSystem.subSystemList[j];
                            j++;
                            i++;
                            subSystemList[i] = systemToInsert;
                        }
                    }
                    else
                    {
                        subSystemList[i] = currentSystem.subSystemList[j];
                        j++;
                    }
                }

                currentSystem.subSystemList = subSystemList;
            }
        }
    }


}
