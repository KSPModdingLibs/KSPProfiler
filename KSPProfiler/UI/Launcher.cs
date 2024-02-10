using KsmUI;
using KSP.UI.Screens;
using KSP.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
using UnityEngine;
using KSPProfiler.UI;

namespace KSPProfiler
{
    internal class Launcher
    {
        ApplicationLauncherButton appButton;

        public Launcher()
        {
            GameEvents.onGUIApplicationLauncherReady.Add(Create);
        }


        public void Create()
        {
            if (appButton == null)
            {
                Texture2D icon = GameDatabase.Instance.GetTexture("KSPProfiler/Icons/appIcon", false);

                appButton = UnityEngine.Object.Instantiate(ApplicationLauncher.Instance.listItemPrefab);
                appButton.Setup(null, null, null, null, null, null, icon);
                appButton.VisibleInScenes =
                    ApplicationLauncher.AppScenes.MAINMENU
                    | ApplicationLauncher.AppScenes.SPACECENTER
                    | ApplicationLauncher.AppScenes.FLIGHT
                    | ApplicationLauncher.AppScenes.MAPVIEW
                    | ApplicationLauncher.AppScenes.TRACKSTATION
                    | ApplicationLauncher.AppScenes.VAB
                    | ApplicationLauncher.AppScenes.SPH;

                ApplicationLauncher.Instance.AddModApplication(appButton);

                appButton.onLeftClick = () => MainWindow.Toggle();
            }
        }
    }
}
