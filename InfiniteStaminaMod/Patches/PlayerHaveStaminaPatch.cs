using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.HaveStamina))]
    internal static class PlayerHaveStaminaPatch
    {
        [HarmonyPrefix]
        internal static bool ForceHaveStamina(ref bool __result, Player __instance)
        {
            var plugin = InfiniteStaminaPlugin.Instance;

            if (plugin == null || !plugin.IsInfiniteStaminaEnabled)
            {
                return true;
            }

            if (plugin.IsEnableOnlyWhileRunning && !PlayerCheckRunPatch.IsInsideCheckRun)
            {
                return true;
            }

            if (Player.m_localPlayer == null || __instance != Player.m_localPlayer)
            {
                return true;
            }

            __result = true;
            return false;
        }
    }
}
