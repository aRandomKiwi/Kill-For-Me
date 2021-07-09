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
                    //Si entitée downed est affecté en tant qu'ennemis on previent une potentielle affecation infinie en cas de bug 

                    Thing downer = dinfo.Value.Instigator;
                    Pawn pawnDowner = null;
                    Comp_Killing ck = null;
                    if (downer is Pawn)
                    {
                        pawnDowner = (Pawn)downer;
                        ck = downer.TryGetComp<Comp_Killing>();
                    }

                    //Si le downer est membre d'une pack et qu'il est affecté ET parametres en mode dont attack untail death incrémentation bonus point d'attaque de la meute
                    if (pawnDowner != null && Utils.hasLearnedKilling(pawnDowner) && ck != null && ck.KFM_affected)
                    {
                        //Si point d'attaque bonus autorisé 
                        if (Settings.allowPackAttackBonus)
                        {
                            //Log.Message("PAWN DOWNED BY " + dinfo.Value.Instigator.LabelCap);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(downer.Map, ck.KFM_PID));
                        }
                        //On force l'arret du job des membres de la meute  ici pour eviter des reflux inutiles d'integration/sortie de membres disponibles de la meute (Bug des animaux allant et partant aprés mort ennemis)
                        string PMID = Utils.GCKFM.getPackMapID(pawnDowner.Map, ck.KFM_PID);
                        Utils.GCKFM.cancelCurrentPack(pawnDowner.Map, ck.KFM_PID);
                        Utils.GCKFM.resetAffectedEnemy(PMID, pawnDowner.Map);
                        Utils.GCKFM.removeForcedAffectedEnemy(PMID);

                        //On force le prochain check d'ennemis pour éviter une latence dans la prochaine cible a eliminer par la meute (le cas echeant)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }
            }
        }
    }
}