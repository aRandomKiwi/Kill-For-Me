using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using UnityEngine;

namespace aRandomKiwi.KFM
{
    internal class TransferableUIUtility_Patch
    {
        [HarmonyPatch(typeof(TransferableUIUtility), "DoExtraAnimalIcons")]
        public class DoExtraAnimalIcons
        {
            [HarmonyPostfix]
            public static void Listener(Transferable trad, Rect rect, ref float curX)
            {
                Pawn pawn = trad.AnyThing as Pawn;
                //Si animal qui a appris à tué on lui adjoint a son nom sa meute
                if (pawn != null && Utils.hasLearnedKilling(pawn))
                {
                    Comp_Killing ck = pawn.TryGetComp<Comp_Killing>();
                    Rect rect3 = new Rect(curX - 24f, (rect.height - 24f) / 2f, 24f, 24f);
                    curX -= 24f;
                    //TooltipHandler.TipRegion(rect3, PawnColumnWorker_Pregnant.GetTooltipText(pawn));
                    GUI.DrawTexture(rect3, Utils.getPackMinIcon(ck.KFM_PID) );
                }
            }
        }
    }
}