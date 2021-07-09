using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    internal class DesignationManager_Patch
    {
        [HarmonyPatch(typeof(DesignationManager), "RemoveDesignation")]
        public class RemoveDesignation
        {
            [HarmonyPostfix]
            public static void Replacement(DesignationManager __instance, ref Designation des)
            {
                //Log.Message(des.def.defName);
                //On intercepte les libération d'affectation pour pouvoir cancelCurrentPack sur les meutes en mode supervisé en train d'attaquer une cible désaffectée 
                if (des.def.defName == Utils.killDesignation)
                {
                    Thing target = des.target.Thing;
                    if(target != null)
                    {
                        Utils.GCKFM.unselectThingToKill(target);
                    }
                }
            }
        }


        [HarmonyPatch(typeof(DesignationManager), "RemoveAllDesignationsOn")]
        public class RemoveAllDesignationsOn
        {
            [HarmonyPrefix]
            public static bool Replacement(DesignationManager __instance, ref Thing t )
            {
                foreach(var des in __instance.allDesignations)
                {
                    if(des.def.defName == Utils.killDesignation && des.target.Thing == t)
                    {
                        //Log.Message("ICI2");
                        Utils.GCKFM.unselectThingToKill(t);
                    }
                }

                return true;
            }
        }
    }
}