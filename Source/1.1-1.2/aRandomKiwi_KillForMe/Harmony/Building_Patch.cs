using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Reflection;
using System.Reflection.Emit;
using System;

namespace aRandomKiwi.KFM
{
    internal class Building_Patch
    {
        [HarmonyPatch(typeof(Building), "PostApplyDamage")]
        public class PostApplyDamage
        {
            [HarmonyPostfix]
            public static void Listener(Building __instance, DamageInfo dinfo, float totalDamageDealt)
            {
                //Si attacker killer affecté
                if ( dinfo.Instigator != null && (__instance.Destroyed))
                {
                    Thing killer = dinfo.Instigator;
                    Pawn pawnKiller = null;
                    Comp_Killing ck = null;
                    if (killer is Pawn)
                    {
                        pawnKiller = (Pawn)killer;
                        ck = killer.TryGetComp<Comp_Killing>();
                    }

                    //Si le killer est membre d'une pack et qu'il est affecté incrémentation bonus point d'attaque de la meute
                    if (killer != null && ck != null && ck.KFM_affected)
                    {
                        //Bénéfie du bonus que si le batiment été clairement hostile au player et param autorise les bonus
                        if (__instance.HostileTo(Faction.OfPlayer) && Settings.allowPackAttackBonus)
                        {

                            //Log.Message("BUILDING KILLED BY " + dinfo.Instigator.LabelCap);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(killer.Map, ck.KFM_PID));
                        }
                        //On force l'arret du job des membres de la meute  ici pour eviter des reflux inutiles d'integration/sortie de membres disponibles de la meute (Bug des animaux allant et partant aprés mort ennemis)
                        Utils.GCKFM.cancelCurrentPack(pawnKiller.Map, ck.KFM_PID);
                        //On force le prochain check d'ennemis pour éviter une latence dans la prochaine cible a eliminer par la meute (le cas echeant)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }
            }
        }
    }
}