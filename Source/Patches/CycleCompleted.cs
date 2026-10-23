#if !RIMWORLD_1_4

using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalBiosculpter.Patches
{
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
}

#endif
