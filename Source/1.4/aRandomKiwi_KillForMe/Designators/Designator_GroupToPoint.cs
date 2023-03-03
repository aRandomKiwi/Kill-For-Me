﻿using System;
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
            //Check if all pack members can access the point
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

            //We call the delegate by passing him the coordinates
            this.onPointSelected(pos);

            //Sound and visual animation
            SoundDefOf.DraftOn.PlayOneShotOnCamera(null);
            FleckMaker.ThrowDustPuffThick(pos.ToVector3Shifted(), cmap, 4.0f, Color.red);
        }

        private Func<IntVec3,bool> onPointSelected;
        private IntVec3 pos;
        private Map cmap;
        private string PID;
    }
}
