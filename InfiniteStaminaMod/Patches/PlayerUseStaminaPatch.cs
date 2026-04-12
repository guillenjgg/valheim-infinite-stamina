using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
    public static class PlayerUseStaminaPatch
    {
        [HarmonyPrefix]
        private static bool PreventStaminaUse(Player __instance)
        {
            if (InfiniteStaminaPlugin.Instance == null || !InfiniteStaminaPlugin.Instance.IsInfiniteStaminaEnabled || __instance != Player.m_localPlayer)
            {
                return true;
            }

            return false;
        }
    }
}
