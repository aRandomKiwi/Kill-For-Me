using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace aRandomKiwi.KFM
{
	public class JobDriver_EnemyKill : JobDriver
	{
		public Thing Target
		{
			get
			{
				Corpse corpse = this.Corpse;
				if (corpse != null)
				{
					return corpse.InnerPawn;
				}
                //return (Pawn)this.pawn.CurJob.GetTarget(TargetIndex.A).Thing;
                if(this.job.GetTarget(TargetIndex.A).Thing is Pawn)
                    return (Pawn)this.job.GetTarget(TargetIndex.A).Thing;
                else
                    return (Thing)this.job.GetTarget(TargetIndex.A).Thing;
            }
		}


        public Pawn TargetPawn
        {
            get
            {
                Thing tar = Target;
                if (tar is Pawn)
                    return (Pawn)tar;
                else
                    return null;
            }
        }

		private Corpse Corpse
		{
			get
			{
                //return this.pawn.CurJob.GetTarget(TargetIndex.A).Thing as Corpse;
                return this.job.GetTarget(TargetIndex.A).Thing as Corpse;
            }
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look<bool>(ref this.firstHit, "firstHit", false, false);
            Scribe_Values.Look<int>(ref this.mapUID, "HFM_mapUID", -1, false);
            Scribe_Values.Look<bool>(ref this.localAllowRangedAttack, "HFM_localAllowRangedAttack", false, false);
        }

		public override string GetReport()
		{
			if (this.Corpse != null)
			{
				return base.ReportStringProcessed(JobDefOf.HaulToCell.reportString);
			}
			return base.GetReport();
		}


		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
            //Unique ID save of the map
            mapUID = this.pawn.Map.uniqueID;
            localAllowRangedAttack = Settings.allowRangedAttack;
            Comp_Killing ck = Utils.getCachedCKilling(this.pawn);
            //Unit marking as assigned for killé
            ck.KFM_affected = true;

            return true;
            //return ReservationUtility.Reserve(this.pawn, this.Target, this.job, 1, -1, null);
        }


		protected override IEnumerable<Toil> MakeNewToils()
		{
            base.AddFinishAction(delegate
			{
				base.Map.attackTargetsCache.UpdateTarget(this.pawn);
                OnEndKillingTarget();
            });

            //pawn.jobs.debugLog = true;

            //Add end of job handler
            //globalFinishActions.Add(OnEndKillingTarget);

            this.job.playerForced = true;
            Comp_Killing ck = Utils.getCachedCKilling(this.pawn);
            //Map restoration universally
            Map cmap = null;
            foreach (var x in Find.Maps)
            {
                if (x.uniqueID == mapUID)
                {
                    cmap = x;
                    //PMID definition
                    PMID = Utils.GCKFM.getPackMapID(cmap, ck.KFM_PID);
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
                //Member registration at this time because otherwise the job can be queued and never execute an end code (JID perpetually assigned)
                Utils.GCKFM.incAffectedMember(cmap2, ck.KFM_PID, this.job.loadID);
            });
            yield return initToil;

            yield return Toils_General.DoAtomic(delegate
            {
                base.Map.attackTargetsCache.UpdateTarget(this.pawn);
            });

            //If the predator is already at the place of formation of the pack (selectedWaitingSite)
            if (ck.KFM_waitingPoint.x >= 0)
            {
                if (pawn.Position.DistanceToSquared(ck.KFM_waitingPoint) <= 4)
                {
                    ck.KFM_arrivedToWaitingPoint = true;
                }
            }

            Action hitAction = delegate()
			{
				Thing Target = this.Target;
                bool surpriseAttack = this.firstHit; //&& !Target.IsColonist;
				if (this.pawn.meleeVerbs.TryMeleeAttack(Target, this.job.verbToUse, surpriseAttack))
					base.Map.attackTargetsCache.UpdateTarget(this.pawn);
				this.firstHit = false;
			};

            //if applicable Waiting until all members have arrived (at the leader's point)
            if (!Utils.GCKFM.isPackArrivedToWaitingPoint(PMID))
            {
                //Move to coordinates
                Toil idle = new Toil();
                Toil gotoWaitingPoint = Toils_Goto.
                    GotoCell(ck.KFM_waitingPoint, PathEndMode.ClosestTouch).
                    JumpIf((() => Find.TickManager.TicksGame % 60 == 0 && ( Utils.GCKFM.isPackArrivedToWaitingPoint(PMID) )), idle).EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Succeeded);
                yield return gotoWaitingPoint;
                yield return Toils_General.Wait(15);
                yield return Toils_Goto.GotoCell(ck.KFM_waitingPoint2, PathEndMode.ClosestTouch).
                    JumpIf((() => Find.TickManager.TicksGame % 60 == 0 && Utils.GCKFM.isPackArrivedToWaitingPoint(PMID)), idle);
                //Waiting for other members as long as they are not close
                yield return idle;
                yield return Toils_Jump.JumpIf(gotoWaitingPoint, delegate
                {
                    this.job.playerForced = true;
                    //We notify that the pawn has arrived at the meeting point
                    ck.KFM_arrivedToWaitingPoint = true;
                    //Check if waitingPoint still defined (All members arrived at the destination point)
                    bool allArrived = Utils.GCKFM.isPackArrivedToWaitingPoint(PMID);
                    if (allArrived)
                    {
                        //We reset the arrival flag at the waiting point for the next meeting
                        ck.KFM_arrivedToWaitingPoint = false;
                    }

                    return !allArrived;
                });
            }
            else
            {
                //if no pack we now start the forcing to prevent the creature flee in case of threat
                //this.job.playerForced = true;

                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
            }

            Toil nothing = new Toil();

            //Case of remote attacks via tool (? Perhaps via a mod) OR remote attacks by analyzing the verbs in presence
            if (!Settings.ignoredRangedAttack.Contains(pawn.def.defName) && (localAllowRangedAttack || Settings.allowRangedAttack) && ((pawn.equipment != null && pawn.equipment.Primary != null && pawn.equipment.Primary.def.IsRangedWeapon)
                || Utils.hasRemoteVerbAttack(pawn.verbTracker.AllVerbs, pawn)))
            {
                ////Log.Message("RANGED ATTACK");
                this.EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Succeeded);
                yield return Toils_Combat.TrySetJobToUseAttackVerb(TargetIndex.A);
                //Toil wait = Toils_General.Wait(5);
                Toil doNothing = new Toil();
                //yield return wait;
                Toil gotoCastPos = Toils_Combat.GotoCastPosition(TargetIndex.A, TargetIndex.None, false, 0.95f).JumpIf((() => Find.TickManager.TicksGame % 250 == 0), doNothing);
                yield return gotoCastPos;
                Toil jumpIfCannotHit = Toils_Jump.JumpIfTargetNotHittable(TargetIndex.A, gotoCastPos);
                yield return doNothing;
                yield return jumpIfCannotHit;
                //Ranged attack, if in not completed target mode then stop the attack and jump to nothing
                yield return Toils_Combat.CastVerb(TargetIndex.A, true).FailOn(OnFailAttack).JumpIf((() => !Settings.isAttackUntilDeathEnabled(pawn) && ( TargetPawn != null && TargetPawn.Downed) ), nothing);
                //L'animal arrete d'attaquer si sa santé <= 50%
                /*yield return Toils_Jump.JumpIf(nothing, delegate
                {
                    return (this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f);
                });*/

                //yield return Toils_Jump.Jump(jumpIfCannotHit);
            }
            else
            {
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                //yield return new Toil();

                ////Log.Message("MELEE ATTACK");
                //Melee attack and check if in safe mode the animal not in danger ==> abandon mission
                Toil toil = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).FailOn(OnFailAttack);//.JumpIf(() => (this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f), nothing);
                yield return toil;
                /*yield return Toils_Jump.JumpIf(nothing, delegate
                {
                    return (this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f);
                });*/
            }
            //The animal stops attacking if its health <= 50%
            /*yield return Toils_Jump.JumpIf(nothing, delegate
            {
                return !(this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f);
            });*/

            //Pawn moves to the retreat point with Safe Mode enabled
            yield return nothing;

            yield break;
		}

        public bool OnFailAttack()
        {
            LocalTargetInfo pos = this.job.GetTarget(TargetIndex.A);

            return (!Settings.isAttackUntilDeathEnabled(this.pawn) && TargetPawn != null && TargetPawn.Downed)
                || (Find.TickManager.TicksGame > this.startTick + Utils.maxTimeToKill
                && ( pos != null && (float)(pos.Cell - this.pawn.Position).LengthHorizontalSquared > 4f));
        }
        public override void Notify_DamageTaken(DamageInfo dinfo)
        {
            base.Notify_DamageTaken(dinfo);
            if (dinfo.Def.ExternalViolenceFor(this.pawn) && dinfo.Def.isRanged && dinfo.Instigator != null && dinfo.Instigator != this.Target && !this.pawn.InMentalState && !this.pawn.Downed)
            {
                this.pawn.mindState.StartFleeingBecauseOfPawnAction(dinfo.Instigator);
            }
        }


        /*
         *  Action summoned when the target is dead or the canvas abruptly ends
         */
        private void OnEndKillingTarget()
        {
            Comp_Killing ck = Utils.getCachedCKilling(this.pawn);
            ////Log.Message("Arret du JOB KILL "+this.job.GetUniqueLoadID());
            //Animal demobilization
            ck.KFM_affected = false;
            //Decrement number of members participating in the attack for the given pack (PID) on the given map
            Utils.GCKFM.decAffectedMember(pawn.Map, ck.KFM_PID, this.job.loadID);
        }

        public int mapUID = 0;
        public bool manualStop = false;
        public string PMID; 


        public bool localAllowRangedAttack = false;
        public const TargetIndex TargetInd = TargetIndex.A;
		private const TargetIndex CorpseInd = TargetIndex.A;
		private bool firstHit = true;
	}
}
