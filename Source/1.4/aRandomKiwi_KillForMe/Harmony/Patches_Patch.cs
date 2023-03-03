using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace aRandomKiwi.KFM
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            /*foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefs)
            {
                if (thingDef.race != null && thingDef.race.Animal)
                {
                    thingDef.comps.Add(new CompProps_Killing());
                }
            }*/
            var inst = new Harmony("rimworld.randomKiwi.KFM");
            inst.PatchAll(Assembly.GetExecutingAssembly());

            Utils.setAllowAllToKillState();
        }

        public static FieldInfo MapFieldInfo;
    }
}
