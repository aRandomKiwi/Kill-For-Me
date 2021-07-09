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

        protected override void SetValue(Pawn pawn, bool value)
        {
            if (pawn.GetComp<Comp_Killing>() == null)
                return;

            //Si désactivation alors on arrete les jobs relié au kill For Me auxquelle l'animal pourrait etre lié
            if(value == false)
            {
                //Si animal actuellement mobilisé (via sa meute) on le fait arreter son travail
                Utils.GCKFM.cancelCurrentPackMemberJob(pawn);
                //Si animal actuellement en mode regroupement on le fait arreter son travail 
                Utils.GCKFM.cancelCurrentPackMemberGroupJob(pawn);
            }
            pawn.GetComp<Comp_Killing>().KFM_killState = value;
        }
    }
}
