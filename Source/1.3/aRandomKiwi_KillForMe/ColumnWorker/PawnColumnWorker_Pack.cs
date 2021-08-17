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
            Comp_Killing ck = Utils.getCachedCKilling(pawn);
            if (ck == null)
                return;

            Rect rect2 = rect.ContractedBy(2f);
            //Callback called to return the current animal's assignment pack
            Func<Pawn, string> getPayload = new Func<Pawn, string>(PackSelectButton_GetPackName);
            //Generation callback from the drop-down list of available packs
            Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<string>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<string>>>(PackSelectButton_GenerateMenu);
            string part = "";
            
            if (ck != null)
            {
                if (ck.KFM_isKing)
                    part = " (" + ("KFM_KingText").Translate() + ")";
                else if (ck.KFM_isWarrior)
                    part = " (" + ("KFM_WarriorText").Translate() + ")";
            }
            string buttonLabel = ("KFM_"+ck.KFM_PID+"ColorLib").Translate().Truncate(rect.width, null)+part;
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
            Comp_Killing ck = Utils.getCachedCKilling(pawn);
            if (ck == null)
            {
                return 1;
            }
            return 2;
        }

        private string GetValueToCompare2(Pawn pawn)
        {
            Comp_Killing ck = Utils.getCachedCKilling(pawn);
            if (ck != null)
            {
                return ("KFM_" + ck.KFM_PID + "ColorLib").Translate();
            }
            return string.Empty;
        }

        /*
         * Obtaining pack of running animal
         */
        private static string PackSelectButton_GetPackName(Pawn pet)
        {
            Comp_Killing ck = Utils.getCachedCKilling(pet);
            if (ck != null)
                return ("KFM_" + ck.KFM_PID + "ColorLib").Translate();
            else
                return "";
        }

        /*
         * Obtaining the list of packs assignable to the running animal
         */
        private static IEnumerable<Widgets.DropdownMenuElement<string>> PackSelectButton_GenerateMenu(Pawn p)
        {
            Comp_Killing ck = Utils.getCachedCKilling(p);

            if (ck == null)
                yield break;

            string curPID = ck.KFM_PID;
            for (int i=0; i!= Utils.PACKS.Count(); i++)
            {
                var PID = Utils.PACKS[i];
                //We remove the pack of the running animal if it has one.
                if (curPID == PID)
                    continue;
                yield return new Widgets.DropdownMenuElement<string>
                {
                    option = new FloatMenuOption(("KFM_"+PID+"ColorLib").Translate(), delegate
                    {
                        Comp_Killing ck2 = Utils.getCachedCKilling(p);
                        if (ck2 == null)
                            return;

                        //If animal is a kings we refuse
                        if (ck2.KFM_isKing)
                        {
                            Messages.Message("KFM_cannotChangeKingPack".Translate(p.LabelCap),MessageTypeDefOf.NeutralEvent);
                            return;
                        }
                        //If warrior we remove the status
                        if (ck2.KFM_isWarrior)
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
            Comp_Killing ck = Utils.getCachedCKilling(p);
            if (ck == null)
                return;
            //If an animal is currently mobilized (via its pack), it is made to stop its work
            Utils.GCKFM.cancelCurrentPackMemberJob(p);
            //If the animal is currently in grouping mode, it is made to stop its work
            Utils.GCKFM.cancelCurrentPackMemberGroupJob(p);

            //We remove the pawn from its current pack
            Utils.GCKFM.removePackMember(ck.KFM_PID, p);
            //Addition to the news
            Utils.GCKFM.addPackMember(PID, p);
            ck.KFM_PID = PID;
        }

    }
}