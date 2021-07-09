using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class Pawn_Patch
    {
        [HarmonyPatch(typeof(Pawn), "Kill")]
        public class Kill
        {
            [HarmonyPostfix]
            public static void Listener(Pawn __instance, DamageInfo? dinfo, Hediff exactCulprit = null)
            {
                if (dinfo != null && dinfo.Value.Instigator != null)
                {
                    Thing killer = dinfo.Value.Instigator;
                    Pawn pawnKiller = null;
                    Comp_Killing ck = null;
                    if (killer is Pawn) {
                        pawnKiller = (Pawn)killer;
                        ck = killer.TryGetComp<Comp_Killing>();
                    }

                    //Si le killer est membre d'une pack et qu'il est affecté
                    if(pawnKiller != null  && Utils.hasLearnedKilling(pawnKiller) && ck != null && ck.KFM_affected)
                    {
                        //Si points bonus autorisés
                        if (Settings.allowPackAttackBonus)
                        {
                            //Log.Message("PAWN KILLED BY " + dinfo.Value.Instigator.LabelCap + " pack " + ck.KFM_PID);
                            Utils.GCKFM.packIncAttackBonus(Utils.GCKFM.getPackMapID(killer.Map, ck.KFM_PID));
                            //Augmentation compteur nb ennemis tués
                            ck.incNbKilledEnemy();
                        }
                        //On force l'arret du job des membres de la meute  ici pour eviter des reflux inutiles d'integration/sortie de membres disponibles de la meute (Bug des animaux allant et partant aprés mort ennemis)
                        Utils.GCKFM.cancelCurrentPack(pawnKiller.Map, ck.KFM_PID);
                        //On force le prochain check d'ennemis pour éviter une latence dans la prochaine cible a eliminer par la meute (le cas echeant)
                        Utils.GCKFM.forceNextCheckEnemy = true;
                    }
                }

                //Si créature tuée est un animal dans une meute 
                if(Utils.hasLearnedKilling(__instance))
                {
                    Comp_Killing kck = __instance.TryGetComp<Comp_Killing>();
                    if (kck != null)
                    {
                        //Log.Message("Suppression du membre décédé "+ __instance.LabelCap+" de la meute "+ __instance.TryGetComp<Comp_Killing>().KFM_PID);

                        //S'il sagit d'un rois on le notifis et on définis les nouvelles elections loins dans le temps
                        if (kck.KFM_isKing)
                        {
                            Utils.GCKFM.kingPackDeath(__instance);
                        }

                        //On sort l'animal de la meute poiur éviter les enregistrements de valeur null
                        Utils.GCKFM.removePackMember(kck.KFM_PID, __instance);
                        Utils.GCKFM.resetCompKilling(kck);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Pawn), "DrawExtraSelectionOverlays")]
        public class DrawExtraSelectionOverlays
        {
            [HarmonyPostfix]
            public static void Listener(Pawn __instance)
            {
                //Si animal qui a appris à tué
                if (Utils.hasLearnedKilling(__instance))
                {
                    Comp_Killing ck = __instance.TryGetComp<Comp_Killing>();
                    //Affichage que si l'animal est un animal valide pour le mode kill (éviter qu'en cliquant sur animal down ou en mental break que le trait s'affiche)
                    if (ck != null && Utils.GCKFM.isValidPackMember(__instance, ck))
                    {
                        string PMID = Utils.GCKFM.getPackMapID(ck.parent.Map, ck.KFM_PID);
                        //Dessin des commandes prévisitionnelles de deplacement (groupToPoint mode) et de kill forcé 
                        Thing thingToKill = Utils.GCKFM.getPackForcedAffectionEnemy(PMID);
                        if (thingToKill != null)
                        {
                            GenDraw.DrawLineBetween(ck.parent.TrueCenter(), thingToKill.TrueCenter(), SimpleColor.Red);
                        }

                        //Si mode regroupement
                        if (Utils.GCKFM.isPackInGroupMode(PMID))
                        {
                            //Dessin de la ligne de raliment prévu
                            IntVec3 pos = Utils.GCKFM.getGroupPoint(PMID);
                            GenDraw.DrawLineBetween(ck.parent.TrueCenter(), pos.ToVector3(), SimpleColor.Blue);
                        }
                    }
                }
            }
        }


        [HarmonyPatch(typeof(Pawn), "SetFaction")]
        public class SetFaction
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn __instance, Faction newFaction, Pawn recruiter = null)
            {
                //Log.Message("LOLOLOLOLOOLOL )===============================================> "+ Utils.hasLearnedKilling(__instance).ToString()+" ");
                //S'il sagit d'un animal ayant appris a tuer ET qu'on affecte sa faction a une autre valeure que player (===> runWild,vente a un marchant) on le sort de sa meute
                if (Utils.hasLearnedKilling(__instance) && newFaction != Faction.OfPlayer)
                {
                    //Log.Message("LOLOLOLOLOOLOL )===============================================>ICI2");
                    Comp_Killing ck = __instance.TryGetComp<Comp_Killing>();
                    if (ck != null)
                    {
                        Utils.GCKFM.removePackMember(ck.KFM_PID, __instance);

                        //S'il sagit d'un rois on le notifis et on définis les nouvelles elections loins dans le temps
                        if (ck.KFM_isKing)
                        {
                            Utils.GCKFM.kingPackDeath(__instance);
                        }

                        //Reset du composant de l'animal
                        Utils.GCKFM.resetCompKilling(ck);
                    }
                }

                return true;
            }
        }
    }
}