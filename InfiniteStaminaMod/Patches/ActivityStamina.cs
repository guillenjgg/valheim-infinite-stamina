using System;
using HarmonyLib;

namespace InfiniteStaminaMod.Patches
{
    internal static class ActivityStamina
    {
        private static readonly Action<Player, long, float> ConsumeStamina =
            AccessTools.MethodDelegate<Action<Player, long, float>>(
                AccessTools.Method(typeof(Player), "RPC_UseStamina", new[] { typeof(long), typeof(float) }));

        internal static bool HaveStamina(Character character, float stamina, bool free)
        {
            return free || character.HaveStamina(stamina);
        }

        internal static void UseStamina(Character character, float stamina, bool free)
        {
            if (free)
            {
                return;
            }

            Player player = character as Player;

            if (PlayerToolStaminaPatch.IsLocalPlayer(player))
            {
                if (stamina != 0f && !float.IsNaN(stamina))
                {
                    ConsumeStamina(player, 0L, stamina * Game.m_staminaRate);
                }
            }
            else
            {
                character.UseStamina(stamina);
            }
        }
    }
}
