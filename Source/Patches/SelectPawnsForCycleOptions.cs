using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalBiosculpter.Patches
{
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
                        bool selectResult = (bool)select.Invoke(__instance, selectParams)!;
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
