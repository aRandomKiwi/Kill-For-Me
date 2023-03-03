using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;


namespace aRandomKiwi.KFM
{
    public class Designator_Rally : Designator_Zone
    {
        public Designator_Rally()
        {
            this.defaultLabel = "KFM_DesignatorRallyLabel".Translate();
            this.defaultDesc = "KFM_DesignatorRallyDesc".Translate();
            this.soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            this.soundDragChanged = null;
            this.soundSucceeded = SoundDefOf.Designate_ZoneDelete;
            this.useMouseIcon = true;
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Flag", true);
            this.hotKey = KeyBindingDefOf.Misc4;
        }

        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            this.drawCircle(UI.MouseCell());

            //If defined drawing of the current zone
            IntVec3 cur = Utils.GCKFM.getRallyPoint(Find.CurrentMap.GetUniqueLoadID());
            if(cur.x >= 0 )
                this.drawCircle(cur);
        }

        private void drawCircle(IntVec3 pos)
        {
            DrawUtils.DrawRadiusRingEx(pos, 6f);
            //GenDraw.DrawCircleOutline(pos.ToVector3(), 12f,SimpleColor.Red);
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 sq)
        {
            if (!sq.InBounds(base.Map))
            {
                return false;
            }

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

            Utils.GCKFM.setRallyPoint(Find.CurrentMap.GetUniqueLoadID(), pos);
            //We delete the cached rect
            Utils.cachedRallyRect = new CellRect(-1,-1,0,0);
        }

        private IntVec3 pos;
        private Map cmap;
    }
}

