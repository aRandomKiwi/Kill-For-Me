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
            public static void Listener(Pawn_HealthTracker __instance, DamageInfo? dinfo, Hediff hediff) {
                if (dinfo?.Instigator is Pawn instigatorPawn && !Settings.isAttackUntilDeathEnabled(instigatorPawn))
                {
                    //If the downed entity is affected as an enemy, a potential infinite affectation is prevented in the event of a bug
                    Comp_Killing ck = Utils.getCachedCKilling(instigatorPawn);
                    Map map = instigatorPawn.Map;
                    //If the downer is a member of a pack and is affected AND settings in whose attack until death mode bonus incrementation pack attack point
                    if (Utils.hasLearnedKilling(instigatorPawn) && ck != null && ck.KFM_affected)
                    {
                        //If bonus attack point allowed
                        if (Settings.allowPackAttackBonus)
                        {
                            //Log.Message("PAWN DOWNED BY " + dinfo.Value.Instigator.LabelCap);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(map, ck.KFM_PID));
                        }
                        //We force the stop of the job of the members of the pack here to avoid unnecessary reflux of integration / exit of available members of the pack (Bug of animals going and leaving after enemy death)
                        string PMID = Utils.GCKFM.getPackMapID(map, ck.KFM_PID);
                        Utils.GCKFM.cancelCurrentPack(map, ck.KFM_PID);
                        Utils.GCKFM.resetAffectedEnemy(PMID, map);
                        Utils.GCKFM.removeForcedAffectedEnemy(PMID);

                        //We force the next enemy check to avoid latency in the next target to be eliminated by the pack (if applicable)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }
            }
        }
    }
}