using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.CheckRun))]
    internal static class PlayerCheckRunPatch
    {
        internal static bool IsInsideCheckRun;

        [HarmonyPrefix]
        internal static void Prefix(Player __instance)
        {
            IsInsideCheckRun = false;

            var plugin = Plugin.Instance;

            if (plugin == null || !plugin.ModEnabled.Value || !plugin.EnableOnlyWhileRunning.Value)
            {
                return;
            }

            if (Player.m_localPlayer == null || __instance != Player.m_localPlayer)
            {
                return;
            }

            IsInsideCheckRun = true;
        }

        [HarmonyFinalizer]
        internal static void Finalizer()
        {
            IsInsideCheckRun = false;
        }
    }
}
