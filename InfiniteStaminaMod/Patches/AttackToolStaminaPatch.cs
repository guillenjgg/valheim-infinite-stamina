using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace InfiniteStaminaMod.Patches
{
    [HarmonyPatch]
    internal static class AttackToolStaminaPatch
    {
        private static DateTime _lastStaminaLogTime;

        private static readonly AccessTools.FieldRef<Attack, ItemDrop.ItemData> Weapon =
            AccessTools.FieldRefAccess<Attack, ItemDrop.ItemData>("m_weapon");

        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(typeof(Attack), nameof(Attack.Start), new[]
            {
                typeof(Humanoid), typeof(UnityEngine.Rigidbody), typeof(ZSyncAnimation),
                typeof(CharacterAnimEvent), typeof(VisEquipment), typeof(ItemDrop.ItemData),
                typeof(Attack), typeof(float), typeof(float)
            });
            yield return AccessTools.Method(typeof(Attack), nameof(Attack.Update), new[] { typeof(float) });
            yield return AccessTools.Method(typeof(Attack), nameof(Attack.FireProjectileBurst), new System.Type[] { });
        }

        [HarmonyTranspiler]
        internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo haveStamina = AccessTools.Method(typeof(Character), nameof(Character.HaveStamina), new[] { typeof(float) });
            MethodInfo useStamina = AccessTools.Method(typeof(Character), nameof(Character.UseStamina), new[] { typeof(float) });

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(haveStamina) || instruction.Calls(useStamina))
                {
                    bool check = instruction.Calls(haveStamina);
                    CodeInstruction context = new CodeInstruction(OpCodes.Ldarg_0);
                    context.labels.AddRange(instruction.labels);
                    context.blocks.AddRange(instruction.blocks);
                    instruction.labels.Clear();
                    instruction.blocks.Clear();
                    yield return context;

                    instruction.opcode = OpCodes.Call;
                    instruction.operand = AccessTools.Method(typeof(AttackToolStaminaPatch),
                        check ? nameof(HaveAttackStamina) : nameof(UseAttackStamina));
                }

                yield return instruction;
            }
        }

        private static bool? GetFreeAttack(Character character, Attack attack)
        {
            if (!PlayerToolStaminaPatch.IsLocalPlayer(character as Player))
            {
                return null;
            }

            ItemDrop.ItemData weapon = Weapon(attack);

            if (weapon == null)
            {
                return null;
            }

            switch (weapon.m_shared.m_skillType)
            {
                case Skills.SkillType.Farming:
                    return Plugin.Instance.ModEnabled.Value && Plugin.Instance.NoCultivatorStaminaDrain.Value;
                case Skills.SkillType.Pickaxes:
                    return Plugin.Instance.ModEnabled.Value && Plugin.Instance.NoPickaxeStaminaDrain.Value;
                default:
                    return null;
            }
        }

        private static bool HaveAttackStamina(Character character, float stamina, Attack attack)
        {
            bool? free = GetFreeAttack(character, attack);

            return free.HasValue
                ? ActivityStamina.HaveStamina(character, stamina, free.Value)
                : character.HaveStamina(stamina);
        }

        private static void UseAttackStamina(Character character, float stamina, Attack attack)
        {
            bool? free = GetFreeAttack(character, attack);

            if (free.HasValue)
            {
                Player player = character as Player;
                float before = player.GetStamina();

                ActivityStamina.UseStamina(character, stamina, free.Value);

                if (Plugin.Instance.DebugLogging.Value && DateTime.UtcNow - _lastStaminaLogTime >= TimeSpan.FromSeconds(1))
                {
                    _lastStaminaLogTime = DateTime.UtcNow;
                    ItemDrop.ItemData weapon = Weapon(attack);
                    Plugin.Log.LogInfo($"{weapon.m_shared.m_skillType}: infinite={free.Value}, cost={stamina:0.##}, worldRate={Game.m_staminaRate:0.##}, stamina={before:0.##}->{player.GetStamina():0.##}");
                }
            }
            else
            {
                character.UseStamina(stamina);
            }
        }
    }
}
