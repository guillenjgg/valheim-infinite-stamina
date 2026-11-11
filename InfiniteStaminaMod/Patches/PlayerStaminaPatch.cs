using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UseStamina), new[] { typeof(float) })]
    internal static class PlayerUseStaminaPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Player __instance)
        {
            Plugin plugin = Plugin.Instance;

            return !PlayerToolStaminaPatch.IsLocalPlayer(__instance) ||
                !plugin.ModEnabled.Value || plugin.OnlySelectedActivities.Value;
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.HaveStamina), new[] { typeof(float) })]
    internal static class PlayerHaveStaminaPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Player __instance, ref bool __result)
        {
            Plugin plugin = Plugin.Instance;

            if (!PlayerToolStaminaPatch.IsLocalPlayer(__instance) ||
                !plugin.ModEnabled.Value || plugin.OnlySelectedActivities.Value)
            {
                return true;
            }

            __result = true;

            return false;
        }
    }
}
