// -----------------------------------------------------------------------
// <copyright file="ExplodeDestructiblePatch.cs" company="Mistaken">
// Copyright (c) Mistaken. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using InventorySystem.Items.ThrowableProjectiles;
using NorthwoodLib.Pools;

#pragma warning disable SA1118 // Parameter should not span multiple lines

namespace Mistaken.ShootableDoors
{
    [HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.ExplodeDestructible))]
    internal static class ExplodeDestructiblePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            var skipLabel = generator.DefineLabel();
            var continueLabel = generator.DefineLabel();

            newInstructions[0].WithLabels(continueLabel);

            newInstructions.InsertRange(0, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Isinst, typeof(ButtonTargetScript)),
                new CodeInstruction(OpCodes.Brtrue_S, skipLabel),

                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Isinst, typeof(DoorTargetScript)),
                new CodeInstruction(OpCodes.Brtrue_S, skipLabel),

                new CodeInstruction(OpCodes.Br_S, continueLabel),

                new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(skipLabel),
                new CodeInstruction(OpCodes.Ret),
            });

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }
    }

    [HarmonyPatch(typeof(ExplosionGrenade), nameof(ExplosionGrenade.Explode))]
    internal static class ExplodePatch
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            List<CodeInstruction> newInstructions = ListPool<CodeInstruction>.Shared.Rent(instructions);

            int index = newInstructions.FindIndex(x => x.opcode == OpCodes.Callvirt && x.operand is MethodInfo info && info.Name == "Add");

            newInstructions[index - 4].operand = newInstructions[index - 15].operand;

            for (int z = 0; z < newInstructions.Count; z++)
                yield return newInstructions[z];

            ListPool<CodeInstruction>.Shared.Return(newInstructions);
            yield break;
        }
    }
}
