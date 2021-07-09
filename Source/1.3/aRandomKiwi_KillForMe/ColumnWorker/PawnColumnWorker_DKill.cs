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
            //Obtention PID des meutes targettant une cible a NE PAS TUER
            List<string> PID = Utils.GCKFM.getPawnPackTargetedPID(pawn);
            if (PID != null)
            {
                foreach (var cpid in PID)
                {
                    //Si des meutes target la selection on les deboutes
                    if (PID.Count() != 0)
                    {
                        Utils.GCKFM.cancelCurrentPack(pawn.Map, cpid);
                    }
                }
            }
        }
    }
}
