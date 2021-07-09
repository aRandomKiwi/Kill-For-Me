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
            //Sauvegarde ID Unique de la map
            mapUID = this.pawn.Map.uniqueID;
            //Marquage unitée comme affectée pour killé 
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
            //Eviter rotation vers la cible du pawn on définis la cible de rotation vers un index non définis
            this.rotateToFace = TargetIndex.C;
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
            });
            yield return initToil;

            //le cas échéant Attente tant que tout les membres ne sont pas arrivés ( au point du leader )

            //Déplacement aux coordonnées du point de groupement
            Toil updatePos = new Toil();
            Toil nothing = new Toil();
            Toil gotoWaitingPoint = Toils_Goto.
                GotoCell(TargetIndex.A, PathEndMode.ClosestTouch).FailOn(OnEndGroupMode).JumpIf(delegate
                {
                    if (Find.TickManager.TicksGame % 100 == 0)
                    {
                        //Si changement point de destination entre temps changement de position
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

            //Le cas échéant changement destination
             updatePos.initAction = delegate ()
             {
                //Obtention point d'attente
                IntVec3 point = Utils.GCKFM.getGroupPoint(cmap, pawn.TryGetComp<Comp_Killing>().KFM_PID);

                 if (!OnEndGroupMode() && !groupPoint.Equals(point) && point.x >= 0) {
                     IntVec3 pointDec = new IntVec3(point.ToVector3());
                     Utils.setRandomChangeToVector(ref pointDec, 0, 4);
                     groupPoint = point;

                    //Génération d'une version proche pour placer effectivement l'animal
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
         *  Action invoquée lorsque la cible est morte ou que la toil se termine de maniere abrupte
         */
        private void OnEndKillingTarget()
        {
            ////Log.Message("Arret du JOB KILL "+this.job.GetUniqueLoadID());
            Comp_Killing ch = pawn.TryGetComp<Comp_Killing>();
            //Démobilisation de l'animal
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
