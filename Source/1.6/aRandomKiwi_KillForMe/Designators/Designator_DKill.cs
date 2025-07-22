using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace aRandomKiwi.KFM
{
    public class Designator_DKill : Designator
    {
        private List<Thing> justDesignated = new List<Thing>();

        protected override DesignationDef Designation
        {
            get
            {
                return DefDatabase<DesignationDef>.GetNamed(Utils.dKillDesignation);
            }
        }

        public Designator_DKill()
        {
            this.defaultLabel = "KFM_DesignatorDKillLabel".Translate();
            this.defaultDesc = "KFM_DesignatorDKillDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/DKillDesignation", true);
            this.soundDragSustain = SoundDefOf.Designate_DragStandard;
            this.soundDragChanged = SoundDefOf.Designate_DragStandard_Changed;
            this.useMouseIcon = true;
            this.soundSucceeded = SoundDefOf.Designate_Claim;
            this.hotKey = KeyBindingDefOf.Misc7;
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

        protected override bool RemoveAllDesignationsAffects(LocalTargetInfo target)
        {
            return base.RemoveAllDesignationsAffects(target);
        }


        public override void DesignateSingleCell(IntVec3 loc)
        {
            foreach (Thing current in this.KillablesInCell(loc))
            {
                this.DesignateThing(current);
            }
        }

        public override AcceptanceReport CanDesignateThing(Thing t)
        {
            if ( Utils.isValidEnemy(t) && base.Map.designationManager.DesignationOn(t, this.Designation) == null)
            {
                return true;
            }
            return false;
        }

        public override void DesignateThing(Thing t)
        {
            base.Map.designationManager.RemoveAllDesignationsOn(t, false);
            base.Map.designationManager.AddDesignation(new Designation(t, this.Designation));
            this.justDesignated.Add(t);
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            //We cancel the potential packs targetting the thing
            foreach (var t in justDesignated)
            {
                //Obtaining PID of packs targeting a DO NOT KILL target
                List<string> PID = Utils.GCKFM.getPawnPackTargetedPID(t);
                if (PID != null)
                {
                    foreach (var cpid in PID)
                    {
                        //If the packs target the selection, we stand them up
                        if (PID.Count() != 0)
                        {
                            Utils.GCKFM.cancelCurrentPack(t.Map, cpid);
                        }
                    }
                }
            }
            
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
    }
}