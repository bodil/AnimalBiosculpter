using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalBiosculpter.Patches
{
    [StaticConstructorOnStartup]
    public class Patcher
    {
        public static Harmony instance;
        static Patcher()
        {
            instance = new Harmony("bodilpwnz.AnimalBiosculpter");
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(Building_GrowthVat), nameof(Building_GrowthVat.CanAcceptPawn))]
    internal static class Building_GrowthVat_CanAcceptPawn_Patch
    {
        static void Postfix(Pawn pawn, Building_GrowthVat __instance, ref AcceptanceReport __result)
        {
            if (!__result.Accepted && __result.Reason == "" && pawn.RaceProps.Animal && pawn.HomeFaction == Faction.OfPlayer)
            {
                if (pawn.ageTracker.Adult)
                {
                    __result = "TooOld".Translate(pawn.Named("PAWN"), pawn.ageTracker.AdultMinAge.Named("AGEYEARS"));
                }
                else
                {
                    __result = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CompBiosculpterPod))]
    [HarmonyPatch(nameof(CompBiosculpterPod.CannotUseNowPawnCycleReason))]
    [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Pawn), typeof(CompBiosculpterPod_Cycle), typeof(bool) })]
    internal static class CompBiosculpterPod_CannotUseNowPawnCycleReason_Patch
    {
        static void Postfix(Pawn hauler, Pawn biosculptee, CompBiosculpterPod_Cycle cycle, bool checkIngredients, CompBiosculpterPod __instance, ref string __result)
        {
            if (cycle is CompBiosculpterPod_PleasureCycle && biosculptee.needs.mood == null)
            {
                __result = "AnimalBiosculpterNoPleasureCycle".Translate();
            }
        }
    }

    [HarmonyPatch(typeof(CompBiosculpterPod), "SelectPawnsForCycleOptions")]
    internal static class CompBiosculpterPod_SelectPawnsForCycleOptions_Patch
    {
        static void Postfix(CompBiosculpterPod_Cycle cycle, ref List<FloatMenuOption> options, bool shortCircuit, Dictionary<CompBiosculpterPod_Cycle, CacheAnyPawnEligibleCycle> ___cachedAnyPawnEligible, Pawn? ___biotunedTo, CompBiosculpterPod __instance, ref bool __result)
        {
            int ticksGame = Find.TickManager.TicksGame;
            if (shortCircuit && (float)ticksGame < ___cachedAnyPawnEligible[cycle].gameTime + 2f)
            {
                return;
            }

            if (___biotunedTo == null)
            {
                var pawns = __instance.parent.Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
                foreach (Pawn pawn in pawns)
                {
                    if (pawn.RaceProps.Animal)
                    {
                        var select = AccessTools.Method(typeof(CompBiosculpterPod), "SelectPawnCycleOption", new Type[] { typeof(Pawn), typeof(CompBiosculpterPod_Cycle), typeof(FloatMenuOption).MakeByRefType() });
                        var selectParams = new object[] { pawn, cycle, null! };
                        bool selectResult = (bool)select.Invoke(__instance, selectParams);
                        if (selectResult && shortCircuit)
                        {
                            ___cachedAnyPawnEligible[cycle].anyEligible = true;
                            __result = true;
                            return;
                        }
                        options.Add((FloatMenuOption)selectParams[2]);
                    }
                }
            }
            var anyEligible = options.Count > 0;
            ___cachedAnyPawnEligible[cycle].anyEligible = anyEligible;
            __result = anyEligible;
        }
    }

#if RIMWORLD_1_4

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

#else

    [HarmonyPatch(typeof(CompBiosculpterPod), "CycleCompleted")]
    internal static class CompBiosculpterPod_CycleCompleted_Patch
    {
        static void Prefix(ref int ___tickEntered, CompBiosculpterPod __instance)
        {
            var occupant = __instance.Occupant;
            if (occupant != null && occupant.RaceProps.Animal)
            {
                // The OG CompBiosculpterPod.CycleCompleted() will try to access
                // the occupant's drug policy if tickEntered is greater than
                // zero, regardless of whether there even is an occupant, which
                // throws an exception for animals. So we just set tickEntered
                // to 0 to bypass this.
                ___tickEntered = 0;
            }
        }
    }

#endif
}
