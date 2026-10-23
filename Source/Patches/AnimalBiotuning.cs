#if RIMWORLD_1_4

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalBiosculpter.Patches
{
    // These two transpilers make the biosculpter pod treat being biotuned
    // to an animal the same as not being biotuned at all: animals don't
    // benefit from biotuning, and shouldn't get the biotune gizmos or the
    // biotuned tick behaviour. Only needed on Rimworld 1.4.

    [HarmonyPatch(typeof(CompBiosculpterPod), "CompTick")]
    public static class CompTick_Patcher
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldfld && code[i+1].opcode == OpCodes.Brfalse_S)
                {
                    insertionIndex = i + 2;
                    break;
                }
            }
            if (insertionIndex != -1)
            {
                Label jump = (Label)(code[insertionIndex - 1].operand);

                var ItI = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompBiosculpterPod), "biotunedTo")),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps", new Type[] { })),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_Animal", new Type[] { })),
                    new CodeInstruction(OpCodes.Brtrue_S, jump)
                };

                code.InsertRange(insertionIndex, ItI);
            }

            return code;
        }
    }

    [HarmonyPatch]
    public static class CompGetGizmosExtra_Patcher
    {
        public static MethodBase TargetMethod()
        {
            var type = AccessTools.FirstInner(typeof(CompBiosculpterPod), t => t.Name.Contains("<CompGetGizmosExtra>d__88"));
            return AccessTools.FirstMethod(type, method => method.Name.Contains("MoveNext"));
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = new List<CodeInstruction>(instructions);

            int insertionIndex = -1;
            for (int i = 0; i < code.Count - 1; i++)
            {
                if (code[i].opcode == OpCodes.Ldloc_2 && code[i + 1].opcode == OpCodes.Ldfld && code[i + 2].opcode == OpCodes.Brfalse)
                {
                    insertionIndex = i + 3;
                    break;
                }
            }
            if (insertionIndex != -1)
            {
                Label jump = (Label)(code[insertionIndex - 1].operand);

                var ItI = new List<CodeInstruction>
                {
                    new CodeInstruction(OpCodes.Ldloc_2),
                    new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CompBiosculpterPod), "biotunedTo")),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Pawn), "get_RaceProps", new Type[] { })),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(RaceProperties), "get_Animal", new Type[] { })),
                    new CodeInstruction(OpCodes.Brtrue, jump)
                };

                code.InsertRange(insertionIndex, ItI);
            }

            return code;
        }
    }
}

#endif
