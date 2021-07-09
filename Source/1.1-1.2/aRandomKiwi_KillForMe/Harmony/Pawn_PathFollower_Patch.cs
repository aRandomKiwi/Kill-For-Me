using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class Pawn_PathFollower_Patch
    {   
        /*
         * Patch permettant d'éviter une erreur liée au mod qui fait que soit jobs soit curdriver == null lors de l'appel a cette fonction
         */
        [HarmonyPatch(typeof(Pawn_PathFollower), "PatherFailed")]
        public class PatherFailed
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn_PathFollower __instance, Pawn ___pawn)
            {
                __instance.StopDead();
                if(___pawn.jobs != null && ___pawn.jobs.curDriver != null)
                    ___pawn.jobs.curDriver.Notify_PatherFailed();

                return false;
            }
        }
    }
}