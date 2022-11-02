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

    [HarmonyPatch(typeof(CompBiosculpterPod), "SelectPawnsForCycleOptions")]
    internal static class CompBiosculpterPod_SelectPawnsForCycleOptions_Patch
    {
        static AccessTools.FieldRef<CompBiosculpterPod, Dictionary<CompBiosculpterPod_Cycle, CacheAnyPawnEligibleCycle>> cachedAnyPawnEligibleRef = AccessTools.FieldRefAccess<CompBiosculpterPod, Dictionary<CompBiosculpterPod_Cycle, CacheAnyPawnEligibleCycle>>("cachedAnyPawnEligible");

        static AccessTools.FieldRef<CompBiosculpterPod, Pawn> biotunedToRef = AccessTools.FieldRefAccess<CompBiosculpterPod, Pawn>("biotunedTo");

        static void Postfix(CompBiosculpterPod_Cycle cycle, ref List<FloatMenuOption> options, bool shortCircuit, CompBiosculpterPod __instance, ref bool __result)
        {
            var cache = cachedAnyPawnEligibleRef(__instance)[cycle];
            int ticksGame = Find.TickManager.TicksGame;
            if (shortCircuit && (float)ticksGame < cache.gameTime + 2f)
            {
                return;
            }

            if (biotunedToRef(__instance) == null)
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
                            cache.anyEligible = true;
                            __result = true;
                            return;
                        }
                        options.Add((FloatMenuOption)selectParams[2]);
                    }
                }
            }
            var anyEligible = options.Count > 0;
            cache.anyEligible = anyEligible;
            __result = anyEligible;
        }
    }
}
