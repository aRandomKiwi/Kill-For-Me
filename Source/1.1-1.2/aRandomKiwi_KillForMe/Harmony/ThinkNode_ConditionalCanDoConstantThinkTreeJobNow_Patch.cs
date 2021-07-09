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
                //Si creature et dans un job "KFM_KillTarget" 
                //ou alors il a recemment quitté la meute alors on force la non fuite au dépend de sa vie en face d'ennemis ( la créature fuira quand meme si elle se fait blessée physiquement)
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