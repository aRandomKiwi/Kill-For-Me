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
            Comp_Killing ck = Utils.getCachedCKilling(pawn);
            if (ck != null)
                return ck.KFM_killState;
            else
                return false;
        }

        protected override void SetValue(Pawn pawn, bool value, PawnTable table)
        {
            Comp_Killing ck = Utils.getCachedCKilling(pawn);
            if (ck == null)
                return;

            //If deactivation then we stop the jobs linked to the kill For Me to which the animal could be linked
            if (value == false)
            {
                //If an animal is currently mobilized (via its pack), it is made to stop its work
                Utils.GCKFM.cancelCurrentPackMemberJob(pawn);
                //If the animal is currently in grouping mode, it is made to stop its work
                Utils.GCKFM.cancelCurrentPackMemberGroupJob(pawn);
            }
            ck.KFM_killState = value;
        }
    }
}
