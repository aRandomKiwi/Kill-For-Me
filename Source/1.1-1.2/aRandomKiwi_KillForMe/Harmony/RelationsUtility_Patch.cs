using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class RelationsUtility_Patch
    {
        [HarmonyPatch(typeof(RelationsUtility), "TryDevelopBondRelation")]
        public class TryDevelopBondRelation
        {
            [HarmonyPrefix]
            public static bool Replacement( ref bool __result, Pawn humanlike, Pawn animal, float baseChance)
            {
                //Log.Message("BOND1");
                //Si parametre activé ET animal tueur ==> on return false
                if ( Settings.disableKillerBond && Utils.hasLearnedKilling(animal))
                {
                    //Log.Message("BOND2");
                    __result = false;
                    return false;
                }
                return true;
            }
        }
    }
}