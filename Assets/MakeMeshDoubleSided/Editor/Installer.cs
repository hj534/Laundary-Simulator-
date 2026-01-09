using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
#endif
namespace Kamgam.MMDS
{
    public class Installer
    {
        public const string AssetName = "Make Mesh";
        public const string Version = "1.0.2";

        public static string AssetRootPath = "Assets/MakeMeshDoubleSided/";

        public static Version GetVersion() => new Version(Version);

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts(998001)]
        public static void InstallIfNeeded()
        {
            bool versionChanged = VersionHelper.UpgradeVersion(GetVersion, out Version oldVersion, out Version newVersion);
            if (versionChanged)
            {
                if (versionChanged)
                {
                    Debug.Log(AssetName + " version changed from " + oldVersion + " to " + newVersion);
                    showWelcomeMessage();
                }
            }
        }

        public int callbackOrder => 0;

        static void showWelcomeMessage()
        {
            MaterialShaderFixer.FixMaterialsDelayed(null);
        }

        [MenuItem("Tools/" + AssetName + "/More Assets by KAMGAM", priority = 511)]
        public static void MoreAssets()
        {
            Application.OpenURL("https://kamgam.com/unity-assets?ref=asset");
        }

        [MenuItem("Tools/" + AssetName + "/Version " + Version, priority = 512)]
        public static void LogVersion()
        {
            Debug.Log(AssetName + " v" + Version);
        }
#endif
    }
}