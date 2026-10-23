#if !RIMWORLD_1_4 && !RIMWORLD_1_5

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimalBiosculpter.Patches
{
    // Mirrors the vanilla logic for carrying downed colonists to a
    // biosculpter pod, but for colony animals.
    internal static class CarryToBiosculpterUtility
    {
        internal static bool IsCarryableAnimal(Pawn pawn)
        {
            return pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && pawn.Downed;
        }

        internal static IEnumerable<FloatMenuOption> CarryOptions(Pawn hauler, Pawn clickedPawn)
        {
            if (!hauler.CanReserveAndReach(clickedPawn, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, ignoreOtherReservations: true))
            {
                yield break;
            }
            Thing thing = CompBiosculpterPod.FindPodFor(hauler, clickedPawn, biotuned: true) ?? CompBiosculpterPod.FindPodFor(hauler, clickedPawn, biotuned: false);
            if (thing == null)
            {
                yield break;
            }
            CompBiosculpterPod podComp = thing.TryGetComp<CompBiosculpterPod>();
            if (podComp == null)
            {
                yield break;
            }
            foreach (CompBiosculpterPod_Cycle cycle in podComp.AvailableCycles)
            {
                CompBiosculpterPod_Cycle cycle1 = cycle;
                TaggedString label = "CarryToBiosculpterPod".Translate(clickedPawn.Named("PAWN"), cycle1.Props.label.Named("CYCLE"));

                if (clickedPawn.IsQuestLodger())
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label + " (" + "CryptosleepCasketGuestsNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), hauler, clickedPawn);
                    yield break;
                }

                if (clickedPawn.GetExtraHostFaction() != null)
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label + " (" + "CryptosleepCasketGuestPrisonersNotAllowed".Translate() + ")", null, MenuOptionPriority.Default, null, clickedPawn), hauler, clickedPawn);
                    yield break;
                }

                string reason = podComp.CannotUseNowCycleReason(cycle1) ?? podComp.CannotUseNowPawnCycleReason(hauler, clickedPawn, cycle1);
                if (reason != null)
                {
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("BiosculpterCannotStartCycle".Translate(cycle1.Props.label) + ": " + reason.CapitalizeFirst(), null, MenuOptionPriority.Default, null, clickedPawn), hauler, clickedPawn);
                    yield break;
                }

                void action()
                {
                    if (!podComp.CanAcceptOnceCycleChosen(clickedPawn))
                    {
                        Messages.Message("CannotCarryToBiosculpterPod".Translate() + ": " + "NoBiosculpterPod".Translate(), clickedPawn, MessageTypeDefOf.RejectInput, historical: false);
                    }
                    else
                    {
                        podComp.PrepareCycleJob(hauler, clickedPawn, cycle1, JobMaker.MakeJob(JobDefOf.CarryToBiosculpterPod, clickedPawn, LocalTargetInfo.Invalid, podComp.parent));
                    }
                }

                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Default, null, clickedPawn), hauler, clickedPawn);
            }
        }
    }

    // In Rimworld 1.6 and later, the carry option is added by
    // FloatMenuOptionProvider_CarryToBiosculpterPod, which skips pawns that
    // are neither colonists nor prisoners. Append equivalent options for
    // colony animals.
    [HarmonyPatch(typeof(FloatMenuOptionProvider_CarryToBiosculpterPod), nameof(FloatMenuOptionProvider_CarryToBiosculpterPod.GetOptionsFor), new Type[] { typeof(Pawn), typeof(FloatMenuContext) })]
    internal static class FloatMenuOptionProvider_CarryToBiosculpterPod_GetOptionsFor_Patch
    {
        static void Postfix(Pawn clickedPawn, FloatMenuContext context, ref IEnumerable<FloatMenuOption> __result)
        {
            if (CarryToBiosculpterUtility.IsCarryableAnimal(clickedPawn) && context.FirstSelectedPawn != null)
            {
                __result = __result.Concat(CarryToBiosculpterUtility.CarryOptions(context.FirstSelectedPawn, clickedPawn));
            }
        }
    }
}

#endif
