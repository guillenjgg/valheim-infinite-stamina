using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch]
    internal static class PlayerToolStaminaPatch
    {
        private static readonly AccessTools.FieldRef<Player, PieceTable> BuildPieces =
            AccessTools.FieldRefAccess<Player, PieceTable>("m_buildPieces");

        internal static bool IsLocalPlayer(Player player)
        {
            Plugin plugin = Plugin.Instance;
            return plugin != null && Player.m_localPlayer != null && player == Player.m_localPlayer;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Player), nameof(Player.UpdatePlacement), new[] { typeof(bool), typeof(float) });
            yield return AccessTools.Method(typeof(Player), nameof(Player.Repair), new[] { typeof(ItemDrop.ItemData), typeof(Piece) });
        }

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
                    instruction.operand = AccessTools.Method(typeof(PlayerToolStaminaPatch), nameof(HaveToolStamina));
                }
                else if (instruction.Calls(useStamina))
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(PlayerToolStaminaPatch), nameof(UseToolStamina));
                }

                yield return instruction;
            }
        }

        internal static bool IsFreePlacement(Player player)
        {
            if (!IsLocalPlayer(player) || !Plugin.Instance.ModEnabled.Value)
            {
                return false;
            }

            Plugin plugin = Plugin.Instance;

            if (!plugin.OnlySelectedActivities.Value)
            {
                return true;
            }

            PieceTable pieces = BuildPieces(player);
            if (pieces == null)
            {
                return false;
            }

            ItemDrop.ItemData tool = player.RightItem;

            if (pieces.m_skill == Skills.SkillType.Farming ||
                (tool != null && tool.m_shared.m_skillType == Skills.SkillType.Farming))
            {
                return plugin.NoCultivatorStaminaDrain.Value;
            }

            Piece piece = pieces.GetSelectedPiece();
            if (piece != null && (piece.GetComponent<TerrainModifier>() != null || piece.GetComponent<TerrainOp>() != null))
            {
                return plugin.NoHoeStaminaDrain.Value;
            }

            return plugin.NoBuildingStaminaDrain.Value;
        }

        private static bool HaveToolStamina(Character character, float stamina)
        {
            return ActivityStamina.HaveStamina(character, stamina, IsFreePlacement(character as Player));
        }

        private static void UseToolStamina(Character character, float stamina)
        {
            ActivityStamina.UseStamina(character, stamina, IsFreePlacement(character as Player));
        }
    }

    [HarmonyPatch(typeof(Attack), nameof(Attack.GetAttackStamina), new System.Type[] { })]
    internal static class AttackGetAttackStaminaPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Humanoid ___m_character, ItemDrop.ItemData ___m_weapon, ref float __result)
        {
            if (!PlayerToolStaminaPatch.IsLocalPlayer(___m_character as Player) || !Plugin.Instance.ModEnabled.Value || ___m_weapon == null)
            {
                return;
            }

            Plugin plugin = Plugin.Instance;
            Skills.SkillType skill = ___m_weapon.m_shared.m_skillType;
            if ((skill == Skills.SkillType.Farming && (!plugin.OnlySelectedActivities.Value || plugin.NoCultivatorStaminaDrain.Value)) ||
                (skill == Skills.SkillType.Pickaxes && (!plugin.OnlySelectedActivities.Value || plugin.NoPickaxeStaminaDrain.Value)))
            {
                __result = 0f;
            }
        }
    }
}
