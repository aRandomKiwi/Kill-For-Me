using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using UnityEngine;

namespace aRandomKiwi.KFM
{
    internal class JobGiver_GroupToPoint : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            return (JobGiver_EnemyKill)base.DeepCopy(resolve);
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            string jobName = "KFM_GroupToPoint";
            Comp_Killing ch;
            ch = pawn.TryGetComp<Comp_Killing>();

            if (pawn == null || !pawn.GetComp<Comp_Killing>().killEnabled() || !Utils.GCKFM.isPackInGroupMode(pawn.Map, ch.KFM_PID))
                return null;

            return new Job(DefDatabase<JobDef>.GetNamed(jobName, true), ch.KFM_groupWaitingPointDec);
        }

        private const float MinDistFromEnemy = 27f;
    }
}
