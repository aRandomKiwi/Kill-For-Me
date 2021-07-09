using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace aRandomKiwi.KFM
{
    class PawnColumnWorker_KillState : PawnColumnWorker_Checkbox
    {
        protected override bool HasCheckbox(Pawn pawn)
        {
            return Utils.hasLearnedKilling(pawn);
        }

        protected override bool GetValue(Pawn pawn)
        {
            if (pawn.TryGetComp<Comp_Killing>() != null)
                return pawn.TryGetComp<Comp_Killing>().KFM_killState;
            else
                return false;
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            if (pawn.GetComp<Comp_Killing>() == null)
                return;

            //If deactivation then we stop the jobs linked to the kill For Me to which the animal could be linked
            if (value == false)
            {
                //If an animal is currently mobilized (via its pack), it is made to stop its work
                Utils.GCKFM.cancelCurrentPackMemberJob(pawn);
                //If the animal is currently in grouping mode, it is made to stop its work
                Utils.GCKFM.cancelCurrentPackMemberGroupJob(pawn);
            }
            pawn.GetComp<Comp_Killing>().KFM_killState = value;
        }
    }
}
