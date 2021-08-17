using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class Pawn_HealthTracker_Patch
    {
        [HarmonyPatch(typeof(Pawn_HealthTracker), "MakeDowned")]
        public class MakeDowned
        {
            [HarmonyPostfix]
            public static void Listener(Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff)
            {
                if (dinfo != null && dinfo.Value.Instigator != null && !Settings.isAttackUntilDeathEnabled((Pawn)dinfo.Value.Instigator))
                {
                    //If the downed entity is affected as an enemy, a potential infinite affecation is prevented in the event of a bug

                    Thing downer = dinfo.Value.Instigator;
                    Pawn pawnDowner = null;
                    Comp_Killing ck = null;
                    if (downer is Pawn)
                    {
                        pawnDowner = (Pawn)downer;
                        ck = Utils.getCachedCKilling(pawnDowner);
                    }

                    //If the downer is a member of a pack and is affected AND settings in whose attack untail death mode bonus incrementation pack attack point
                    if (pawnDowner != null && Utils.hasLearnedKilling(pawnDowner) && ck != null && ck.KFM_affected)
                    {
                        //If bonus attack point allowed
                        if (Settings.allowPackAttackBonus)
                        {
                            //Log.Message("PAWN DOWNED BY " + dinfo.Value.Instigator.LabelCap);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(downer.Map, ck.KFM_PID));
                        }
                        //We force the stop of the job of the members of the pack here to avoid unnecessary reflux of integration / exit of available members of the pack (Bug of animals going and leaving after enemy death)
                        string PMID = Utils.GCKFM.getPackMapID(pawnDowner.Map, ck.KFM_PID);
                        Utils.GCKFM.cancelCurrentPack(pawnDowner.Map, ck.KFM_PID);
                        Utils.GCKFM.resetAffectedEnemy(PMID, pawnDowner.Map);
                        Utils.GCKFM.removeForcedAffectedEnemy(PMID);

                        //We force the next enemy check to avoid latency in the next target to be eliminated by the pack (if applicable)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }
            }
        }
    }
}