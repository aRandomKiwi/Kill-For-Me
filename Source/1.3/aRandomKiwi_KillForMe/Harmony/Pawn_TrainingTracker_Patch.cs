using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace aRandomKiwi.KFM
{
    internal static class Pawn_TrainingTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_TrainingTracker), "TrainingTrackerTickRare")]
        public class TrainingTrackerTickRare
        {
            [HarmonyPostfix]
            public static void Listener(Pawn_TrainingTracker __instance, Pawn ___pawn)
            {
                Comp_Killing ck = Utils.getCachedCKilling(___pawn);
                if (ck == null || ___pawn.training == null)
                    return;
                bool hasLearnedKilling = ___pawn.training.HasLearned(Utils.killingTrainingDef);

                if (___pawn.Faction == Faction.OfPlayer)
                {
                    //If not learned killing and have a PID, we take it out to use it
                    if (!hasLearnedKilling && ck.KFM_PID != "")
                    {
                        Utils.GCKFM.removePackMember(ck.KFM_PID, ___pawn);
                        ck.KFM_PID = "";
                    }

                    //If the player has learned to kill and animal does not have a PID, it is assigned to the PID by default
                    if (hasLearnedKilling && ck.KFM_PID == "")
                    {
                        Utils.GCKFM.addPackMember(Utils.PACK_GREEN, ___pawn);
                        ck.KFM_PID = Utils.PACK_GREEN;

                        //If the bonding deletion param is activated, the animal's attachment is deleted if applicable
                        if (Settings.disableKillerBond)
                            Utils.removeAnimalBonding(___pawn);
                    }
                }
            }
        }
    }
}
