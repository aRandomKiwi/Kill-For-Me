using System.Collections.Generic;
using System.Linq;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace aRandomKiwi.KFM
{
    internal static class Pawn_MindState_Patch
    {
        [HarmonyPatch(typeof(Pawn_MindState), "GetGizmos")]
        public class Pawn_MindState_GetGizmos_Patch
        {
            private static Pawn prevPawn = null;
            private static Comp_Killing ck = null;

            [HarmonyPostfix]
            public static void Listener(Pawn_MindState __instance, ref IEnumerable<Gizmo> __result)
            {
                List<Gizmo> ret;
                Gizmo cur;
                if (__instance == null || __instance.pawn == null || __result == null)
                    return;

                if (__instance.pawn.HostileTo(Faction.OfPlayer) && !__instance.pawn.Downed)
                {
                    ret = new List<Gizmo>();
                    //Add button on enemies allowing to choose via a floatMenu the pack to forcefully affect
                    cur = new Command_Action
                    {
                        icon = Utils.texForceKill,
                        defaultLabel = "KFM_ForceKillLabel".Translate(),
                        defaultDesc = "KFM_ForceKillDesc".Translate(),
                        action = delegate ()
                        {
                            Utils.GCKFM.showFloatMenuForceKill(__instance.pawn);
                        }
                    };
                    ret.Add(cur);
                    __result = __result.Concat(ret);
                }

                if(prevPawn != __instance.pawn)
                {
                    prevPawn = __instance.pawn;
                    ck = Utils.getCachedCKilling(__instance.pawn);
                }

                //Following buttons reserved for killer animals
                if (ck == null || !ck.killEnabled())
                    return;

                //Add button to cancel an order from a current pack (target)
                if (Utils.hasLearnedKilling(__instance.pawn)){
                    ret = new List<Gizmo>();

                    //Add button allowing to change the pack if not a king
                    if (ck.KFM_PID != null && ck.KFM_PID != "" && !ck.KFM_isKing)
                     {
                        Material iconMat = Utils.getPackTexture(ck.KFM_PID);
                        Texture2D icon = (Texture2D)iconMat.mainTexture;

                        cur = new Command_Action
                        {
                            icon = icon,
                            defaultLabel = "",
                            defaultDesc = "",
                            action = delegate ()
                            {
                                if (__instance != null && __instance.pawn != null)
                                {
                                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                                    for (int i = 0; i != Utils.PACKS.Count(); i++)
                                    {
                                        var PID = Utils.PACKS[i];

                                        opts.Add(new FloatMenuOption(("KFM_" + PID + "ColorLib").Translate(), delegate
                                        {
                                            List<object> receptors = Find.Selector.SelectedObjects;
                                            if (receptors == null)
                                                return;

                                            foreach (var entry in receptors)
                                            {
                                                try
                                                {
                                                    if (entry == null || !(entry is Pawn))
                                                        continue;
                                                }
                                                catch (Exception)
                                                {
                                                    continue;
                                                }

                                                Pawn cpawn = (Pawn)entry;
                                                ck = Utils.getCachedCKilling(cpawn);
                                                if (ck == null || !cpawn.Faction.IsPlayer || !ck.killEnabled() || !Utils.hasLearnedKilling(cpawn))
                                                    continue;

                                                //If warrior we remove the status
                                                if (ck.KFM_isWarrior)
                                                {
                                                    Utils.GCKFM.unsetPackWarrior(cpawn);
                                                }
                                                //If king we deactivate the modification
                                                if (ck.KFM_isKing)
                                                {
                                                    Messages.Message("KFM_cannotChangeKingPack".Translate(cpawn.LabelCap), MessageTypeDefOf.NeutralEvent);
                                                    continue;
                                                }

                                                //If an animal is currently mobilized (via its pack), it is made to stop its work
                                                Utils.GCKFM.cancelCurrentPackMemberJob(cpawn);
                                                //If the animal is currently in grouping mode, it is made to stop its work
                                                Utils.GCKFM.cancelCurrentPackMemberGroupJob(cpawn);

                                                //We remove the pawn from its current pack
                                                Utils.GCKFM.removePackMember(ck.KFM_PID, cpawn);
                                                //Addition to the news
                                                Utils.GCKFM.addPackMember(PID, cpawn);
                                                ck.KFM_PID = PID;
                                            }

                                        }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                    }


                                    FloatMenu floatMenuMap = new FloatMenu(opts, "");
                                    Find.WindowStack.Add(floatMenuMap);
                                }
                            }
                        };
                        ret.Add(cur);
                    }

                    //If parameters allow it, it is possible to define as real the kings of a pack
                    if (Settings.allowManualKingSet)
                    {
                        //If the current pawn is not already kings
                        if (!ck.KFM_isKing)
                        {
                            cur = new Command_Action
                            {
                                icon = Utils.texManualKing,
                                defaultLabel = "KFM_ManualKingSetLabel".Translate(),
                                defaultDesc = "KFM_ManualKingSetDesc".Translate(),
                                action = delegate ()
                                {
                                    Utils.GCKFM.unsetPackKing(ck.KFM_PID);
                                    Utils.GCKFM.setPackKing(ck.KFM_PID, __instance.pawn);
                                }
                            };
                            ret.Add(cur);
                        }
                    }

                    if (ck.KFM_isKing)
                    {
                        cur = new Command_Action
                        {
                            icon = Utils.texTransfertKing,
                            defaultLabel = "KFM_TransfertKing".Translate(),
                            defaultDesc = "KFM_TransfertKingDesc".Translate(),
                            action = delegate ()
                            {
                                List<FloatMenuOption> opts = new List<FloatMenuOption>();

                                for (int i = 0; i != Utils.PACKS.Count(); i++)
                                {
                                    var PID = Utils.PACKS[i];
                                    List<Pawn> cpack = Utils.GCKFM.getPack(PID);

                                    //Logic, we don't want the current pack AND only packs with members
                                    if (cpack == null || PID == ck.KFM_PID || cpack.Count <= 1)
                                        continue;

                                    opts.Add(new FloatMenuOption(("KFM_" + PID + "ColorLib").Translate(), delegate
                                    {
                                        Utils.GCKFM.reassignKing(ck.KFM_PID, PID);

                                    }, MenuOptionPriority.Default, null, null, 0f, null, null));
                                }


                                FloatMenu floatMenuMap = new FloatMenu(opts, "");
                                Find.WindowStack.Add(floatMenuMap);
                            }
                        };
                        ret.Add(cur);
                    }

                    //Added button allowing the pack to designate a target
                    ret.Add( new Designator_TargetToKill(ck.KFM_PID));


                    Thing affectedEnemy = Utils.GCKFM.getAffectedEnemy(__instance.pawn.Map, ck.KFM_PID);
                    if (affectedEnemy != null)
                    {
                        //Cancellation of mobilization in progress
                        cur = new Command_Action
                        {
                            icon = Utils.texCancelKill,
                            defaultLabel = "KFM_ForceCancelKillLabel".Translate(),
                            defaultDesc = "KFM_ForceCancelKillDesc".Translate(),
                            action = delegate ()
                            {
                                if (__instance != null && __instance.pawn!= null)
                                {
                                    Comp_Killing ck2 = Utils.getCachedCKilling(__instance.pawn);
                                    if (ck2 != null && affectedEnemy != null)
                                    {
                                        if (Utils.GCKFM.isHalfOfPackNearEachOther(ck2.parent.Map, ck2.KFM_PID))
                                        {
                                            //If necessary, we force non-return to the rallying point of the members to save time
                                            Utils.GCKFM.setLastAffectedEndedGT(Utils.GCKFM.getPackMapID(ck2.parent.Map, ck2.KFM_PID), Find.TickManager.TicksGame);
                                        }

                                        string optText2 = ("KFM_PackColor" + ck2.KFM_PID).Translate();
                                        Utils.GCKFM.cancelCurrentPack(__instance.pawn.Map, ck2.KFM_PID);
                                        Messages.Message("KFM_ForceDeallocateOK".Translate(optText2.CapitalizeFirst(), affectedEnemy.LabelCap), MessageTypeDefOf.NeutralEvent, false);
                                    }
                                }
                            }
                        };
                        ret.Add(cur);
                        __result = __result.Concat(ret);
                    }


                    //If parameters allow it
                    if (!Settings.disallowPackGroupMode && (!Settings.allowPackGroupModeOnlyIfKing
                        || Utils.GCKFM.packHasKing(ck.KFM_PID)))
                    {
                        //Added pack grouping button
                        Designator draft = new Designator_GroupToPoint(ck.KFM_PID, delegate (IntVec3 pos)
                        {
                            Utils.GCKFM.enablePackGroupMode(ck.KFM_PID, __instance.pawn.Map, pos);
                            return true;
                        });

                        ret.Add(draft);

                        //Add button to cancel a regrouping if the pack is in regrouping mode
                        if (Utils.GCKFM.isPackInGroupMode(ck.parent.Map, ck.KFM_PID))
                        {
                            cur = new Command_Action
                            {
                                icon = Utils.texCancelRegroup,
                                defaultLabel = "KFM_CancelRegroupLabel".Translate(),
                                defaultDesc = "KFM_CancelRegroupDesc".Translate(),
                                action = delegate ()
                                {
                                    if (Utils.GCKFM.isHalfOfPackNearEachOther(ck.parent.Map, ck.KFM_PID))
                                    {
                                        //If necessary, we force non-return to the rallying point of the members to save time
                                        Utils.GCKFM.setLastAffectedEndedGT( Utils.GCKFM.getPackMapID(ck.parent.Map, ck.KFM_PID), Find.TickManager.TicksGame);
                                    }

                                    Utils.GCKFM.disableGroupMode(ck.KFM_PID, ck.parent.Map);
                                }
                            };
                            ret.Add(cur);
                        }
                    }


                    __result = __result.Concat(ret);
                }

            }
        }






        [HarmonyPatch(typeof(Pawn_MindState), "StartFleeingBecauseOfPawnAction")]
        public class StartFleeingBecauseOfPawnAction
        {
            [HarmonyPrefix]
            public static bool Replacement(Pawn_MindState __instance, Thing instigator)
            {
                // if it is an animal in killer mode there will be a different treatment from the others, otherwise we send to the vanilla routine with the possibility of the herdAnimals herd escaping
                // Indeed because for animals in killer mode of the herAnimals type (elephnt, rhino, etc ...) there is an additional code in the vanilla which means that there is a 10% chance that they will all fly in a ray
                // of 25 if one of them is injured which is not tolerable
                Comp_Killing ck = Utils.getCachedCKilling(__instance.pawn);
                if ( (__instance.pawn.CurJob != null && __instance.pawn.CurJob.def.defName == Utils.killJob)
                    || (ck != null
                        && Find.TickManager.TicksGame - Utils.GCKFM.getLastAffectedEndedGT(__instance.pawn.Map, ck.KFM_PID) <= Utils.gtBeforeReturnToRallyPoint))
                {
                    //If not in termiantor mode, in terminator mode no leak
                    if (!Settings.preventFleeWhenHit)
                    {
                        List<Thing> threats = new List<Thing>
                        {
                            instigator
                        };
                        IntVec3 fleeDest = CellFinderLoose.GetFleeDest(__instance.pawn, threats, __instance.pawn.Position.DistanceTo(instigator.Position) + 14f);
                        if (fleeDest != __instance.pawn.Position)
                        {
                            __instance.pawn.jobs.StartJob(new Job(JobDefOf.Flee, fleeDest, instigator), JobCondition.InterruptOptional, null, false, true, null, null, false);
                        }
                    }
                    return false;
                }


                return true;
            }
        }
    }
}
