using BetterReplay;
using HarmonyLib;
using ReplayEditor;
using SkaterXL.Data;
using System;

[HarmonyPatch(typeof(ReplayPlaybackController))]
[HarmonyPatch("LoadReplayData", new Type[] { typeof(ReplayPlayerRecording), typeof(bool) })]
public static class LoadReplayDataPatch
{
    static void Prefix(ref ReplayPlayerRecording data, bool startAtBeginning)
    {
        if (Main.settings.load_current_customizations)
        {
            data.customizations = PlayerController.Instance.characterCustomizer.CurrentCustomizations;
        }
    }
}
