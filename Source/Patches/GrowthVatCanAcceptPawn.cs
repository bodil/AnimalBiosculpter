using HarmonyLib;
using RimWorld;
using Verse;

namespace AnimalBiosculpter.Patches
{
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
}
