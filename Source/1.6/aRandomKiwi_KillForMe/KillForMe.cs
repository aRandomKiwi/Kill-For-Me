using HarmonyLib;
using System.Reflection;
using Verse;
using UnityEngine;

namespace aRandomKiwi.KFM
{
    [StaticConstructorOnStartup]
    class KillForMe : Mod
    {
        public KillForMe(ModContentPack content) : base(content)
        {
            base.GetSettings<Settings>();
        }

        public void Save()
        {
            LoadedModManager.GetMod<KillForMe>().GetSettings<Settings>().Write();
        }

        public override string SettingsCategory()
        {
            return "Kill For Me";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Settings.DoSettingsWindowContents(inRect);
        }
    }
}