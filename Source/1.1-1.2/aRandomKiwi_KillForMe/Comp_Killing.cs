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
         * Check si le kill est autorisé pour l'animal
         */
        public bool killEnabled()
        {
            return KFM_killState && Utils.GCKFM.hasRallyPoint(parent.Map.GetUniqueLoadID());
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

            //Si kill activé et masquage icone de meute pas désactivé
            if (killEnabled() && !Settings.hidePackIcon)
            {
                Vector3 vector;

                //Guerrier de meute
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
                    //Check si creature bien present dans sa meute au contrario on l'ajoute
                    Utils.GCKFM.addPackMember(KFM_PID, (Pawn)parent);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            base.CompInspectStringExtra();

            StringBuilder ret = new StringBuilder();
            
            //Si map non définie ou pawn hote n'a pas appris à chasser on quitte
            if (parent.Map == null || !Utils.hasLearnedKilling((Pawn)parent))
                return "";

            string PMID = Utils.GCKFM.getPackMapID(parent.Map, KFM_PID);
            //Affichage point attaque bonus le cas echeant
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

                //Affichage total le cas échéant (points du rois)
                if (Utils.GCKFM.packHasKing(KFM_PID))
                {
                    bonusAttack = Utils.GCKFM.getPackKingBonusAttack(KFM_PID);
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append("KFM_PackKingBonusAttackInspectString".Translate((int)(bonusAttack * 100)).CapitalizeFirst());
                }

                //Si membre possede un rois OU c'est un rois OU c'est un guerrier on affiche le total des points
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

                //Si rois affichage nombre ennemis à tuer avant niveau suivant
                if (KFM_isKing)
                {
                    ret.Append("\n");
                    ret.Append("KFM_PackKingBonusAttackEnemyToKillBeforeNextReward".Translate( Utils.GCKFM.getPackKingNbEnemyToKillBeforeNextReward(KFM_PID) ).CapitalizeFirst());
                }

                //Si rois ou guerrier affichage compteur nb ennemis tués 
                if(KFM_isWarrior || KFM_isKing)
                {
                    ret.Append("\n");
                    ret.Append("KFM_PackBonusAttackNbKilledEnemiesInspectString".Translate( KFM_nbKilled ).CapitalizeFirst());
                }
            }

            //Si icone de meute masqué affichage dans le texte
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

            //SI animal affecté et n'est pas en mode regroupement
            if (KFM_affected && !Utils.GCKFM.isPackInGroupMode(parent.Map,KFM_PID))
            {
                bool arrived = Utils.GCKFM.isPackArrivedToWaitingPoint(PMID);
                //SI coordonée d'attente définie ET untingArrivedToWaitingPoint == false ==> en cours de rejointe de la meute
                if (!arrived && !KFM_arrivedToWaitingPoint)
                {
                    if (ret.Length > 0)
                        ret.Append("\n");
                    ret.Append("KFM_JoiningPackToKill".Translate());
                }
                //SI coordonée d'attente définie ET untingArrivedToWaitingPoint == true ==> arrivé au point de formation ==> attente des autres membres
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
         * Incrémentation nb ennemis tué par le parent
         */
        public void incNbKilledEnemy()
        {
            KFM_nbKilled++;
            //Check si l'animal peut devenir un warrior
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