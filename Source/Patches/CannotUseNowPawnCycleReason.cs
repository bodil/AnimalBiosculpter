using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalBiosculpter.Patches
{
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
}
