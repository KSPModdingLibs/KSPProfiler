using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KSPProfiler
{
    public static class Lib
    {
        private static StringBuilder sb = new StringBuilder();

        private static string gameDataPath;
        private static string GameDataPath
        {
            get
            {
                if (gameDataPath == null)
                    gameDataPath = Path.GetFullPath(KSPUtil.ApplicationRootPath);
 
                return gameDataPath;
            }
        }

        public static string GetAssemblyDetails(Assembly assembly, out string assemblyName, out string kspAssemblyName, out string version, out string kspVersion, out string location)
        {
            sb.Length = 0;
            AssemblyName assemblyNameInfo;
            KSPAssembly kspAssembly;

            try
            {
                location = assembly.Location;
                assemblyNameInfo = assembly.GetName();
                kspAssembly = assembly.GetCustomAttribute<KSPAssembly>();
            }
            catch (Exception)
            {
                assemblyName = string.Empty;
                kspAssemblyName = string.Empty;
                version = string.Empty;
                kspVersion = string.Empty;
                location = string.Empty;
                return string.Empty;
            }

            assemblyName = assemblyNameInfo.Name;
            sb.Append(assemblyName);

            if (kspAssembly != null)
            {
                kspAssemblyName = kspAssembly.name;
                sb.Append(" / ").Append(kspAssemblyName);
            }
            else
            {
                kspAssemblyName = string.Empty;
            }

            version = assemblyNameInfo.Version.ToString();
            sb.Append(version);

            if (kspAssembly != null)
            {
                kspVersion = kspAssembly.versionMajor + "." + kspAssembly.versionMinor + "." + kspAssembly.versionRevision;
                sb.Append(" / ").Append(kspVersion);
            }
            else
            {
                kspVersion = string.Empty;
            }

            sb.Append(" (");
            if (location.Contains("GameData") && location.Length > GameDataPath.Length + 9)
                location = location.Remove(0, GameDataPath.Length + 9);

            sb.Append(location);
            sb.Append(")");

            return sb.ToString();
        }

        public static string GetMethodName(MethodBase method)
        {
            string name;
            if (method.Name == "MoveNext")
            {
                name = method.DeclaringType.Name;
                int start = name.IndexOf('<') + 1;
                int length = name.IndexOf('>') - start;
                name = method.DeclaringType.DeclaringType.Name + "." + name.Substring(start, length) + " (Coroutine)";
            }
            else
            {
                name = method.DeclaringType.Name + "." + method.Name;
            }

            int idx = name.IndexOf("_Patch");
            if (idx > 0)
                name = name.Substring(0, idx);

            return name;
        }
    }
}
