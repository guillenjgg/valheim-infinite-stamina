using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.UpdateStats), new[] { typeof(float) })]
    internal static class PlayerUpdateStatsPatch
    {
        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            FieldInfo drainField = AccessTools.Field(typeof(Player), nameof(Player.m_encumberedStaminaDrain));
            MethodInfo getDrain = AccessTools.Method(typeof(PlayerUpdateStatsPatch), nameof(GetEncumberedStaminaDrain));
            MethodInfo useStamina = AccessTools.Method(typeof(Character), nameof(Character.UseStamina), new[] { typeof(float) });
            bool found = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(useStamina))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(PlayerUpdateStatsPatch), nameof(UseEncumberedStamina));
                }

                yield return instruction;

                if (instruction.LoadsField(drainField))
                {
                    found = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Call, getDrain);
                }
            }

            if (!found && Plugin.Instance.DebugLogging.Value)
            {
                Plugin.Log.LogError("Could not find encumbered stamina drain in Player.UpdateStats. Encumbrance patch was not applied.");
            }
        }

        private static void UseEncumberedStamina(Character character, float stamina)
        {
            ActivityStamina.UseStamina(character, stamina, false);
        }

        private static float GetEncumberedStaminaDrain(float drain, Player player)
        {
            Plugin plugin = Plugin.Instance;

            if (plugin == null || !plugin.ModEnabled.Value || !plugin.NoEncumberedStaminaDrain.Value)
            {
                return drain;
            }

            if (Player.m_localPlayer == null || player != Player.m_localPlayer)
            {
                return drain;
            }

            return 0f;
        }
    }
}