using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace KSPProfiler
{
    public class ProfiledMethodGroup
    {
        public static List<ProfiledMethodGroup> allGroups = new List<ProfiledMethodGroup>();

        public string name;
        public List<ProfiledMethod> methods = new List<ProfiledMethod>();

        public ProfiledMethodGroup()
        {
            allGroups.Add(this);
        }
    }

    public class ProfiledMethodAssemblyGroup : ProfiledMethodGroup
    {
        public string assemblyName;
        public string assemblyVersion;
        public string assemblyLocation;

        private Assembly assembly;
        private Type[] assemblyTypes;

        public List<ProfiledMethod> monoBehaviourMethods = new List<ProfiledMethod>();
        public List<ProfiledMethod> kspObjectMethods = new List<ProfiledMethod>();
        public List<ProfiledMethod> coroutineMethods = new List<ProfiledMethod>();

        public ProfiledMethodAssemblyGroup(Assembly assembly)
        {
            this.assembly = assembly;

            AddMonoBehaviourMethods();
            AddKSPObjectMethods();
            AddCoroutineMethods();
            ProfiledMethod.onCoroutineFound += OnCoroutineFound;

            methods.Sort((a, b) => a.name.CompareTo(b.name));

            name = Lib.GetAssemblyDetails(assembly, out assemblyName, out string kspAssemblyName, out assemblyVersion, out string kspVersion, out assemblyLocation);
            if (!string.IsNullOrEmpty(kspAssemblyName))
                assemblyName += " / " + kspAssemblyName;
            if (!string.IsNullOrEmpty(kspVersion))
                assemblyVersion += " / " + kspVersion;
        }

        private Type[] AssemblyTypes
        {
            get
            {
                if (assemblyTypes == null)
                {
                    try
                    {
                        assemblyTypes = assembly.GetTypes();
                    }
                    catch (Exception)
                    {
                        assemblyTypes = Type.EmptyTypes;
                    }
                }

                return assemblyTypes;
            }
        }

        private void AddMonoBehaviourMethods()
        {
            Type t_MonoBehaviour = typeof(MonoBehaviour);
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (Type type in AssemblyTypes)
            {
                if (type.IsSubclassOf(t_MonoBehaviour))
                {
                    foreach (MethodInfo methodInfo in type.GetMethods(bindingFlags))
                    {
                        switch (methodInfo.Name)
                        {
                            case "FixedUpdate":
                            case "LateUpdate":
                            case "OnCollisionStay":
                            case "OnGUI":
                            case "OnPostRender":
                            case "OnPreCull":
                            case "OnPreRender":
                            case "OnRenderImage":
                            case "OnRenderObject":
                            case "OnTriggerStay":
                            case "OnWillRenderObject":
                            case "Update":
                                break;
                            default:
                                continue;
                        }

                        if (methodInfo.IsAbstract || methodInfo.ReflectedType != methodInfo.DeclaringType)
                            continue;

                        ProfiledMethod profiledMethod = ProfiledMethod.Get(methodInfo);
                        monoBehaviourMethods.Add(profiledMethod);
                        methods.Add(profiledMethod);
                    }
                }
            }
        }

        private void AddKSPObjectMethods()
        {
            Type t_PartModule = typeof(PartModule);
            Type t_InternalPart = typeof(InternalPart);
            Type t_InternalModule = typeof(InternalModule);
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (Type type in AssemblyTypes)
            {
                if (type.IsSubclassOf(t_PartModule)
                    || type.IsSubclassOf(t_InternalPart)
                    || type.IsSubclassOf(t_InternalModule))
                {
                    foreach (MethodInfo methodInfo in type.GetMethods(bindingFlags))
                    {
                        switch (methodInfo.Name)
                        {
                            case "OnFixedUpdate":
                            case "OnUpdate":
                                break;
                            default:
                                continue;
                        }

                        if (methodInfo.IsAbstract || methodInfo.ReflectedType != methodInfo.DeclaringType)
                            continue;

                        ProfiledMethod profiledMethod = ProfiledMethod.Get(methodInfo);
                        kspObjectMethods.Add(profiledMethod);
                        methods.Add(profiledMethod);
                    }
                }

            }
        }

        /// <summary>
        /// We can't statically find coroutines, we need to wait for them to be started once from a StartCoroutine() call.
        /// This mean that when the group is created, not all coroutines might have been found
        /// </summary>
        private void AddCoroutineMethods()
        {
            foreach (ProfiledMethod coroutineMethod in ProfiledMethod.AllCoroutinesInAssembly(assembly))
            {
                coroutineMethods.Add(coroutineMethod);
                methods.Add(coroutineMethod);
            }
        }

        private void OnCoroutineFound(Assembly assembly, ProfiledMethod profiledMethod)
        {
            if (this.assembly != assembly)
                return;

            coroutineMethods.Add(profiledMethod);
            methods.Add(profiledMethod);
        }
    }
}
