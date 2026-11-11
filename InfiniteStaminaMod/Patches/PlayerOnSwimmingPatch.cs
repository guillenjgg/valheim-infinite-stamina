using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.OnSwimming), new[] { typeof(UnityEngine.Vector3), typeof(float) })]
    internal static class PlayerOnSwimmingPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo haveStamina = AccessTools.Method(typeof(Character), nameof(Character.HaveStamina), new[] { typeof(float) });
            MethodInfo useStamina = AccessTools.Method(typeof(Character), nameof(Character.UseStamina), new[] { typeof(float) });

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(haveStamina))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(PlayerOnSwimmingPatch), nameof(HaveSwimStamina));
                }
                else if (instruction.Calls(useStamina))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(PlayerOnSwimmingPatch), nameof(UseSwimStamina));
                }

                yield return instruction;
            }
        }

        private static bool IsFreeSwimming(Character character)
        {
            return PlayerToolStaminaPatch.IsLocalPlayer(character as Player) && Plugin.Instance.ModEnabled.Value && (!Plugin.Instance.OnlySelectedActivities.Value || Plugin.Instance.NoSwimmingStaminaDrain.Value);
        }

        private static bool HaveSwimStamina(Character character, float stamina)
        {
            return ActivityStamina.HaveStamina(character, stamina, IsFreeSwimming(character));
        }

        private static void UseSwimStamina(Character character, float stamina)
        {
            ActivityStamina.UseStamina(character, stamina, IsFreeSwimming(character));
        }
    }
}
