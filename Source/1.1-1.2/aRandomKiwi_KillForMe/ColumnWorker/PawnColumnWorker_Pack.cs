using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace aRandomKiwi.KFM
{
    public class PawnColumnWorker_Pack : PawnColumnWorker
    {
        protected override GameFont DefaultHeaderFont
        {
            get
            {
                return GameFont.Tiny;
            }
        }

        public override int GetMinWidth(PawnTable table)
        {
            return Mathf.Max(base.GetMinWidth(table), 100);
        }

        public override int GetOptimalWidth(PawnTable table)
        {
            return Mathf.Clamp(170, this.GetMinWidth(table), this.GetMaxWidth(table));
        }

        public override void DoCell(Rect rect, Pawn pawn, PawnTable table)
        {
            if (!this.CanAssignPack(pawn) || pawn == null)
            {
                return;
            }
            Comp_Killing ck = pawn.TryGetComp<Comp_Killing>();
            if (ck == null)
                return;

            Rect rect2 = rect.ContractedBy(2f);
            //Callback appelée pour retourner la pack d'affectation de l'animal courant
			Func<Pawn, string> getPayload = new Func<Pawn, string>(PackSelectButton_GetPackName);
            //Callbacl de généraition de la liste déroulante des packs disponibles
			Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<string>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<string>>>(PackSelectButton_GenerateMenu);
            string part = "";
            
            if (ck != null)
            {
                if (ck.KFM_isKing)
                    part = " (" + ("KFM_KingText").Translate() + ")";
                else if (ck.KFM_isWarrior)
                    part = " (" + ("KFM_WarriorText").Translate() + ")";
            }
            string buttonLabel = ("KFM_"+pawn.TryGetComp<Comp_Killing>().KFM_PID+"ColorLib").Translate().Truncate(rect.width, null)+part;
			//string dragLabel = "LOL";
			Widgets.Dropdown<Pawn, string>(rect2, pawn, getPayload, menuGenerator, buttonLabel, null, buttonLabel, null, null, false);
        }

        public override int Compare(Pawn a, Pawn b)
        {
            int valueToCompare = this.GetValueToCompare1(a);
            int valueToCompare2 = this.GetValueToCompare1(b);
            if (valueToCompare != valueToCompare2)
            {
                return valueToCompare.CompareTo(valueToCompare2);
            }
            return this.GetValueToCompare2(a).CompareTo(this.GetValueToCompare2(b));
        }

        private bool CanAssignPack(Pawn pawn)
        {
            return Utils.hasLearnedKilling(pawn);
        }

        private int GetValueToCompare1(Pawn pawn)
        {
            if (!this.CanAssignPack(pawn))
            {
                return 0;
            }
            if (pawn.TryGetComp<Comp_Killing>() == null)
            {
                return 1;
            }
            return 2;
        }

        private string GetValueToCompare2(Pawn pawn)
        {
            if (pawn.TryGetComp<Comp_Killing>() != null)
            {
                return ("KFM_" + pawn.TryGetComp<Comp_Killing>().KFM_PID + "ColorLib").Translate();
            }
            return string.Empty;
        }

        /*
         * Obtention meute de l'animal courant
         */
        private static string PackSelectButton_GetPackName(Pawn pet)
        {   
            return ("KFM_" + pet.TryGetComp<Comp_Killing>().KFM_PID+ "ColorLib").Translate();
        }

        /*
         * Obtention de la liste des meutes affectables à l'animal courant
         */
        private static IEnumerable<Widgets.DropdownMenuElement<string>> PackSelectButton_GenerateMenu(Pawn p)
        {
            Comp_Killing ck = p.TryGetComp<Comp_Killing>();

            if (ck == null)
                yield break;

            string curPID = ck.KFM_PID;
            for (int i=0; i!= Utils.PACKS.Count(); i++)
            {
                var PID = Utils.PACKS[i];
                //On enleve la meute de l'animal courant s'il en possede une
                if (curPID == PID)
                    continue;
                yield return new Widgets.DropdownMenuElement<string>
                {
                    option = new FloatMenuOption(("KFM_"+PID+"ColorLib").Translate(), delegate
                    {
                        if (p.TryGetComp<Comp_Killing>() == null)
                            return;

                        //Si animal est un rois on refuse
                        if (p.TryGetComp<Comp_Killing>().KFM_isKing)
                        {
                            Messages.Message("KFM_cannotChangeKingPack".Translate(p.LabelCap),MessageTypeDefOf.NeutralEvent);
                            return;
                        }
                        //Si warrior on enleve le status
                        if (p.TryGetComp<Comp_Killing>().KFM_isWarrior)
                        {
                            Find.WindowStack.Add(new Dialog_Msg("KFM_ConfirmWarriorChangePackTitle".Translate(), "KFM_ConfirmWarriorChangePackDetail".Translate(p.LabelCap), delegate
                            {
                                Utils.GCKFM.unsetPackWarrior(p);
                                changePack(p, PID);
                            }, false));
                        }
                        else
                        {
                            changePack(p,PID);
                        }
                        
                    }, MenuOptionPriority.Default, null, null, 0f, null, null)
                };
            }
            yield break;
        }

        static private void changePack(Pawn p, string PID)
        {
            if (p.TryGetComp<Comp_Killing>() == null)
                return;
            //Si animal actuellement mobilisé (via sa meute) on le fait arreter son travail
            Utils.GCKFM.cancelCurrentPackMemberJob(p);
            //Si animal actuellement en mode regroupement on le fait arreter son travail 
            Utils.GCKFM.cancelCurrentPackMemberGroupJob(p);

            //On enleve le pawn de son actuel pack
            Utils.GCKFM.removePackMember(p.TryGetComp<Comp_Killing>().KFM_PID, p);
            //Ajout a la nouvelle
            Utils.GCKFM.addPackMember(PID, p);
            p.TryGetComp<Comp_Killing>().KFM_PID = PID;
        }

    }
}