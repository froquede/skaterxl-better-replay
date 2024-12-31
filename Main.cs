using HarmonyLib;
using RapidGUI;
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
        static Texture2D bg;

        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            harmonyInstance?.UnpatchAll(harmonyInstance.Id);
            UnityEngine.Object.Destroy(gameObject);
            return true;
        }

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            harmonyInstance = new Harmony(modEntry.Info.Id);
            harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

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
            try
            {
                if (bg == null)
                {
                    bg = new Texture2D(1, 1);
                    bg.SetPixels(new[] { new Color(.35f, .35f, .35f, .6f) });
                    bg.Apply();
                    RGUIStyle.button.active.background = bg;
                    RGUIStyle.button.normal.background = bg;
                    RGUIStyle.button.focused.background = bg;
                    RGUIStyle.button.hover.background = bg;
                }
            } catch { }

            GUILayout.Label("   ");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical(GUILayout.Width(256));
            GUILayout.Box("<b>Replay handle</b>", GUILayout.Height(21f));
            settings.handle_size = RapidGUI.RGUI.SliderFloat(settings.handle_size, 0f, 64f, 25f, "Size");
            settings.handle_color.a = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.a, 0f, 255f, 0f, "Opacity");
            settings.handle_color.r = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.r, 0f, 255f, 0f, "Red");
            settings.handle_color.g = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.g, 0f, 255f, 0f, "Green");
            settings.handle_color.b = (byte)RapidGUI.RGUI.SliderFloat(settings.handle_color.b, 0f, 255f, 0f, "Blue");

            GUILayout.Box("<b>Pin options</b>", GUILayout.Height(21f));
            settings.pin_movespeed = RGUI.SliderFloat(settings.pin_movespeed, 0f, 60f, 10f, "Move speed");
            settings.pin_rotationspeed = RGUI.SliderFloat(settings.pin_rotationspeed, 0f, 360f, 180f, "Rotation speed");

            GUILayout.Box("<b>Trackers</b>", GUILayout.Height(21f));
            if (RGUI.Button(!settings.disable_messages, "Messages"))
            {
                settings.disable_messages = !settings.disable_messages;
            }
            if (RGUI.Button(!settings.disable_rb_tracker, "RigidBody trackers"))
            {
                settings.disable_rb_tracker = !settings.disable_rb_tracker;
            }
            if (RGUI.Button(!settings.disable_hinge_tracker, "Hinge trackers"))
            {
                settings.disable_hinge_tracker = !settings.disable_hinge_tracker;
            }
            if (RGUI.Button(!settings.disable_animator_tracker, "Animator trackers"))
            {
                settings.disable_animator_tracker = !settings.disable_animator_tracker;
            }
            if (RGUI.Button(!settings.disable_audiosource_tracker, "AudioSource trackers"))
            {
                settings.disable_audiosource_tracker = !settings.disable_audiosource_tracker;
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(256));

            GUILayout.Box("<b>Light (toggled with ctrl + \"L\")</b>", GUILayout.Height(21f));

            settings.light_intensity = RGUI.SliderFloat(settings.light_intensity, 0f, 1000f, 6f, "Intensity");
            settings.light_range = RGUI.SliderFloat(settings.light_range, 0f, 200f, 5f, "Range");
            settings.light_spotangle = RGUI.SliderFloat(settings.light_spotangle, 1f, 360f, 120f, "Angle");
            settings.light_temperature = RGUI.SliderFloat(settings.light_temperature, 0f, 10000f, 6500f, "Color temperature (kelvin)");
            settings.light_dimmer = RGUI.SliderFloat(settings.light_dimmer, 0f, 16f, 0f, "Volumetric dimmer");

            GUILayout.Space(8);
            settings.light_offset.x = RGUI.SliderFloat(settings.light_offset.x, -2f, 2f, 0f, "Light position X");
            settings.light_offset.y = RGUI.SliderFloat(settings.light_offset.y, -2f, 2f, 0f, "Light position Y");
            settings.light_offset.z = RGUI.SliderFloat(settings.light_offset.z, -2f, 2f, 0f, "Light position Z");

            GUILayout.Space(8);

            if (RGUI.Button(settings.double_tap, "Toggle light with double tap on left stick"))
            {
                settings.double_tap = !settings.double_tap;
            }

            GUILayout.Space(5);

            GUILayout.Box("<b>Actions</b>", GUILayout.Height(21f));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Destroy Animator trackers", RGUIStyle.button, GUILayout.Height(42f), GUILayout.Width(212f)))
            {
                gameObject.DestroyAnimatorTracker();
            }

            if (GUILayout.Button("Destroy RigidBody/Hinge trackers", RGUIStyle.button, GUILayout.Height(42f), GUILayout.Width(212f)))
            {
                gameObject.DestroyObjectTracker();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Destroy AudioSource trackers", RGUIStyle.button, GUILayout.Height(42f), GUILayout.Width(212f)))
            {
                gameObject.DestroyAudioSourceTracker();
            }
            if (GUILayout.Button("Rescan objects", RGUIStyle.button, GUILayout.Height(42f), GUILayout.Width(212f)))
            {
                gameObject.AddObjectTrackers();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.Box("<b>Options</b>", GUILayout.Height(21f));

            if (RGUI.Button(settings.mini_info, "Mini info menu"))
            {
                settings.mini_info = !settings.mini_info;
                gameObject.hidden = false;
            }

            GUILayout.Space(5);

            if (RGUI.Button(settings.load_current_customizations, "Load currently equipped gear in saved replays"))
            {
                settings.load_current_customizations = !settings.load_current_customizations;
            }

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(44));
            GUILayout.Label("   ");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

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