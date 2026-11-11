using HarmonyLib;
using System;
using UnityEngine;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Hud), "UpdateStamina", new[] { typeof(Player), typeof(float) })]
    internal static class HudUpdateStaminaPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Player player, Animator ___m_staminaAnimator)
        {
            Plugin plugin = Plugin.Instance;

            if (plugin == null || player == null || player != Player.m_localPlayer || ___m_staminaAnimator == null)
            {
                return;
            }

            if (ShouldHideStamina(player))
            {
                ___m_staminaAnimator.SetBool("Visible", false);
            }
            else if (DateTime.UtcNow < plugin.ShowStaminaUntil)
            {
                ___m_staminaAnimator.SetBool("Visible", true);
            }
        }

        internal static bool ShouldHideStamina(Player player)
        {
            Plugin plugin = Plugin.Instance;

            if (!PlayerToolStaminaPatch.IsLocalPlayer(player) || !plugin.ModEnabled.Value)
            {
                return false;
            }

            if (!plugin.OnlySelectedActivities.Value)
            {
                return true;
            }

            bool protectedActivity = false;

            if (player.IsEncumbered())
            {
                if (!plugin.NoEncumberedStaminaDrain.Value)
                {
                    return false;
                }

                protectedActivity = true;
            }

            if (player.IsSwimming() && !player.IsOnGround())
            {
                return plugin.NoSwimmingStaminaDrain.Value;
            }

            if (player.IsRunning())
            {
                if (!plugin.NoRunningStaminaDrain.Value)
                {
                    return false;
                }

                protectedActivity = true;
            }

            if (player.InPlaceMode())
            {
                return PlayerToolStaminaPatch.IsFreePlacement(player);
            }

            ItemDrop.ItemData tool = player.RightItem;

            if (tool != null)
            {
                switch (tool.m_shared.m_skillType)
                {
                    case Skills.SkillType.Farming:
                        return plugin.NoCultivatorStaminaDrain.Value;
                    case Skills.SkillType.Pickaxes:
                        return plugin.NoPickaxeStaminaDrain.Value;
                }
            }

            return protectedActivity;
        }
    }
}
