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
    internal class Projectile_Patch
    {
        [HarmonyPatch(typeof(Projectile), "get_DamageAmount")]
        public class get_DamageAmount
        {
            [HarmonyPostfix]
            public static void Listener(Projectile __instance, ref int __result, Thing ___launcher )
            {
                Pawn pawn = null;
                Comp_Killing ck = null;

                if (___launcher is Pawn)
                {
                    pawn = (Pawn)___launcher;
                    ck = pawn.TryGetComp<Comp_Killing>();
                }
                

                //Application bonus que si unité affectée pendant l'attaque
                if (ck != null && ck.KFM_affected)
                {
                    //Log.Message("Attaque à distance ");
                    float bonus = Utils.GCKFM.getPackAttackBonus(pawn.Map, ck.KFM_PID, pawn);
                    if (bonus != 0f)
                    {
                        //Log.Message("Augmentation coefficient d'attaque " + pawn.LabelCap + " de " + __result + " à " + (__result * (1 + bonus)));
                        __result = (int) (__result * (1 + bonus));
                    }
                }
            }
        }
    }
}