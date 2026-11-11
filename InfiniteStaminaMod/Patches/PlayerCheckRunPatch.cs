using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.CheckRun), new[] { typeof(UnityEngine.Vector3), typeof(float) })]
    internal static class PlayerCheckRunPatch
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
                    instruction.operand = AccessTools.Method(typeof(PlayerCheckRunPatch), nameof(HaveRunStamina));
                }
                else if (instruction.Calls(useStamina))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(PlayerCheckRunPatch), nameof(UseRunStamina));
                }

                yield return instruction;
            }
        }

        private static bool IsFreeRunning(Character character)
        {
            return PlayerToolStaminaPatch.IsLocalPlayer(character as Player) && Plugin.Instance.ModEnabled.Value && Plugin.Instance.NoRunningStaminaDrain.Value;
        }

        private static bool HaveRunStamina(Character character, float stamina)
        {
            return ActivityStamina.HaveStamina(character, stamina, IsFreeRunning(character));
        }

        private static void UseRunStamina(Character character, float stamina)
        {
            ActivityStamina.UseStamina(character, stamina, IsFreeRunning(character));
        }
    }
}
