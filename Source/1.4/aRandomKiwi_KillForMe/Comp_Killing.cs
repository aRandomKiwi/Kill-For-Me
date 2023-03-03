using System;
using Verse;
using Verse.AI;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace aRandomKiwi.KFM
{
    public class Comp_Killing : ThingComp
    {
        public CompProps_Killing Props
        {
            get
            {
                return (CompProps_Killing)this.props;
            }
        }

        /*
         * Check if the kill is allowed for the animal
         */
        public bool killEnabled()
        {
            return KFM_killState && parent.Map != null && Utils.GCKFM.hasRallyPoint(parent.Map.GetUniqueLoadID());
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            if (killEnabled())
            {
                Pawn pawn = (Pawn)parent;
                if (pawn.pather.curPath != null)
                {
                    pawn.pather.curPath.DrawPath(pawn);
                }
                pawn.jobs.DrawLinesBetweenTargets();
            }
        }

        public override void PostDraw()
        {
            base.PostDraw();

            if (this.KFM_PID == "")
                return;

            Material packAvatar;

            //If kill enabled and pack icon masking not disabled
            if (killEnabled() && !Settings.hidePackIcon)
            {
                Vector3 vector;

                //Pack warrior
                if (KFM_isWarrior && Settings.allowPackAttackBonus)
                {
                    vector = this.parent.TrueCenter();
                    vector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28124f;
                    vector.z += 1.3f;
                    vector.x += this.parent.def.size.x / 2;
                    Graphics.DrawMesh(MeshPool.plane10, vector, Quaternion.identity, Utils.texWarrior, 0);
                }


                packAvatar = Utils.getPackTexture(KFM_PID);
                if (packAvatar != null)
                {
                    vector = this.parent.TrueCenter();
                    vector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28125f;
                    vector.z += 1.2f;
                    vector.x += this.parent.def.size.x / 2;

                    Graphics.DrawMesh(MeshPool.plane08, vector, Quaternion.identity, packAvatar, 0);
                }

                if (KFM_isKing && Settings.allowPackAttackBonus)
                {
                    vector = this.parent.TrueCenter();
                    vector.y = Altitudes.AltitudeFor(AltitudeLayer.MetaOverlays) + 0.28127f;
                    vector.z += 1.6f;
                    vector.x += this.parent.def.size.x / 2;

                    Graphics.DrawMesh(MeshPool.plane08, vector, Quaternion.identity, Utils.texCrown, 0);
                }

            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.KFM_isKing, "KFM_isKing", false, false);
            Scribe_Values.Look<bool>(ref this.KFM_isWarrior, "KFM_isWarrior", false, false);
            Scribe_Values.Look<int>(ref this.KFM_nbKilled, "KFM_nbKilled", 0, false);
            Scribe_Values.Look<bool>(ref this.KFM_killState, "KFM_killState", true, false);
            Scribe_Values.Look<string>(ref this.KFM_PID, "KFM_PID", "", false);
            Scribe_Values.Look<bool>(ref this.KFM_affected, "KFM_affected", false, false);
            Scribe_Values.Look<bool>(ref this.KFM_arrivedToWaitingPoint, "KFM_ArrivedToWaitingPoint", false, false);
            Scribe_Values.Look<IntVec3>(ref this.KFM_waitingPoint, "KFM_WaitingPoint", new IntVec3(-1,-1,-1), false);
            Scribe_Values.Look<IntVec3>(ref this.KFM_waitingPoint2, "KFM_WaitingPoint2", new IntVec3(-1, -1, -1), false);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void CompTick()
        {
            base.CompTick();
            int CGT = Find.TickManager.TicksGame;
            if (CGT % 360 == 0)
            {
                if(KFM_PID != null && KFM_PID != "")
                {
                    //Check if creature is present in its pack, on the contrary, we add it
                    Utils.GCKFM.addPackMember(KFM_PID, (Pawn)parent);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            base.CompInspectStringExtra();

            StringBuilder ret = new StringBuilder();

            //If map not defined or pawn host has not learned to hunt, we quit
            if (parent.Map == null || !Utils.hasLearnedKilling((Pawn)parent))
                return "";

            string PMID = Utils.GCKFM.getPackMapID(parent.Map, KFM_PID);
            //Display bonus attack point if applicable
            if (Settings.allowPackAttackBonus)
            {
                float bonusAttack=0.0f;
                float bonus = Utils.GCKFM.getPackAttackBonus(parent.Map, KFM_PID, null, true);
                if (bonus != 0f)
                {
                    int expireGTE = Utils.GCKFM.getPackAttackBonusGTE(parent.Map, KFM_PID);
                    int diff = expireGTE - Find.TickManager.TicksGame;

                    if (diff < 0)
                        ret.Append("KFM_PackBonusAttackInspectStringExpired".Translate((int)(bonus * 100)));
                    else
                        ret.Append("KFM_PackBonusAttackInspectString".Translate((int)(bonus * 100), Utils.TranslateTicksToTextIRLSeconds(diff)));
                }

                //Total display if applicable (kings points)
                if (Utils.GCKFM.packHasKing(KFM_PID))
                {
                    bonusAttack = Utils.GCKFM.getPackKingBonusAttack(KFM_PID);
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append("KFM_PackKingBonusAttackInspectString".Translate((int)(bonusAttack * 100)).CapitalizeFirst());
                }

                //If member has a kings OR it is a kings OR it is a warrior we display the total of the points
                if (KFM_isKing || KFM_isWarrior || Utils.GCKFM.packHasKing(KFM_PID)) {
                    if (KFM_isKing)
                    {
                        bonusAttack += Settings.kingAttackBonus;
                    }
                    else if (KFM_isWarrior)
                    {
                        bonusAttack += Settings.warriorAttackBonus;
                    }

                    ret.Append("\n");
                    ret.Append("KFM_PackBonusAttackTotalInspectString".Translate((int)((bonusAttack + bonus) * 100)).CapitalizeFirst());
                }

                //If kings display number of enemies to kill before next level
                if (KFM_isKing)
                {
                    ret.Append("\n");
                    ret.Append("KFM_PackKingBonusAttackEnemyToKillBeforeNextReward".Translate( Utils.GCKFM.getPackKingNbEnemyToKillBeforeNextReward(KFM_PID) ).CapitalizeFirst());
                }

                //If kings or warrior display counter number of enemies killed
                if (KFM_isWarrior || KFM_isKing)
                {
                    ret.Append("\n");
                    ret.Append("KFM_PackBonusAttackNbKilledEnemiesInspectString".Translate( KFM_nbKilled ).CapitalizeFirst());
                }
            }

            //If pack icon hidden display in text
            if (Settings.hidePackIcon)
            {
                if (KFM_isKing)
                {
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append(  "KFM_PackKingOf".Translate(("KFM_PackColor" + KFM_PID).Translate()));
                }
                else
                {
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append(("KFM_PackColor" + KFM_PID).Translate().CapitalizeFirst());
                }
            }

            //IF animal affected and not in grouping mode
            if (KFM_affected && !Utils.GCKFM.isPackInGroupMode(parent.Map,KFM_PID))
            {
                bool arrived = Utils.GCKFM.isPackArrivedToWaitingPoint(PMID);
                //IF waiting coordinate defined AND untingArrivedToWaitingPoint == false ==> in the process of joining the pack
                if (!arrived && !KFM_arrivedToWaitingPoint)
                {
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append("KFM_JoiningPackToKill".Translate());
                }
                //IF waiting coordinate defined AND untingArrivedToWaitingPoint == true ==> arrived at the training point ==> waiting for other members
                if (!arrived && KFM_arrivedToWaitingPoint)
                {
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append("KFM_WaintingForOtherToKill".Translate());
                }
            }

            return ret.ToString();
        }


        /*
         * Increment number of enemies killed by the parent
         */
        public void incNbKilledEnemy()
        {
            KFM_nbKilled++;
            //Check if the animal can become a warrior
            if (!KFM_isWarrior)
            {
                if(KFM_nbKilled >= Settings.warriorNbToKill)
                {
                    KFM_isWarrior = true;
                }
            }
        }

        public bool KFM_isKing = false;
        public bool KFM_isWarrior = false;
        public bool KFM_arrivedToWaitingPoint = false;
        public int KFM_nbKilled = 0;
        public IntVec3 KFM_waitingPoint = new IntVec3(-1,-1,-1);
        public IntVec3 KFM_waitingPoint2 = new IntVec3(-1, -1, -1);

        public IntVec3 KFM_groupWaitingPoint = new IntVec3(-1,-1,-1);
        public IntVec3 KFM_groupWaitingPointDec = new IntVec3(-1, -1, -1);

        public bool KFM_affected = false;
        public bool KFM_killState = true;
        public string KFM_PID = "";
    }
}