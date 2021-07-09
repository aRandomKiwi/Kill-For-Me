using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class ThinkNode_ConditionalCanDoConstantThinkTreeJobNow_Patch
    {
        [HarmonyPatch(typeof(ThinkNode_ConditionalCanDoConstantThinkTreeJobNow), "Satisfied")]
        public class Satisfied
        {
            [HarmonyPrefix]
            public static bool Replacement(ThinkNode_ConditionalCanDoConstantThinkTreeJobNow __instance, ref bool __result, Pawn pawn)
            {
                //If creature and in a "KFM_KillTarget" job
                //or he has recently left the pack so we force non-flight at the expense of his life in front of enemies (the creature will flee anyway if it is physically injured)
                if ((pawn.CurJob != null && pawn.CurJob.def.defName == Utils.killJob)
                    || (pawn.TryGetComp<Comp_Killing>() != null
                        && Find.TickManager.TicksGame - Utils.GCKFM.getLastAffectedEndedGT(pawn.Map, pawn.TryGetComp<Comp_Killing>().KFM_PID) <= Utils.gtBeforeReturnToRallyPoint))
                {
                    ////Log.Message("KPC" + pawn.LabelCap);
                    __result = false;
                    return false;
                }
                    
                return true;
            }
        }
    }
}