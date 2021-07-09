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
    public class Designator_GroupToPoint : Designator_Zone
    {
        // Token: 0x06002E1E RID: 11806 RVA: 0x00157D28 File Offset: 0x00156128
        public Designator_GroupToPoint(string PID, Func<IntVec3, bool> onPointSelected)
        {
            this.PID = PID;
            this.defaultLabel = "KFM_GroupPack".Translate();
            this.defaultDesc = "KFM_GroupPackDesc".Translate();
            this.soundDragSustain = SoundDefOf.Designate_DragAreaDelete;
            this.soundDragChanged = null;
            this.soundSucceeded = SoundDefOf.Designate_ZoneDelete;
            this.useMouseIcon = true;
            this.onPointSelected = onPointSelected;
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/Regroup", true);
            this.hotKey = KeyBindingDefOf.Misc4;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 sq)
        {
            if (!sq.InBounds(base.Map))
            {
                return false;
            }
            if (!sq.Walkable(Current.Game.CurrentMap))
                return false;
            //Check si tout les membres de la meute peuvent accéder au point
            if (!Utils.GCKFM.canPackMembersReach(Current.Game.CurrentMap, PID, sq))
                return false;
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

            //On appel le délégué en lui passant les coordonées
            this.onPointSelected(pos);

            //ANimation sonore et visuelle
            SoundDefOf.DraftOn.PlayOneShotOnCamera(null);
            MoteMaker.ThrowDustPuffThick(pos.ToVector3Shifted(), cmap, 4.0f, Color.red);
        }

        private Func<IntVec3,bool> onPointSelected;
        private IntVec3 pos;
        private Map cmap;
        private string PID;
    }
}
