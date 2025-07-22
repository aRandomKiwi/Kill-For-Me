using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace aRandomKiwi.KFM
{
    public class Designator_Kill : Designator
    {
        private List<Thing> justDesignated = new List<Thing>();
        protected override DesignationDef Designation
        {
            get
            {
                return DefDatabase<DesignationDef>.GetNamed(Utils.killDesignation);
            }
        }

        public Designator_Kill()
        {
            this.defaultLabel = "KFM_DesignatorKillLabel".Translate();
            this.defaultDesc = "KFM_DesignatorKillDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/KillDesignation", true);
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
            //We force the call of the next checkEnemy
            Utils.GCKFM.forceNextCheckEnemy = true;

            /*foreach (PawnKindDef kind in (from p in this.justDesignated
                                          select p.kindDef).Distinct<PawnKindDef>())
                                          
            {
                this.ShowDesignationWarnings(this.justDesignated.First((Pawn x) => x.kindDef == kind));
            }
            this.justDesignated.Clear();*/
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