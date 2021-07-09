using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class Pawn_Patch
    {
        [HarmonyPatch(typeof(Pawn), "Kill")]
        public class Kill
        {
            [HarmonyPostfix]
            public static void Listener(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
            {
                if (dinfo != null && dinfo.Value.Instigator != null)
                {
                    Thing killer = dinfo.Value.Instigator;
                    Pawn pawnKiller = null;
                    Comp_Killing ck = null;
                    if (killer is Pawn) {
                        pawnKiller = (Pawn)killer;
                        ck = killer.TryGetComp<Comp_Killing>();
                    }

                    //If the killer is a member of a pack and is affected
                    if (pawnKiller != null  && Utils.hasLearnedKilling(pawnKiller) && ck != null && ck.KFM_affected)
                    {
                        //If bonus points allowed
                        if (Settings.allowPackAttackBonus)
                        {
                            //Log.Message("PAWN KILLED BY " + dinfo.Value.Instigator.LabelCap + " pack " + ck.KFM_PID);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(killer.Map, ck.KFM_PID));
                            //Augmentation compteur nb ennemis tués
                            ck.incNbKilledEnemy();
                        }
                        //We force the stop of the job of the members of the pack here to avoid unnecessary reflux of integration / exit of available members of the pack (Bug of animals going and leaving after enemy death)
                        Utils.GCKFM.cancelCurrentPack(pawnKiller.Map, ck.KFM_PID);
                        //We force the next enemy check to avoid latency in the next target to be eliminated by the pack (if applicable)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }

                //If slain creature is an animal in a pack
                if (Utils.hasLearnedKilling(__instance))
                {
                    Comp_Killing kck = __instance.TryGetComp<Comp_Killing>();
                    if (kck != null)
                    {
                        //Log.Message("Suppression du membre décédé "+ __instance.LabelCap+" de la meute "+ __instance.TryGetComp<Comp_Killing>().KFM_PID);

                        //If it is about a kings we notify it and we define the new elections far in time
                        if (kck.KFM_isKing)
                        {
                            Utils.GCKFM.kingPackDeath(__instance);
                        }

                        //We take the animal out of the pack to avoid null records
                        Utils.GCKFM.removePackMember(kck.KFM_PID, __instance);
                        Utils.GCKFM.resetCompKilling(kck);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Pawn), "DrawExtraSelectionOverlays")]
        public class DrawExtraSelectionOverlays
        {
            [HarmonyPostfix]
            public static void Listener(Pawn __instance)
            {
                //If an animal that has learned to kill
                if (Utils.hasLearnedKilling(__instance))
                {
                    Comp_Killing ck = __instance.TryGetComp<Comp_Killing>();
                    //Display only if the animal is a valid animal for the kill mode (avoid that by clicking on animal down or in mental break that the trait is displayed)
                    if (ck != null && Utils.GCKFM.isValidPackMember(__instance, ck))
                    {
                        string PMID = Utils.GCKFM.getPackMapID(ck.parent.Map, ck.KFM_PID);
                        //Drawing of the preview commands for displacement (groupToPoint mode) and forced kill
                        Thing thingToKill = Utils.GCKFM.getPackForcedAffectionEnemy(PMID);
                        if (thingToKill != null)
                        {
                            GenDraw.DrawLineBetween(ck.parent.TrueCenter(), thingToKill.TrueCenter(), SimpleColor.Red);
                        }

                        //If grouping mode
                        if (Utils.GCKFM.isPackInGroupMode(PMID))
                        {
                            //Drawing of the planned slowdown line
                            IntVec3 pos = Utils.GCKFM.getGroupPoint(PMID);
                            GenDraw.DrawLineBetween(ck.parent.TrueCenter(), pos.ToVector3(), SimpleColor.Blue);
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Pawn), "SetFaction")]
        public class SetFaction
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn __instance, Faction newFaction, Pawn recruiter = null)
            {
                //If it is an animal that has learned to kill AND that its faction is assigned to a value other than player (===> runWild, sale to a merchant) it is removed from its pack
                if (Utils.hasLearnedKilling(__instance) && newFaction != Faction.OfPlayer)
                {
                    Comp_Killing ck = __instance.TryGetComp<Comp_Killing>();
                    if (ck != null)
                    {
                        Utils.GCKFM.removePackMember(ck.KFM_PID, __instance);

                        //If it is about a kings we notify it and we define the new elections far in time
                        if (ck.KFM_isKing)
                        {
                            Utils.GCKFM.kingPackDeath(__instance);
                        }

                        //Animal component reset
                        Utils.GCKFM.resetCompKilling(ck);
                    }
                }

                return true;
            }
        }
    }
}