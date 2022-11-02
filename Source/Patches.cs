using System;
using System.Collections.Generic;
using System.Linq;
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

        static AccessTools.FieldRef<CompBiosculpterPod, List<FloatMenuOption>> cycleEligiblePawnOptionsRef = AccessTools.FieldRefAccess<CompBiosculpterPod, List<FloatMenuOption>>("cycleEligiblePawnOptions");

        static void Postfix(CompBiosculpterPod_Cycle cycle, ref List<FloatMenuOption> options, bool shortCircuit, CompBiosculpterPod __instance, ref bool __result)
        {
            int ticksGame = Find.TickManager.TicksGame;
            if (shortCircuit && (float)ticksGame < cachedAnyPawnEligibleRef(__instance)[cycle].gameTime + 2f)
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
                        FloatMenuOption option = (FloatMenuOption)selectParams[2];
                        if (selectResult && shortCircuit)
                        {
                            cachedAnyPawnEligibleRef(__instance)[cycle].anyEligible = true;
                            __result = cachedAnyPawnEligibleRef(__instance)[cycle].anyEligible;
                            return;
                        }
                        cycleEligiblePawnOptionsRef(__instance).Add(option);
                    }
                }
            }
            cachedAnyPawnEligibleRef(__instance)[cycle].anyEligible = (cycleEligiblePawnOptionsRef(__instance).Count > 0);
            __result = cachedAnyPawnEligibleRef(__instance)[cycle].anyEligible;
        }
    }
}
