using System;
using System.Collections.Generic;
using System.Reflection;
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
        static void Postfix(CompBiosculpterPod_Cycle cycle, ref List<FloatMenuOption> options, bool shortCircuit, Dictionary<CompBiosculpterPod_Cycle, CacheAnyPawnEligibleCycle> ___cachedAnyPawnEligible, Pawn ___biotunedTo, CompBiosculpterPod __instance, ref bool __result)
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
}
