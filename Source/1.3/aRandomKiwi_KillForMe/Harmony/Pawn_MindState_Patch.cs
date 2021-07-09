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
        public class GetGizmos
        {
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
                    //Bouton ajouter sur les ennemies permettant de choisir via un floatMenu la meute à affecter de manière forcée
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

                //Boutons suivants réservés aux animaux tueurs
                if (__instance.pawn.TryGetComp<Comp_Killing>() == null || !__instance.pawn.TryGetComp<Comp_Killing>().killEnabled())
                    return;

                //Ajout boutton d'annulation d'un ordre d'une meute en cours (target)
                if (Utils.hasLearnedKilling(__instance.pawn)){
                    ret = new List<Gizmo>();
                    Comp_Killing ck = __instance.pawn.TryGetComp<Comp_Killing>();


                    //Ajout bouton permetant de changer la meute si pas un king
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
                                                ck = cpawn.TryGetComp<Comp_Killing>();
                                                if (ck == null || !cpawn.Faction.IsPlayer || !ck.killEnabled() || !Utils.hasLearnedKilling(cpawn))
                                                    continue;

                                                //Si warrior on enleve le status
                                                if (ck.KFM_isWarrior)
                                                {
                                                    Utils.GCKFM.unsetPackWarrior(cpawn);
                                                }
                                                //Si king on desactive la modif
                                                if (ck.KFM_isKing)
                                                {
                                                    Messages.Message("KFM_cannotChangeKingPack".Translate(cpawn.LabelCap), MessageTypeDefOf.NeutralEvent);
                                                    continue;
                                                }

                                                //Si animal actuellement mobilisé (via sa meute) on le fait arreter son travail
                                                Utils.GCKFM.cancelCurrentPackMemberJob(cpawn);
                                                //Si animal actuellement en mode regroupement on le fait arreter son travail 
                                                Utils.GCKFM.cancelCurrentPackMemberGroupJob(cpawn);

                                                //On enleve le pawn de son actuel pack
                                                Utils.GCKFM.removePackMember(ck.KFM_PID, cpawn);
                                                //Ajout a la nouvelle
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

                    //Si parametres le permette permet de définir en tant réel le rois d'une meute
                    if (Settings.allowManualKingSet)
                    {
                        //Si le pawn en cours n'est pas déjà rois
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

                                    //Logique on veut pas la meute courante ET que des meutes ayant des membres
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

                    //Ajout boutton permettant a la meute de désigner une cible
                    ret.Add( new Designator_TargetToKill(ck.KFM_PID));


                    Thing affectedEnemy = Utils.GCKFM.getAffectedEnemy(__instance.pawn.Map, ck.KFM_PID);
                    if (affectedEnemy != null)
                    {
                        //Annulation mobilisation en cours
                        cur = new Command_Action
                        {
                            icon = Utils.texCancelKill,
                            defaultLabel = "KFM_ForceCancelKillLabel".Translate(),
                            defaultDesc = "KFM_ForceCancelKillDesc".Translate(),
                            action = delegate ()
                            {
                                if (__instance != null && __instance.pawn!= null)
                                {
                                    Comp_Killing ck2 = __instance.pawn.TryGetComp<Comp_Killing>();
                                    if (ck2 != null && affectedEnemy != null)
                                    {
                                        if (Utils.GCKFM.isHalfOfPackNearEachOther(ck2.parent.Map, ck2.KFM_PID))
                                        {
                                            //Le cas échéant on force le non retour au point de ralliement des membres pour gagner du temps
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
                   

                    //Si parametres le permette
                    if (!Settings.disallowPackGroupMode && (!Settings.allowPackGroupModeOnlyIfKing
                        || Utils.GCKFM.packHasKing(ck.KFM_PID)))
                    {
                        //Ajout bouton de regroupement des meutes
                        Designator draft = new Designator_GroupToPoint(ck.KFM_PID, delegate (IntVec3 pos)
                        {
                            Utils.GCKFM.enablePackGroupMode(ck.KFM_PID, __instance.pawn.Map, pos);
                            return true;
                        });

                        ret.Add(draft);

                        //Ajout bouton d'annulation d'un regroupement si la meute est en mode regroupement
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
                                        //Le cas échéant on force le non retour au point de ralliement des membres pour gagner du temps
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
                //s'il sagit d'un animal en mode tueur il va y avoir un traitement différent des autres, sinon on envois à la routine vanilla avec la pssibilité de la fuite des troupeau de herdAnimals
                //En effet car pour les animaux en mode tueur de type herAnimals (elephnt, rhino, etc...) il y a un code supplementaire dans le vanilla qui fait q'uil y a 10% de chance qu'ils fly tous dans un rayon
                //de 25 si un des leurs est blessé ce qui n'est pas tolerable
                if ( (__instance.pawn.CurJob != null && __instance.pawn.CurJob.def.defName == Utils.killJob)
                    || (__instance.pawn.TryGetComp<Comp_Killing>() != null
                        && Find.TickManager.TicksGame - Utils.GCKFM.getLastAffectedEndedGT(__instance.pawn.Map, __instance.pawn.TryGetComp<Comp_Killing>().KFM_PID) <= Utils.gtBeforeReturnToRallyPoint))
                {
                    //Si pas en mode termiantor, en mode terminator pas de fuite
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
