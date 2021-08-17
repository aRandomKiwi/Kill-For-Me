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
    internal class VerbProperties_Patch
    {
        [HarmonyPatch(typeof(VerbProperties), "AdjustedMeleeDamageAmount", new Type[] { typeof(Tool), typeof(Pawn), typeof(Thing), typeof(HediffComp_VerbGiver) })]
        public class AdjustedMeleeDamageAmount
        {
            [HarmonyPostfix]
            public static void Listener(VerbProperties __instance, ref float __result, Tool tool, Pawn attacker, Thing equipment, HediffComp_VerbGiver hediffCompSource)
            {
                Comp_Killing ck = Utils.getCachedCKilling(attacker);

                //Bonus application only if unit affected during attack
                if (ck != null && ck.KFM_affected)
                {
                    float bonus = Utils.GCKFM.getPackAttackBonus(attacker.Map, ck.KFM_PID, attacker);
                    if (bonus != 0f) {
                        __result = __result * (1 + bonus);
                    }
                }
            }
        }
    }
}