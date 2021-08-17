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

            //Constitution list of pack pointers
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

            //Rally points per map
            Scribe_Collections.Look(ref this.rallyPoint, "rallyPoint", LookMode.Value);
            //State of the regrouping mode of a pack
            Scribe_Collections.Look(ref this.packGroupMode, "packGroupMode", LookMode.Value);
            Scribe_Collections.Look(ref this.packGroupPoint, "packGroupPoint", LookMode.Value);
            Scribe_Collections.Look(ref this.packGroupModeGT, "packGroupModeGT", LookMode.Value);

            Scribe_Collections.Look(ref this.packFilterByEnemy, "packFilterByEnemy", LookMode.Value);


            //Storage for each pack kings of the number of enemies killed
            Scribe_Collections.Look(ref this.kingNbKilledEnemy, "kingNbKilledEnemy", LookMode.Value);
            Scribe_Collections.Look(ref this.kingAttackBonus, "kingAttackBonus", LookMode.Value);
            Scribe_Collections.Look(ref this.kingNbEnemyToKill, "kingNbEnemyToKill", LookMode.Value);


            //Attack bonus per pack
            Scribe_Collections.Look(ref this.packAttackBonus, "packAttackBonus", LookMode.Value);
            //GT end packs attack bonuses
            Scribe_Collections.Look(ref this.packAttackBonusGTE, "packAttackBonusGTE", LookMode.Value);
            //Assignment by MID (Map) -pack of enemies
            Scribe_Collections.Look<string, Thing>(ref this.packAffectedEnemy, "packAffectedEnemy",  LookMode.Value, LookMode.Reference, ref packAffectedEnemyKeys, ref packAffectedEnemyValues);
            //Pawn assignment forcing with packs
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if (packForcedAffectionEnemyValues == null)
                {
                    packForcedAffectionEnemyKeys = new List<string>();
                    packForcedAffectionEnemyValues = new List<Thing>();
                }
            }
            Scribe_Collections.Look<string, Thing>(ref this.packForcedAffectionEnemy, "packForcedAffectionEnemy", LookMode.Value, LookMode.Reference, ref packForcedAffectionEnemyKeys, ref packForcedAffectionEnemyValues);
            //Store the GTs at the end of the last pack mission per map in order to avoid reforming a pack at the rally point
            Scribe_Collections.Look(ref this.lastAffectedEndedGT, "lastAffectedEndedGT", LookMode.Value);
            //Definition if a pack has arrived at the waiting point
            Scribe_Collections.Look(ref this.packAffectedArrivedToWaitingPoint, "packAffectedArrivedToWaitingPoint",LookMode.Value);
            //Number of units affected on the enemy
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                //Serialisation
                foreach (KeyValuePair<string, List<int>> entry in packNbAffected)
                {
                    //Serialization of job IDs
                    packNbAffectedSerialized.Add( entry.Key+","+string.Join(",", entry.Value.Select(x => x.ToString()).ToArray()) );
                }
            }
            Scribe_Collections.Look(ref packNbAffectedSerialized, "packNbAffectedSerialized", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                if (packNbAffectedSerialized != null)
                {
                    //DeSerialization
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
                                    //Convert array from string to int
                                    res2 = Array.ConvertAll(res, s => int.Parse(s));
                                    packNbAffected[key] = new List<int>(res2);
                                }
                            }
                        }
                    }
                }
            }

            //Constitution list of pack pointers
            resetPacks();

            //Removal of null references (dead pack members etc ...)
            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                foreach(var entry in packs)
                {
                    entry.Value.RemoveAll(item => item == null);
                }
            }

            //Initialization of null fields if applicable And if param ok deletion of existing bonding relations
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                initNull();

                foreach (var entry in packs)
                {
                    entry.Value.RemoveAll(item => item == null);
                }

                //Check the validity of pack members
                foreach (var entry in packs)
                {
                    for (int i = entry.Value.Count-1; i >= 0;i--)
                    {
                        Pawn member = entry.Value[i];
                        if (member.Faction != Faction.OfPlayer || member.Dead)
                        {
                            Comp_Killing ck = Utils.getCachedCKilling(member);
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

                //Reset references to non-existent maps (PMID), not supposed to be used (normally managed in the MapDeiniter patch) but we never ...
                //We are based on rallyPoint
                foreach (var entry in rallyPoint.ToList())
                {
                    //IF Map does not exist, we launch the procedure to delete it
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

            //Check of the kings of the pack elections
            if (GT % 2500 == 0)
            {
                checkKings();
            }
            if (GT % 500 == 0)
            {
                //Check pack bonuses to reset
                checkPackBonusAttackToReset();
                //Check packs in group mode to be released
                checkTimeoutPackInGroupMode();
            }
            //Check if there is a threat to the player on each of the existing maps
            if (GT % 180 == 0 || forceNextCheckEnemy)
            {
                packsCanReCheckNearestTarget = true;
                forceNextCheckEnemy = false;
                //Check validated enemies forcibly assigned to prevent a pack from stopping taking over new enemies while trying to kill a departed enemies 
                checkForcedAffectedEnemies();
                checkAffectedEnemies();
                //Check arrived at the waitingPoint of all the members of a pack (to launch the assault)
                checkArrivedToWaitingPointPacks();

                //Check of members to be reintegrated into packs in regroup mode
                checkFreeMembersToIntegrateInPackGrouping();

                //Check enemies through the maps
                checkEnemies();
            }
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
            Utils.resetCachedComps();
        }

        public override void StartedNewGame()
        {

        }


        /*
         * Removal of the component's internal structures from references (PMID to the specified map)
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
         * Obtaining the list of members of a pack from its packID
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
         * Adding a pack member
         */
        public void addPackMember(string PID, Pawn member)
        {
            //Obtaining the pack in question
            List<Pawn> pack = getPack(PID);

            if (pack == null)
                return;

            //Search if pawn not already referenced in the pack
            for (int i = pack.Count -1; i >= 0; i--)
            {
                if (pack[i] == member)
                    return;
            }
            pack.Add(member);
        }

        /*
         * Removal of a member from a pack
         */
        public void removePackMember(string PID, Pawn member)
        {
            //Obtaining the pack in question
            List<Pawn> pack = getPack(PID);

            if (pack == null)
                return;

            //RSearch if pawn not already referenced in the pack
            for (int i = pack.Count - 1; i >= 0; i--)
            {
                if (pack[i] == member)
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
         * Setting the lastAffectedEndedGT for the defined PMID
         */
        public void setLastAffectedEndedGT(string PMID, int val)
        {
            lastAffectedEndedGT[PMID] = val;
        }


        /*
         * Increments the number of limbs assigned to kill an enemy
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
         * Decrease in the number of members affected to kill an enemy (downed, killed, succeed in killing the prey, ....)
         */
        public void decAffectedMember(Map map, string PID, int JID)
        {
            //Log.Message("==>DEC d'une unitée de pack=" + PID+" JID="+JID);
            string PMID = getPackMapID(map, PID);
            if (packNbAffected.ContainsKey(PMID)) {
                //Check if packNbAffected contains the job ID
                for ( int i = packNbAffected[PMID].Count -1; i >= 0;i--)
                {
                    if (packNbAffected[PMID][i] == JID)
                    {
                        packNbAffected[PMID].Remove(packNbAffected[PMID][i]);
                        //If for the given pack more affected members ==> delete entry to avoid empty entries that could bug the parser in ExposeData
                        if (packNbAffected.Count == 0)
                            packNbAffected.Remove(PMID);
                    }
                }
                //Log.Message("==>New packNbAffected == "+ packNbAffected[PMID].Count);
                //f == 0 then we reset the target for the pack ==> end of the task of killing the target
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
                //We define the lastAffectedEndedGT in order to avoid reorganization of the pack at the rallying point if immediately remobilized
                lastAffectedEndedGT[PMID] = Find.TickManager.TicksGame;
                //We force invocation checkEnemies if there are enemies available
                if (activeThreatNearRallyPoint(map))
                    forceNextCheckEnemy = true;
            }
        }


        /*
         * Adding an attack bonus to a pack
         */
        public void packIncAttackBonus(string PMID)
        {
            string PID;
            string MID;

            //Obtain PID
            PID = getPIDFromPMID(PMID);
            MID = getMIDFromPMID(PMID);

            if (!packAttackBonus.ContainsKey(PMID))
            {
                packAttackBonus[PMID] = 0f;
                packAttackBonusGTE[PMID] = 0;
            }

            //If the ceiling is reached we do nothing
            if (packAttackBonus[PMID] < 1.0f)
            {
                //We postpone the bonus end date
                packAttackBonusGTE[PMID] = Find.TickManager.TicksGame + Settings.bonusAttackDurationHour * 2500;
                packAttackBonus[PMID] += Settings.bonusAttackByEnemyKilled;

                //Bonus cap
                if (packAttackBonus[PMID] > 1.0f)
                {
                    packAttackBonus[PMID] = 1.0f;
                }

                //Display on all the members of the pack of the map of the advantage
                if (packs.ContainsKey(PID))
                {
                    foreach (var member in packs[PID])
                    {
                        Comp_Killing ck = Utils.getCachedCKilling(member);
                        if (member != null && member.Spawned
                            && !member.Dead && member.Map != null
                            && ck != null
                            && ck.KFM_PID == PID
                            && ck.KFM_affected
                            && member.Map.GetUniqueLoadID() == MID)
                        {
                            MoteMaker.ThrowText(member.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), member.Map, "+" + ((int)(Settings.bonusAttackByEnemyKilled * 100)) + "%", Color.green, -1f);
                        }
                    }
                }
            }

            //Check the royal bonus if applicable
            if ( packHasKing(PID) && kingAttackBonus.ContainsKey(PID) && kingAttackBonus[PID] < 1.0f)
            {
                kingNbKilledEnemy[PID]++;
                //If pack has reached the level of obtaining new bonus kill points
                if (kingNbKilledEnemy[PID] >= kingNbEnemyToKill[PID])
                {
                    //We notify on the screen
                    Pawn king = getPackKing(PID);
                    Messages.Message("KFM_MessagePackKingBonusAttackInc".Translate( ("KFM_PackColor"+PID).Translate(), kingNbEnemyToKill[PID]), MessageTypeDefOf.PositiveEvent, false);

                    kingNbKilledEnemy[PID] = 0;
                    kingNbEnemyToKill[PID] += 5;
                    kingAttackBonus[PID] += 0.05f;
                }
            }
        }


        /*
         * Obtaining the attack bonus for a pack in question on a map
         */
        public float getPackAttackBonus(Map map, string PID, Pawn attacker, bool tempOnly=false)
        {
            float ret=0f;
            string PMID = getPackMapID(map, PID);
            Comp_Killing ck=null;
            
            if(attacker != null)
                ck = Utils.getCachedCKilling(attacker);

            if (!packAttackBonus.ContainsKey(PMID))
            {
                ret = 0f;
            }
            else
                ret = packAttackBonus[PMID];

            //If applicable, additional pack leader bonus
            if (!tempOnly && kingAttackBonus.ContainsKey(PID))
            {
                ret += kingAttackBonus[PID];
            }

            //IF attacker leader or warrior add immediately
            if (ck != null)
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

            ck = Utils.getCachedCKilling(king);
            if(destKing != null)
                dck = Utils.getCachedCKilling(destKing);

            if (ck == null)
                return;

            //If an animal is currently mobilized (via its pack), it is made to stop its work
            cancelCurrentPackMemberJob(king);
            //If the animal is currently in grouping mode, it is made to stop its work
            cancelCurrentPackMemberGroupJob(king);

            //We remove the pawn from its current pack
            removePackMember(ck.KFM_PID, king);
            //Addition to the news
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

            // INIT data to avoid continuously checking the existence of fields in the following code
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

            //If no swapping, delete data from the SOURCE PACK because no more king
            if (dck == null)
            {
                kingNbKilledEnemy.Remove(SPID);
                kingAttackBonus.Remove(SPID);
                kingNbEnemyToKill.Remove(SPID);
            }
        }

        /*
         * Treatment operated when a king of a pack dies
         */
        public void kingPackDeath(Pawn king)
        {
            Comp_Killing kck = Utils.getCachedCKilling(king);

            if (kck == null)
                return;

            //GT definition with penalty for new elections
            Utils.GCKFM.unsetPackKing(kck.KFM_PID);
            Utils.GCKFM.setPackKingElectionGT(kck.KFM_PID, (Settings.kingElectionHourPenalityCausedByKingDeath * 2500) + Utils.GCKFM.getKingNextElectionGT(Find.TickManager.TicksGame));
            Find.LetterStack.ReceiveLetter("KFM_PackKingDeadLabel".Translate(), "KFM_PackKingDeadDesc".Translate(king.LabelCap, ("KFM_PackColor" + kck.KFM_PID).Translate(), (int)(Utils.GCKFM.getPackKingBonusAttack(kck.KFM_PID) * 100)), LetterDefOf.NegativeEvent, king, null, null);
        }

        /*
         * Reset of a pack member to his comp_killing level
         */
        public void resetCompKilling(Comp_Killing ck)
        {
            ck.KFM_PID = "";
            ck.KFM_affected = false;
            ck.KFM_isKing = false;
            ck.KFM_isWarrior = false;
        }

        /*
         * Obtaining the bonus expiry GTE
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
         * Check if all the members of a pack have arrived at the point of completion
         */
        public bool isPackArrivedToWaitingPoint(string PMID)
        {
            if (packAffectedArrivedToWaitingPoint.ContainsKey(PMID))
                return packAffectedArrivedToWaitingPoint[PMID];
            else
                return false;
        }

        /*
         * Cancel a current pack attack on a map
         */
        public void cancelCurrentPack(Map map, string PID)
        {
            if (map == null || !packs.ContainsKey(PID))
                return;

            string PMID = getPackMapID(map, PID);
            packOrderSource[PMID] = 0;

            for (int i =packs[PID].Count - 1 ; i >= 0; i--)
            {
                Pawn member = packs[PID][i];
                if (member != null && member.Spawned && !member.Dead && member.Map == map
                    &&  member.CurJob != null && member.CurJob.def.defName == Utils.killJob)
                {
                    //Log.Message(">> FORCING CANCEL JOB KILLING de " + member.LabelCap);
                    //Job cancellation
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
         * Cancel an attack by a member of a current pack
         */
        public void cancelCurrentPackMemberJob(Pawn member)
        {
            if (member.CurJob != null && member.CurJob.def.defName == Utils.killJob)
            {
                member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            }
        }

        /*
         * Unassign a member to a collection
         */
        public void cancelCurrentPackMemberGroupJob(Pawn member)
        {
            if ( member.CurJob != null && member.CurJob.def.defName == Utils.groupToPointJob)
            {
                member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
            }
        }

        /*
        * Order the rallying animals to attack directly (sends a signal that everything is ok)
        */
        public void launchCurrentFormingPack(Map map, string PID)
        {
            //Log.Message("Forcing attacking without wait waiting point");
            for (int i = packs[PID].Count - 1 ; i >= 0; i--)
            {
                Pawn member = packs[PID][i];
                if (member != null && member.Spawned && !member.Dead && member.Map == map
                    && member.CurJob != null && member.CurJob.def.defName == Utils.killJob)
                {
                    Comp_Killing ck = Utils.getCachedCKilling(member);
                    if (ck != null)
                        ck.KFM_waitingPoint.x = -1;
                }
            }
        }


        /*
         * Force a manual attack on an enemy by the player for the selected map pack
         */
        public void forcePackAttack(Map map, string PID, Pawn target, bool noWait=false, bool interactive=false)
        {
            List<Pawn> members = new List<Pawn>();
            float enemyScore = processScore(target);
            float packScore;
            packScore = getPackScore(map, PID, ref members, target);

            //If there are no members we quit forcing impossible
            if (members.Count() <= 0)
            {
                //Display error message if interactive call
                if (interactive)
                {

                }
                return;
            }

            //We send the animals
            setPackTarget(ref members, map, PID, target, false, noWait);
        }

        /*
         * Obtain enemies targeted by a defined PMID
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
         * Check if the thing passed in parameter is referenced in the targets of the packs, if so and that the pack in supervised mode, then we cancel the job
         */
        public void unselectThingToKill(Thing thing)
        {
            string PID;
            //Finds whether the target is referenced in the current target list
            foreach (var entry in packAffectedEnemy)
            {
                //This is a target
                if (entry.Value == thing)
                {
                    //Obtaining PID from PMID (key)
                    PID = getPIDFromPMID(entry.Key);
                    //If pack in supervised mode
                    if (Settings.isSupModeEnabled(PID))
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
         * Returns the current hold point for the given pack
         */
        public IntVec3 getGroupPoint(string PMID)
        {
            if (!packGroupPoint.ContainsKey(PMID))
                return noCoord;
            else
                return packGroupPoint[PMID];
        }


        /*
         * Returns the current hold point for the given pack
         */
        public IntVec3 getGroupPoint(Map map, string PID)
        {
            string PMID = getPackMapID(map, PID);
            return getGroupPoint(PMID);
        }

        /*
         * Enabling grouping mode for the designated pack
         */
        public void enablePackGroupMode(string PID, Map map, IntVec3 pos)
        {
            string PMID = getPackMapID(map, PID);
            //Log.Message("Regroupement de la meute " + PMID);
            //Obtaining a list of valid members
            if (!packs.ContainsKey(PID) || pos.x < 0)
                return;

            List<Pawn> members = packs[PID];
            List<Pawn> selected = new List<Pawn>();

            foreach (var member in members)
            {
                Comp_Killing ck = Utils.getCachedCKilling(member);
                if (member != null && member.Spawned && !member.Dead && isValidPackMember(member, ck)
                    && member.Map == map)
                {
                    selected.Add(member);
                }
            }

            //If no affectable members we leave
            if (selected.Count == 0)
                return;

            packGroupPoint[PMID] = pos;

            //GT group mode notification of the start of the mode (To automatically release the animals in the event of the player forgetting) OR if already activated, updating the GT
            packGroupModeGT[PMID] = Find.TickManager.TicksGame;


            //Mode already activated we quit
            if (packGroupMode.ContainsKey(PMID) && packGroupMode[PMID])
            {
                return;
            }

            //Group mode notification activated for the pack
            packGroupMode[PMID] = true;

            //Assigning GroupToPoint jobs
            setPackGroupMode(ref selected, map, PID);
        }


        /*
         * Disable grouping mode for the designated pack
         */
        public void disableGroupMode(string PID, Map map)
        {
            string PMID = getPackMapID(map, PID);
            //Mode already deactivated on return
            if (packGroupMode.ContainsKey(PID) && !packGroupMode[PMID])
                return;

            //Group mode deactivated notification for the pack

            packGroupMode[PMID] = false;

            if (map == null)
                return;
            for (int i = packs[PID].Count - 1; i >= 0; i--)
            {
                Pawn member = packs[PID][i];
                if (member != null && member.Spawned && !member.Dead && member.Map == map
                    && member.CurJob != null && member.CurJob.def.defName == "KFM_GroupToPoint")
                {
                    //Log.Message(">> FORCING CANCEL JOB GROUP2POINT de " + member.LabelCap);
                    //Job cancellation
                    if (member.jobs != null)
                        member.jobs.EndCurrentJob(JobCondition.InterruptForced, false);
                }
            }
        }




        /*
         * Check if at least one member of a pack can access a coordinate
         */
        public bool canPackMembersReach(Map map, string PID, IntVec3 pos)
        {
            if (!packs.ContainsKey(PID))
                return false;
            bool ret = false;
            LocalTargetInfo target = new LocalTargetInfo(pos);
            List<Pawn> members = packs[PID];

            foreach (var member in members)
            {
                if(member != null && member.Map == map && member.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                {
                    ret = true;
                    break;
                }

            }
            return ret;
        }



        /*
         * Check if at least 50% of the members (forecast) of a pack on a given map are close to each other
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
            //Deduction of what 50% means for the pack in question
            half = (int)(getPackNbAffectableMembers(map, PID) * Settings.percentageMemberNearToAvoidRallyPoint);

            //Deduction number of pack members with "half" numbers of other members close
            foreach (var member in members)
            {
                Comp_Killing ck1 = Utils.getCachedCKilling(member);
                if (member != null && member.Map == map && isValidPackMember(member, ck1))
                {
                    nbm = 0;
                    foreach (var member2 in members)
                    {
                        Comp_Killing ck2 = Utils.getCachedCKilling(member2);
                        if (member2.Map == map &&  isValidPackMember(member2, ck2))
                        {
                            if (member.Position.DistanceTo(member2.Position) <= 10f)
                            {
                                ////Log.Message(">>>" + member.LabelCap + " near of " + member2.LabelCap);
                                nbm++;
                            }
                        }
                    }
                    //If the member is close to 50% or more of his pack then we increment the general counter
                    if (nbm >= half)
                        nb++;
                }
            }

            //Log.Message("Near members : "+nb+", half requirement : "+half);
            //If the number of members close to each other is greater than or equal to 50%, we conclude that 50% of the members of the pack are already packed
            if (nb >= half)
                ret = true;

            return ret;
        }


        /*
         * Calculation of the number of members affected in a pack
         */
        public int getPackNbAffectedMembers(Map map, string PID)
        {
            List<Pawn> members = packs[PID];
            int nb = 0;

            //Deduction number of pack members with "half" numbers of other members close
            foreach (var member in members)
            {
                Comp_Killing ck = Utils.getCachedCKilling(member);
                if (member != null && member.Map == map && ck != null  && ck.KFM_affected)
                    nb++;
            }

            return nb;
        }

        /*
         * Calculation of assignable members in a pack
         */
        public int getPackNbAffectableMembers(Map map, string PID)
        {
            if (!packs.ContainsKey(PID))
                return 0;

            List<Pawn> members = packs[PID];
            int nb = 0;

            //Deduction number of pack members with "half" numbers of other members close
            foreach (var member in members)
            {
                Comp_Killing ck = Utils.getCachedCKilling(member);
                if (member != null && member.Map == map && isValidPackMember(member, ck))
                    nb++;
            }

            return nb;
        }

        /*
         * Starting GroupToPoint jobs
         */
        private void setPackGroupMode(ref List<Pawn> members, Map map, string PID)
        {
            List<Verse.AI.Job> jobs = new List<Verse.AI.Job>();
            string PMID;
            PMID = getPackMapID(map, PID);
            IntVec3 dec;
            Vector3 tmp = packGroupPoint[PMID].ToVector3();
            int nbReal = 0;

            for (int i = packs[PID].Count - 1; i >= 0; i--)
            {
                Pawn member = packs[PID][i];
                if (member != null)
                {
                    Comp_Killing ck = Utils.getCachedCKilling(member);
                    if (ck != null)
                    {
                        ck.KFM_groupWaitingPoint = packGroupPoint[PMID];
                        dec = new IntVec3(tmp);
                        Utils.setRandomChangeToVector(ref dec, 0, 4);
                        ck.KFM_groupWaitingPointDec = CellFinder.RandomSpawnCellForPawnNear(packGroupPoint[PMID], map);
                        setPackMemberGroupMode(member, ref jobs);
                    }
                }
            }


            //Start of jobs if necessary
            int count = members.Count();
            for (int i = 0; i != count; i++)
            {
                if (jobs[i] != null)
                {
                    members[i].jobs.StartJob(jobs[i], JobCondition.InterruptForced);
                    nbReal++;
                }
            }

            //If there are no members, we cancel the group mode for the pack
            if (nbReal == 0)
            {
                packGroupModeGT[PMID] = 0;
                packGroupMode[PMID] = false;
                packGroupPoint[PMID] = noCoord;
            }
        }

        /*
         * Assignment of a task to go and wait at a given point
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
         * Menu display allowing to send a pack to kill the enemy
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

            //List of packs assigned to the target
            List<string> affectedPacks = getPawnPackTargetedPID(target);

            //Listing of the colonists except those present in the exception list (exp)
            foreach (KeyValuePair<string, List<Pawn>> pack in packs)
            {
                affected = false;
                regroup = false;
                PMID = getPackMapID(target.Map, pack.Key);
                nbDispo = 0;
                //Calculation of available members
                foreach (var member in packs[pack.Key])
                {
                    Comp_Killing ck = Utils.getCachedCKilling(member);
                    if (member != null && member.Spawned && !member.Dead && isValidPackMember(member, ck)
                        && member.Map == target.Map)
                    {
                        nbDispo++;
                    }
                }
                //Check if current pack in grouping mode
                if (isPackInGroupMode(target.Map, pack.Key))
                    regroup = true;
                else
                {
                    //Check if current pack affected
                    if (affectedPacks != null)
                    {
                        foreach (var cpack in affectedPacks)
                        {
                            //Common pack affected
                            if (cpack == pack.Key)
                            {
                                affected = true;
                                break;
                            }
                        }
                    }
                }

                //Display possibility of pack control only if nbDipo> 0 (members) and if unsupervised mode OR supervised mode and target has the kill selector
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

                            //We check if at least 50% of the members of the pack are close to each other
                            if (isHalfOfPackNearEachOther(target.Map, pack.Key))
                            {
                                //If necessary, we force non-return to the rallying point of the members to save time
                                lastAffectedEndedGT[PMID] = Find.TickManager.TicksGame;
                            }

                            //Forcing kill of the target by the selected pack
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
                            //We check if at least 50% of the members of the pack are close to each other
                            if (isHalfOfPackNearEachOther(target.Map, pack.Key))
                            {
                                //If necessary, we force non-return to the rallying point of the members to save time
                                lastAffectedEndedGT[PMID] = Find.TickManager.TicksGame;
                            }

                            manualAllocatePack(pack.Key, target);

                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                    }
                }
            }
            //IF not choice display of the reason
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
         * Attempt to manually allocate a given pack (PID) to a target
         */
        public void manualAllocatePack(string PID, Thing target, bool verbose=true, int sourceMode = 1)
        {
            string PMID = getPackMapID(target.Map, PID);

            //Non-priority order
            if (packOrderSource.ContainsKey(PMID) && (packOrderSource[PMID] > sourceMode))
                return;

            string optText2 = ("KFM_PackColor" + PID).Translate();
            //Check if target killable by the pack (supervised mode)
            if (Settings.isSupModeEnabled(PID)
                            && (target.Map.designationManager.DesignationOn(target) == null || target.Map.designationManager.DesignationOn(target).def.defName != Utils.killDesignation))
            {
                if(verbose)
                    Messages.Message("KFM_ForceAllocateFAILEDSupMode".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NegativeEvent, false);
                return;
            }

            //Forcing kill of the target by the selected pack
            //Log.Message("Attribuer Meute " + PID);

            //If parameter active check strength of the pack and of the target
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

            //If the pack is in regrouping mode, we stop the regouppement
            if (isPackInGroupMode(target.Map, PID))
            {
                //We force the assignment of the pack to the target at the next iteration of checkEnemy which will happen just after
                forceNextCheckEnemy = true;
                packForcedAffectionEnemy[PMID] = target;
                //Log.Message("Annulation mode regoupement meute puis définition cible de kill  (meute=" + PMID + ", target=" + target.GetUniqueLoadID() + ")");
                //Stop grouping mode
                disableGroupMode(PID, target.Map);

                if (verbose)
                    Messages.Message("KFM_ForceAllocateAfterDeallocateRegroupOK".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
            }
            //If the pack is already assigned, we stop its assignment AND if toogle mode is activated
            else if (packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] != null)
            {
                //If pack already targets the requested target
                if (packAffectedEnemy[PMID] == target)
                {
                    if (verbose)
                        Messages.Message("KFM_ForceAllocateAfterDeallocateFAILED".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                    return;
                }

                //Getting current target
                Thing ctarget = packAffectedEnemy[PMID];
                //We force the assignment of the pack to the target at the next iteration of checkEnemy
                forceNextCheckEnemy = true;
                packForcedAffectionEnemy[PMID] = target;
                //Log.Message("packForcedAffectionEnemy forced " + PMID + " = " + target.GetUniqueLoadID());
                //Stop current assignment
                cancelCurrentPack(target.Map, PID);

                if (verbose)
                    Messages.Message("KFM_ForceAllocateAfterDeallocateOK".Translate(optText2.CapitalizeFirst(), ctarget.LabelCap, target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
            }
            else
            {
                //We force the assignment of the pack to the target at the next iteration of checkEnemy
                packForcedAffectionEnemy[PMID] = target;
                if (verbose)
                    Messages.Message("KFM_ForceAllocateOK".Translate(optText2.CapitalizeFirst(), target.LabelCap), MessageTypeDefOf.NeutralEvent, false);
            }

            packOrderSource[PMID] = sourceMode;
        }


        /*
         * Definition of the next execution WG of the leader of the pack elections
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
         * Obtaining the forced target of a given pack on a given map
         */
        public Thing getPackForcedAffectionEnemy(string PMID)
        {
            if(!packForcedAffectionEnemy.ContainsKey(PMID))
                return null;

            return packForcedAffectionEnemy[PMID];
        }

        /*
         * Check if there is a threat near the rally area
         */
        private bool activeThreatNearRallyPoint(Map map)
        {
            CellRect area = getExtendedRallyAreaRect(map);
            bool activeThreat = false;
            HashSet<IAttackTarget> hashSet = map.attackTargetsCache.TargetsHostileToFaction(Faction.OfPlayer);
            foreach (IAttackTarget target in hashSet)
            {
                //If the threat position is within the zone of the fulfillment point, then there is a threat !!
                if (GenHostility.IsActiveThreatTo(target, Faction.OfPlayer) && area.Contains(target.Thing.Position))
                {
                    activeThreat = true;
                    break;
                }
            }
            return activeThreat;
        }


        /*
         * Check the validity of enemies currently assigned to packs
         */
        private void checkCurrentEnnemiesValidity()
        {
            foreach(var entry in packAffectedEnemy)
            {

            }
        }

        /*
         * Check if there is a threat near the rallying area if yes cancels the packs being rallied ==> gives order to attack directly
         */
        private void checkThreatNearRallyPoint(Map map)
        {
            if (activeThreatNearRallyPoint(map) )
            {
                //Log.Message("!!!!!! Threat detected near the rallying point forcing attack of the rallying packs !!!!!!!");
                string PMID;
                //Cancellation of packs in training (lord of the hens)
                foreach (KeyValuePair<string, List<Pawn>> pack in packs)
                {
                    PMID = getPackMapID(map, pack.Key);
                    //if pack being formed and not arrived at the point of formation
                    if ( packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] != null
                        && !packAffectedArrivedToWaitingPoint[PMID])
                    {
                        //Direct assault
                        launchCurrentFormingPack(map, pack.Key);
                        packAffectedArrivedToWaitingPoint[PMID] = true;
                    }
                }
            }

        }


        /*
         * Check if all the members of a PMID have arrived at the waiting point to launch the assault
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

                //IF arrived at the waiting point or no pawn (definition to null)
                if ((packAffectedArrivedToWaitingPoint.ContainsKey(pack.Key) && packAffectedArrivedToWaitingPoint[pack.Key]) || pack.Value == null)
                    continue;

                MID = getMIDFromPMID(pack.Key);
                PID = getPIDFromPMID(pack.Key);
                nbArrived = 0;
                if (packs.ContainsKey(PID))
                {
                    //Calculation of the number of members of the pack for the given map have the flag arrivedToWaitingPoint if this value is equal to or greater than the number of affected Member ==> we launch the assault
                    for (int i = packs[PID].Count - 1; i >= 0; i--)
                    {
                        Pawn member = packs[PID][i];
                        Comp_Killing ck = Utils.getCachedCKilling(member);
                        //We only deal with the part of the pack on the specified map
                        if (member != null && member.Spawned && !member.Dead && member.Map.GetUniqueLoadID() == MID
                            && ck != null
                            && ck.KFM_arrivedToWaitingPoint)
                        {
                            nbArrived++;
                        }
                        if(member.CurJob != null)
                            curJobs.Add(member.CurJob.loadID);
                    }

                    //Deletion of the list of packNbAffected of jobID entries that no longer exist
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
                            //If not present we remove from packNbAffected the loadJobID which for one reason or another is obsolete
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
                    //Cap of nbAffected based on the number of members valid in the package
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



                //All the animals have arrived, we send the signal to the waiting animals (waitingPoint.x at -1)
                if (packAffectedArrivedToWaitingPoint.ContainsKey(pack.Key) && nbArrived >= nbAffected)
                {
                    //Log.Message(nbArrived + " " + nbAffected);
                    //We define the pack as having arrived at the waiting point OK ==> no more processing thereafter
                    packAffectedArrivedToWaitingPoint[pack.Key] = true;
                }
            }
        }

        /*
         * Check of free members to be reintegrated into packs in regroup mode
         */
        private void checkFreeMembersToIntegrateInPackGrouping()
        {
            Comp_Killing ck;
            foreach (var map in Find.Maps)
            {
                foreach (var cpack in packs)
                {
                    for (int i = cpack.Value.Count - 1; i >= 0; i--)
                    {
                        Pawn member = cpack.Value[i];
                        if (member != null)
                        {
                            ck = Utils.getCachedCKilling(member);

                            if (member != null && member.Spawned && member.Map != null && member.Faction == Faction.OfPlayer && ck != null && !ck.KFM_affected)
                            {
                                //Integration into groupings of free elements
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
         * Check pack bonuses to reset
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
                    //Display on the pack of the end of the attack bonus
                    for (int i = packs[PID].Count - 1; i >= 0; i--)
                    {
                        Pawn member = packs[PID][i];
                        Comp_Killing ck = Utils.getCachedCKilling(member);
                        if (member != null && member.Map != null && ck != null && ck.KFM_PID == PID && member.Map.GetUniqueLoadID() == MID)
                        {
                            MoteMaker.ThrowText(member.TrueCenter() + new Vector3(0.5f, 0f, 0.5f), member.Map, "KFM_BonusAttackLost".Translate(), Color.red, -1f);
                        }
                    }
                }
            }
        }


        /*
         * Check packs in regrouping mode where the assignment timeout has arrived (release of animals to prevent them from starving)
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
         * Dispatcher from enemy attack
         */
        private void checkEnemies()
        {
            Comp_Killing ck;

            foreach (var map in Find.Maps)
            {
                //If no rallyPoint we ignore the map
                if (getRallyPoint(map.GetUniqueLoadID()).x == -1)
                    continue;

                //Threat on the map check activation of animal packs
                if (activeThreatOrDesignatedEnemies(map))
                {
                    //Log.Message("Threat or enemies designated on the map detected !!!");

                    //Check threat near the rally point, if necessary cancellation of packs being created and recreated to attack target directly
                    checkThreatNearRallyPoint(map);


                    //Integration of free members
                    foreach (var cpack in packs)
                    {
                        for (int i = cpack.Value.Count - 1; i >= 0; i--)
                        {
                            Pawn member = cpack.Value[i];
                            ck = Utils.getCachedCKilling(member);

                            if (member != null && member.Faction == Faction.OfPlayer && ck != null && !ck.KFM_affected)
                            {
                                //Integration into mobilized packs of free elements (retreating animals, treated and operational animals)
                                if (member.Spawned && member.Map != null)
                                    integrateFreeMember(member, ck);
                            }
                        }
                    }

                    List<Thing> targets = new List<Thing>();

                    //Check naturally hostile enemies
                    foreach (IAttackTarget target in map.attackTargetsCache.TargetsHostileToColony)
                    {
                        targets.Add(target.Thing);
                        //Log.Message("==>Threat identified : " + target.Thing.LabelCap);
                        //processEnemy(map, target.Thing);
                    }

                    //Check downed enemies
                    foreach (var target in map.mapPawns.SpawnedDownedPawns)
                    {
                        if (target.HostileTo(Faction.OfPlayer))
                            targets.Add(target);
                    }

                    //Check designated enemies
                    foreach (var des in map.designationManager.SpawnedDesignationsOfDef(Utils.killDesignationDef))
                    {
                        if(!targets.Contains(des.target.Thing))
                            targets.Add(des.target.Thing);
                        //Log.Message("==>Designated threat identified : " + des.target.Thing.LabelCap);
                        //processEnemy(map, des.target.Thing);
                    }

                    //Check des forced ennemy attribution s'ils n'ont pas été traité ci-dessus
                    foreach (var enemy in packForcedAffectionEnemy.ToList())
                    {
                        if (!targets.Contains(enemy.Value))
                            targets.Add(enemy.Value);
                        //processEnemy(map, enemy.Value);
                    }

                    //Calculation of the average positions of the packs in force
                    Dictionary<string, IntVec3> packsCoordinates = new Dictionary<string, IntVec3>();
                    foreach (var cpack in packs)
                    {
                        List<Pawn> members = cpack.Value;
                        if (members.Count == 0)
                            continue;

                        //We calculate the coordinate mean of the group
                        List<IntVec3> coordinates = new List<IntVec3>();
                        foreach (var m in members)
                        {
                            if (m != null && isValidPackMember(m, Utils.getCachedCKilling(m)))
                            {
                                coordinates.Add(m.Position);
                            }
                        }
                        packsCoordinates[cpack.Key] = Utils.GetMeanVector(coordinates);
                    }

                    //Map of assignments during the pass
                    foreach (var cp in packs)
                    {
                        checkEnemiesCurrentAffected[cp.Key] = null;
                    }


                    //Closest Enemy Pack Analysis
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

                        //If there are no OR pack members on a map already assigned AND (not in closest rde recheck menance mode OR if target defined by Human Operator) we move on to the next pack
                        if (members.Count == 0 
                            || isPackInGroupMode(map, cpack.Key) 
                            || (packAffectedEnemy.ContainsKey(PMID) && (!packsCanReCheckNearestTarget || (packOrderSource.ContainsKey(PMID) && packOrderSource[PMID] == 1))))
                            continue;

                        //We calculate the coordinate mean of the group
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

                            //Check if enemy assigned to a pack, if applicable check if priority assignment (1) if yes order cancellation
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

                            //Deduction of the pack closest to the enemy
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

                                //Distance calculation of the current pack
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

                            //IF pack in progress closest and enemies closer than the previous one analyzed
                            if ( ((dist == -1 || dist > cdist) ) && nearestPack != null)//&& (!packAffectedEnemy.ContainsValue(t) || (packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] == t))))
                            {
                                dist = cdist;
                                selEnemy = t;
                                selPID = nearestPack;
                            }
                        }

                        if (selEnemy != null)
                        {
                            //Obtaining affected pack, if applicable
                            if (packAffectedEnemy.ContainsValue(selEnemy))
                            {
                                var key = packAffectedEnemy.FirstOrDefault(x => x.Value == selEnemy).Key;
                                if (key != null)
                                {
                                    string MID = getMIDFromPMID(key);
                                    string PID = getPIDFromPMID(key);

                                    //If the pack to which the enemy is currently assigned is different from the pack to which the enemies must be assigned
                                    if (PID != selPID)
                                    {
                                        //Stop the current assignment of a pack to the enemy
                                        cancelCurrentPack(Utils.getMapFromMUID(MID), PID);
                                    }
                                }
                            }
                            //Log.Message("Manually affected pack " + selPID + " on " + selEnemy.LabelCap);
                            checkEnemiesCurrentAffected[selPID] = selEnemy;
                            //Log.Message(selPID + " need target " + selEnemy.LabelCap);
                            //Launch of the pack to assault the enemies


                            manualAllocatePack(selPID, selEnemy, false, 0);
                            processEnemy(map, selEnemy);
                        }
                    }
                }
            }
            packsCanReCheckNearestTarget = false;
        }

        /*
         * Treatment of an enemy
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

            //Log.Message("=>" + Utils.isValidEnemy(thing)+ " "+ hasForcedAffectedPack(thing)+" "+ !Settings.ignoredTargets.Contains(thing.def.defName)+ " "+ thingNotAlreadyTargetedByPack(thing));
            // target has a forced pending OR Pawn assignment hostile to the player and not already targeted by a pack and is not part of the list of targets not to attack
            // And if attack until death mode not activated NOT DOWN
            // AND if enemie does not have a DontKILL designation !!!
            if (Utils.isValidEnemy(thing)
                && (hasForcedAffectedPack(thing)
                || (!Settings.ignoredTargets.Contains(thing.def.defName)
                && thingNotAlreadyTargetedByPack(thing))))
            {
                //Log.Message("=>processEnemy " + thing.LabelCap);
                // We try to make it targeted by an available pack with a score> = at the pawn
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
                // We have obtained a pack that can face the hostile pawn
                if (PID != null)
                {
                    //If there are threats near the rallying area ==> pack formation with direct attack
                    if (activeThreatNearRallyPoint(map))
                    {
                        //Log.Message("==>Mobilization of the "+ PID +" pack to directly (threat near rally point) smash "+ thing.LabelCap);
                        setPackTarget(ref members, map, PID, thing, false, true);
                    }
                    else
                    {
                        //If at least 50% of the members are close, we directly attack the enemies
                        if (Utils.GCKFM.isHalfOfPackNearEachOther(map, PID))
                        {
                            //If necessary, we force non-return to the rallying point of the members to save time
                            Utils.GCKFM.setLastAffectedEndedGT(Utils.GCKFM.getPackMapID(map, PID), Find.TickManager.TicksGame);
                        }

                        //Log.Message ("==> Mobilization of the" + PID + "pack to smash" + thing.LabelCap);
                        setPackTarget(ref members, map, PID, thing);
                    }
                }
            }
        }

        /*
         * Check if there is for the given map an active threat (or enemies designated to kill)
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
         * Routine laden with delirium a new kings if the waiting time reaches
         */
        private void checkKings()
        {
            //If the bonus system is deactivated OR kings defined manually in the param ons out
            if (!Settings.allowPackAttackBonus || Settings.allowManualKingSet)
                return;

            int gt = Find.TickManager.TicksGame;
            foreach (var cpack in packs)
            {
                //if pack has no kings
                if (!kingNbKilledEnemy.ContainsKey(cpack.Key))
                {
                    //If no election count, set up if conditions met (pack with 4 and more members)
                    if (cpack.Value.Count >= Settings.kingElectionMinMembers && !kingNextElections.ContainsKey(cpack.Key))
                    {
                        kingNextElections[cpack.Key] = getKingNextElectionGT(gt);
                    }
                    //IF election countdown reached
                    if (kingNextElections.ContainsKey(cpack.Key) && kingNextElections[cpack.Key] <= gt)
                    {
                        kingNextElections.Remove(cpack.Key);
                        //Countdown reached and pack conditions ok
                        if (cpack.Value.Count >= Settings.kingElectionMinMembers)
                        {
                            Pawn king = cpack.Value.RandomElement();
                            //Royal data initialization
                            setPackKing(cpack.Key, king);

                            Find.LetterStack.ReceiveLetter("KFM_KingElectionNewKingLabel".Translate(), "KFM_KingElectionNewKingDesc".Translate(king.LabelCap, ("KFM_PackColor" + cpack.Key).Translate() ), LetterDefOf.PositiveEvent, new TargetInfo(king), null, null);
                        }
                        else
                        {
                            //Relaunching the countdown
                            kingNextElections[cpack.Key] = getKingNextElectionGT(gt);
                        }
                    }
                }
            }
        }

        /*
         * Check validity of forcedAffectedTarget
         */
        public void checkForcedAffectedEnemies()
        {
            if (packForcedAffectionEnemy.Count() == 0)
                return;

            foreach (KeyValuePair<string, Thing> entry in packForcedAffectionEnemy.ToList())
            {
                //If the target becomes invalid, we remove it
                if ( entry.Value == null || entry.Value.DestroyedOrNull() || entry.Value.Map == null)
                {
                    //Log.Message("REMOVE AFFECTED");
                    packForcedAffectionEnemy.Remove(entry.Key);
                }
            }
        }

        /*
         * Check validity of affected enemy
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
                //If the target becomes invalid, we remove it

                if (entry.Value == null || entry.Value.DestroyedOrNull() || entry.Value.Map == null || !Utils.isValidEnemy(entry.Value,PID) ||  ( supMode && !supModeOk ) )
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
         * Creation of a new kings for the given pack
         */
        public void setPackKing(string PID, Pawn king)
        {
            if (king == null)
                return;
            Comp_Killing ck = new Comp_Killing();

            kingNbKilledEnemy[PID] = 0;
            kingNbEnemyToKill[PID] = 5;
            kingAttackBonus[PID] = 0.05f;

            ck = Utils.getCachedCKilling(king);
            if (ck != null)
            {
                ck.KFM_isWarrior = false;
                ck.KFM_isKing = true;
            }
        }


        /*
         * Removed a kings for the given pack
         */
        public void unsetPackKing(string PID)
        {
            Comp_Killing ck = new Comp_Killing();
            Pawn king = getPackKing(PID);

            if (king == null)
                return;

            ck = Utils.getCachedCKilling(king);

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
         * Reset a warrior
         */
        public void unsetPackWarrior(Pawn pet)
        {
            Comp_Killing ck = Utils.getCachedCKilling(pet);
            if (ck != null)
            {
                ck.KFM_isWarrior = false;
                ck.KFM_nbKilled = 0;
            }
        }


        /*
         * Check if a pack has a kings
         */
        public bool packHasKing(string PID)
        {
            return kingAttackBonus.ContainsKey(PID);
        }

        /*
         * Obtaining the kings of a pack
         */
        public Pawn getPackKing(string PID)
        {
            Pawn ret=null;

            if (!packs.ContainsKey(PID))
                return null;

            foreach(var entry in packs[PID])
            {
                Comp_Killing ck = Utils.getCachedCKilling(entry);
                if (ck != null && ck.KFM_isKing)
                {
                    return entry;
                }
            }
            return ret;
        }

        /*
         * Obtaining the royal attack bonus of a pack
         */
        public float getPackKingBonusAttack(string PID)
        {
            if (kingAttackBonus.ContainsKey(PID))
                return kingAttackBonus[PID];
            else
                return 0f;
        }

        /*
         * Obtaining for a given pack the number of enemies to kill before the next level
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
         * Check if the target has a forced assignment 
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
         * Integration into packs by grouping free elements
         */
        private void integrateFreeMemberToGroup(Pawn member)
        {
            Comp_Killing ck = Utils.getCachedCKilling(member);
            if (ck == null)
                return;
            string PMID = getPackMapID(member.Map, ck.KFM_PID);
            //Log.Message(">>>"+member.LabelCap);
            //Check if invalid
            if (!isPackInGroupMode(member.Map, ck.KFM_PID) || !isValidPackMember(member, ck))
                return;

            //Log.Message ("=> Mobilization of a free recruit in his pack" + ck.KFM_PID);
            List<Pawn> members = new List<Pawn>();
            members.Add(member);

            setPackGroupMode(ref members, member.Map, ck.KFM_PID);
        }

        /*
         * Integration into mobilized packs of free elements (retreating animals, treated and operational animals)
         */
        private void integrateFreeMember(Pawn member, Comp_Killing ck)
        {
            //If the element's pack is not mobilized or it is not a valid member to enter the pack, we leave
            string PMID = getPackMapID( member.Map, ck.KFM_PID);
            bool noWait = false;
            if (!( (packAffectedEnemy.ContainsKey(PMID) && packAffectedEnemy[PMID] != null ) &&  isValidPackMember(member, ck)))
                return;
            // We can integrate the element
            //Log.Message("=>Mobilization of a free recruit in his pack "+ ck.KFM_PID);
            List<Pawn> members = new List<Pawn>();
            members.Add(member);

            // Obtain target from its UID
            Thing target = packAffectedEnemy[PMID];
            Pawn targetPawn = null;
            if (target is Pawn)
                targetPawn = (Pawn)target;


            // We check if the pack of the mobilized animal has already arrived at the point of realization
            if (packAffectedArrivedToWaitingPoint.ContainsKey(PMID) && packAffectedArrivedToWaitingPoint[PMID])
                noWait = true;

            // We check if the target is not dead and that the target is reachable by the animal before assigning the job
            if (target != null && ( (targetPawn != null && !targetPawn.Dead) || !target.IsBrokenDown())
                && member.CanReach(target, PathEndMode.Touch, Danger.Deadly)) {
                ///Log.Message("=> Assignment of the free recruit to kill "+ target.LabelCap);
                setPackTarget(ref members, member.Map, ck.KFM_PID, target, noWait);
            }
            else
            {
                //Log.Message("=> Unable to assign the recruit because the target is no longer valid! ");
            }
        }


        /*
         * Have a pack of a given map target the selected creature
         * @members: list of members assigned the kill job
         * @map: the job occurrence map
         * @PID: the pack involved
         * @target: the target of the pack
         * @freeIntegratedUnit: Addition of a latecomer to the pack(object of the current call)
         * @noWait: Force not to form a pack at the rally point(forced alone)
         */
        private void setPackTarget(ref List<Pawn> members, Map map, string PID, Thing target, bool freeIntegratedUnit=false, bool noWait=false)
        {
            List<Verse.AI.Job> jobs = new List<Verse.AI.Job>();
            bool alone = false;
            string PMID;
            if (members.Count() == 1 || noWait)
                alone = true;

            PMID = getPackMapID(map, PID);
            //If free member to integrate no need to take care of this, it has already been done
            if (!freeIntegratedUnit)
            {
                //If only no need to wait for other members
                if (alone)
                    packAffectedArrivedToWaitingPoint[PMID] = true;
                else
                {
                    //If lastAffectedEndedGT <= gtBeforeReturnToRallyPoint -> no pack reformation, the elements are close enough to attack again
                    if (lastAffectedEndedGT.ContainsKey(PMID) && (Find.TickManager.TicksGame - lastAffectedEndedGT[PMID]) <= Utils.gtBeforeReturnToRallyPoint)
                    {
                        //Log.Message("GT - lastAffectedEndedGT ( "+ (Find.TickManager.TicksGame - lastAffectedEndedGT[PMID]) + " ) < gtBeforeReturnToRallyPoint");
                        packAffectedArrivedToWaitingPoint[PMID] = true;
                        //We force the fact of not reaching the rallying point by defeating it in single player mode
                        alone = true;
                    }
                    else
                        packAffectedArrivedToWaitingPoint[PMID] = false;
                }
            }

            //Sending of the members of the pack present on the map in combat => JobGiver
            for (int i = members.Count - 1; i >= 0; i--)
            {
                Pawn member = members[i];
                if(member != null)
                    setPackMember(member, target, alone, ref jobs);
            }

            //If free unit we adjust the packNbAffecred (incrementation)
            if (freeIntegratedUnit)
            {
                //if(jobs.Count > 0)
                //packNbAffected[PMID].Add(jobs[0].loadID);
            }
            else
            {
                // We define the contingent of the pack on the map as being assigned to this pawn
                packAffectedEnemy[PMID] = target;
            }

            //Start of jobs if necessary
            int count = members.Count();
            for (int i = 0; i != count; i++)
            {
                if(jobs[i] != null)
                {
                    members[i].jobs.StartJob( jobs[i], JobCondition.InterruptForced);
                }
            }
        }

        /*
         * Check if pawn is not already supplied by a pack
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
         * Obtaining the PID of the pack having taken as target pawn
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
         * Check if in the specified pack there is at least one valid member
         */
        private bool AnyAvailableMemberInPack(string PID)
        {
            if (packs.ContainsKey(PID))
            {
                for (int i = packs[PID].Count - 1; i >= 0; i--)
                {
                    Pawn member = packs[PID][i];
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
         * Obtain combat score from members of a pack present on the specified map
         */
        public float getPackScore(Map map, string PID, ref List<Pawn> packMembers, Thing target)
        {
            float ret = 0;
            Comp_Killing ck;
            packMembers.Clear();
            if (packs.ContainsKey(PID))
            {
                for (int i = packs[PID].Count - 1; i >= 0; i--)
                {
                    Pawn member = packs[PID][i];
                    if (member == null || !member.Spawned || member.Dead || !member.CanReach(target, PathEndMode.Touch, Danger.Deadly))
                        continue;
                    ////Log.Message(")))"+member.LabelCap);
                    ck = Utils.getCachedCKilling(member);
                    //Include pack member only if kill mode enabled and not dead and not downed and (safeMode not active or safeMode active and health> = 50%)
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
         * Check if the animal passed in parameter is valid to enter a pack
         */
        public bool isValidPackMember(Pawn member, Comp_Killing ck)
        {
            return member.Spawned && !member.Downed && !member.Dead && member.MentalState == null
                &&(ck != null && ck.killEnabled() &&  member.health.summaryHealth.SummaryHealthPercent >= 0.25f);
        }

        /*
         * Obtaining the identifier of an available pack
         */
        private string getFreePID(Map map, float enemyScore, out float packScore, ref List<Pawn> members, out Thing forcedThing, Thing target)
        {
            forcedThing = null;
            string PMID;
            string requiredPMID=null;
            string requiredPID = null;
            bool okSup = true;

            //If applicable, search for the required PMID in the case of a planned forced pack
            if (packForcedAffectionEnemy.Count() > 0)
            { 
                foreach (KeyValuePair<string, Thing> entry in packForcedAffectionEnemy.ToList())
                {
                    //Same PMID
                    if (entry.Value ==target)
                    {
                        requiredPMID = entry.Key;
                        requiredPID = getPIDFromPMID(requiredPMID);

                        //If assignment has a pack that should not kill a downed enemy, a downed enemy is removed from the manual assignment
                        if ( target != null && ( target is Pawn && ((Pawn)target).Downed && !Settings.isAttackUntilDeathEnabled(null, requiredPID)))
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
                // If the pack is assigned to a group OR if the pack is already assigned, we go to the next one OR if the pack is reserved and its PMID and different from the requiredPMID (to avoid also excluding the real beneficiary)
                // Or if the pack is in manual mode and the requiredPMID is == null
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
                //if pack in supervised mode check if target marked as killer target
                if (Settings.isSupModeEnabled(pack.Key)
                    && (map.designationManager.DesignationOn(target) == null || map.designationManager.DesignationOn(target).def.defName != Utils.killDesignation) )
                    okSup = false;

                //Log.Message("=>"+pack.Key+" "+ (pack.Value.Count() >= 0) + " "+ (packScore > 0f) + " "+ (enemyScore <= packScore || Settings.isAllowAttackStrongerTargetEnabled(pack.Key))+" "+ (requiredPMID == null || requiredPMID == PMID) + " "+ okSup);
                //The pack has members and there is a non-null score greater than or equal to the enemy's score OR parameters allow attacks for the pack of stronger creatures
                if (pack.Value.Count() >= 0 && packScore > 0f && ( enemyScore <= packScore || Settings.isAllowAttackStrongerTargetEnabled(pack.Key)) && (requiredPMID == null || requiredPMID == PMID) && okSup)
                {
                    //Check if no pawn is forced to be targeted with this pack on this lao (PMID)
                    if (requiredPMID != null && packForcedAffectionEnemy.ContainsKey(PMID))
                    {
                        forcedThing = packForcedAffectionEnemy[PMID];
                        packForcedAffectionEnemy.Remove(PMID);
                    }
                    /*if(requiredPMID != null)
                        Log.Message(pack.Key + " " + (Settings.isManualModeEnabled(pack.Key))+" "+(requiredPMID));*/
                    return pack.Key;
                }
            }
            packScore = 0f;
            members.Clear();
            return null;
        }

        /*
         * Check if a pack is reserved in the enemy forced affection list
         */
        private bool packIsReserved(string PID)
        {
            string entryPID;
            foreach (KeyValuePair<string, Thing> entry in packForcedAffectionEnemy.ToList())
            {
                //Obtain PID of the input
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
         * Obtaining the rally point of a map, if not already cached
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
         * Getting the enlarged rally point of a map
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
         * Assigning a task to go kill an enemy to an animal
         */
        private bool setPackMember(Pawn member, Thing enemy, bool alone, ref List<Verse.AI.Job> jobs)
        {
            JobGiver_EnemyKill jgk = new JobGiver_EnemyKill();
            jgk.alone = alone;

            // Deduction coordinates of the waiting point in the authorized zone (rallyPoint)
            // If not hidden caching
            CellRect rallyArea = getRallyAreaRect(enemy.Map);
            jgk.selectedWaitingPoint = CellFinder.RandomSpawnCellForPawnNear(rallyArea.RandomCell, member.Map, 10);
            jgk.selectedWaitingPoint2 = CellFinder.RandomSpawnCellForPawnNear(rallyArea.RandomCell, member.Map, 10);
            //Target definition
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
        //Number of enemies killed by the pack since the last bonus point added
        private Dictionary<string, int> kingNbKilledEnemy = new Dictionary<string, int>();
        //Number of enemies the pack must kill to increase the kings bonus points
        private Dictionary<string, int> kingNbEnemyToKill = new Dictionary<string, int>();
        //King of the Pack Bonus Point
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

        //Store the current order type of the PMID (0 = auto, 1 = human override no possibility to change it)
        private Dictionary<string, int> packOrderSource = new Dictionary<string, int>();
        //Allows to store if we can search target packs 
        private bool packsCanReCheckNearestTarget = false;

        private Dictionary<string, Thing> checkEnemiesCurrentAffected = new Dictionary<string, Thing>();

        private List<string> packAffectedEnemyKeys = new List<string>();
        private List<Thing> packAffectedEnemyValues = new List<Thing>();
        private List<string> packForcedAffectionEnemyKeys = new List<string>();
        private List<Thing> packForcedAffectionEnemyValues = new List<Thing>();

        public bool forceNextCheckEnemy = false;
    }
}