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

            var plugin = InfiniteStaminaPlugin.Instance;

            if (plugin == null || !plugin.IsInfiniteStaminaEnabled || !plugin.IsEnableOnlyWhileRunning)
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
