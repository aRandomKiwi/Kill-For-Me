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

            //Check if this is a valid enemy
            if ( !Utils.isValidEnemy(t) )
                return false;

            //Check if all pack members can access the target
            if (!Utils.GCKFM.canPackMembersReach(t.Map, PID, t.Position))
                return false;

            target = t;

            return true;
        }

        public override bool DragDrawMeasurements
        {
            get
            {
                return false;
            }
        }

        public override void DesignateSingleCell(IntVec3 c)
        {
            this.pos = c;
            this.cmap = Current.Game.CurrentMap;
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();

            //We check if at least 50% of the members of the pack are close to each other
            if (Utils.GCKFM.isHalfOfPackNearEachOther(target.Map, PID))
            {
                string PMID = Utils.GCKFM.getPackMapID(target.Map, PID);
                //If necessary, we force non-return to the rallying point of the members to save time
                Utils.GCKFM.setLastAffectedEndedGT(PMID,Find.TickManager.TicksGame);
            }

            //We force the call to checkEnnemies at the next Tick to avoid latencies
            Utils.GCKFM.forceNextCheckEnemy = true;

            //We assign the pack to the new target ....
            Utils.GCKFM.manualAllocatePack(PID, target);

            //Sound and visual animation
            SoundDefOf.DraftOn.PlayOneShotOnCamera(null);
            FleckMaker.ThrowDustPuffThick(pos.ToVector3Shifted(), cmap, 4.0f, Color.red);
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
