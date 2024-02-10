using KSP.UI.Screens;
using UnityEngine;

namespace KSPProfiler
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class KSPProfilerAddon : MonoBehaviour
    {
        public static ApplicationLauncherButton AppButton {get; private set; }

        void Start()
        {
            DontDestroyOnLoad(this);
            GameEvents.onGUIApplicationLauncherReady.Add(Create);
        }

        public void Create()
        {
            if (AppButton != null)
                return;

            Texture2D icon = GameDatabase.Instance.GetTexture("KSPProfiler/Icons/appIcon", false);

            AppButton = Instantiate(ApplicationLauncher.Instance.listItemPrefab);
            AppButton.Setup(null, null, null, null, null, null, icon);
            AppButton.VisibleInScenes =
                ApplicationLauncher.AppScenes.MAINMENU
                | ApplicationLauncher.AppScenes.SPACECENTER
                | ApplicationLauncher.AppScenes.FLIGHT
                | ApplicationLauncher.AppScenes.MAPVIEW
                | ApplicationLauncher.AppScenes.TRACKSTATION
                | ApplicationLauncher.AppScenes.VAB
                | ApplicationLauncher.AppScenes.SPH;

            ApplicationLauncher.Instance.AddModApplication(AppButton);

            AppButton.onLeftClick = UIManager.Toggle;
        }
    }
}
