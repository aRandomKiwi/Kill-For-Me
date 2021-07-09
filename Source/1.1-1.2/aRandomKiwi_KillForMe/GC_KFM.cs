using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using UnityEngine;

namespace aRandomKiwi.KFM
{
    public class GC_KFM : GameComponent
    {

        public GC_KFM(Game game)
        {
            this.game = game;
            Utils.GCKFM = this;

            //Constiution liste des pointeurs de meutes
            resetPacks();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            if(Scribe.mode == LoadSaveMode.LoadingVars)
            {
                reset();
            }

            List<string> packNbAffectedSerialized = new List<string>();

            Scribe_Collections.Look<Pawn>(ref this.packBlack, "packBlack", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packBlue, "packBlue", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packGray, "packGray", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packGreen, "packGreen", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packOrange, "packOrange", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packPink, "packPink", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packPurple, "packPurple", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packRed, "packRed", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packWhite, "packWhite", LookMode.Reference, new object[0]);
            Scribe_Collections.Look<Pawn>(ref this.packYellow, "packYellow", LookMode.Reference, new object[0]);

            //Points de ralliment par map
            Scribe_Collections.Look(ref this.rallyPoint, "rallyPoint", LookMode.Value);
            //Etat du mode de regroupement d'une meute
            Scribe_Collections.Look(ref this.packGroupMode, "packGroupMode", LookMode.Value);
            Scribe_Collections.Look(ref this.packGroupPoint, "packGroupPoint", LookMode.Value);
            Scribe_Collections.Look(ref this.packGroupModeGT, "packGroupModeGT", LookMode.Value);

            Scribe_Collections.Look(ref this.packFilterByEnemy, "packFilterByEnemy", LookMode.Value);
            

            //Stockage pour chaque rois de meute du nombre d'ennemis tués
            Scribe_Collections.Look(ref this.kingNbKilledEnemy, "kingNbKilledEnemy", LookMode.Value);
            Scribe_Collections.Look(ref this.kingAttackBonus, "kingAttackBonus", LookMode.Value);
            Scribe_Collections.Look(ref this.kingNbEnemyToKill, "kingNbEnemyToKill", LookMode.Value);


            //Bonus d'attaque par meute 
            Scribe_Collections.Look(ref this.packAttackBonus, "packAttackBonus", LookMode.Value);
            //GT end des bonus d'attaque des meutes
            Scribe_Collections.Look(ref this.packAttackBonusGTE, "packAttackBonusGTE", LookMode.Value);
            //Affectation par MID(Map)-pack des ennemies
            Scribe_Collections.Look<string, Thing>(ref this.packAffectedEnemy, "packAffectedEnemy",  LookMode.Value, LookMode.Reference, ref packAffectedEnemyKeys, ref packAffectedEnemyValues);
            //Forcing d'affectation de pawn avec des meutes
            if(Scribe.mode == LoadSaveMode.Saving)
            {
                if (packForcedAffectionEnemyValues == null)
                {
                    packForcedAffectionEnemyKeys = new List<string>();
                    packForcedAffectionEnemyValues = new List<Thing>();
                }
            }
            Scribe_Collections.Look<string, Thing>(ref this.packForcedAffectionEnemy, "packForcedAffectionEnemy", LookMode.Value, LookMode.Reference, ref packForcedAffectionEnemyKeys, ref packForcedAffectionEnemyValues);
            //Stocke les GT de fin de la derniere mission des meutes par map  afin d'éviter de reformer une meute au point de ralliement
            Scribe_Collections.Look(ref this.lastAffectedEndedGT, "lastAffectedEndedGT", LookMode.Value);
            //Définition si meute arrivé au point d'attente
            Scribe_Collections.Look(ref this.packAffectedArrivedToWaitingPoint, "packAffectedArrivedToWaitingPoint",LookMode.Value);
            //Nb unitées affectées sur l'ennemie
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //Serialisation
                foreach (KeyValuePair<string, List<int>> entry in packNbAffected)
                {
                    //Serialisation des ID de job
                    packNbAffectedSerialized.Add( entry.Key+","+string.Join(",", entry.Value.Select(x => x.ToString()).ToArray()) );
                }
            }
            Scribe_Collections.Look(ref packNbAffectedSerialized, "packNbAffectedSerialized", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (packNbAffectedSerialized != null)
                {
                    //DeSerialisation
                    string[] res;
                    int[] res2;
                    string key = "";
                    packNbAffected.Clear();
                    foreach (var entry in packNbAffectedSerialized)
                    {
                        if (entry != "" && entry != null) {
                            res = entry.Split(',');
                            if (res.Count() != 0)
                            {
                                key = res[0];
                                res = res.Skip(1).ToArray();
                                if (res.Count() == 0 || (res.Count() == 1 && res[0] == "" ))
                                {
                                    packNbAffected[key] = new List<int>();
                                }
                                else
                                {
                                    //Conversion tableau de string en int
                                    res2 = Array.ConvertAll(res, s => int.Parse(s));
                                    packNbAffected[key] = new List<int>(res2);
                                }
                            }
                        }
                    }
                }
            }

            //Constiution liste des pointeurs de meutes
            resetPacks();

            //Suppression des références null ( membres de meute dead  etc...)
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach(var entry in packs)
                {
                    entry.Value.RemoveAll(item => item == null);
                }
            }

            //Initialisation des champs null le cas echeant Et si param ok suppresion des relations de bonding existantes
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                initNull();

                foreach (var entry in packs)
                {
                    entry.Value.RemoveAll(item => item == null);
                }

                //Check de la validitées des membres de meute
                foreach (var entry in packs)
                {
                    foreach (var member in entry.Value.ToList())
                    {
                        if (member.Faction != Faction.OfPlayer || member.Dead)
                        {
                            Comp_Killing ck = member.TryGetComp<Comp_Killing>();
                            if (ck != null)
                            {
                                removePackMember(ck.KFM_PID, member);
                                Utils.GCKFM.resetCompKilling(ck);
                            }
                        }
                    }
                }

                if (Settings.disableKillerBond)
                {
                    foreach (var entry in packs)
                    {
                        foreach (var member in entry.Value)
                        {
                            Utils.removeAnimalBonding(member);
                        }
                    }
                }

                //Reset des références aux maps inexistantes (PMID), pas censé servir  (géré dans le patch de MapDeiniter normalement) mais on s'est jamais...
                //On se base sur rallyPoint
                foreach (var entry in rallyPoint.ToList())
                {
                    //SI Map existe pas on lance la procédure pour la supprimée
                    if (Find.Maps.Where(x => x.GetUniqueLoadID() == entry.Key).Count() == 0)
                    {
                        purgeMapReference(entry.Key);
                    }
                }
            }
        }


        public override void GameComponentTick()
        {
            int GT = Find.TickManager.TicksGame;

            //Check des elections de rois de meute
            if (GT % 2500 == 0)
            {
                checkKings();
            }
            if (GT % 500 == 0)
            {
                //Check des bonus de meute à reseter
                checkPackBonusAttackToReset();
                //Check des meutes en mode group à libérer
                checkTimeoutPackInGroupMode();
            }
            //Check s'il y a une menace pour le player sur chacune des maps existantes
            if (GT % 180 == 0 || forceNextCheckEnemy)
            {
                packsCanReCheckNearestTarget = true;
                forceNextCheckEnemy = false;
                //Check validitée des ennemis assignés de maniere forcée pour éviter qu'une meute arrete de prendre en charge les nouveaux ennemis en essayant de tuer un ennemis partis 
                checkForcedAffectedEnemies();
                checkAffectedEnemies();
                //Check arrivé au waitingPoint de tout les membres d'un pack (pour lancer l'assault)
                checkArrivedToWaitingPointPacks();

                //Check des membres à réintégrer dans des meutes en mode regroupement
                checkFreeMembersToIntegrateInPackGrouping();

                //Check enemies à travers les maps
                checkEnemies();
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();

            
        }

        public override void StartedNewGame()
        {

        }


        /*
         * Suppression des structures internes au composant des références (PMID a la map spécifiée)
         */
        public void purgeMapReference(string MID)
        {
            var toDel = rallyPoint.Where(x => x.Key == MID).ToArray();
            foreach (var item in toDel)
                rallyPoint.Remove(item.Key);

            var toDel2 = packGroupPoint.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel2)
                packGroupPoint.Remove(item.Key);

            var toDel3 = packGroupMode.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel3)
                packGroupMode.Remove(item.Key);

            var toDel4 = packAttackBonus.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel4)
                packAttackBonus.Remove(item.Key);

            var toDel5 = packAttackBonusGTE.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel5)
                packAttackBonusGTE.Remove(item.Key);

            var toDel6 = lastAffectedEndedGT.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel6)
                lastAffectedEndedGT.Remove(item.Key);

            var toDel7 = packAffectedArrivedToWaitingPoint.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel7)
                packAffectedArrivedToWaitingPoint.Remove(item.Key);

            var toDel8 = packNbAffected.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel8)
                packNbAffected.Remove(item.Key);

            var toDel9 = packGroupModeGT.Where(x => getMIDFromPMID(x.Key) == MID).ToArray();
            foreach (var item in toDel9)
                packGroupModeGT.Remove(item.Key);
        }

        /*
         * Obtention de la liste des membres d'une pack à partir de son packID
         */
        public List<Pawn> getPack(string PID)
        {
            if (PID == Utils.PACK_BLACK)
                return packBlack;
            else if (PID == Utils.PACK_BLUE)
                return packBlue;
            else if (PID == Utils.PACK_GRAY)
                return packGray;
            else if (PID == Utils.PACK_GREEN)
                return packGreen;
            else if (PID == Utils.PACK_ORANGE)
                return packOrange;
            else if (PID == Utils.PACK_PINK)
                return packPink;
            else if (PID == Utils.PACK_PURPLE)
                return packPurple;
            else if (PID == Utils.PACK_RED)
                return packRed;
            else if (PID == Utils.PACK_WHITE)
                return packWhite;
            else if (PID == Utils.PACK_YELLOW)
                return packYellow;
            else
                return null;
        }



        /*
         * Ajout d'un membre d'une meute 
         */
        public void addPackMember(string PID, Pawn member)
        {
            //Obtention pack en question
            List<Pawn> pack = getPack(PID);

            if (pack == null)
                return;

            //Recherche si pawn pas déjà référencé dans la pack
            foreach(var p in pack.ToList())
            {
                if (p == member)
                    return;
            }
            pack.Add(member);
        }

        /*
         * Retrait d'un membre d'une meute 
         */
        public void removePackMember(string PID, Pawn member)
        {
            //Obtention pack en question
            List<Pawn> pack = getPack(PID);

            if (pack == null)
                return;

            //Recherche si pawn pas déjà référencé dans la pack
            foreach (var p in pack.ToList())
            {
                if (p == member)
                {
                    pack.Remove(member);
                    return;
                }
            }
        }

        public bool hasRallyPoint(string MID)
        {
            if (!rallyPoint.ContainsKey(MID))
                return false;
            else
                return true;
        }

        public IntVec3 getRallyPoint(string MID)
        {
            if (!rallyPoint.ContainsKey(MID))
                return noCoord;
            else
                return rallyPoint[MID];
        }


        public void setRallyPoint(string MID, IntVec3 point)
        {
            rallyPoint[MID] = point;
        }

        public int getLastAffectedEndedGT(Map map, string PID)
        {
            string PMID = getPackMapID(map, PID);

            if(lastAffectedEndedGT.ContainsKey(PMID))
            {
                return lastAffectedEndedGT[PMID];
            }
            else
            {
                return 0;
            }
        }

        /*
         * Définition de la lastAffectedEndedGT pour la PMID définie
         */
        public void setLastAffectedEndedGT(string PMID, int val)
        {
            lastAffectedEndedGT[PMID] = val;
        }


        /*
         * Incrémente le nombre de membre affectés à tuer un ennemie
         */
         public void incAffectedMember(Map map, string PID, int JID)
        {
            string PMID = getPackMapID(map, PID);
            if (!packNbAffected.ContainsKey(PMID))
                packNbAffected[PMID] = new List<int>();

            packNbAffected[PMID].Add(JID);

            //Log.Message("==>INC AFFECTED MEMBER pack="+PID+" JID="+JID);
        }


        /*
         * Décrémentation du nombre de membre affecté à tuer un ennemie (downed, killé, réussir a tuer la proie, ....)
         */
        public void decAffectedMember(Map map, string PID, int JID)
        {
            //Log.Message("==>DEC d'une unitée de pack=" + PID+" JID="+JID);
            string PMID = getPackMapID(map, PID);
            if (packNbAffected.ContainsKey(PMID)) {
                //Check si packNbAffected contient le job ID
                foreach(var jid in packNbAffected[PMID].ToList())
                {
                    if (jid == JID)
                    {
                        packNbAffected[PMID].Remove(jid);
                        //Si pour la meute donnée plus de membres affectés ==> suppression entrée pour éviter les entrées vides pouvant faire bugger le parser dans ExposeData
                        if (packNbAffected.Count == 0)
                            packNbAffected.Remove(PMID);
                    }
                }
                //Log.Message("==>New packNbAffected == "+ packNbAffected[PMID].Count);
                //Si == 0 alors on reset la cible pour la meute ==> fin de la tache de tuer la cible
                resetAffectedEnemy(PMID, map);
            }
        }

        public void resetAffectedEnemy(string PMID, Map map)
        {
            if (!packNbAffected.ContainsKey(PMID) || packNbAffected[PMID].Count <= 0)
            {
                //Log.Message("==> FIN AFFECTATION PACK " + PMID);
                if(packAffectedEnemy.ContainsKey(PMID))
                    packAffectedEnemy.Remove(PMID);
                //On définis le lastAffectedEndedGT afin d'éviter les réorganisation de meute au point de ralliment si remobilisés immédiatemment
                lastAffectedEndedGT[PMID] = Find.TickManager.TicksGame;
                //On force invocation checkEnemies si il y a des ennemis dispos
                if (activeThreatNearRallyPoint(map))
                    forceNextCheckEnemy = true;
            }
        }


        /*
         * Ajouter à une meute un bonus d'attaque
         */
        public void packIncAttackBonus(string PMID)
        {
            string PID;
            string MID;

            //Obtention PID
            PID = getPIDFromPMID(PMID);
            MID = getMIDFromPMID(PMID);

            if (!packAttackBonus.ContainsKey(PMID))
            {
                packAttackBonus[PMID] = 0f;
                packAttackBonusGTE[PMID] = 0;
            }

            //Si plafond atteint on ne fait rien 
            if (packAttackBonus[PMID] < 1.0f)
            {
                //On repousse la date de fin de bonus 
                packAttackBonusGTE[PMID] = Find.TickManager.TicksGame + Settings.bonusAttackDurationHour * 2500;
                packAttackBonus[PMID] += Settings.bonusAttackByEnemyKilled;

                //Plafonnement du bonus
                if (packAttackBonus[PMID] > 1.0f)
                {
                    packAttackBonus[PMID] = 1.0f;
                }

                //Affichage sur tout les membres de la meute de la map de l'avantage
                if (packs.ContainsKey(PID))
                {
                    foreach (var member in packs[PID])
                    {
                        if (member != null && member.Spawned
                            && !member.Dead && member.Map != null
                            && member.TryGetComp<Comp_Killing>() != null
                            && member.TryGetComp<Comp_Killing>().KFM_PID == PID
                            && member.TryGetComp<Comp_Killing>().KFM_affected
                            && member.Map.GetUniqueLoadID() == MID)
                        {
                            MoteMaker.ThrowText(member.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), member.Map, "+" + ((int)(Settings.bonusAttackByEnemyKilled * 100)) + "%", Color.green, -1f);
                        }
                    }
                }
            }

            //Check du bonus royal le cas echeant
            if( packHasKing(PID) && kingAttackBonus.ContainsKey(PID) && kingAttackBonus[PID] < 1.0f)
            {
                kingNbKilledEnemy[PID]++;
                //Si meute à atteint le niveau d'obtention de nouveaux points bonus de kill
                if(kingNbKilledEnemy[PID] >= kingNbEnemyToKill[PID])
                {
                    //On notifis à l'écran 
                    Pawn king = getPackKing(PID);
                    Messages.Message("KFM_MessagePackKingBonusAttackInc".Translate( ("KFM_PackColor"+PID).Translate(), kingNbEnemyToKill[PID]), MessageTypeDefOf.PositiveEvent, false);

                    kingNbKilledEnemy[PID] = 0;
                    kingNbEnemyToKill[PID] += 5;
                    kingAttackBonus[PID] += 0.05f;
                }
            }
        }


        /*
         * Obtention du bonus d'attaque pour une pack en question sur une map
         */
        public float getPackAttackBonus(Map map, string PID, Pawn attacker, bool tempOnly=false)
        {
            float ret=0f;
            string PMID = getPackMapID(map, PID);
            Comp_Killing ck=null;
            
            if(attacker != null)
                ck = attacker.TryGetComp<Comp_Killing>();

            if (!packAttackBonus.ContainsKey(PMID))
            {
                ret = 0f;
            }
            else
                ret = packAttackBonus[PMID];

            //Le cas echeant rajout bonus chef de meute
            if (!tempOnly && kingAttackBonus.ContainsKey(PID))
            {
                ret += kingAttackBonus[PID];
            }

            //SI attacker chef ou warrior rajout aussit
            if(ck != null)
            {
                if (ck.KFM_isKing)
                {
                    ret += Settings.kingAttackBonus;
                }
                if (ck.KFM_isWarrior)
                {
                    ret += Settings.warriorAttackBonus;
                }
            }


            return ret;
        }

        public void reassignKing(string SPID, string DPID)
        {
            Comp_Killing ck, dck=null;
            Pawn king = getPackKing(SPID);
            Pawn destKing = getPackKing(DPID);

            if (king == null)
                return;

            ck = king.TryGetComp<Comp_Killing>();
            if(destKing != null)
                dck = destKing.TryGetComp<Comp_Killing>();

            if (ck == null)
                return;

            //Si animal actuellement mobilisé (via sa meute) on le fait arreter son travail
            cancelCurrentPackMemberJob(king);
            //Si animal actuellement en mode regroupement on le fait arreter son travail 
            cancelCurrentPackMemberGroupJob(king);

            //On enleve le pawn de son actuel pack
            removePackMember(ck.KFM_PID, king);
            //Ajout a la nouvelle
            addPackMember(DPID, king);
            ck.KFM_PID = DPID;

            if (dck != null)
            {
                cancelCurrentPackMemberJob(destKing);
                cancelCurrentPackMemberGroupJob(destKing);

                removePackMember(dck.KFM_PID, destKing);
                addPackMember(SPID, destKing);
                dck.KFM_PID = SPID;
            }

            // INIT des donnée pour éviter de sans arret checker l'existance de champs dans le code suivant
            if (!kingNbKilledEnemy.ContainsKey(DPID))
                kingNbKilledEnemy[DPID] = 0;
            if (!kingNbKilledEnemy.ContainsKey(SPID))
                kingNbKilledEnemy[SPID] = 0;
            if (!kingAttackBonus.ContainsKey(DPID))
                kingAttackBonus[DPID] = 0;
            if (!kingAttackBonus.ContainsKey(SPID))
                kingAttackBonus[SPID] = 0;
            if (!kingNbEnemyToKill.ContainsKey(DPID))
                kingNbEnemyToKill[DPID] = 0;
            if (!kingNbEnemyToKill.ContainsKey(SPID))
                kingNbEnemyToKill[SPID] = 0;

            int prevKingNbKilledEnemy = kingNbKilledEnemy[SPID];
            float prevKingAttackBonus = kingAttackBonus[SPID];
            int prevKingNbEnemyToKill = kingNbEnemyToKill[SPID];

            if (dck != null)
            {
                kingNbKilledEnemy[SPID] = kingNbKilledEnemy[DPID];
                kingAttackBonus[SPID] = kingAttackBonus[DPID];
                kingNbEnemyToKill[SPID] = kingNbEnemyToKill[DPID];
            }
            kingNbKilledEnemy[DPID] = prevKingNbKilledEnemy;
            kingAttackBonus[DPID] = prevKingAttackBonus;
            kingNbEnemyToKill[DPID] = prevKingNbEnemyToKill;

            //Si pas de swapping suppression des données de la SOURCE PACK car plus de roi
            if(dck == null)
            {
                kingNbKilledEnemy.Remove(SPID);
                kingAttackBonus.Remove(SPID);
                kingNbEnemyToKill.Remove(SPID);
            }
        }

        /*
         * Traitement a opéré quand un roi d'une meute meurt
         */
        public void kingPackDeath(Pawn king)
        {
            Comp_Killing kck = king.TryGetComp<Comp_Killing>();

            if (kck == null)
                return;

            //Définition GT avec pénalité pour les nouvelles élections
            Utils.GCKFM.unsetPackKing(kck.KFM_PID);
            Utils.GCKFM.setPackKingElectionGT(kck.KFM_PID, (Settings.kingElectionHourPenalityCausedByKingDeath * 2500) + Utils.GCKFM.getKingNextElectionGT(Find.TickManager.TicksGame));
            Find.LetterStack.ReceiveLetter("KFM_PackKingDeadLabel".Translate(), "KFM_PackKingDeadDesc".Translate(king.LabelCap, ("KFM_PackColor" + kck.KFM_PID).Translate(), (int)(Utils.GCKFM.getPackKingBonusAttack(kck.KFM_PID) * 100)), LetterDefOf.NegativeEvent, king, null, null);
        }

        /*
         * Reset d'un membre d'une meute au niveau de son comp_killing
         */
         public void resetCompKilling(Comp_Killing ck)
        {
            ck.KFM_PID = "";
            ck.KFM_affected = false;
            ck.KFM_isKing = false;
            ck.KFM_isWarrior = false;
        }

        /*
         * Obtention du GTE d'expiration du bonus
         */
        public int getPackAttackBonusGTE(Map map, string PID)
        {
            string PMID = getPackMapID(map, PID);

            if (!packAttackBonusGTE.ContainsKey(PMID))
            {
                return 0;
            }
            else
                return packAttackBonusGTE[PMID];
        }


        /*
         * Check si tout les membres d'une pack arrivé au point de raliement
         */
        public bool isPackArrivedToWaitingPoint(string PMID)
        {
            if (packAffectedArrivedToWaitingPoint.ContainsKey(PMID))
                return packAffectedArrivedToWaitingPoint[PMID];
            else
                return false;
        }

        /*
         * Annuler une attaque de meute en court sur une map
         */
        public void cancelCurrentPack(Map map, string PID)
        {
            if (map == null || !packs.ContainsKey(PID))
                return;

            string PMID = getPackMapID(map, PID);
            packOrderSource[PMID] = 0;

            foreach (var member in packs[PID].ToList())
            {
                if (member != null && member.Spawned && !member.Dead && member.Map == map
                    &&  member.CurJob != null && member.CurJob.def.defName == Utils.killJob)
                {
                    //Log.Message(">> FORCING CANCEL JOB KILLING de " + member.LabelCap);
                    //Annulation du job
                    if (member.jobs != null)
                    {
                        try
                        {
                            member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                        }
                        catch (Exception e)
                        {
                            Log.Message("[KFM EndCurrentJob] : " + e.Message + "\nStackTrace : " + e.StackTrace);
                        }
                    }
                }
            }
            if(packAffectedEnemy.ContainsKey(PMID))
                packAffectedEnemy.Remove(PMID);
        }

        /*
         * Annuler une attaque d'un membre d'une meute en cours
         */
        public void cancelCurrentPackMemberJob(Pawn member)
        {
            if (member.CurJob != null && member.CurJob.def.defName == Utils.killJob)
            {
                member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            }
        }

        /*
         * Annuler l'affectation d'un membre à un regroupement
         */
        public void cancelCurrentPackMemberGroupJob(Pawn member)
        {
            if ( member.CurJob != null && member.CurJob.def.defName == Utils.groupToPointJob)
            {
                member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            }
        }

        /*
        * Ordonner aux animaux en cours de ralliement de directement attaquer (envoit signal comme quoi tout est ok)
        */
        public void launchCurrentFormingPack(Map map, string PID)
        {
            //Log.Message("Forcing attacking without wait waiting point");
            foreach (var member in packs[PID].ToList())
            {
                if (member != null && member.Spawned && !member.Dead && member.Map == map
                    && member.CurJob != null && member.CurJob.def.defName == Utils.killJob)
                {
                    if(member.TryGetComp<Comp_Killing>() != null)
                        member.TryGetComp<Comp_Killing>().KFM_waitingPoint.x = -1;
                }
            }
        }


        /*
         * Forcer une attaque manuel sur un ennemie par le joueur pour la meute de la maps selectionnée
         */
        public void forcePackAttack(Map map, string PID, Pawn target, bool noWait=false, bool interactive=false)
        {
            List<Pawn> members = new List<Pawn>();
            float enemyScore = processScore(target);
            float packScore;
            packScore = getPackScore(map, PID, ref members, target);

            //S'il n'y à pas de membres on quitte forcing impossible
            if (members.Count() <= 0)
            {
                //Affichage message erreur si interactif appel
                if (interactive)
                {

                }
                return;
            }

            //On envoit les animaux
            setPackTarget(ref members, map, PID, target, false, noWait);
        }

        /*
         * Obtention de l'ennemis targeted par un PMID définis
         */
        public Thing getAffectedEnemy(Map map, string PID)
        {
            string PMID = getPackMapID(map, PID);

            if (packAffectedEnemy.ContainsKey(PMID))
                return packAffectedEnemy[PMID];
            else
                return null;
        }

        /*
         * Check si la thing passé en parametre est référencé dans les targets des meutes, si oui et que meute en mode supervisée, alors on cancel le job 
         */
        public void unselectThingToKill(Thing thing)
        {
            string PID;
            //Recherche si la target est référencée dans la liste des cibles actuel
            foreach (var entry in packAffectedEnemy)
            {
                //Il sagit d'une cible
                if(entry.Value == thing)
                {
                    //Obtention PID à partir PMID (clée )
                    PID = getPIDFromPMID(entry.Key);
                    //Si meute en mode supervisé
                    if(Settings.isSupModeEnabled(PID))
                        cancelCurrentPack(thing.Map, PID);
                    break;
                }
            }
        }

        public bool isPackInGroupMode(string PMID)
        {
            if (!packGroupMode.ContainsKey(PMID))
                return false;
            else
                return packGroupMode[PMID];
        }

        public bool isPackInGroupMode(Map map, string PID)
        {
            string PMID = getPackMapID(map, PID);
            return isPackInGroupMode(PMID);
        }

        /*
         * Retourne le point d'attente en cours pour la meute donné
         */
        public IntVec3 getGroupPoint(string PMID)
        {
            if (!packGroupPoint.ContainsKey(PMID))
                return noCoord;
            else
                return packGroupPoint[PMID];
        }


        /*
         * Retourne le point d'attente en cours pour la meute donné
         */
        public IntVec3 getGroupPoint(Map map, string PID)
        {
            string PMID = getPackMapID(map, PID);
            return getGroupPoint(PMID);
        }

        /*
         * Activation du mode de regoupement pour la meute désignée 
         */
        public void enablePackGroupMode(string PID, Map map, IntVec3 pos)
        {
            string PMID = getPackMapID(map, PID);
            //Log.Message("Regroupement de la meute " + PMID);
            //Obtention liste des membres valides
            if (!packs.ContainsKey(PID) || pos.x < 0)
                return;

            List<Pawn> members = packs[PID];
            List<Pawn> selected = new List<Pawn>();

            foreach (var member in members)
            {
                if (member != null && member.Spawned && !member.Dead && isValidPackMember(member, member.TryGetComp<Comp_Killing>())
                    && member.Map == map)
                {
                    selected.Add(member);
                }
            }

            //Si pas de membres affectable on quitte
            if (selected.Count == 0)
                return;

            packGroupPoint[PMID] = pos;

            //Notif mode groupe GT de départ du mode (Pour auto libérer les animaux en cas d'oublis du joeur) OU si déjà activé actualisation du GT
            packGroupModeGT[PMID] = Find.TickManager.TicksGame;


            //Mode déjà activé on quitte
            if (packGroupMode.ContainsKey(PMID) && packGroupMode[PMID])
            {
                return;
            }

            //Notif mode groupe activé pour la meute
            packGroupMode[PMID] = true;

            //Attribution des jobs de GroupToPoint
            setPackGroupMode(ref selected, map, PID);
        }


        /*
         * Désactivation du mode de regroupement pour la meute désignée
         */
        public void disableGroupMode(string PID, Map map)
        {
            string PMID = getPackMapID(map, PID);
            //Mode déjà désactivé on return
            if (packGroupMode.ContainsKey(PID) && !packGroupMode[PMID])
                return;

            //Notif mode groupe désactivé pour la meute

            packGroupMode[PMID] = false;

            if (map == null)
                return;
            foreach (var member in packs[PID].ToList())
            {
                if (member != null && member.Spawned && !member.Dead && member.Map == map
                    && member.CurJob != null && member.CurJob.def.defName == "KFM_GroupToPoint")
                {
                    //Log.Message(">> FORCING CANCEL JOB GROUP2POINT de " + member.LabelCap);
                    //Annulation du job
                    if (member.jobs != null)
                        member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                }
            }
        }




        /*
         * Check si tout les membres d'une meute peuvent accéder à une coordonnée 
         */
         public bool canPackMembersReach(Map map, string PID, IntVec3 pos)
        {
            if (!packs.ContainsKey(PID))
                return false;
            bool ret = true;
            LocalTargetInfo target = new LocalTargetInfo(pos);
            List<Pawn> members = packs[PID];

            foreach (var member in members)
            {
                if(member != null && member.Map == map && !member.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                {
                    ret = false;
                    break;
                }

            }
            return ret;
        }



        /*
         * Check si au moin 50% des membres (prévisionnel) d'une meute sur une map donnée sont proches les uns des autres
         */
         public bool isHalfOfPackNearEachOther(Map map, string PID)
         {
            bool ret = false;
            if (!packs.ContainsKey(PID))
                return false;

            List<Pawn> members = packs[PID];
            int half = 0;
            int nb = 0;
            int nbm = 0;
            //Déduction de ce que 50% veux dire pour la meute en question
            half = (int)(getPackNbAffectableMembers(map, PID) * Settings.percentageMemberNearToAvoidRallyPoint);

            //Déduction nb de membres de la meute dont "half" nombres d'autres membres sont proches
            foreach (var member in members)
            {
                if (member != null && member.Map == map && isValidPackMember(member, member.TryGetComp<Comp_Killing>()))
                {
                    nbm = 0;
                    foreach (var member2 in members)
                    {
                        if (member2.Map == map &&  isValidPackMember(member2, member2.TryGetComp<Comp_Killing>()))
                        {
                            if (member.Position.DistanceTo(member2.Position) <= 10f)
                            {
                                ////Log.Message(">>>" + member.LabelCap + " near of " + member2.LabelCap);
                                nbm++;
                            }
                        }
                    }
                    //Si le membre est proche de 50% ou plus de sa meute alors on incremente le compteur general
                    if (nbm >= half)
                        nb++;
                }
            }

            //Log.Message("Near members : "+nb+", half requirement : "+half);
            //Si le nombre de membre proche les uns des autres est supérieur ou égal a 50% on en conclus que 50% des membres de la meutes sont déjà packés
            if (nb >= half)
                ret = true;

            return ret;
        }


        /*
         * Calcul nb de membres affectés dans une meute
         */
        public int getPackNbAffectedMembers(Map map, string PID)
        {
            List<Pawn> members = packs[PID];
            int nb = 0;

            //Déduction nb de membres de la meute dont "half" nombres d'autres membres sont proches
            foreach (var member in members)
            {
                if (member != null && member.Map == map && member.TryGetComp<Comp_Killing>() != null  && member.TryGetComp<Comp_Killing>().KFM_affected)
                    nb++;
            }

            return nb;
        }

        /*
         * Calcul nb de membres affectable dans une meute
         */
        public int getPackNbAffectableMembers(Map map, string PID)
        {
            if (!packs.ContainsKey(PID))
                return 0;

            List<Pawn> members = packs[PID];
            int nb = 0;

            //Déduction nb de membres de la meute dont "half" nombres d'autres membres sont proches
            foreach (var member in members)
            {
                if (member != null && member.Map == map && isValidPackMember(member, member.TryGetComp<Comp_Killing>()))
                    nb++;
            }

            return nb;
        }

        /*
         * Démarrage des jobs de GroupToPoint
         */
        private void setPackGroupMode(ref List<Pawn> members, Map map, string PID)
        {
            List<Verse.AI.Job> jobs = new List<Verse.AI.Job>();
            string PMID;
            PMID = getPackMapID(map, PID);
            IntVec3 dec;
            Vector3 tmp = packGroupPoint[PMID].ToVector3();
            int nbReal = 0;

            foreach (var member in members.ToList())
            {
                if (member != null)
                {
                    if (member.TryGetComp<Comp_Killing>() != null)
                    {
                        member.TryGetComp<Comp_Killing>().KFM_groupWaitingPoint = packGroupPoint[PMID];
                        dec = new IntVec3(tmp);
                        Utils.setRandomChangeToVector(ref dec, 0, 4);
                        member.TryGetComp<Comp_Killing>().KFM_groupWaitingPointDec = CellFinder.RandomSpawnCellForPawnNear(packGroupPoint[PMID], map);
                        setPackMemberGroupMode(member, ref jobs);
                    }
                }
            }


            //Démarrage des jobs le cas échéant
            for (int i = 0; i != members.Count(); i++)
            {
                if (jobs[i] != null)
                {
                    members[i].jobs.StartJob(jobs[i], JobCondition.InterruptForced);
                    nbReal++;
                }
            }

            //S'il n'y à aucuns membres on annule le mode group pour la meute
            if(nbReal == 0)
            {
                packGroupModeGT[PMID] = 0;
                packGroupMode[PMID] = false;
                packGroupPoint[PMID] = noCoord;
            }
        }

        /*
         * Attribution d'une tache d'aller attendre à un point donné
         */
        private bool setPackMemberGroupMode(Pawn member, ref List<Verse.AI.Job> jobs)
        {
            JobGiver_GroupToPoint jgtp = new JobGiver_GroupToPoint();
            JobIssueParams jb = new JobIssueParams();
            ThinkResult res = jgtp.TryIssueJobPackage(member, jb);
            if (res.IsValid)
            {
                jobs.Add(res.Job);
                return true;
            }
            else
            {
                jobs.Add(null);
            }
            return false;
        }



        /*
         * Affichage menu permettant d'envoyer une meute tuer l'ennemie
         */
        public void showFloatMenuForceKill(Thing target)
        {
            List<FloatMenuOption> opts = new List<FloatMenuOption>();
            FloatMenu floatMenuMap;
            string PMID;
            int nbDispo=0;
            string title;
            string btnTitle;
            string optText;
            bool affected = false;
            bool regroup = false;

            //Liste des meutes affectées à la cible
            List<string> affectedPacks = getPawnPackTargetedPID(target);

            //Listing des colons sauf ceux présents dans la liste d'exception (exp)
            foreach (KeyValuePair<string, List<Pawn>> pack in packs)
            {
                affected = false;
                regroup = false;
                PMID = getPackMapID(target.Map, pack.Key);
                nbDispo = 0;
                //Calcul nb membres disponibles 
                foreach(var member in packs[pack.Key])
                {
                    if (member != null && member.Spawned && !member.Dead && isValidPackMember(member, member.TryGetComp<Comp_Killing>())
                        && member.Map == target.Map)
                    {
                        nbDispo++;
                    }
                }
                //Check si meite courante en mode regroupement
                if (isPackInGroupMode(target.Map, pack.Key))
                    regroup = true;
                else
                {
                    //Check si meute courante affectée
                    if (affectedPacks != null)
                    {
                        foreach (var cpack in affectedPacks)
                        {
                            //Meute courante affectée
                            if (cpack == pack.Key)
                            {
                                affected = true;
                                break;
                            }
                        }
                    }
                }

                //Affichage possibilité controle meute que si nbDipo > 0 (membres) et si mode pas supervisé OU mode supervisé et target posséde le selecteur de kill
                if (nbDispo > 0 
                    && (!Settings.isSupModeEnabled(pack.Key) ||
                    (Settings.isSupModeEnabled(pack.Key)
                            && (target.Map.designationManager.DesignationOn(target) != null && target.Map.designationManager.DesignationOn(target).def.defName == Utils.killDesignation)))
                    )
                {
                    optText = ("KFM_PackColor" + pack.Key).Translate();
                    if (affected)
                    {
                        opts.Add(new FloatMenuOption("KFM_PackAvailableDeallocate".Translate(optText, nbDispo), delegate
                        {
                            string optText2 = ("KFM_PackColor" + pack.Key).Translate();

                            //On check si au moins 50% des membres de la meute sont proches les uns des autres
                            if (isHalfOfPackNearEachOther(target.Map, pack.Key))
                            {
                                //Le cas échéant on force le non retour au point de ralliement des membres pour gagner du temps
                                lastAffectedEndedGT[PMID] = Find.TickManager.TicksGame;
                            }

                            //Forcing kill de la target par la meute selectionnée
                            //Log.Message("Désattribuer Meute " + pack.Key);
                            cancelCurrentPack(target.Map, pack.Key);

                            Messages.Message("KFM_ForceDeallocateOK".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);


                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                    else
                    {
                        if (regroup)
                            btnTitle = "KFM_PackAvailableAllocateAfterRegroupMode";
                        else
                            btnTitle = "KFM_PackAvailableAllocate";

                        opts.Add(new FloatMenuOption(btnTitle.Translate(optText, nbDispo), delegate
                        {
                            //On check si au moins 50% des membres de la meute sont proches les uns des autres
                            if (isHalfOfPackNearEachOther(target.Map, pack.Key))
                            {
                                //Le cas échéant on force le non retour au point de ralliement des membres pour gagner du temps
                                lastAffectedEndedGT[PMID] = Find.TickManager.TicksGame;
                            }

                            manualAllocatePack(pack.Key, target);

                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                }
            }
            //SI pas choix affichage de la raison 
            if (opts.Count == 0)
            {
                optText = "KFM_FloatMenuForceKillNoAvailablePack".Translate();
                opts.Add(new FloatMenuOption(optText, null, MenuOptionPriority.Default, null, null, 0f, null, null));
            }
            title = "KFM_FloatMenuForceKillAvailablePack".Translate();
            floatMenuMap = new FloatMenu(opts, title );
            Find.WindowStack.Add(floatMenuMap);
        }


        /*
         * Tentative d'allocation manuelle d'une meute donnée (PID) à une cible (target)
         */
        public void manualAllocatePack(string PID, Thing target, bool verbose=true, int sourceMode = 1)
        {
            string PMID = getPackMapID(target.Map, PID);

            //Ordre non prioritaire
            if (packOrderSource.ContainsKey(PMID) && (packOrderSource[PMID] > sourceMode))
                return;

            string optText2 = ("KFM_PackColor" + PID).Translate();
            //Check si target killable par la meute (mode supervisé)
            if (Settings.isSupModeEnabled(PID)
                            && (target.Map.designationManager.DesignationOn(target) == null || target.Map.designationManager.DesignationOn(target).def.defName != Utils.killDesignation))
            {
                if(verbose)
                    Messages.Message("KFM_ForceAllocateFAILEDSupMode".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NegativeEvent, false);
                return;
            }

            //Forcing kill de la target par la meute selectionnée
            //Log.Message("Attribuer Meute " + PID);

            //Si parametre active check force de la meute et de la cible
            if (!Settings.isAllowAttackStrongerTargetEnabled(PID))
            {
                List<Pawn> list = new List<Pawn>();
                float enemyScore;
                float packScore = Utils.GCKFM.getPackScore(target.Map, PID, ref list, target);

                if (target is Pawn)
                    enemyScore = Utils.GCKFM.processScore((Pawn)target);
                else
                    enemyScore = Utils.GCKFM.processScore(target);

                //Log.Message("Pack score = "+packScore + " Enemy score = " + enemyScore);
                if (enemyScore > packScore)
                {
                    if (verbose)
                        Messages.Message("KFM_CannotTargetStrongerEnemie".Translate(packScore, enemyScore), MessageTypeDefOf.NegativeEvent, false);
                    return;
                }
            }

            PMID = getPackMapID(target.Map, PID);

            //Si meute en mode regroupement on stop le regouppement
            if (isPackInGroupMode(target.Map, PID))
            {
                //On force l'affectation de la meute à la target à la prochaine itération de checkEnemy qui va arriver juste aprés
                forceNextCheckEnemy = true;
                packForcedAffectionEnemy[PMID] = target;
                //Log.Message("Annulation mode regoupement meute puis définition cible de kill  (meute=" + PMID + ", target=" + target.GetUniqueLoadID() + ")");
                //Stop mode regroupement
                disableGroupMode(PID, target.Map);

                if (verbose)
                    Messages.Message("KFM_ForceAllocateAfterDeallocateRegroupOK".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
            }
            //Si meute déjà affecté on stop son affectation ET si mode toogle activé
            else if (packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] != null)
            {
                //Si meute cible déjà la cible demandé
                if(packAffectedEnemy[PMID] == target)
                {
                    if (verbose)
                        Messages.Message("KFM_ForceAllocateAfterDeallocateFAILED".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                //Obtention cible actuelle
                Thing ctarget = packAffectedEnemy[PMID];
                //On force l'affectation de la meute à la target à la prochaine itération de checkEnemy
                forceNextCheckEnemy = true;
                packForcedAffectionEnemy[PMID] = target;
                //Log.Message("packForcedAffectionEnemy forced " + PMID + " = " + target.GetUniqueLoadID());
                //Stop affectation actuelle
                cancelCurrentPack(target.Map, PID);

                if (verbose)
                    Messages.Message("KFM_ForceAllocateAfterDeallocateOK".Translate(optText2.CapitalizeFirst(), ctarget.LabelCap, target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
            }
            else
            {
                //On force l'affectation de la meute à la target à la prochaine itération de checkEnemy
                packForcedAffectionEnemy[PMID] = target;
                if (verbose)
                    Messages.Message("KFM_ForceAllocateOK".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
            }

            packOrderSource[PMID] = sourceMode;
        }


        /*
         * Définition du GT de prochaine execution des elections du chef de meute
         */
        public void setPackKingElectionGT(string PID, int gt)
        {
            if (kingNextElections.ContainsKey(PID)) {
                kingNextElections[PID] = gt;
            }
        }

        public void removeForcedAffectedEnemy(string PMID)
        {
            if(packForcedAffectionEnemy.ContainsKey(PMID))
                packForcedAffectionEnemy.Remove(PMID);
        }


        /*
         * Obtention de la cible forcée d'une meute donnée sur une map donnée
         */
         public Thing getPackForcedAffectionEnemy(string PMID)
        {
            if(!packForcedAffectionEnemy.ContainsKey(PMID))
                return null;

            return packForcedAffectionEnemy[PMID];
        }

        /*
         * Check s'il y a une menace proche de la zone de ralliement
         */
        private bool activeThreatNearRallyPoint(Map map)
        {
            CellRect area = getExtendedRallyAreaRect(map);
            bool activeThreat = false;
            HashSet<IAttackTarget> hashSet = map.attackTargetsCache.TargetsHostileToFaction(Faction.OfPlayer);
            foreach (IAttackTarget target in hashSet)
            {
                //Si position menace comprise dans la zone du point de raliement alors menace présente !!
                if (GenHostility.IsActiveThreatTo(target, Faction.OfPlayer) && area.Contains(target.Thing.Position))
                {
                    activeThreat = true;
                    break;
                }
            }
            return activeThreat;
        }


        /*
         * Check de la validitée des ennemies actuellement affectés aux packs
         */
         private void checkCurrentEnnemiesValidity()
        {
            foreach(var entry in packAffectedEnemy)
            {

            }
        }

        /*
         * Check s'il y a une menace proche de la zone de ralliement si oui annule les meutes en cours de ralliement ==> donne ordre attaquer directement
         */
        private void checkThreatNearRallyPoint(Map map)
        {
            if (activeThreatNearRallyPoint(map) )
            {
                //Log.Message("!!!!!! Menace détectée prêt du point de ralliement forcing attaque des meutes en cours de ralliement !!!!!!!");
                string PMID;
                //Annulation des meutes en cours de formation (lord of the poules)
                foreach (KeyValuePair<string, List<Pawn>> pack in packs)
                {
                    PMID = getPackMapID(map, pack.Key);
                    //si meute en cours de formation et pas arrivé au point de formation
                    if( packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] != null
                        && !packAffectedArrivedToWaitingPoint[PMID])
                    {
                        //Assult direct
                        launchCurrentFormingPack(map, pack.Key);
                        packAffectedArrivedToWaitingPoint[PMID] = true;
                    }
                }
            }

        }


        /*
         * Checker si tout les membres d'un PMID sont arrivé au point d'attente pour lancer l'assault
         */
        private void checkArrivedToWaitingPointPacks()
        {
            string MID;
            string PID;
            int nbArrived = 0;
            List<int> curJobs = new List<int>();

            foreach (KeyValuePair<string, Thing> pack in packAffectedEnemy.ToList())
            {
                curJobs.Clear();

                //SI arrivé au point d'attente ou pas de pawn ( définition à null)
                if ((packAffectedArrivedToWaitingPoint.ContainsKey(pack.Key) && packAffectedArrivedToWaitingPoint[pack.Key]) || pack.Value == null)
                    continue;

                MID = getMIDFromPMID(pack.Key);
                PID = getPIDFromPMID(pack.Key);
                nbArrived = 0;
                if (packs.ContainsKey(PID))
                {
                    //Calcul nb membre de la meute pour la map donnée ont le flag arrivedToWaitingPoint si cette valeur est égal ou sup au nb affected Member ==> on lance l'assault
                    foreach (var member in packs[PID].ToList())
                    {
                        //On ne s'occupe que de la partie de la meute sur la map spécifiée
                        if (member != null && member.Spawned && !member.Dead && member.Map.GetUniqueLoadID() == MID
                            && member.TryGetComp<Comp_Killing>() != null
                            && member.TryGetComp<Comp_Killing>().KFM_arrivedToWaitingPoint)
                        {
                            nbArrived++;
                        }
                        if(member.CurJob != null)
                            curJobs.Add(member.CurJob.loadID);
                    }

                    //Suppression de la liste des packNbAffected des entrée de jobID n'existant plus
                    bool ok = false;
                    List<int> toDel = null;
                    if (packNbAffected.ContainsKey(pack.Key))
                    {
                        foreach (var mjid in packNbAffected[pack.Key])
                        {
                            ok = false;
                            foreach (var jid in curJobs)
                            {
                                if (jid == mjid)
                                {
                                    ok = true;
                                    break;
                                }
                            }
                            //Si pas present on supprime de packNbAffected le loadJobIDqui pour une raison ou une autre est obsolete
                            if (!ok)
                            {
                                if (toDel == null)
                                    toDel = new List<int>();
                                toDel.Add(mjid);
                            }
                        }
                    }

                    if (toDel != null)
                    {
                        foreach (var jid in toDel)
                        {
                            try
                            {
                                if (packNbAffected.ContainsKey(pack.Key) && packNbAffected[pack.Key] != null)
                                    packNbAffected[pack.Key].Remove(jid);
                            }
                            catch (Exception)
                            {

                            }
                        }
                        toDel.Clear();
                    }
                }

                int nbAffected = 0;
                if (packNbAffected.ContainsKey(pack.Key)) { 
                    //Plafonnement de nbAffected basé sur le nb de membre valide dans la pack
                    nbAffected = packNbAffected[pack.Key].Count();
                }
                /*Map map = Utils.getMapFromMUID(MID);
                if (map != null)
                {
                    int tmp = getPackNbAffectableMembers(map,PID);
                    if(tmp < nbAffected)
                    {
                        nbAffected = tmp; 
                    }
                }*/



                //Tout les animaux sont arrivés on envoit le signal aux animaux qui attendent (  waitingPoint.x à -1 ) 
                if (packAffectedArrivedToWaitingPoint.ContainsKey(pack.Key) && nbArrived >= nbAffected)
                {
                    //Log.Message(nbArrived + " " + nbAffected);
                    //On définis la meute comme étant arrivé au point d'attente OK ==> plus de traitemtn par la suite
                    packAffectedArrivedToWaitingPoint[pack.Key] = true;
                }
            }
        }

        /*
         * Check des membres libres à réintégrer dans des meutes en mode regroupement
         */
        private void checkFreeMembersToIntegrateInPackGrouping()
        {
            Comp_Killing ck;
            foreach (var map in Find.Maps)
            {
                foreach (var cpack in packs)
                {
                    foreach (var member in cpack.Value.ToList())
                    {
                        if (member != null)
                        {
                            ck = member.TryGetComp<Comp_Killing>();

                            if (member != null && member.Spawned && member.Map != null && member.Faction == Faction.OfPlayer && ck != null && !ck.KFM_affected)
                            {
                                //Intégration aux regroupements des éléments libres 
                                if (isPackInGroupMode(map, cpack.Key))
                                {
                                    integrateFreeMemberToGroup(member);
                                }
                            }
                        }
                    }
                }

            }
        }


        /*
         * Check des bonus de meute à reseter
         */
         private void checkPackBonusAttackToReset()
        {
            int cgt = Find.TickManager.TicksGame;
            string PID;
            string MID;

            foreach(var entry in packAttackBonus.ToList())
            {
                if(packAttackBonusGTE.ContainsKey(entry.Key) && packAttackBonusGTE[entry.Key] != 0 && cgt >= packAttackBonusGTE[entry.Key] )
                {
                    packAttackBonusGTE[entry.Key] = 0;
                    packAttackBonus[entry.Key] = 0f;

                    PID = getPIDFromPMID(entry.Key);
                    MID = getMIDFromPMID(entry.Key);
                    //Affichage sur la meute de la fin du bonus d'attaque
                    foreach (var member in packs[PID].ToList())
                    {
                        if (member != null && member.Map != null && member.TryGetComp<Comp_Killing>() != null && member.TryGetComp<Comp_Killing>().KFM_PID == PID && member.Map.GetUniqueLoadID() == MID)
                        {
                            MoteMaker.ThrowText(member.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), member.Map, "KFM_BonusAttackLost".Translate(), Color.red, -1f);
                        }
                    }
                }
            }
        }


        /*
         * Check des meutes en mode regroupement ou le timeout d'affectation est arrivé (libération des animaux pour éviter qu'ils meurts de faim)
         */
        private void checkTimeoutPackInGroupMode()
        {
            int GT = Find.TickManager.TicksGame;
            string MID;
            string PID;

            foreach (var entry in  packGroupModeGT.ToList())
            {
                if( GT - entry.Value >= (Settings.hoursBeforeAutoDisableGroupMode*2500))
                {
                    MID = getMIDFromPMID(entry.Key);
                    PID = getPIDFromPMID(entry.Key);
                    //Log.Message("DESACTIVATION MEUTE EN MODE REGROUPEMENT " + PID + " (TImeout atteint)");

                    Map map = Find.Maps.Find((x) => x.GetUniqueLoadID() == MID);
                    if (map != null)
                    {
                        disableGroupMode(PID, map);
                    }
                    packGroupModeGT.Remove(entry.Key);
                }
            }
        }

        /*
         * Dispatcher d'attaque d'ennemies
         */
        private void checkEnemies()
        {
            Comp_Killing ck;

            foreach (var map in Find.Maps)
            {
                //Si pas de rallyPoint on ignore la map
                if (getRallyPoint(map.GetUniqueLoadID()).x == -1)
                    continue;

                //Menace sur la map check activation de packs d'animaux
                if (activeThreatOrDesignatedEnemies(map))
                {
                    //Log.Message("Menace ou ennemies désignés sur la map détecté !!!");

                    //Check menance pret du point de ralliement, le cas écheant annulation des meutes en cours de création et recréation pour attaquer cible directement
                    checkThreatNearRallyPoint(map);


                    //Integration des membres libres
                    foreach (var cpack in packs)
                    {
                        foreach (var member in cpack.Value.ToList())
                        {
                            ck = member.TryGetComp<Comp_Killing>();

                            if (member != null && member.Faction == Faction.OfPlayer && ck != null && !ck.KFM_affected)
                            {
                                //Intégration aux meutes mobilisées des éléments libres ( animaux battant en retraite, animaux soignés et opérationnels)
                                if (member.Spawned && member.Map != null)
                                    integrateFreeMember(member, ck);
                            }
                        }
                    }

                    List<Thing> targets = new List<Thing>();

                    //Check les ennemis naturellements hostiles 
                    foreach (IAttackTarget target in map.attackTargetsCache.TargetsHostileToColony)
                    {
                        targets.Add(target.Thing);
                        //Log.Message("==>Menace identifiée : " + target.Thing.LabelCap);
                        //processEnemy(map, target.Thing);
                    }

                    //Check des ennemis downed
                    foreach (var target in map.mapPawns.SpawnedDownedPawns)
                    {
                        if (target.HostileTo(Faction.OfPlayer))
                            targets.Add(target);
                    }

                    //Check les ennemis désignés
                    foreach (var des in map.designationManager.SpawnedDesignationsOfDef(Utils.killDesignationDef))
                    {
                        if(!targets.Contains(des.target.Thing))
                            targets.Add(des.target.Thing);
                        //Log.Message("==>Menace désignée identifiée : " + des.target.Thing.LabelCap);
                        //processEnemy(map, des.target.Thing);
                    }

                    //Check des forced ennemy attribution s'ils n'ont pas été traité ci-dessus
                    foreach (var enemy in packForcedAffectionEnemy.ToList())
                    {
                        if (!targets.Contains(enemy.Value))
                            targets.Add(enemy.Value);
                        //processEnemy(map, enemy.Value);
                    }

                    //Calcul des positions moyenne des packs en vigueur
                    Dictionary<string, IntVec3> packsCoordinates = new Dictionary<string, IntVec3>();
                    foreach (var cpack in packs)
                    {
                        List<Pawn> members = cpack.Value;
                        if (members.Count == 0)
                            continue;

                        //On calcul la moyenne de coordonée du groupe
                        List<IntVec3> coordinates = new List<IntVec3>();
                        foreach (var m in members)
                        {
                            if (m != null && isValidPackMember(m, m.TryGetComp<Comp_Killing>()))
                            {
                                coordinates.Add(m.Position);
                            }
                        }
                        packsCoordinates[cpack.Key] = Utils.GetMeanVector(coordinates);
                    }

                    //Carte des affectation durant la passe
                    foreach(var cp in packs)
                    {
                        checkEnemiesCurrentAffected[cp.Key] = null;
                    }
                    

                    //Analyse par meute de l'enemmis le plus proche
                    foreach (var cpack in packs)
                    {
                        List<Pawn> members = cpack.Value;
                        string PMID = getPackMapID(map, cpack.Key);

                        if (packOrderSource.ContainsKey(PMID) && packOrderSource[PMID] == 1)
                        {
                            if (packForcedAffectionEnemy.ContainsKey(PMID))
                            {
                                processEnemy(map, packForcedAffectionEnemy[PMID]);
                            }
                            else
                                continue;
                        }

                        //Si il n'y a pas de membres OU pack sur map déjà affecté ET ( pas en mode rde recheck menance la plus proche OU si cible définis par Operateur humain) on passe a la meute suivante
                        if (members.Count == 0 
                            || isPackInGroupMode(map, cpack.Key) 
                            || (packAffectedEnemy.ContainsKey(PMID) && (!packsCanReCheckNearestTarget || (packOrderSource.ContainsKey(PMID) && packOrderSource[PMID] == 1))))
                            continue;

                        //On calcul la moyenne de coordonée du groupe
                        /*List<IntVec3> coordinates = new List<IntVec3>();
                        foreach(var m in members)
                        {
                            if (isValidPackMember(m, m.TryGetComp<Comp_Killing>()))
                            {
                                coordinates.Add(m.Position);
                            }
                        }
                        IntVec3 averagePos = Utils.GetMeanVector(coordinates);*/
                        float dist = -1;
                        Thing selEnemy = null;
                        string selPID = null;
                        foreach(var t in targets)
                        {
                            if (checkEnemiesCurrentAffected.ContainsValue(t))
                                continue;

                            //Check si ennemy affecté a une pack, le cas echeant check si affectation en prioritaure (1) si oui annulation ordre
                            if (packAffectedEnemy.ContainsValue(t))
                            {
                                var key = packAffectedEnemy.FirstOrDefault(x => x.Value == t).Key;
                                if (key != null)
                                {
                                    if (packOrderSource.ContainsKey(key) && packOrderSource[key] == 1)
                                        continue;
                                }
                            }
                            /*if(getPackForcedAffectionEnemy(PMID) == t && (packOrderSource.ContainsKey(PMID) && packOrderSource[PMID] == 1))
                            {
                                selEnemy = t;
                                break;
                            }*/
                            if (!Utils.isValidEnemy(t, cpack.Key) 
                                || (Settings.isSupModeEnabled(cpack.Key) && (map.designationManager.DesignationOn(t) == null || map.designationManager.DesignationOn(t).def.defName != Utils.killDesignation))
                                || (Settings.ignoredTargets.Contains(t.def.defName)) 
                                || !Utils.GCKFM.canPackMembersReach(map, cpack.Key, t.Position))
                                continue;


                            string nearestPack = null;
                            float cdist = t.Position.DistanceTo(packsCoordinates[cpack.Key]);

                            //Déduction meute la plus proche de l'ennemis
                            float dist2 = -1;
                            float cdist2;
                            foreach(var el in packsCoordinates)
                            {
                                if (!Utils.isValidEnemy(t, el.Key))
                                    continue;

                                string CPMID = getPackMapID(map, el.Key);
                                List<Pawn> cpMembers = packs[el.Key];

                                if (checkEnemiesCurrentAffected.ContainsKey(el.Key) && checkEnemiesCurrentAffected[el.Key] != null)
                                    continue;

                                //Calcul distance de la pack en cours
                                cdist2 = t.Position.DistanceTo(packsCoordinates[el.Key]);
                                //Pack must be unassigned and not in grouping mode and 
                                if ((dist2 == -1 || dist2 > cdist2)
                                    && (!packOrderSource.ContainsKey(CPMID) || packOrderSource[CPMID] != 1)
                                    && (!(cpMembers.Count == 0
                                            || isPackInGroupMode(map, el.Key)
                                            || (Settings.isManualModeEnabled(el.Key) && getPackForcedAffectionEnemy(CPMID) != t) // if in manual mode not assigned to the current target
                                            || (!Settings.isManualModeEnabled(el.Key) && packForcedAffectionEnemy.ContainsValue(t)) // if the target is already reserved
                                            || (packAffectedEnemy.ContainsKey(CPMID) && (!packsCanReCheckNearestTarget || (packOrderSource.ContainsKey(CPMID) && packOrderSource[CPMID] == 1))))))
                                {
                                    dist2 = cdist2;
                                    nearestPack = el.Key;
                                }
                            }
                            
                            //SI pack en cours la plus proche et ennemis plus proche que le precedent analysé
                            if ( ((dist == -1 || dist > cdist) ) && nearestPack != null)//&& (!packAffectedEnemy.ContainsValue(t) || (packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] == t))))
                            {
                                dist = cdist;
                                selEnemy = t;
                                selPID = nearestPack;
                            }
                        }

                        if (selEnemy != null)
                        {
                            //OBtention pack affecté le cas écheant
                            if (packAffectedEnemy.ContainsValue(selEnemy))
                            {
                                var key = packAffectedEnemy.FirstOrDefault(x => x.Value == selEnemy).Key;
                                if (key != null)
                                {
                                    string MID = getMIDFromPMID(key);
                                    string PID = getPIDFromPMID(key);

                                    //Si meute a laquelle est affectée actuellement l'ennemis est differente de la meute a laquelle il faut affecter l'ennemis 
                                    if (PID != selPID)
                                    {
                                        //Stop affectation courante d'une pack sur l'ennemie
                                        cancelCurrentPack(Utils.getMapFromMUID(MID), PID);
                                    }
                                }
                            }
                            //Log.Message("Manually affected pack " + selPID + " on " + selEnemy.LabelCap);
                            checkEnemiesCurrentAffected[selPID] = selEnemy;
                            //Log.Message(selPID + " doit cibler " + selEnemy.LabelCap);
                            //Lancement de la meute a l'assault de l'ennemis
                       
                            
                            manualAllocatePack(selPID, selEnemy, false, 0);
                            processEnemy(map, selEnemy);
                        }
                    }
                }
            }
            packsCanReCheckNearestTarget = false;
        }

        /*
         * Traitement d'un ennemis 
         */
        private void processEnemy(Map map, Thing thing)
        {
            float packScore;
            string PID;
            List<Pawn> members = new List<Pawn>();
            Pawn pawn;

            if (thing is Pawn)
                pawn = (Pawn)thing;
            else
                pawn = null;
            
            //target possede une affectation forced pending OU Pawn hostile au player et pas déjà ciblée par une meute et fait pas partit de la liste des cible à ne pas attaquer  
            //Et si mode attaquer jusqua mort pas activé PAS DOWN
            //ET si enemie ne posséde pas de designation de DontKILL !!!
            if (Utils.isValidEnemy(thing)
                && (hasForcedAffectedPack(thing)
                || (!Settings.ignoredTargets.Contains(thing.def.defName)
                && thingNotAlreadyTargetedByPack(thing))))
            {
                //On essait de le faire ciblé par une meute disponible ayant un score >= au pawn
                float enemyScore;
                if (pawn != null)
                    enemyScore = processScore(pawn);
                else
                    enemyScore = processScore(thing);
                Thing forcedThing = null;
                members.Clear();
                PID = getFreePID(map, enemyScore, out packScore, ref members, out forcedThing, thing);
                if (forcedThing != null)
                    thing = forcedThing;

                //Log.Message(PID+" Targeting " + thing.LabelCap);
                //On a obtenue une meute pouvant affronter le pawn hostile
                if (PID != null)
                {
                    //Si il y à des menaces pret de la zone de ralliement ==> formation meute avec attaque directe 
                    if (activeThreatNearRallyPoint(map))
                    {
                        //Log.Message("==>Mobilisation de la pack " + PID + " pour directement (menace proche point de ralliement) défoncer " + thing.LabelCap);
                        setPackTarget(ref members, map, PID, thing, false, true);
                    }
                    else
                    {
                        //Si au moin 50% des membres sont proches on attaque directement l'ennemis
                        if (Utils.GCKFM.isHalfOfPackNearEachOther(map, PID))
                        {
                            //Le cas échéant on force le non retour au point de ralliement des membres pour gagner du temps
                            Utils.GCKFM.setLastAffectedEndedGT(Utils.GCKFM.getPackMapID(map, PID), Find.TickManager.TicksGame);
                        }

                        //Log.Message("==>Mobilisation de la pack " + PID + " pour défoncer " + thing.LabelCap);
                        setPackTarget(ref members, map, PID, thing);
                    }
                }
            }
        }

        /*
         * Check s'il existe pour la map donné une menance active (ou des ennemis désigné à tuer)
         */
        private bool activeThreatOrDesignatedEnemies(Map map)
        {
            bool downedEnemy = false;
            foreach (var target in map.mapPawns.SpawnedDownedPawns)
            {
                if (target.HostileTo(Faction.OfPlayer))
                {
                    downedEnemy = true;
                    break;
                }
            }
            downedEnemy = downedEnemy && (Settings.greenAttackUntilDeath || Settings.blackAttackUntilDeath
                || Settings.blueAttackUntilDeath || Settings.redAttackUntilDeath || Settings.orangeAttackUntilDeath || Settings.yellowAttackUntilDeath
                || Settings.purpleAttackUntilDeath || Settings.grayAttackUntilDeath || Settings.whiteAttackUntilDeath || Settings.pinkAttackUntilDeath);

            return GenHostility.AnyHostileActiveThreatToPlayer(map)
                || downedEnemy
                || (map.designationManager.SpawnedDesignationsOfDef(Utils.killDesignationDef).Count() > 0)
                || (packForcedAffectionEnemy.Count() > 0);
        }

        /*
         * Routine chargée de délire un nouveau rois si le délais d'attente atteint 
         */
        private void checkKings()
        {
            //Si systeme de bonus désactivé OU rois définis en manuel dans les param on sort
            if (!Settings.allowPackAttackBonus || Settings.allowManualKingSet)
                return;

            int gt = Find.TickManager.TicksGame;
            foreach (var cpack in packs)
            {
                //si meute n'a pas de rois
                if (!kingNbKilledEnemy.ContainsKey(cpack.Key))
                {
                    //Si pas de décompte d'election, mise en place si conditions de réunies (meute ayant 4 et plus de membres)
                    if (cpack.Value.Count >= Settings.kingElectionMinMembers && !kingNextElections.ContainsKey(cpack.Key))
                    {
                        kingNextElections[cpack.Key] = getKingNextElectionGT(gt);
                    }
                    //SI compte a rebours d'élection atteint 
                    if (kingNextElections.ContainsKey(cpack.Key) && kingNextElections[cpack.Key] <= gt)
                    {
                        kingNextElections.Remove(cpack.Key);
                        //Compte a rebours atteint et conditions de la meute ok
                        if (cpack.Value.Count >= Settings.kingElectionMinMembers)
                        {
                            Pawn king = cpack.Value.RandomElement();
                            //Initialisation des données royales
                            setPackKing(cpack.Key, king);

                            Find.LetterStack.ReceiveLetter("KFM_KingElectionNewKingLabel".Translate(), "KFM_KingElectionNewKingDesc".Translate(king.LabelCap, ("KFM_PackColor" + cpack.Key).Translate() ), LetterDefOf.PositiveEvent, new TargetInfo(king), null, null);
                        }
                        else
                        {
                            //Relancement du compte à rebours
                            kingNextElections[cpack.Key] = getKingNextElectionGT(gt);
                        }
                    }
                }
            }
        }

        /*
         * Check validitée des forcedAffectedTarget
         */
        public void checkForcedAffectedEnemies()
        {
            if (packForcedAffectionEnemy.Count() == 0)
                return;

            foreach (KeyValuePair<string, Thing> entry in packForcedAffectionEnemy.ToList())
            {
                //Si cible devenue invalide on la retire
                if ( entry.Value == null || entry.Value.DestroyedOrNull() || entry.Value.Map == null)
                {
                    //Log.Message("REMOVE AFFECTED");
                    packForcedAffectionEnemy.Remove(entry.Key);
                }
            }
        }

        /*
         * Check validitée des affected enemy
         */
        public void checkAffectedEnemies()
        {
            if (packAffectedEnemy.Count() == 0)
                return;

            foreach (KeyValuePair<string, Thing> entry in packAffectedEnemy.ToList())
            {
                string PID = getPIDFromPMID(entry.Key);
                string mid = getMIDFromPMID(entry.Key);
                Map map = Utils.getMapFromMUID(mid);

                if (PID == null || map == null)
                    continue;


                bool supMode = Settings.isSupModeEnabled(PID);
                bool supModeOk = entry.Value != null && map.designationManager.DesignationOn(entry.Value) != null && map.designationManager.DesignationOn(entry.Value).def.defName == Utils.killDesignation;
                //Si cible devenue invalide on la retire

                if (entry.Value == null || entry.Value.DestroyedOrNull() || entry.Value.Map == null || !Utils.isValidEnemy(entry.Value, PID) || (supMode && !supModeOk))
                {
                    //Log.Message("=>"+(entry.Value == null)+" "+entry.Value.DestroyedOrNull()+" "+(entry.Value.Map == null)+" "+(!Utils.isValidEnemy(entry.Value, PID))+" "+(!supMode || (supMode && supModeOk)));
                    packAffectedEnemy.Remove(entry.Key);
                    cancelCurrentPack(map, PID);
                }
            }
        }



        public int getKingNextElectionGT(int gt)
        {
            return gt + Rand.Range(Settings.kingElectionMinHour * 2500, Settings.kingElectionMaxHour * 2500);
        }

        /*
         * Création d'un nouveau rois pour la meute donnée
         */
        public void setPackKing(string PID, Pawn king)
        {
            if (king == null)
                return;
            Comp_Killing ck = new Comp_Killing();

            kingNbKilledEnemy[PID] = 0;
            kingNbEnemyToKill[PID] = 5;
            kingAttackBonus[PID] = 0.05f;

            ck = king.TryGetComp<Comp_Killing>();
            if (ck != null)
            {
                ck.KFM_isWarrior = false;
                ck.KFM_isKing = true;
            }
        }


        /*
         * Suppression d'un rois pour la meute donnée
         */
         public void unsetPackKing(string PID)
        {
            Comp_Killing ck = new Comp_Killing();
            Pawn king = getPackKing(PID);

            if (king == null)
                return;

            ck = king.TryGetComp<Comp_Killing>();

            if (ck == null)
                return;

            ck.KFM_isKing = false;
            if (kingNbKilledEnemy.ContainsKey(PID))
                kingNbKilledEnemy.Remove(PID);
            if (kingAttackBonus.ContainsKey(PID))
                kingAttackBonus.Remove(PID);
            if (kingNbEnemyToKill.ContainsKey(PID))
                kingNbEnemyToKill.Remove(PID);
        }


        /*
         * Reset d'un warrior
         */
         public void unsetPackWarrior(Pawn pet)
        {
            Comp_Killing ck = pet.TryGetComp<Comp_Killing>();
            if (ck != null)
            {
                ck.KFM_isWarrior = false;
                ck.KFM_nbKilled = 0;
            }
        }


        /*
         * Check si meute posséde un rois
         */
         public bool packHasKing(string PID)
        {
            return kingAttackBonus.ContainsKey(PID);
        }

        /*
         * Obtention du rois d'une meute
         */
        public Pawn getPackKing(string PID)
        {
            Pawn ret=null;

            if (!packs.ContainsKey(PID))
                return null;

            foreach(var entry in packs[PID])
            {
                if (entry.TryGetComp<Comp_Killing>() != null && entry.TryGetComp<Comp_Killing>().KFM_isKing)
                {
                    return entry;
                }
            }
            return ret;
        }

        /*
         * Obtention du bonus d'attaque royale d'une meute
         */
        public float getPackKingBonusAttack(string PID)
        {
            if (kingAttackBonus.ContainsKey(PID))
                return kingAttackBonus[PID];
            else
                return 0f;
        }

        /*
         * Obtention pour une meute donnée du nombre d'ennemis à tuer avant le prochain niveau
         */
         public int getPackKingNbEnemyToKillBeforeNextReward(string PID)
        {
            if (kingNbEnemyToKill.ContainsKey(PID) && kingNbKilledEnemy.ContainsKey(PID))
            {
                return kingNbEnemyToKill[PID] - kingNbKilledEnemy[PID];
            }
            else
            {
                return 0;
            }
        }

        /*
         * Check si la cible à une affectation forcée 
         */
        private bool hasForcedAffectedPack(Thing target)
        {
            //(packForcedAffectionEnemy.All((x) => x.Value == pawn.GetUniqueLoadID())
            foreach(KeyValuePair<string, Thing> entry in packForcedAffectionEnemy)
            {
                if (entry.Value == target)
                    return true;
            }
            return false;
        }

        /*
         * Intégration aux meutes en regroupement des éléments libres
         */
        private void integrateFreeMemberToGroup(Pawn member)
        {
            Comp_Killing ck = member.TryGetComp<Comp_Killing>();
            if (ck == null)
                return;
            string PMID = getPackMapID(member.Map, ck.KFM_PID);
            //Log.Message(">>>"+member.LabelCap);
            //Check si invalide 
            if (!isPackInGroupMode(member.Map, ck.KFM_PID) || !isValidPackMember(member, ck))
                return;

            //Log.Message("=>Mobilisation d'une recrue libre dans sa meute " + ck.KFM_PID);
            List<Pawn> members = new List<Pawn>();
            members.Add(member);

            setPackGroupMode(ref members, member.Map, ck.KFM_PID);
        }

        /*
         * Intégration aux meutes mobilisées des éléments libres ( animaux battant en retraite, animaux soignés et opérationnels)
         */
        private void integrateFreeMember(Pawn member, Comp_Killing ck)
        {
            //Si la meute de l'élément n'est pas mobilisé ou ce n'est pas un membre valide pour entrer dans la meute on quitte
            string PMID = getPackMapID( member.Map, ck.KFM_PID);
            bool noWait = false;
            if (!( (packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] != null ) &&  isValidPackMember(member, ck)))
                return;
            //On peut intégrer l'élément
            //Log.Message("=>Mobilisation d'une recrue libre dans sa meute "+ck.KFM_PID);
            List<Pawn> members = new List<Pawn>();
            members.Add(member);

            //Obtention target à partir de son UID
            Thing target = packAffectedEnemy[PMID];
            Pawn targetPawn = null;
            if (target is Pawn)
                targetPawn = (Pawn)target;


            //On check si la meute de l'animal mobilisé est déjà arrivé au point de raliement
            if (packAffectedArrivedToWaitingPoint.ContainsKey(PMID) && packAffectedArrivedToWaitingPoint[PMID])
                noWait = true;

            //On check si la cible n'est pas morte et que la cible est atteignable par l'animal avant d'attribuer le job
            if (target != null && ( (targetPawn != null && !targetPawn.Dead) || !target.IsBrokenDown())
                && member.CanReach(target, PathEndMode.Touch, Danger.Deadly)) {
                //Log.Message("=>Affectation de la recrue libre pour tuer " + target.LabelCap);
                setPackTarget(ref members, member.Map, ck.KFM_PID, target, noWait);
            }
            else
            {
                //Log.Message("=>Affectation impossible de la recrue car la cible n'est plus valide !");
            }
        }


        /*
         * Faire prendre une meute d'une map donné pour cible la la creature selectionnée
         * @members : liste des membres auxuquel affecté le job de kill
         * @map : la map de survenance du job
         * @PID : la pack impliquée
         * @target : la cible de la meute
         * @freeIntegratedUnit : Rajout d'un retardataire à la meute (objet de l'appel courant)
         * @noWait : Forcer à ne pas former de meute au point de ralliement (alone forcé)
         */
        private void setPackTarget(ref List<Pawn> members, Map map, string PID, Thing target, bool freeIntegratedUnit=false, bool noWait=false)
        {
            List<Verse.AI.Job> jobs = new List<Verse.AI.Job>();
            bool alone = false;
            string PMID;
            if (members.Count() == 1 || noWait)
                alone = true;

            PMID = getPackMapID(map, PID);
            //Si membre libre à intégrer pas besoin de s'occuper de cela, sa à déjà été effectué chztte
            if (!freeIntegratedUnit)
            {
                //Si seul pas besoin d'attendre d'autres membres
                if (alone)
                    packAffectedArrivedToWaitingPoint[PMID] = true;
                else
                {
                    //Si lastAffectedEndedGT <= gtBeforeReturnToRallyPoint --> pas de reformation de meute, les éléments sont assez proches pour réattaquer
                    if (lastAffectedEndedGT.ContainsKey(PMID) && (Find.TickManager.TicksGame - lastAffectedEndedGT[PMID]) <= Utils.gtBeforeReturnToRallyPoint)
                    {
                        //Log.Message("GT - lastAffectedEndedGT ( "+ (Find.TickManager.TicksGame - lastAffectedEndedGT[PMID]) + " ) < gtBeforeReturnToRallyPoint");
                        packAffectedArrivedToWaitingPoint[PMID] = true;
                        //On force le fait de ne pas atteindre le point de ralliement en défissant le en mode solo
                        alone = true;
                    }
                    else
                        packAffectedArrivedToWaitingPoint[PMID] = false;
                }
            }

            //Envois des membres de la meute présents sur la map au combat => JobGiver
            foreach (var member in members.ToList())
            {
                if(member != null)
                    setPackMember(member, target, alone, ref jobs);
            }

            //Si unitée libre on ajuste le packNbAffecred (incrémentation)
            if (freeIntegratedUnit)
            {
                //if(jobs.Count > 0)
                //packNbAffected[PMID].Add(jobs[0].loadID);
            }
            else
            {
                // On définis le contingent de la meute sur la map comme étant affecté à ce pawn
                packAffectedEnemy[PMID] = target;
            }

            //Démarrage des jobs le cas échéant
            for(int i = 0; i != members.Count(); i++)
            {
                if(jobs[i] != null)
                {
                    members[i].jobs.StartJob( jobs[i], JobCondition.InterruptForced);
                }
            }
        }

        /*
         * Check si pawn pas déjà targété par une pack
         */
        private bool thingNotAlreadyTargetedByPack(Thing thing)
        {
            foreach(KeyValuePair<string, Thing> entry in packAffectedEnemy.ToList())
            {
                if (entry.Value == thing)
                    return false;
            }
            return true;
        }

        /*
         * Obtention du PID de la meute ayant pris pour target pawn
         */
        public List<string> getPawnPackTargetedPID(Thing pawn)
        {
            List<string> ret = null;

            foreach (KeyValuePair<string, Thing> entry in packAffectedEnemy.ToList())
            {
                if (entry.Value == pawn)
                {
                    if(ret == null)
                        ret = new List<string>();
                    ret.Add(getPIDFromPMID(entry.Key));
                }
            }
            return ret;
        }

        /*
         * Check si dans la meute spécifiée il y au moins un membre valide
         */
        private bool AnyAvailableMemberInPack(string PID)
        {
            if (packs.ContainsKey(PID))
            {
                foreach (var member in packs[PID].ToList())
                {
                    if (member != null && !member.Downed && !member.Dead)
                        return true;
                }
            }
            return false;
        }

        public float processScore(Pawn pawn)
        {
            return pawn.kindDef.combatPower * pawn.health.summaryHealth.SummaryHealthPercent * pawn.ageTracker.CurLifeStage.bodySizeFactor;
        }

        public float processScore(Thing thing)
        {
            return 100f;
        }

        public string getPackMapID(Map map, string PID)
        {
            return map.GetUniqueLoadID() + "-" + PID;
        }

        public string getMIDFromPMID(string PMID)
        {
            return PMID.Substring(0,PMID.IndexOf("-"));
        }

        public string getPIDFromPMID(string PMID)
        {
            return PMID.Substring(PMID.IndexOf("-")+1);
        }

        /*
         * Obtention score de combat des membres d'une meute présents sur la map spécifiée
         */
        public float getPackScore(Map map, string PID, ref List<Pawn> packMembers, Thing target)
        {
            float ret = 0;
            Comp_Killing ck;
            packMembers.Clear();
            if (packs.ContainsKey(PID))
            {
                foreach (var member in packs[PID].ToList())
                {
                    if (member == null || !member.Spawned || member.Dead || !member.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                        continue;
                    ////Log.Message(")))"+member.LabelCap);
                    ck = member.TryGetComp<Comp_Killing>();
                    //Inclusion membre de pack que si kill mode activé et pas mort et pas downed et (safeMode pas actif ou safeMode actif et santé >= 50%)
                    if (ck != null && isValidPackMember(member, ck)
                        && member.Map == map)
                    {
                        ret += processScore(member);
                        packMembers.Add(member);
                    }
                }
            }
            return ret;
        }

        /*
         * Check si l'animal passé en paramétre est valide pour rentrer dans une meute
         */
        public bool isValidPackMember(Pawn member, Comp_Killing ck)
        {
            return member.Spawned && !member.Downed && !member.Dead && member.MentalState == null
                &&(ck != null && ck.killEnabled() &&  member.health.summaryHealth.SummaryHealthPercent >= 0.25f);
        }

        /*
         * Obtention de l'identifiant d'une meute disponible
         */
        private string getFreePID(Map map, float enemyScore, out float packScore, ref List<Pawn> members, out Thing forcedThing, Thing target)
        {
            forcedThing = null;
            string PMID;
            string requiredPMID=null;
            string requiredPID = null;
            bool okSup = true;

            //Le cas échéant recherche du required PMID dans le cas d'un forced pack planifié
            if (packForcedAffectionEnemy.Count() > 0)
            { 
                foreach (KeyValuePair<string, Thing> entry in packForcedAffectionEnemy.ToList())
                {
                    //Same PMID
                    if (entry.Value ==target)
                    {
                        requiredPMID = entry.Key;
                        requiredPID = getPIDFromPMID(requiredPMID);

                        //Si affectation a une pack ne devant pas killer un enemis downed un enemis downed justement on le supprime de l'affectation manuelle
                        if( target != null && ( target is Pawn && ((Pawn)target).Downed && !Settings.isAttackUntilDeathEnabled(null, requiredPID)))
                        {
                            packForcedAffectionEnemy.Remove(entry.Key);
                            requiredPMID = null;
                            requiredPID = null;
                        }
                        //Log.Message("Required PMID !!! " + requiredPID);
                        break;
                    }
                }
            }


            foreach (KeyValuePair<string, List<Pawn>> pack in packs.ToList())
            {
                okSup = true;
                //Si meute affecté à un regroupement OU si meute déjà affecté on passe a la suivante OU si meute réservée et son PMID et différente du requiredPMID (pour éviter exclure aussi le vrai bénificiaire)
                // Ou si la meute est en mode manuel et le requiredPMID est == null
                PMID = getPackMapID(map, pack.Key);
                if (isPackInGroupMode(map, pack.Key)
                    || (packAffectedEnemy.ContainsKey(PMID) && ( packAffectedEnemy[PMID] != null && requiredPMID == null))
                    || (packIsReserved(pack.Key) && PMID != requiredPMID)
                    || (Settings.isManualModeEnabled(pack.Key) && requiredPMID == null))
                {
                    //Log.Message("SKIP=> "+pack.Key+" "+ isPackInGroupMode(map, pack.Key)+" "+ (packAffectedEnemy.ContainsKey(PMID) && (packAffectedEnemy[PMID] != null && requiredPMID == null)) + " "+ (packIsReserved(pack.Key) && PMID != requiredPMID)+" "+ (Settings.isManualModeEnabled(pack.Key) && requiredPMID == null));
                    continue;
                }
                packScore = getPackScore(map, pack.Key, ref members,target);
                //si meute en mode supervisé check si target marqué comme cible de tueur
                if (Settings.isSupModeEnabled(pack.Key)
                    && (map.designationManager.DesignationOn(target) == null || map.designationManager.DesignationOn(target).def.defName != Utils.killDesignation) )
                    okSup = false;

                //La meute posséde des membres et il y elle posséde un score non null et supérieur ou égal au score de l'ennemy OU parametres autorise les attaques pour la meute de creatures plus fortes
                if (pack.Value.Count() >= 0 && packScore > 0f && ( enemyScore <= packScore || Settings.isAllowAttackStrongerTargetEnabled(pack.Key)) && (requiredPMID == null || requiredPMID == PMID) && okSup)
                {
                    //Check si pas de pawn forcé à être targeted avec cette meute sur cette lao (PMID)
                    if (requiredPMID != null && packForcedAffectionEnemy.ContainsKey(PMID))
                    {
                        forcedThing = packForcedAffectionEnemy[PMID];
                        packForcedAffectionEnemy.Remove(PMID);
                    }
                    return pack.Key;
                }
            }
            packScore = 0f;
            members.Clear();
            return null;
        }

        /*
         * Check si une meute est réservée dans la liste des forced affection ennemy
         */
        private bool packIsReserved(string PID)
        {
            string entryPID;
            foreach (KeyValuePair<string, Thing> entry in packForcedAffectionEnemy.ToList())
            {
                //Obtention PID de l'entrée
                entryPID = getPIDFromPMID(entry.Key);
                //Same PID
                if (entryPID == PID)
                {
                    return true;
                }
            }
            return false;
        }

        /*
         * Obtention du point de ralliement d'une map, si pas déjà le cas mise en cache
         */
        private CellRect getRallyAreaRect(Map map)
        {
            if (Utils.cachedRallyRect.minX == -1)
            {
                Utils.cachedRallyRect = CellRect.CenteredOn(rallyPoint[map.GetUniqueLoadID()], 6);
            }

            return Utils.cachedRallyRect;
        }

        /*
         * Obtention du point de ralliement agrandie d'une map
         */
        private CellRect getExtendedRallyAreaRect(Map map)
        {
            if (Utils.cachedExtendedRallyRect.minX == -1)
            {
                Utils.cachedExtendedRallyRect = CellRect.CenteredOn(rallyPoint[map.GetUniqueLoadID()], 15);
            }

            return Utils.cachedExtendedRallyRect;
        }

        /*
         * Attribution d'un tache d'aller tuer un ennemie à un animal
         */
        private bool setPackMember(Pawn member, Thing enemy, bool alone, ref List<Verse.AI.Job> jobs)
        {
            JobGiver_EnemyKill jgk = new JobGiver_EnemyKill();
            jgk.alone = alone;

            //Déduction coordonnées du waiting point dans la zone autorisée (rallyPoint)
            //Si pas caché mise en cache
            CellRect rallyArea = getRallyAreaRect(enemy.Map);
            jgk.selectedWaitingPoint = CellFinder.RandomSpawnCellForPawnNear(rallyArea.RandomCell, member.Map, 10);
            jgk.selectedWaitingPoint2 = CellFinder.RandomSpawnCellForPawnNear(rallyArea.RandomCell, member.Map, 10);
            //Définition de la cible
            jgk.target = enemy;

            JobIssueParams jb = new JobIssueParams();
            ThinkResult res = jgk.TryIssueJobPackage(member, jb);
            if (res.IsValid)
            {
                jobs.Add(res.Job);
                //member.jobs.StartJob(res.Job);
                return true;
            }
            else
            {
                jobs.Add(null);
            }
            return false;
        }



        private void reset()
        {
            packBlack.Clear();
            packBlue.Clear();
            packGray.Clear();
            packGreen.Clear();
            packOrange.Clear();
            packPink.Clear();
            packPurple.Clear();
            packRed.Clear();
            packWhite.Clear();
            packYellow.Clear();

            rallyPoint.Clear();
            packAffectedEnemy.Clear();
            packForcedAffectionEnemy.Clear();
            packAffectedArrivedToWaitingPoint.Clear();

            lastAffectedEndedGT.Clear();
            packAttackBonus.Clear();
            packAttackBonusGTE.Clear();
            packs.Clear();
            packNbAffected.Clear();
            packGroupPoint.Clear();
            packGroupModeGT.Clear();
            packGroupMode.Clear();
            kingNbKilledEnemy.Clear();
            kingNbEnemyToKill.Clear();
            kingAttackBonus.Clear();
            packOrderSource.Clear();
            checkEnemiesCurrentAffected.Clear();

            packsCanReCheckNearestTarget = false;
            Utils.cachedRallyRect = new CellRect(-1, -1, 0, 0);
            Utils.cachedExtendedRallyRect = new CellRect(-1, -1, 0, 0);
        }

        private void resetPacks()
        {
            packs[Utils.PACK_BLACK] = packBlack;
            packs[Utils.PACK_BLUE] = packBlue;
            packs[Utils.PACK_GRAY] = packGray;
            packs[Utils.PACK_GREEN] = packGreen;
            packs[Utils.PACK_ORANGE] = packOrange;
            packs[Utils.PACK_PINK] = packPink;
            packs[Utils.PACK_PURPLE] = packPurple;
            packs[Utils.PACK_RED] = packRed;
            packs[Utils.PACK_WHITE] = packWhite;
            packs[Utils.PACK_YELLOW] = packYellow;
        }

        private void initNull()
        {
            if (packBlack == null)
                packBlack = new List<Pawn>();
            if (packBlue == null)
                packBlue = new List<Pawn>();
            if (packGray == null)
                packGray = new List<Pawn>();
            if (packGreen == null)
                packGreen = new List<Pawn>();
            if (packOrange == null)
                packOrange = new List<Pawn>();
            if (packPink == null)
                packPink = new List<Pawn>();
            if (packPurple == null)
                packPurple = new List<Pawn>();
            if (packRed == null)
                packRed = new List<Pawn>();
            if (packWhite == null)
                packWhite = new List<Pawn>();
            if (packYellow == null)
                packYellow = new List<Pawn>();

            if (rallyPoint == null)
                rallyPoint = new Dictionary<string, IntVec3>();
            if (packAffectedEnemy == null)
                packAffectedEnemy = new Dictionary<string, Thing>();
            if (packForcedAffectionEnemy == null)
                packForcedAffectionEnemy = new Dictionary<string, Thing>();
            if (packAffectedArrivedToWaitingPoint == null)
                packAffectedArrivedToWaitingPoint = new Dictionary<string, bool>();

            if (lastAffectedEndedGT == null)
                lastAffectedEndedGT = new Dictionary<string, int>();
            if (packAttackBonus == null)
                packAttackBonus = new Dictionary<string, float>();
            if (packAttackBonusGTE == null)
                packAttackBonusGTE = new Dictionary<string, int>();
            if (packs == null)
                packs = new Dictionary<string, List<Pawn>>();
            if (packNbAffected == null)
                packNbAffected = new Dictionary<string, List<int>>();
            if (packGroupPoint == null)
                packGroupPoint = new Dictionary<string, IntVec3>();
            if (packGroupModeGT == null)
                packGroupModeGT = new Dictionary<string, int>();
            if (packGroupMode == null)
                packGroupMode = new Dictionary<string, bool>();
            if (kingNbKilledEnemy == null)
                kingNbKilledEnemy = new Dictionary<string, int>();
            if (kingAttackBonus == null)
                kingAttackBonus = new Dictionary<string, float>();
            if (kingNbEnemyToKill == null)
                kingNbEnemyToKill = new Dictionary<string, int>();
            if (packOrderSource == null)
                packOrderSource = new Dictionary<string, int>();
            if (checkEnemiesCurrentAffected == null)
                checkEnemiesCurrentAffected = new Dictionary<string, Thing>();
        }

        private Game game;

        private List<Pawn> packBlack = new List<Pawn>();
        private List<Pawn> packBlue = new List<Pawn>();
        private List<Pawn> packGray = new List<Pawn>();
        private List<Pawn> packGreen = new List<Pawn>();
        private List<Pawn> packOrange = new List<Pawn>();
        private List<Pawn> packPink = new List<Pawn>();
        private List<Pawn> packPurple = new List<Pawn>();
        private List<Pawn> packRed = new List<Pawn>();
        private List<Pawn> packWhite = new List<Pawn>();
        private List<Pawn> packYellow = new List<Pawn>();

        private Dictionary<string,List<Pawn>> packs = new Dictionary<string, List<Pawn>>();

        private Dictionary<string, int> kingNextElections = new Dictionary<string, int>();
        //Nombre d'ennemis tués par la meute depuis le dernier point bonus ajouté
        private Dictionary<string, int> kingNbKilledEnemy = new Dictionary<string, int>();
        //Nombre d'ennemis que doit tuer la meute pour incrémenter les points bonus du rois
        private Dictionary<string, int> kingNbEnemyToKill = new Dictionary<string, int>();
        //Point bonus du rois de meute
        private Dictionary<string, float> kingAttackBonus = new Dictionary<string, float>();
        private Dictionary<string, bool> packGroupMode = new Dictionary<string, bool>();
        private Dictionary<string, IntVec3> packGroupPoint = new Dictionary<string, IntVec3>();
        private Dictionary<string, int> packGroupModeGT = new Dictionary<string, int>();
        private Dictionary<string, IntVec3> rallyPoint = new Dictionary<string, IntVec3>();
        private Dictionary<string, float> packAttackBonus = new Dictionary<string, float>();
        private Dictionary<string, int> packAttackBonusGTE = new Dictionary<string, int>();
        private Dictionary<string, bool> packAffectedArrivedToWaitingPoint = new Dictionary<string, bool>();
        private Dictionary<string, Thing> packAffectedEnemy = new Dictionary<string, Thing>();
        private Dictionary<string, Thing> packForcedAffectionEnemy = new Dictionary<string, Thing>();
        private Dictionary<string, List<int>> packNbAffected = new Dictionary<string, List<int>>();
        private Dictionary<string, int> lastAffectedEndedGT = new Dictionary<string, int>();
        private Dictionary<string, string> packFilterByEnemy = new Dictionary<string, string>();
        private readonly IntVec3 noCoord = new IntVec3(-1, -1, -1);

        //Stock le type d'ordre en cours de la PMID (0 = auto, 1 =override humain pas de possibilité de le changer)
        private Dictionary<string, int> packOrderSource = new Dictionary<string, int>();
        //Permet de stocker si on peut recheker target des meutes 
        private bool packsCanReCheckNearestTarget = false;

        private Dictionary<string, Thing> checkEnemiesCurrentAffected = new Dictionary<string, Thing>();

        private List<string> packAffectedEnemyKeys = new List<string>();
        private List<Thing> packAffectedEnemyValues = new List<Thing>();
        private List<string> packForcedAffectionEnemyKeys = new List<string>();
        private List<Thing> packForcedAffectionEnemyValues = new List<Thing>();

        public bool forceNextCheckEnemy = false;
    }
}