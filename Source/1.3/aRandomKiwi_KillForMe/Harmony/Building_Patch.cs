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
                //If attacker killer affected
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

                    //If the killer is a member of a pack and is assigned the pack attack point bonus increment
                    if (killer != null && ck != null && ck.KFM_affected)
                    {
                        //Benefit from the bonus only if the building was clearly hostile to the player and allows the bonuses
                        if (__instance.HostileTo(Faction.OfPlayer) && Settings.allowPackAttackBonus)
                        {

                            //Log.Message("BUILDING KILLED BY " + dinfo.Instigator.LabelCap);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(killer.Map, ck.KFM_PID));
                        }
                        //We force the stop of the job of the members of the pack here to avoid unnecessary reflux of integration / exit of available members of the pack (Bug of animals going and leaving after enemy death)
                        Utils.GCKFM.cancelCurrentPack(pawnKiller.Map, ck.KFM_PID);
                        //We force the next enemy check to avoid latency in the next target to be eliminated by the pack (if applicable)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }
            }
        }
    }
}