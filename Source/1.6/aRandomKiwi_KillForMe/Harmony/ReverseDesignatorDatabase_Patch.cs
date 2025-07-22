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
    internal static class ReverseDesignatorDatabase_Patch
    {
        [HarmonyPatch(typeof(ReverseDesignatorDatabase), "InitDesignators")]
        public class InitDesignators
        {
            [HarmonyPostfix]
            public static void Listener(InitDesignators __instance, ref List<Designator> ___desList)
            {
                ___desList.Add(new Designator_Kill());
                ___desList.Add(new Designator_DKill());
            }
        }
    }
}
