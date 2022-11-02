using UnityEngine;
using Verse;
using System;
using System.Collections.Generic;

namespace aRandomKiwi.KFM
{
    class Settings : ModSettings
    {
        public static List<string> ignoredTargets = new List<string>() { "Boomrat" , "Boomalope", "HPLovecraft_MistStalker", "HPLovecraft_MistStalkerTwo" };
        public static List<string> ignoredRangedAttack = new List<string>() { "Green_Dragon_Race", "Black_Dragon_Race",
            "Brown_Dragon_Race", "Gold_Dragon_Race", "Silver_Dragon_Race", "Red_Dragon_Race", "Blue_Dragon_Race", "White_Dragon_Race",
        "Purple_Dragon_Race", "Cryo_Dragon_Race", "Flamingo_Dragon_Race", "Rock_Dragon_Race", "Yellow_Dragon_Race", "True_Dragon_Race", "Royal_Dragon_Race"};
        public static List<string> defaultBuildingToAttack = new List<string>() { "CrashedPsychicEmanatorShipPart", "CrashedPoisonShipPart", "ROM_PitChthonian" };


        public static bool hideRallyPointWarning = false;
        public static bool allowRangedAttack = true;
        public static bool allowKillSelfPawns = false;
        public static bool allowAllToKill = false;
        public static bool allowPackAttackBonus = true;
        public static bool attackUntilDeath = false;
        public static bool disableKillerBond = true;
        public static bool hidePackIcon = false;
        public static bool allowPackGroupModeOnlyIfKing = false;
        public static bool disallowPackGroupMode = false;
        public static bool preventFleeWhenHit = false;
        public static bool allowManualKingSet = false;

        public static int bonusAttackDurationHour = 2;
        public static float bonusAttackByEnemyKilled = 0.05f;
        public static int hoursBeforeAutoDisableGroupMode = 8;
        public static int kingElectionHourPenalityCausedByKingDeath = 72;
        public static float percentageMemberNearToAvoidRallyPoint = 0.5f;

        public static int kingElectionMinHour = 12;
        public static int kingElectionMaxHour = 168;
        public static int kingElectionMinMembers = 4;
        public static float kingAttackBonus = 0.50f;
        public static float warriorAttackBonus = 0.20f;
        public static bool allowTargetAllBuildings = false;

        public static int warriorNbToKill = 3;
        public static bool allowPackAttackStrongerTarget = false;

        //Parameters by pack
        public static bool blackSupMode = true;
        public static bool blueSupMode = true;
        public static bool redSupMode = true;
        public static bool yellowSupMode = true;
        public static bool pinkSupMode = true;
        public static bool purpleSupMode = true;
        public static bool greenSupMode = true;
        public static bool whiteSupMode = true;
        public static bool graySupMode = true;
        public static bool orangeSupMode = true;

        public static bool blackManualMode = false;
        public static bool blueManualMode = false;
        public static bool redManualMode = false;
        public static bool yellowManualMode = false;
        public static bool pinkManualMode = false;
        public static bool purpleManualMode = false;
        public static bool greenManualMode = false;
        public static bool whiteManualMode = false;
        public static bool grayManualMode = false;
        public static bool orangeManualMode = false;

        public static bool blackAttackStrongerTarget = false;
        public static bool blueAttackStrongerTarget = false;
        public static bool redAttackStrongerTarget = false;
        public static bool yellowAttackStrongerTarget = false;
        public static bool pinkAttackStrongerTarget = false;
        public static bool purpleAttackStrongerTarget = false;
        public static bool greenAttackStrongerTarget = false;
        public static bool whiteAttackStrongerTarget = false;
        public static bool grayAttackStrongerTarget = false;
        public static bool orangeAttackStrongerTarget = false;

        public static bool blackAttackUntilDeath = false;
        public static bool blueAttackUntilDeath = false;
        public static bool redAttackUntilDeath = false;
        public static bool yellowAttackUntilDeath = false;
        public static bool pinkAttackUntilDeath = false;
        public static bool purpleAttackUntilDeath = false;
        public static bool greenAttackUntilDeath = false;
        public static bool whiteAttackUntilDeath = false;
        public static bool grayAttackUntilDeath = false;
        public static bool orangeAttackUntilDeath = false;


        private static bool sectionGeneralExpanded = false;
        private static bool sectionGroupingExpanded = false;
        private static bool sectionAttackBonusExpanded = false;
        private static bool sectionBlackExpanded = false;
        private static bool sectionBlueExpanded = false;
        private static bool sectionRedExpanded = false;
        private static bool sectionOrangeExpanded = false;
        private static bool sectionYellowExpanded = false;
        private static bool sectionPinkExpanded = false;
        private static bool sectionPurpleExpanded = false;
        private static bool sectionGreenExpanded = false;
        private static bool sectionGrayExpanded = false;
        private static bool sectionWhiteExpanded = false;
        private static bool sectionTargetToIgnoreExpanded = false;
        private static bool sectionForcedMeleeExpanded = false;
        private static bool sectionBuildingAttackExpanded = false;

        public static Vector2 scrollPosition = Vector2.zero;

        public static void DoSettingsWindowContents(Rect inRect)
        {
            if (ignoredTargets == null)
                resetDefaultIgnoredTargets();
            inRect.yMin += 15f;
            inRect.yMax -= 15f;
            
            var defaultColumnWidth = (inRect.width - 50);
            Listing_Standard list = new Listing_Standard() { ColumnWidth = defaultColumnWidth };


            var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height);
            var scrollRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height * 2.2f + (26 * ignoredRangedAttack.Count ) + 2510 + (26 * ignoredTargets.Count ) + (26* defaultBuildingToAttack.Count));
            Widgets.BeginScrollView(outRect, ref scrollPosition, scrollRect, true);

            list.Begin(scrollRect);

            list.ButtonImage(Utils.SettingsHeader, 850, 128);
            list.Gap(10);

            /****************************************************** General parameters ******************************************************/
            if (!sectionGeneralExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_SettingsGeneralSection".Translate()))
                sectionGeneralExpanded = !sectionGeneralExpanded;
            GUI.color = Color.white;

            if (sectionGeneralExpanded)
            {
                //Hide rally point alert
                list.CheckboxLabeled("KFM_HideRallyPointWarning".Translate(), ref hideRallyPointWarning);

                //Hide the pack icon
                list.CheckboxLabeled("KFM_HidePackIcon".Translate(), ref hidePackIcon);

                //Prevent animals from running away when they are bruised
                list.CheckboxLabeled("KFM_SettingsPreventFromFleeWhenHit".Translate(), ref preventFleeWhenHit);

                //allow remote attacks
                list.CheckboxLabeled("KFM_SettingsAllowRangedAttack".Translate(), ref allowRangedAttack);


                list.CheckboxLabeled("KFM_SettingsAllowKillSelfPawns".Translate(), ref allowKillSelfPawns);

                //Allow packs to target all types of buildings
                list.CheckboxLabeled("KFM_SettingsAllowTargetAllBuildings".Translate(), ref allowTargetAllBuildings);

                //Attack enemies to the death
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref attackUntilDeath);

                //Allow learning of 'Hunt' to all animals
                list.CheckboxLabeled("KFM_SettingsAllowAllToKill".Translate(), ref allowAllToKill);
                Utils.setAllowAllToKillState();

                //Disabled bond animals
                list.CheckboxLabeled("KFM_SettingsDisableKillerBond".Translate(), ref disableKillerBond);

                //Percentage of members of a nearby pack to avoid point of achievement
                list.Label("KFM_SettingsPercentageMemberNearToAvoidRallyPoint".Translate((int)(percentageMemberNearToAvoidRallyPoint * 100)));
                percentageMemberNearToAvoidRallyPoint = list.Slider(percentageMemberNearToAvoidRallyPoint, 0.0f, 1.00f);
            }

            /****************************************************** Grouping mode ******************************************************/
            if (!sectionGroupingExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;

            if (list.ButtonText("KFM_SettingsGroupModeSection".Translate()))
                sectionGroupingExpanded = !sectionGroupingExpanded;
            GUI.color = Color.white;

            if (sectionGroupingExpanded)
            {
                //Only authorize the grouping mode of packs for those with a leader
                list.CheckboxLabeled("KFM_SettingsAllowGroupModeOnlyIfKing".Translate(), ref allowPackGroupModeOnlyIfKing);

                //Disable grouping mode
                list.CheckboxLabeled("KFM_SettingsDisallowPackGroupMode".Translate(), ref disallowPackGroupMode);
            }

            /****************************************************** Attack bonus ******************************************************/
            if (!sectionAttackBonusExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_SettingsBonusAttackSection".Translate()))
                sectionAttackBonusExpanded = !sectionAttackBonusExpanded;
            GUI.color = Color.white;

            if (sectionAttackBonusExpanded)
            {
                //Allow attack bonuses per pack
                list.CheckboxLabeled("KFM_SettingsAllowPackAttackBonus".Translate(), ref allowPackAttackBonus);

                //Allow to directly define the kings of a pack
                list.CheckboxLabeled("KFM_SettingsAllowManualKingSet".Translate(), ref allowManualKingSet);

                //Duration of validity of attack bonuses
                list.Label("KFM_SettingsBonusAttackDuration".Translate(Utils.TranslateTicksToTextIRLSeconds(bonusAttackDurationHour * 2500)));
                bonusAttackDurationHour = (int)list.Slider(bonusAttackDurationHour, 1, 18);

                //Warrior attack bonus, number of enemies to kill before becoming a warrior
                list.Label("KFM_SettingsBonusAttackWarriorNbToKill".Translate((int)warriorNbToKill));
                warriorNbToKill = (int)list.Slider(warriorNbToKill, 1, 150);

                //Attack bonus per enemies slain
                list.Label("KFM_SettingsBonusAttackByEnemyKilled".Translate((int)(bonusAttackByEnemyKilled * 100)));
                bonusAttackByEnemyKilled = list.Slider(bonusAttackByEnemyKilled, 0.01f, 0.20f);

                //Kings minimum time before new election
                list.Label("KFM_SettingsBonusKingElectionMinHour".Translate(Utils.TranslateTicksToTextIRLSeconds(kingElectionMinHour * 2500)));
                kingElectionMinHour = (int)list.Slider(kingElectionMinHour, 1, 720);

                if (kingElectionMaxHour < kingElectionMinHour)
                {
                    kingElectionMaxHour = kingElectionMinHour + 1;
                }

                //Kings maximum delay before new election
                list.Label("KFM_SettingsBonusKingElectionMaxHour".Translate(Utils.TranslateTicksToTextIRLSeconds(kingElectionMaxHour * 2500)));
                kingElectionMaxHour = (int)list.Slider(kingElectionMaxHour, kingElectionMinHour, 740);

                //Additional time in the event of a new election following the death of the current kings
                list.Label("KFM_SettingsBonusKingElectionPenalityCausedByKingDeath".Translate(Utils.TranslateTicksToTextIRLSeconds(kingElectionHourPenalityCausedByKingDeath * 2500)));
                kingElectionHourPenalityCausedByKingDeath = (int)list.Slider(kingElectionHourPenalityCausedByKingDeath, 12, 740);

                //Condition of definition of a pack leader
                list.Label("KFM_SettingsBonusKingElectionMinMembers".Translate(kingElectionMinMembers));
                kingElectionMinMembers = (int)list.Slider(kingElectionMinMembers, 3, 30);

                //Time before auto-cancellation of animal grouping mode
                list.Label("KFM_SettingsHoursBeforeAutoDisableGroupMode".Translate(Utils.TranslateTicksToTextIRLSeconds(hoursBeforeAutoDisableGroupMode * 2500)));
                hoursBeforeAutoDisableGroupMode = (int)list.Slider(hoursBeforeAutoDisableGroupMode, 2, 24);

                //Kings attack bonus
                list.Label("KFM_SettingsBonusAttackOfKings".Translate((int)(kingAttackBonus * 100)));
                kingAttackBonus = list.Slider(kingAttackBonus, 0.01f, 1.5f);

                //Warrior attack bonus
                list.Label("KFM_SettingsBonusAttackOfWarriors".Translate((int)(warriorAttackBonus * 100)));
                warriorAttackBonus = list.Slider(warriorAttackBonus, 0.01f, 1.0f);
            }

            //*********************************************  List of parameters by pack *************************************

            //Black
            if (!sectionBlackExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorblack".Translate().CapitalizeFirst()))
                sectionBlackExpanded = !sectionBlackExpanded;
            GUI.color = Color.white;
            if (sectionBlackExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref blackSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref blackManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref blackAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref blackAttackUntilDeath);
            }

            if (!sectionBlueExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorblue".Translate().CapitalizeFirst()))
                sectionBlueExpanded = !sectionBlueExpanded;
            GUI.color = Color.white;
            if (sectionBlueExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref blueSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref blueManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref blueAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref blueAttackUntilDeath);
            }

            if (!sectionGrayExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorgray".Translate().CapitalizeFirst()))
                sectionGrayExpanded = !sectionGrayExpanded;
            GUI.color = Color.white;
            if (sectionGrayExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref graySupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref grayManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref grayAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref grayAttackUntilDeath);
            }

            if (!sectionGreenExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorgreen".Translate().CapitalizeFirst()))
                sectionGreenExpanded = !sectionGreenExpanded;
            GUI.color = Color.white;
            if (sectionGreenExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref greenSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref greenManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref greenAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref greenAttackUntilDeath);
            }

            if (!sectionOrangeExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColororange".Translate().CapitalizeFirst()))
                sectionOrangeExpanded = !sectionOrangeExpanded;
            GUI.color = Color.white;
            if (sectionOrangeExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref orangeSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref orangeManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref orangeAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref orangeAttackUntilDeath);
            }

            if (!sectionPinkExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorpink".Translate().CapitalizeFirst()))
                sectionPinkExpanded = !sectionPinkExpanded;
            GUI.color = Color.white;
            if (sectionPinkExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref pinkSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref pinkManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref pinkAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref pinkAttackUntilDeath);
            }

            if (!sectionPurpleExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorpurple".Translate().CapitalizeFirst()))
                sectionPurpleExpanded = !sectionPurpleExpanded;
            GUI.color = Color.white;
            if (sectionPurpleExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref purpleSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref purpleManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref purpleAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref purpleAttackUntilDeath);
            }

            if (!sectionRedExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorred".Translate().CapitalizeFirst()))
                sectionRedExpanded = !sectionRedExpanded;
            GUI.color = Color.white;
            if (sectionRedExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref redSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref redManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref redAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref redAttackUntilDeath);
            }

            if (!sectionWhiteExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColorwhite".Translate().CapitalizeFirst()))
                sectionWhiteExpanded = !sectionWhiteExpanded;
            GUI.color = Color.white;
            if (sectionWhiteExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref whiteSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref whiteManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref whiteAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref whiteAttackUntilDeath);
            }

            if (!sectionYellowExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_PackColoryellow".Translate().CapitalizeFirst()))
                sectionYellowExpanded = !sectionYellowExpanded;
            GUI.color = Color.white;
            if (sectionYellowExpanded)
            {
                list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref yellowSupMode);
                list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref yellowManualMode);
                list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref yellowAttackStrongerTarget);
                list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref yellowAttackUntilDeath);
            }

            if (!sectionTargetToIgnoreExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_SettingsIgnoredTargets".Translate()))
                sectionTargetToIgnoreExpanded = !sectionTargetToIgnoreExpanded;
            GUI.color = Color.white;

            if (sectionTargetToIgnoreExpanded)
            {
                list.Gap(10f);

                if (list.ButtonText("+"))
                    ignoredTargets.Add("");

                if (list.ButtonText("-"))
                {
                    if (ignoredTargets.Count != 0)
                        ignoredTargets.RemoveLast();
                }

                //nbField = (int)list.Slider(nbField, 1, 100);
                for (var i = 0; i != ignoredTargets.Count; i++)
                {
                    list.Label("KFM_SettingsIgnoredPreyListNumber".Translate(i));
                    ignoredTargets[i] = list.TextEntry(ignoredTargets[i]);
                    list.Gap(4f);
                }

                list.Gap(10f);
                if (list.ButtonText("KFM_SettingsResetignoredTargets".Translate()))
                    resetDefaultIgnoredTargets();
            }

            // Ranged attacking creature exception list
            if (!sectionForcedMeleeExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_SettingsIgnoredRangedAttack".Translate()))
                sectionForcedMeleeExpanded = !sectionForcedMeleeExpanded;
            GUI.color = Color.white;

            if (sectionForcedMeleeExpanded)
            {
                list.Gap(10f);

                if (list.ButtonText("+"))
                    ignoredRangedAttack.Add("");

                if (list.ButtonText("-"))
                {
                    if (ignoredRangedAttack.Count != 0)
                        ignoredRangedAttack.RemoveLast();
                }

                for (var i = 0; i != ignoredRangedAttack.Count; i++)
                {
                    list.Label("#" + i.ToString());
                    ignoredRangedAttack[i] = list.TextEntry(ignoredRangedAttack[i]);
                    list.Gap(4f);
                }
                list.Gap(10f);
                if (list.ButtonText("KFM_SettingsResetIgnoredRangedAttack".Translate()))
                    resetDefaultIgnoredRangedAttack();
            }


            // List of default buildings to attack
            if (!sectionBuildingAttackExpanded)
                GUI.color = Color.green;
            else
                GUI.color = Color.cyan;
            if (list.ButtonText("KFM_SettingsDefaultBuildingToAttack".Translate()))
                sectionBuildingAttackExpanded = !sectionBuildingAttackExpanded;
            GUI.color = Color.white;
            if (sectionBuildingAttackExpanded)
            {
                list.Gap(10f);

                if (list.ButtonText("+"))
                    defaultBuildingToAttack.Add("");

                if (list.ButtonText("-"))
                {
                    if (defaultBuildingToAttack.Count != 0)
                        defaultBuildingToAttack.RemoveLast();
                }

                for (var i = 0; i != defaultBuildingToAttack.Count; i++)
                {
                    list.Label("#" + i.ToString());
                    defaultBuildingToAttack[i] = list.TextEntry(defaultBuildingToAttack[i]);
                    list.Gap(4f);
                }

                list.Gap(10f);
                if (list.ButtonText("KFM_SettingsResetDefaultBuildingToAttack".Translate()))
                    resetDefaultBuildingToAttack();
            }

            list.End();
            Widgets.EndScrollView();
            //settings.Write();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<string>(ref ignoredTargets , "ignoredTargets", LookMode.Value);
            Scribe_Collections.Look<string>(ref ignoredRangedAttack, "ignoredRangedAttack", LookMode.Value);
            Scribe_Collections.Look<string>(ref defaultBuildingToAttack, "defaultBuildingToAttack", LookMode.Value);
            if(defaultBuildingToAttack == null)
            {
                defaultBuildingToAttack = new List<string>();
                resetDefaultBuildingToAttack();
            }
                
            Scribe_Values.Look<bool>(ref allowRangedAttack, "allowRangedAttack", true);
            Scribe_Values.Look<bool>(ref allowPackAttackBonus, "allowPackAttackBonus", true);
            Scribe_Values.Look<bool>(ref allowAllToKill, "allowAllToKill", false);
            Scribe_Values.Look<bool>(ref attackUntilDeath, "attackUntilDeath", false);
            Scribe_Values.Look<bool>(ref disableKillerBond, "disableKillerBond", true);
            
            Scribe_Values.Look<bool>(ref hideRallyPointWarning, "hideRallyPointWarning", false);
            Scribe_Values.Look<bool>(ref hidePackIcon, "hidePackIcon", false);
            Scribe_Values.Look<int>(ref bonusAttackDurationHour, "bonusAttackDurationHour", 2);
            Scribe_Values.Look<float>(ref bonusAttackByEnemyKilled, "bonusAttackByEnemyKilled", 0.05f);
            Scribe_Values.Look<int>(ref hoursBeforeAutoDisableGroupMode, "hoursBeforeAutoDisableGroupMode", 8);
            Scribe_Values.Look<int>(ref kingElectionMinHour, "kingElectionMinHour", 12);
            Scribe_Values.Look<int>(ref kingElectionMaxHour, "kingElectionMaxHour", 168);
            Scribe_Values.Look<int>(ref kingElectionMinMembers, "kingElectionMinMembers", 4);
            Scribe_Values.Look<bool>(ref disallowPackGroupMode, "disallowPackGroupMode", false);
            Scribe_Values.Look<int>(ref warriorNbToKill, "warriorNbToKill", 3);
            Scribe_Values.Look<float>(ref kingAttackBonus, "kingAttackBonus", 0.50f);
            Scribe_Values.Look<float>(ref warriorAttackBonus, "warriorAttackBonus", 0.25f);
            Scribe_Values.Look<bool>(ref allowPackGroupModeOnlyIfKing, "allowPackGroupModeOnlyIfKing", false);
            Scribe_Values.Look<bool>(ref allowKillSelfPawns, "allowKillSelfPawns", false);
            Scribe_Values.Look<bool>(ref allowTargetAllBuildings, "allowTargetAllBuildings", false);
            Scribe_Values.Look<int>(ref kingElectionHourPenalityCausedByKingDeath, "kingElectionHourPenalityCausedByKingDeath", 72);
            Scribe_Values.Look<bool>(ref preventFleeWhenHit, "preventFleeWhenHit", false);
            Scribe_Values.Look<float>(ref percentageMemberNearToAvoidRallyPoint, "percentageMemberNearToAvoidRallyPoint", 0.5f);
            Scribe_Values.Look<bool>(ref allowManualKingSet, "allowManualKingSet", false);

            //Packs
            Scribe_Values.Look<bool>(ref blackSupMode, "blackSupMode", true);
            Scribe_Values.Look<bool>(ref blueSupMode, "blueSupMode", true);
            Scribe_Values.Look<bool>(ref graySupMode, "graySupMode", true);
            Scribe_Values.Look<bool>(ref greenSupMode, "greenSupMode", true);
            Scribe_Values.Look<bool>(ref orangeSupMode, "orangeSupMode", true);
            Scribe_Values.Look<bool>(ref pinkSupMode, "pinkSupMode", true);
            Scribe_Values.Look<bool>(ref purpleSupMode, "purpleSupMode", true);
            Scribe_Values.Look<bool>(ref redSupMode, "redSupMode", true);
            Scribe_Values.Look<bool>(ref whiteSupMode, "whiteSupMode", true);
            Scribe_Values.Look<bool>(ref yellowSupMode, "yellowSupMode", true);

            Scribe_Values.Look<bool>(ref blackManualMode, "blackManualMode", false);
            Scribe_Values.Look<bool>(ref blueManualMode, "blueManualMode", false);
            Scribe_Values.Look<bool>(ref grayManualMode, "grayManualMode", false);
            Scribe_Values.Look<bool>(ref greenManualMode, "greenManualMode", false);
            Scribe_Values.Look<bool>(ref orangeManualMode, "orangeManualMode", false);
            Scribe_Values.Look<bool>(ref pinkManualMode, "pinkManualMode", false);
            Scribe_Values.Look<bool>(ref purpleManualMode, "purpleManualMode", false);
            Scribe_Values.Look<bool>(ref redManualMode, "redManualMode", false);
            Scribe_Values.Look<bool>(ref whiteManualMode, "whiteManualMode", false);
            Scribe_Values.Look<bool>(ref yellowManualMode, "yellowManualMode", false);


            Scribe_Values.Look<bool>(ref blackAttackStrongerTarget, "blackAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref blueAttackStrongerTarget, "blueAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref grayAttackStrongerTarget, "grayAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref greenAttackStrongerTarget, "greenAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref orangeAttackStrongerTarget, "orangeAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref pinkAttackStrongerTarget, "pinkAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref purpleAttackStrongerTarget, "purpleAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref redAttackStrongerTarget, "redAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref whiteAttackStrongerTarget, "whiteAttackStrongerTarget", false);
            Scribe_Values.Look<bool>(ref yellowAttackStrongerTarget, "yellowAttackStrongerTarget", false);


            Scribe_Values.Look<bool>(ref blackAttackUntilDeath, "blackAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref blueAttackUntilDeath, "blueAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref grayAttackUntilDeath, "grayAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref greenAttackUntilDeath, "greenAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref orangeAttackUntilDeath, "orangeAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref pinkAttackUntilDeath, "pinkAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref purpleAttackUntilDeath, "purpleAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref redAttackUntilDeath, "redAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref whiteAttackUntilDeath, "whiteAttackUntilDeath", false);
            Scribe_Values.Look<bool>(ref yellowAttackUntilDeath, "yellowAttackUntilDeath", false);





            if (ignoredTargets == null)
            {
                resetDefaultIgnoredTargets();
            }
            if (ignoredRangedAttack == null)
            {
                resetDefaultIgnoredRangedAttack();
            }
        }

        /*
         * Check if for the given pack the attack until death mode is activated
         */
        static public bool isAttackUntilDeathEnabled(Pawn pawn, string APID = "")
        {
            string PID = "";

            Comp_Killing ck = Utils.getCachedCKilling(pawn);
            if (APID == "" && (pawn == null || ck == null))
                return false;

            if (APID != "")
                PID = APID;
            else
                PID = ck.KFM_PID;

            bool ret = false;
            if (PID == Utils.PACK_BLACK && blackAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_BLUE && blueAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_RED && redAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_ORANGE && orangeAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_YELLOW && yellowAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_GREEN && greenAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_PINK && pinkAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_PURPLE && purpleAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_WHITE && whiteAttackUntilDeath)
                ret = true;
            else if (PID == Utils.PACK_GRAY && grayAttackUntilDeath)
                ret = true;

            return ret;
        }

        /*
         * Check if for the given pack the supervised mode is activated
         */
        static public bool isSupModeEnabled(string PID)
        {
            bool ret = false;
            if (PID == Utils.PACK_BLACK && blackSupMode)
                ret = true;
            else if (PID == Utils.PACK_BLUE && blueSupMode)
                ret = true;
            else if (PID == Utils.PACK_RED && redSupMode)
                ret = true;
            else if (PID == Utils.PACK_ORANGE && orangeSupMode)
                ret = true;
            else if (PID == Utils.PACK_YELLOW && yellowSupMode)
                ret = true;
            else if (PID == Utils.PACK_GREEN && greenSupMode)
                ret = true;
            else if (PID == Utils.PACK_PINK && pinkSupMode)
                ret = true;
            else if (PID == Utils.PACK_PURPLE && purpleSupMode)
                ret = true;
            else if (PID == Utils.PACK_WHITE && whiteSupMode)
                ret = true;
            else if (PID == Utils.PACK_GRAY && graySupMode)
                ret = true;

            return ret;
        }

        /*
         * Check if for the given pack the manual mode is activated
         */
        static public bool isManualModeEnabled(string PID)
        {
            bool ret = false;
            if (PID == Utils.PACK_BLACK && blackManualMode)
                ret = true;
            else if (PID == Utils.PACK_BLUE && blueManualMode)
                ret = true;
            else if (PID == Utils.PACK_RED && redManualMode)
                ret = true;
            else if (PID == Utils.PACK_ORANGE && orangeManualMode)
                ret = true;
            else if (PID == Utils.PACK_YELLOW && yellowManualMode)
                ret = true;
            else if (PID == Utils.PACK_GREEN && greenManualMode)
                ret = true;
            else if (PID == Utils.PACK_PINK && pinkManualMode)
                ret = true;
            else if (PID == Utils.PACK_PURPLE && purpleManualMode)
                ret = true;
            else if (PID == Utils.PACK_WHITE && whiteManualMode)
                ret = true;
            else if (PID == Utils.PACK_GRAY && grayManualMode)
                ret = true;

            return ret;
        }

        /*
        * Check if for the given pack it can attack stronger targets
        */
        static public bool isAllowAttackStrongerTargetEnabled(string PID)
        {
            bool ret = false;
            if (PID == Utils.PACK_BLACK && blackAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_BLUE && blueAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_RED && redAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_ORANGE && orangeAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_YELLOW && yellowAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_GREEN && greenAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_PINK && pinkAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_PURPLE && purpleAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_WHITE && whiteAttackStrongerTarget)
                ret = true;
            else if (PID == Utils.PACK_GRAY && grayAttackStrongerTarget)
                ret = true;

            return ret;
        }

        static private void resetDefaultIgnoredTargets()
        {
            ignoredTargets = new List<string>();
            ignoredTargets.Add("Boomrat");
            ignoredTargets.Add("Boomalope");
            ignoredTargets.Add("HPLovecraft_MistStalker");
            ignoredTargets.Add("HPLovecraft_MistStalkerTwo");
        }

        static private void resetDefaultIgnoredRangedAttack()
        {
            ignoredRangedAttack = new List<string>() { "Green_Dragon_Race", "Black_Dragon_Race",
            "Brown_Dragon_Race", "Gold_Dragon_Race", "Silver_Dragon_Race", "Red_Dragon_Race", "Blue_Dragon_Race", "White_Dragon_Race",
        "Purple_Dragon_Race", "Cryo_Dragon_Race", "Flamingo_Dragon_Race", "Rock_Dragon_Race", "Yellow_Dragon_Race", "True_Dragon_Race", "Royal_Dragon_Race"};
        }

        static private void resetDefaultBuildingToAttack()
        {
            defaultBuildingToAttack = new List<string>() { "CrashedPsychicEmanatorShipPart", "CrashedPoisonShipPart", "ROM_PitChthonian" };
        }
    }
}