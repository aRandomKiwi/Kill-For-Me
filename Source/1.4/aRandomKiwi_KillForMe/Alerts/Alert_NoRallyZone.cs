using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;

namespace aRandomKiwi.KFM
{
    public class Alert_NoRallyZone : Alert
    {
        public Alert_NoRallyZone()
        {
            this.defaultLabel = "KFM_AlertNoRallyZoneTitle".Translate();
            this.defaultExplanation = "KFM_AlertNoRallyZoneDesc".Translate();
            this.defaultPriority = AlertPriority.Critical;
        }

        protected override Color BGColor
        {
            get
            {
                
                float num = Pulser.PulseBrightness(0.5f, Pulser.PulseBrightness(0.5f, 0.6f));
                return new Color(num, num, num) * color;
            }
        }

        public override AlertReport GetReport()
        {
            bool ok = true;
            IntVec3 pos;
            List<GlobalTargetInfo> obj = null;

            if (Settings.hideRallyPointWarning)
                return false;

            foreach(var map in Find.Maps)
            {
                pos = Utils.GCKFM.getRallyPoint(map.GetUniqueLoadID());
                if ( pos.x < 0)
                {
                    if(obj == null)
                        obj = new List<GlobalTargetInfo>();

                    //Add World Item
                    obj.Add(new GlobalTargetInfo( map.Center , map));

                    ok = false;
                }
            }
            if (!ok)
                return AlertReport.CulpritsAre( obj );
            else
                return false;
        }

        Color color = new Color(0.95294f, 0.611764f, 0.070588f,1.0f);
    }
}
