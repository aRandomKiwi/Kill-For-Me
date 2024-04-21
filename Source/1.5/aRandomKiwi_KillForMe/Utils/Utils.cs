using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;


namespace aRandomKiwi.KFM
{
    [StaticConstructorOnStartup]
    static class Utils
    {
        static Utils()
        {
             killingTrainingDef = DefDatabase<TrainableDef>.GetNamed(killingTraining, true);
        }

        public static bool hasRemoteVerbAttack(IEnumerable<Verb> verbs, Pawn pawn)
        {
            foreach (var v in verbs)
            {
                if (v != null && v.verbProps != null && v.verbProps.LaunchesProjectile && v.IsStillUsableBy(pawn))
                    return true;
            }
            return false;
        }


        /*
         * Dispatcher of isValidEnemy according to parameters
         */
        public static bool isValidEnemy(Thing t, string PID = "")
        {
            if (Settings.allowTargetAllBuildings)
                return isValidEnemyExtended(t, PID);
            else
                return isValidEnemyLow(t, PID);
        }

        /*
         * Check if the designated thing is a valid enemy with regard to the parameters
         */
        public static bool isValidEnemyLow(Thing t, string PID="")
        {
            Pawn pawn = t as Pawn;
            
            bool isBuilding = t is Building_Turret || t is Building_Door || Settings.defaultBuildingToAttack.Contains(t.def.defName);
            /*if (t is Building_CrashedShipPart)
                Log.Message("LOL !!! " + isBuilding);*/
            if ( t.Spawned && (t is Pawn || isBuilding || t is Hive)
                && ((Settings.isSupModeEnabled(PID) && (t.Map.designationManager.DesignationOn(t) != null && t.Map.designationManager.DesignationOn(t).def.defName == Utils.killDesignation)) || !Settings.isSupModeEnabled(PID)) //Valid Supervised mode or not
                && (t.Map.designationManager.DesignationOn(t) == null || t.Map.designationManager.DesignationOn(t).def.defName != Utils.dKillDesignation) //No DontKill designation on target
                && (PID == "" || (Settings.isAttackUntilDeathEnabled(null, PID) || (!Settings.isAttackUntilDeathEnabled(null, PID) && ((pawn == null && !t.IsBrokenDown()) || (pawn != null && !pawn.Downed)))) )
                //&& (Settings.attackUntilDeath || (!Settings.attackUntilDeath && ((pawn == null && !t.IsBrokenDown()) || (pawn != null && !pawn.Downed))))
                && (Settings.allowKillSelfPawns || (!Settings.allowKillSelfPawns && (t.Faction != Faction.OfPlayer) || isBuilding)) 
                && (pawn == null || (!pawn.Dead)))
                return true;
            else
                return false;
        }

        /*
         * Check if the designated thing is a valid enemy with regard to the parameters
         */
        public static bool isValidEnemyExtended(Thing t, string PID="")
        {
            Pawn pawn = t as Pawn;
            bool isBuilding = t is Building;
            if (t.Spawned && (t is Pawn || isBuilding || t is Hive)
                && ((Settings.isSupModeEnabled(PID) && (t.Map.designationManager.DesignationOn(t) != null && t.Map.designationManager.DesignationOn(t).def.defName == Utils.killDesignation)) || !Settings.isSupModeEnabled(PID))  //Valid Supervised mode or not
                && (t.Map.designationManager.DesignationOn(t) == null || t.Map.designationManager.DesignationOn(t).def.defName != Utils.dKillDesignation)    //No DontKill designation on target
                && (PID == "" || (Settings.isAttackUntilDeathEnabled(null, PID) || (!Settings.isAttackUntilDeathEnabled(null, PID) && ((pawn == null && !t.IsBrokenDown()) || (pawn != null && !pawn.Downed)))))
                && (Settings.allowKillSelfPawns || (!Settings.allowKillSelfPawns && (t.Faction != Faction.OfPlayer) || isBuilding))
                && (pawn == null || (!pawn.Dead)))
                return true;
            else
                return false;
        }


        public static string TranslateTicksToTextIRLSeconds(int ticks)
        {
            //If less than one hour ingame then display seconds
            if (ticks < 2500)
                return ticks.ToStringSecondsFromTicks();
            else
                return ticks.ToStringTicksToPeriodVerbose(true);
        }


        /*
         * Dynamic modification of "KillingTraining"
         */
        static public void setAllowAllToKillState()
        {
            if (Settings.allowAllToKill)
            {
                //Removal of all prerequisites except obedience
                foreach (var x in killingTrainingDef.prerequisites.ToList())
                {
                    if (x.defName != "Obedience")
                        killingTrainingDef.prerequisites.Remove(x);
                }
                killingTrainingDef.requiredTrainability = DefDatabase<TrainabilityDef>.GetNamed("None");
            }
            else
            {
                bool findObedience = false;

                //Addition of prerequisites if not found
                foreach (var x in killingTrainingDef.prerequisites.ToList())
                {
                    if (x.defName == "Obedience")
                        findObedience = true;
                }

                if (!findObedience)
                    killingTrainingDef.prerequisites.Add(DefDatabase<TrainableDef>.GetNamed("Obedience"));

                killingTrainingDef.requiredTrainability = DefDatabase<TrainabilityDef>.GetNamed("Intermediate");
            }
        }


        public static Material getPackTexture(string PID)
        {
            if (PID == Utils.PACK_BLACK)
                return texBlackPack;
            else if (PID == Utils.PACK_BLUE)
                return texBluePack;
            else if (PID == Utils.PACK_GRAY)
                return texGrayPack;
            else if (PID == Utils.PACK_GREEN)
                return texGreenPack;
            else if (PID == Utils.PACK_ORANGE)
                return texOrangePack;
            else if (PID == Utils.PACK_PINK)
                return texPinkPack;
            else if (PID == Utils.PACK_PURPLE)
                return texPurplePack;
            else if (PID == Utils.PACK_RED)
                return texRedPack;
            else if (PID == Utils.PACK_WHITE)
                return texWhitePack;
            else if (PID == Utils.PACK_YELLOW)
                return texYellowPack;
            else
                return null;
        }


        /*
         * Check if the pawn is an animal and has learned to "Kill"
         */
        public static bool hasLearnedKilling(Pawn pawn)
        {
            return pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer && pawn.training.HasLearned(killingTrainingDef);
        }

        public static void setRandomChangeToVector(ref IntVec3 vector, int min, int max )
        {
            //Deduction direction of variation
            int dir = 1;
            if (Rand.Chance(0.5f))
                dir = -1;

            vector.x += (dir * Rand.Range(min, max));
            if (vector.x < 0)
                vector.x = 0;

            if (Rand.Chance(0.5f))
                dir = -1;
            else
                dir = 1;

            vector.z += (dir * Rand.Range(min, max));
            if (vector.z < 0)
                vector.z = 0;
        }


        /*
         * Elimination of bonding potentials for the designated animal
         */
        public static void removeAnimalBonding(Pawn animal)
        {
            if (animal == null || animal.relations == null || animal.relations.DirectRelations == null)
                return;
            foreach(var rel in animal.relations.DirectRelations.ToList())
            {
                if (rel.def.defName == "Bond")
                {
                    //Removal of the equivalent relation at the other pawn
                    foreach (var rel2 in rel.otherPawn.relations.DirectRelations.ToList())
                    {
                        if (rel2.def.defName == "Bond" && rel2.otherPawn == animal)
                        {
                            rel.otherPawn.relations.DirectRelations.Remove(rel2);
                            break;
                        }
                    }
                    animal.relations.DirectRelations.Remove(rel);
                }
            }
        }


        /*
         * Obtaining miniature pack
         */
        static public Texture2D getPackMinIcon(string PID)
        {
            if (PID == PACK_BLACK)
                return texBlackMinPack;
            if (PID == PACK_BLUE)
                return texBlueMinPack;
            if (PID == PACK_RED)
                return texRedMinPack;
            if (PID == PACK_ORANGE)
                return texOrangeMinPack;
            if (PID == PACK_YELLOW)
                return texYellowMinPack;
            if (PID == PACK_WHITE)
                return texWhiteMinPack;
            if (PID == PACK_GRAY)
                return texGrayMinPack;
            if (PID == PACK_BLACK)
                return texBlackMinPack;
            if (PID == PACK_GREEN)
                return texGreenMinPack;
            if (PID == PACK_PURPLE)
                return texPurpleMinPack;
            if (PID == PACK_PINK)
                return texPinkMinPack;

            return null;
        }


        public static Map getMapFromMUID(string MUID)
        {
            Map map = null;
            foreach (var cmap in Find.Maps)
            {
                if(cmap.GetUniqueLoadID() == MUID)
                {
                    map = cmap;
                    break;
                }
            }

            return map;
        }

        static public IntVec3 GetMeanVector(List<IntVec3> positions)
        {
            if (positions.Count == 0)
                return IntVec3.Zero;

            float x = 0f;
            float y = 0f;
            float z = 0f;
            foreach (IntVec3 pos in positions)
            {
                x += pos.x;
                y += pos.y;
                z += pos.z;
            }
            return new IntVec3((int)(x / positions.Count), (int)(y / positions.Count), (int)(z / positions.Count));
        }

        public static Comp_Killing getCachedCKilling(Pawn pawn)
        {
            if (pawn == null)
                return null;

            Comp_Killing cpt;
            cachedCK.TryGetValue(pawn, out cpt);
            if (cpt == null)
            {
                cpt = pawn.TryGetComp<Comp_Killing>();
                cachedCK[pawn] = cpt;
            }
            return cpt;
        }

        public static void resetCachedComps()
        {
            cachedCK.Clear();
        }

        public static int maxTimeToKill = 20000;

        //cache
        private static Dictionary<Thing, Comp_Killing> cachedCK = new Dictionary<Thing, Comp_Killing>();
        public static CellRect cachedRallyRect = new CellRect(-1,-1,0,0);
        public static CellRect cachedExtendedRallyRect = new CellRect(-1, -1, 0, 0);

        public static int gtBeforeReturnToRallyPoint = 3600;


        public static TrainableDef killingTrainingDef;

        public static readonly Texture2D texForceKill = ContentFinder<Texture2D>.Get("UI/Icons/ForceKill", true);
        public static readonly Texture2D texCancelKill = ContentFinder<Texture2D>.Get("UI/Icons/CancelKill", true);
        public static readonly Texture2D texCancelRegroup = ContentFinder<Texture2D>.Get("UI/Icons/CancelRegroup", true);
        public static readonly Texture2D texManualKing = ContentFinder<Texture2D>.Get("UI/Icons/Crown", true);
        public static readonly Texture2D texTransfertKing = ContentFinder<Texture2D>.Get("UI/Icons/TransfertCrown", true);

        //Pack avatars
        public static readonly Material texBlackPack = MaterialPool.MatFrom("UI/Packs/Black", ShaderDatabase.MetaOverlay);
        public static readonly Material texBluePack = MaterialPool.MatFrom("UI/Packs/Blue", ShaderDatabase.MetaOverlay);
        public static readonly Material texGrayPack = MaterialPool.MatFrom("UI/Packs/Gray", ShaderDatabase.MetaOverlay);
        public static readonly Material texGreenPack = MaterialPool.MatFrom("UI/Packs/Green", ShaderDatabase.MetaOverlay);
        public static readonly Material texOrangePack = MaterialPool.MatFrom("UI/Packs/Orange", ShaderDatabase.MetaOverlay);
        public static readonly Material texPinkPack = MaterialPool.MatFrom("UI/Packs/Pink", ShaderDatabase.MetaOverlay);
        public static readonly Material texPurplePack = MaterialPool.MatFrom("UI/Packs/Purple", ShaderDatabase.MetaOverlay);
        public static readonly Material texRedPack = MaterialPool.MatFrom("UI/Packs/Red", ShaderDatabase.MetaOverlay);
        public static readonly Material texWhitePack = MaterialPool.MatFrom("UI/Packs/White", ShaderDatabase.MetaOverlay);
        public static readonly Material texYellowPack = MaterialPool.MatFrom("UI/Packs/Yellow", ShaderDatabase.MetaOverlay);
        public static readonly Material texAttackTarget = MaterialPool.MatFrom("UI/Icons/Attack", ShaderDatabase.MetaOverlay);

        public static readonly Texture2D texBlackMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Black", true);
        public static readonly Texture2D texBlueMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Blue", true);
        public static readonly Texture2D texGrayMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Gray", true);
        public static readonly Texture2D texGreenMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Green", true);
        public static readonly Texture2D texOrangeMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Orange", true);
        public static readonly Texture2D texPinkMinPack =ContentFinder<Texture2D>.Get("UI/Packs/Min/Pink", true);
        public static readonly Texture2D texPurpleMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Purple", true);
        public static readonly Texture2D texRedMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Red", true);
        public static readonly Texture2D texWhiteMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/White", true);
        public static readonly Texture2D texYellowMinPack = ContentFinder<Texture2D>.Get("UI/Packs/Min/Yellow", true);

        public static readonly Material texCrown = MaterialPool.MatFrom("UI/Packs/Crown", ShaderDatabase.MetaOverlay);
        public static readonly Material texWarrior = MaterialPool.MatFrom("UI/Packs/Warrior", ShaderDatabase.MetaOverlay);



        public static string PACK_BLACK = "black";
        public static string PACK_BLUE = "blue";
        public static string PACK_GRAY = "gray";
        public static string PACK_GREEN = "green";
        public static string PACK_ORANGE = "orange";
        public static string PACK_PINK = "pink";
        public static string PACK_PURPLE = "purple";
        public static string PACK_RED = "red";
        public static string PACK_WHITE = "white";
        public static string PACK_YELLOW = "yellow";

        public static string[] PACKS = { "black", "blue", "gray", "green", "orange", "pink", "purple", "red", "white", "yellow" };

        public static string killDesignation = "KFM_Kill";
        public static string dKillDesignation = "KFM_DKill";
        public static string killingTraining = "KFM_KillingTraining";

        public static DesignationDef killDesignationDef = DefDatabase<DesignationDef>.GetNamed(killDesignation);
        public static DesignationDef dKillDesignationDef = DefDatabase<DesignationDef>.GetNamed(dKillDesignation);

        public static string killJob = "KFM_KillTarget";
        public static string groupToPointJob = "KFM_GroupToPoint";

        public static readonly Texture2D SettingsHeader = ContentFinder<Texture2D>.Get("UI/Settings/TexSettings", true);

        public static GC_KFM GCKFM;
    }
}
