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
            //Sauvegarde ID Unique de la map
            mapUID = this.pawn.Map.uniqueID;
            localAllowRangedAttack = Settings.allowRangedAttack;
            //Marquage unitée comme affectée pour killé 
            this.pawn.TryGetComp<Comp_Killing>().KFM_affected = true;

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

            //Ajout handler de fin de job
            //globalFinishActions.Add(OnEndKillingTarget);

            this.job.playerForced = true;

            //Restauration Map universellement
            Map cmap = null;
            foreach (var x in Find.Maps)
            {
                if (x.uniqueID == mapUID)
                {
                    cmap = x;
                    //Définition PMID
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
                //Restauration Map universellement
                Map cmap2 = null;
                foreach (var x in Find.Maps)
                {
                    if (x.uniqueID == mapUID)
                        cmap2 = x;
                }
                //Enregistrement du membre à ce moment car aussinon le job peut être queued et ne jamais executer de code de fin ( JID perpetuellement attribué )
                Utils.GCKFM.incAffectedMember(cmap2, this.pawn.TryGetComp<Comp_Killing>().KFM_PID, this.job.loadID);
            });
            yield return initToil;

            yield return Toils_General.DoAtomic(delegate
            {
                base.Map.attackTargetsCache.UpdateTarget(this.pawn);
            });
            
            //Si le predateur est déjà sur le lieu de formation de la meute (selectedWaitingSite)
            if (pawn.TryGetComp<Comp_Killing>().KFM_waitingPoint.x >= 0)
            {
                if (pawn.Position.DistanceToSquared(pawn.TryGetComp<Comp_Killing>().KFM_waitingPoint) <= 4)
                {
                    pawn.TryGetComp<Comp_Killing>().KFM_arrivedToWaitingPoint = true;
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

            //le cas échéant Attente tant que tout les membres ne sont pas arrivés ( au point du leader )
            if (!Utils.GCKFM.isPackArrivedToWaitingPoint(PMID))
            {
                //Déplacement aux coordonnées
                Toil idle = new Toil();
                Toil gotoWaitingPoint = Toils_Goto.
                    GotoCell(pawn.TryGetComp<Comp_Killing>().KFM_waitingPoint, PathEndMode.ClosestTouch).
                    JumpIf((() => Find.TickManager.TicksGame % 60 == 0 && ( Utils.GCKFM.isPackArrivedToWaitingPoint(PMID) )), idle).EndOnDespawnedOrNull(TargetIndex.A, JobCondition.Succeeded);
                yield return gotoWaitingPoint;
                yield return Toils_General.Wait(15);
                yield return Toils_Goto.GotoCell(pawn.TryGetComp<Comp_Killing>().KFM_waitingPoint2, PathEndMode.ClosestTouch).
                    JumpIf((() => Find.TickManager.TicksGame % 60 == 0 && Utils.GCKFM.isPackArrivedToWaitingPoint(PMID)), idle);
                //Attente des autres membres tant que pas proches 
                yield return idle;
                yield return Toils_Jump.JumpIf(gotoWaitingPoint, delegate
                {
                    this.job.playerForced = true;
                    Comp_Killing ch = pawn.TryGetComp<Comp_Killing>();
                    //On notifis que le pawn est arrivé au point de rendez-vous
                    ch.KFM_arrivedToWaitingPoint = true;
                    //Check si waitingPoint toujours définis (Tous les membres arrivés au point de destination)
                    bool allArrived = Utils.GCKFM.isPackArrivedToWaitingPoint(PMID);
                    if (allArrived)
                    {
                        //On reset le flag d'arrivé au point d'attente pour le prochain rassemblement
                        ch.KFM_arrivedToWaitingPoint = false;
                    }

                    return !allArrived;
                });
            }
            else
            {
                //si pas de meute on commence maintenant le forcing pour éviter que la créature flee en cas de menace
                //this.job.playerForced = true;

                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
                yield return new Toil();
            }

            Toil nothing = new Toil();
            
            //Cas des remote attaque via outil (? peut-etre via un mod) OU des remotes attaques en analysant les verbs en presence
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
                //Attaque à distance, si en mode ne pas achevé target alors arrete l'attaque et saute à nothing
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
                //Attaque de mélée et check si en mode safe vie de l'animal pas en danger ==> abandon mission
                Toil toil = Toils_Combat.FollowAndMeleeAttack(TargetIndex.A, hitAction).FailOn(OnFailAttack);//.JumpIf(() => (this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f), nothing);
                yield return toil;
                /*yield return Toils_Jump.JumpIf(nothing, delegate
                {
                    return (this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f);
                });*/
            }
            //L'animal arrete d'attaquer si sa santé <= 50%
            /*yield return Toils_Jump.JumpIf(nothing, delegate
            {
                return !(this.pawn.TryGetComp<Comp_Killing>().KFM_safeMode && pawn.health.summaryHealth.SummaryHealthPercent < 0.5f);
            });*/

            //Pawn se déplace au point de retraite avec le mode Safe activé
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
         *  Action invoquée lorsque la cible est morte ou que la toil se termine de maniere abrupte
         */
        private void OnEndKillingTarget()
        {
            ////Log.Message("Arret du JOB KILL "+this.job.GetUniqueLoadID());
            Comp_Killing ch = pawn.TryGetComp<Comp_Killing>();
            //Démobilisation de l'animal
            ch.KFM_affected = false;
            //Décrémentation nb de membre participant à l'attaque pour la meute donné (PID) sur la map donnée
            Utils.GCKFM.decAffectedMember(pawn.Map, ch.KFM_PID, this.job.loadID);
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
