using HarmonyLib;
using ReplayEditor;
using SkaterXL.Data;
using System;

namespace BetterReplay
{
    [HarmonyPatch(typeof(ReplayPlaybackController))]
    [HarmonyPatch("LoadReplayData", new Type[] { typeof(ReplayPlayerRecording), typeof(bool) })]
    public static class ReplaySavePatch
    {
        static void Prefix(ref ReplayPlayerRecording data, bool startAtBeginning)
        {
            if (Main.settings.load_current_customizations)
            {
                data.customizations = PlayerController.Main.characterCustomizer.CurrentCustomizations;
            }
        }
    }
}
