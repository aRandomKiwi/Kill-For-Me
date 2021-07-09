using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;

namespace aRandomKiwi.KFM
{
    /*
     * Interception signal de despawing d'une MAP pour effacé les références dans le GCKFM  (PMID)
     */
    internal class MapDeiniter_Patch
    {
        [HarmonyPatch(typeof(MapDeiniter), "NotifyEverythingWhichUsesMapReference")]
        public class NotifyEverythingWhichUsesMapReference
        {
            [HarmonyPostfix]
            public static void Listener(Map map)
            {
                //Log.Message("MAP DESPAWN !!!");
                Utils.GCKFM.purgeMapReference(map.GetUniqueLoadID());
            }
        }
    }
}