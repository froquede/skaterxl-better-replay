using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityModManagerNet;

namespace BetterReplay
{
    [EnableReloading]
    static class Main
    {
        public static Settings settings;
        public static Harmony harmonyInstance;
        public static UnityModManager.ModEntry modEntry;
        public static BetterReplay gameObject;

        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            UnityEngine.Object.Destroy(gameObject);
            return true;
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            harmonyInstance = new Harmony(modEntry.Info.Id);
            settings = UnityModManager.ModSettings.Load<Settings>(modEntry);

            gameObject = new GameObject().AddComponent<BetterReplay>();
            UnityEngine.Object.DontDestroyOnLoad(gameObject);

            modEntry.OnGUI = OnGUI;
            modEntry.OnSaveGUI = new Action<UnityModManager.ModEntry>(OnSaveGUI);
            modEntry.OnToggle = new Func<UnityModManager.ModEntry, bool, bool>(OnToggle);
            modEntry.OnUnload = Unload;
            Main.modEntry = modEntry;

            UnityModManager.Logger.Log("Loaded " + modEntry.Info.Id);
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("");
            GUILayout.BeginVertical(GUILayout.Width(256));
            GUILayout.Box("<b>Replay handle color</b>", GUILayout.Height(21f));
            settings.handle_color.r = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.r, 0f, 255f, 0f, "Red");
            settings.handle_color.g = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.g, 0f, 255f, 0f, "Green");
            settings.handle_color.b = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.b, 0f, 255f, 0f, "Blue");
            settings.handle_color.a = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.a, 0f, 255f, 0f, "Opacity");

            GUILayout.Box("<b>Replay handle size</b>", GUILayout.Height(21f));
            settings.handle_size = RapidGUI.RGUI.SliderFloat(settings.handle_size, 0f, 64f, 25f, "Size");
            GUILayout.EndVertical();

            /*if(GUILayout.Button("<b>Save settings</b>", GUILayout.Width(128), GUILayout.Height(32f)))
            {
                settings.Save(modEntry);
            }*/

            settings.Draw(modEntry);
        }

        private static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }

        private static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            UnityModManager.Logger.Log("Toggled " + modEntry.Info.Id);
            return true;
        }
    }
}