using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
    internal static class PlayerUseStaminaPatch
    {
        [HarmonyPrefix]
        internal static bool PreventStaminaUse(Player __instance)
        {
            var plugin = Plugin.Instance;

            if (plugin == null || !plugin.ModEnabled.Value)
            {
                return true;
            }

            if (Player.m_localPlayer == null || __instance != Player.m_localPlayer)
            {
                return true;
            }

            // If EnableOnlyWhileRunning is set to false we never want to consume stamina, so prevent UseStamina from running
            if (!plugin.EnableOnlyWhileRunning.Value)
            {
                return false;
            }

            if (PlayerCheckRunPatch.IsInsideCheckRun)
            {
                return false;
            }

            return true;
        }
    }
}
