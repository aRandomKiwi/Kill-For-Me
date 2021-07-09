using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using Verse.Sound;

namespace aRandomKiwi.KFM
{
    public class Designator_TargetToKill : Designator
    {
        public Designator_TargetToKill(string PID)
        {
            this.PID = PID;
            this.defaultLabel = "KFM_ForceKillPackContextLabel".Translate();
            this.defaultDesc = "KFM_ForceKillPackContextLabel".Translate();
            this.soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            this.soundDragChanged = null;
            this.soundSucceeded = SoundDefOf.Designate_ZoneDelete;
            this.useMouseIcon = true;
            this.icon = Utils.texForceKill;
            this.hotKey = KeyBindingDefOf.Misc4;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!c.InBounds(base.Map))
            {
                return false;
            }
            if (!this.KillablesInCell(c).Any<Thing>())
            {
                return "KFM_DesignatorKillNeedSelectKillable".Translate();
            }
            return true;
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            base.CanDesignateThing(t);

            //Check s'il sagit d'un ennemis valide
            if ( !Utils.isValidEnemy(t) )
                return false;

            //Check si tout les membres de la meute peuvent accéder à la cible
            if (!Utils.GCKFM.canPackMembersReach(t.Map, PID, t.Position))
                return false;

            target = t;

            return true;
        }

        public override int DraggableDimensions
        {
            get
            {
                return 0;
            }
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return false;
            }
        }

        public override void DesignateMultiCell(IEnumerable<IntVec3> cells)
        {
            throw new NotImplementedException();
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            this.pos = c;
            this.cmap = Current.Game.CurrentMap;
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();

            //On check si au moins 50% des membres de la meute sont proches les uns des autres
            if (Utils.GCKFM.isHalfOfPackNearEachOther(target.Map, PID))
            {
                string PMID = Utils.GCKFM.getPackMapID(target.Map, PID);
                //Le cas échéant on force le non retour au point de ralliement des membres pour gagner du temps
                Utils.GCKFM.setLastAffectedEndedGT(PMID,Find.TickManager.TicksGame);
            }

            //On force l'appel a checkEnnemies au prochain Tick pour éviter les latences
            Utils.GCKFM.forceNextCheckEnemy = true;

            //On affecte la meute a la nouvelle cible  ....
            Utils.GCKFM.manualAllocatePack(PID, target);

            //ANimation sonore et visuelle
            SoundDefOf.DraftOn.PlayOneShotOnCamera(null);
            MoteMaker.ThrowDustPuffThick(pos.ToVector3Shifted(), cmap, 4.0f, Color.red);
        }


        [DebuggerHidden]
        private IEnumerable<Thing> KillablesInCell(IntVec3 c)
        {
            if (!c.Fogged(base.Map))
            {
                List<Thing> thingList = c.GetThingList(base.Map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    if (this.CanDesignateThing(thingList[i]).Accepted)
                    {
                        yield return thingList[i];
                    }
                }
            }
        }

        private IntVec3 pos;
        private Thing target;
        private Map cmap;
        private string PID;
    }
}
