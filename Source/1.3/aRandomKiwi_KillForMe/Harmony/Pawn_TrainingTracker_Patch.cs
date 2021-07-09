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
                Comp_Killing ck = ___pawn.TryGetComp<Comp_Killing>();
                if (ck == null || ___pawn.training == null)
                    return;
                bool hasLearnedKilling = ___pawn.training.HasLearned(Utils.killingTrainingDef);

                if (___pawn.Faction == Faction.OfPlayer)
                {
                    //Si pas appris killing et possede un PID on sort l'user 
                    if (!hasLearnedKilling && ck.KFM_PID != "")
                    {
                        Utils.GCKFM.removePackMember(___pawn.TryGetComp<Comp_Killing>().KFM_PID, ___pawn);
                        ck.KFM_PID = "";
                    }

                    //Si appris killing et animal au player possede pas de PID on l'affecte au PID par defaut
                    if (hasLearnedKilling && ck.KFM_PID == "")
                    {
                        Utils.GCKFM.addPackMember(Utils.PACK_GREEN, ___pawn);
                        ck.KFM_PID = Utils.PACK_GREEN;

                        //Si param de suppression du bonding activé, on supprime l'attache de l'animal le cas echeant
                        if (Settings.disableKillerBond)
                            Utils.removeAnimalBonding(___pawn);
                    }
                }
            }
        }
    }
}
