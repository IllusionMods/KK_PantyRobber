using System;
using HarmonyLib;
using KKAPI.Studio;
using Manager;

namespace KK_PantyRobber
{
    internal static class Hooks
    {
        public static void InstallHooks()
        {
            try
            {
                if (!StudioAPI.InsideStudio)
                {
                    PantyRobber.Log("PatchAll Hooks");
                    Harmony.CreateAndPatchAll(typeof(Hooks));
                }
            }
            catch (Exception ex)
            {
                PantyRobber.Log("InstallHooks=" + ex, true);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Scene), "UnLoad", new Type[]
        {
        })]
        public static void PostSceneUnloadHook()
        {
            try
            {
                PantyRobber.Log("PostSceneUnloadHook");
                var currentVisibleGirl = Uty.GetCurrentVisibleGirl();
                if (currentVisibleGirl != null)
                {
                    var chaCtrl = currentVisibleGirl.chaCtrl;
                    if (chaCtrl != null) Uty.GetGameController()?.OnSceneUnload(currentVisibleGirl, chaCtrl);
                }
            }
            catch (Exception ex)
            {
                PantyRobber.Log("PostSceneUnloadHook=" + ex, true);
            }
        }
    }
}