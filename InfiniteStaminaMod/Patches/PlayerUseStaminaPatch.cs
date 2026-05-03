using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina))]
    internal static class PlayerUseStaminaPatch
    {
        [HarmonyPrefix]
        internal static bool PreventStaminaUse(Player __instance)
        {
            if (InfiniteStaminaPlugin.Instance == null || !InfiniteStaminaPlugin.Instance.IsInfiniteStaminaEnabled || __instance != Player.m_localPlayer)
            {
                return true;
            }
            
            return false;
        }
    }
}
