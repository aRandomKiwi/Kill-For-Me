using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace aRandomKiwi.KFM
{
	public class JobDriver_GroupToPoint : JobDriver
	{
		public override void ExposeData()
		{
			base.ExposeData();
            Scribe_Values.Look<int>(ref this.mapUID, "KFM_GTP_mapUID", -1, false);
            Scribe_Values.Look<IntVec3>(ref this.groupPoint, "KFM_GTP_groupPoint", default(IntVec3), false);
        }


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
            //Unique ID save of the map
            mapUID = this.pawn.Map.uniqueID;
            //Unit marking as assigned for killing
            this.pawn.TryGetComp<Comp_Killing>().KFM_affected = true;

            return true;
        }


		protected override IEnumerable<Toil> MakeNewToils()
		{
            base.AddFinishAction(delegate
			{
				base.Map.attackTargetsCache.UpdateTarget(this.pawn);
                OnEndKillingTarget();
            });
            //Avoid rotation towards the pawn target we define the rotation target towards an undefined index
            this.rotateToFace = TargetIndex.C;
            this.job.playerForced = true;

            //Restauration Map universellement
            Map cmap = null;
            foreach (var x in Find.Maps)
            {
                if (x.uniqueID == mapUID)
                {
                    cmap = x;
                    //PMID definition
                    PMID = Utils.GCKFM.getPackMapID(cmap, pawn.TryGetComp<Comp_Killing>().KFM_PID);
                    break;
                }
            }

            if (cmap == null)
            {
                //Log.Message("ERROR MAP NULL");
                yield break;
            }

            Toil initToil = new Toil();
            initToil.AddFinishAction(delegate()
            {
                //Map restoration universally
                Map cmap2 = null;
                foreach (var x in Find.Maps)
                {
                    if (x.uniqueID == mapUID)
                        cmap2 = x;
                }
            });
            yield return initToil;

            //if applicable Waiting until all members have arrived (at the leader's point)

            //Move to the coordinates of the grouping point
            Toil updatePos = new Toil();
            Toil nothing = new Toil();
            Toil gotoWaitingPoint = Toils_Goto.
                GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(OnEndGroupMode).JumpIf(delegate
                {
                    if (Find.TickManager.TicksGame % 100 == 0)
                    {
                        //If change of destination point meanwhile change of position
                        IntVec3 point = Utils.GCKFM.getGroupPoint(cmap, pawn.TryGetComp<Comp_Killing>().KFM_PID);
                        return !groupPoint.Equals(point);
                    }
                    else
                        return false;
                }, updatePos);
            yield return gotoWaitingPoint;
            Toil setSkin = new Toil();
            setSkin.initAction = delegate
            {
                pawn.Rotation = Rot4.South;
            };
            yield return setSkin;
            yield return Toils_General.Wait(35, TargetIndex.None);

            //If necessary, change of destination
            updatePos.initAction = delegate ()
             {
                 //Obtaining waiting point
                 IntVec3 point = Utils.GCKFM.getGroupPoint(cmap, pawn.TryGetComp<Comp_Killing>().KFM_PID);

                 if (!OnEndGroupMode() && !groupPoint.Equals(point) && point.x >= 0) {
                     IntVec3 pointDec = new IntVec3(point.ToVector3());
                     Utils.setRandomChangeToVector(ref pointDec, 0, 4);
                     groupPoint = point;

                     //Generation of a close version to actually place the animal
                     this.job.targetA = new LocalTargetInfo(CellFinder.RandomSpawnCellForPawnNear(pointDec, cmap));
                    this.JumpToToil(gotoWaitingPoint);
                    return;
                }
            };
            yield return updatePos;
            yield return Toils_Jump.Jump(setSkin);
            
            yield return nothing;

            yield break;
		}

        public bool OnEndGroupMode()
        {
            return !Utils.GCKFM.isPackInGroupMode(pawn.Map, pawn.TryGetComp<Comp_Killing>().KFM_PID);
        }

        /*
         *  Action summoned when the target is dead or the canvas abruptly ends
         */
        private void OnEndKillingTarget()
        {
            ////Log.Message("Arret du JOB KILL "+this.job.GetUniqueLoadID());
            Comp_Killing ch = pawn.TryGetComp<Comp_Killing>();
            //Animal demobilization
            ch.KFM_affected = false;
        }

        public int mapUID = 0;
        public IntVec3 groupPoint;
        public IntVec3 groupPointDec;
        public bool manualStop = false;
        public string PMID; 

        public const TargetIndex TargetInd = TargetIndex.A;
		private const TargetIndex CorpseInd = TargetIndex.A;
	}
}
