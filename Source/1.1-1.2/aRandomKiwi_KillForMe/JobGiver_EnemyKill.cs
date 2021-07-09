using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace aRandomKiwi.KFM
{
    internal class JobGiver_EnemyKill : ThinkNode_JobGiver
    {
        public override ThinkNode DeepCopy(bool resolve = true)
        {
            return (JobGiver_EnemyKill)base.DeepCopy(resolve);
        }

        protected override Job TryGiveJob(Pawn pawn)
        {
            string jobName = Utils.killJob;
            Comp_Killing ch;

            if (pawn == null || target == null || !pawn.GetComp<Comp_Killing>().killEnabled())
                return null;

            ch = pawn.TryGetComp<Comp_Killing>();
            if (!alone)
            {
                ch.KFM_waitingPoint = selectedWaitingPoint;
                ch.KFM_waitingPoint2 = selectedWaitingPoint2;
            }
            else
                ch.KFM_waitingPoint.x = -1;

            return new Job(DefDatabase<JobDef>.GetNamed(jobName, true), target)
            {
                killIncappedTarget = Settings.isAttackUntilDeathEnabled(pawn)
            };
        }

        public bool alone = false;
        public bool manualCall = false;
        public IntVec3 selectedWaitingPoint;
        public IntVec3 selectedWaitingPoint2;
        public Thing target;
        private const float MinDistFromEnemy = 27f;
    }
}
