using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace aRandomKiwi.KFM
{
    class PawnColumnWorker_Kill : PawnColumnWorker_Designator
    {
        protected override DesignationDef DesignationType
        {
            get
            {
                return DefDatabase<DesignationDef>.GetNamed(Utils.killDesignation);
            }
        }

        protected override bool HasCheckbox(Pawn pawn)
        {
            return pawn.AnimalOrWildMan() && pawn.RaceProps.IsFlesh && pawn.Faction == null && pawn.SpawnedOrAnyParentSpawned;
        }

        protected override void Notify_DesignationAdded(Pawn pawn)
        {
            pawn.MapHeld.designationManager.TryRemoveDesignationOn(pawn, DefDatabase<DesignationDef>.GetNamed(Utils.dKillDesignation));
        }
    }
}
