using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace aRandomKiwi.KFM
{
    class PawnColumnWorker_DKill : PawnColumnWorker_Designator
    {

        protected override DesignationDef DesignationType
        {
            get
            {
                return DefDatabase<DesignationDef>.GetNamed(Utils.dKillDesignation);
            }
        }

        protected override bool HasCheckbox(Pawn pawn)
        {
            return pawn.AnimalOrWildMan() && pawn.RaceProps.IsFlesh && pawn.Faction == null && pawn.SpawnedOrAnyParentSpawned;
        }

        protected override void Notify_DesignationAdded(Pawn pawn)
        {
            pawn.MapHeld.designationManager.TryRemoveDesignationOn(pawn, DefDatabase<DesignationDef>.GetNamed(Utils.killDesignation));
            //Obtaining PID of packs targeting a DO NOT KILL target
            List<string> PID = Utils.GCKFM.getPawnPackTargetedPID(pawn);
            if (PID != null)
            {
                foreach (var cpid in PID)
                {
                    //If the packs target the selection, we stand them up
                    if (PID.Count() != 0)
                    {
                        Utils.GCKFM.cancelCurrentPack(pawn.Map, cpid);
                    }
                }
            }
        }
    }
}
