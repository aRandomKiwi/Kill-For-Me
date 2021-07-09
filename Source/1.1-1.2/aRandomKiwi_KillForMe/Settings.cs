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

        //Parametres par meute
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

            /****************************************************** Paremetres généraux ******************************************************/
            list.GapLine();
            GUI.color = Color.green;
            list.Label("KFM_SettingsGeneralSection".Translate() + " :");
            GUI.color = Color.white;
            list.GapLine();


            //Masquer l'alerte de rally point
            list.CheckboxLabeled("KFM_HideRallyPointWarning".Translate(), ref hideRallyPointWarning);

            //Masquer l'icone de meute
            list.CheckboxLabeled("KFM_HidePackIcon".Translate(), ref hidePackIcon);

            //Prevenir les animaux de s'enfuir quand ils sont meurtris
            list.CheckboxLabeled("KFM_SettingsPreventFromFleeWhenHit".Translate(), ref preventFleeWhenHit);

            //autoriser les attaques à distance
            list.CheckboxLabeled("KFM_SettingsAllowRangedAttack".Translate(), ref allowRangedAttack);


            list.CheckboxLabeled("KFM_SettingsAllowKillSelfPawns".Translate(), ref allowKillSelfPawns);

            //Autoriser meutes à cibler tout type de batiments
            list.CheckboxLabeled("KFM_SettingsAllowTargetAllBuildings".Translate(), ref allowTargetAllBuildings);

            //Attaquer jusqu'à la mort les ennemies
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref attackUntilDeath);

            //Autoriser l'apprentissage de 'Hunt' à tout les animaux
            list.CheckboxLabeled("KFM_SettingsAllowAllToKill".Translate(), ref allowAllToKill);
            Utils.setAllowAllToKillState();

            //Désactivé les bond animals
            list.CheckboxLabeled("KFM_SettingsDisableKillerBond".Translate(), ref disableKillerBond);

            //Poucentage membres d'une meutes proches pour éviter point de raliement
            list.Label("KFM_SettingsPercentageMemberNearToAvoidRallyPoint".Translate((int)(percentageMemberNearToAvoidRallyPoint * 100)));
            percentageMemberNearToAvoidRallyPoint = list.Slider(percentageMemberNearToAvoidRallyPoint, 0.0f, 1.00f);

            list.Gap(10f);
            /****************************************************** Mode regroupement ******************************************************/
            list.GapLine();
            GUI.color = Color.green;
            list.Label("KFM_SettingsGroupModeSection".Translate() + " :");
            GUI.color = Color.white;
            list.GapLine();


            //Autoriser uniquement le mode regroupement des meutes pour celles disposant d'un chef
            list.CheckboxLabeled("KFM_SettingsAllowGroupModeOnlyIfKing".Translate(), ref allowPackGroupModeOnlyIfKing);

            //Désactiver mode regoupement
            list.CheckboxLabeled("KFM_SettingsDisallowPackGroupMode".Translate(), ref disallowPackGroupMode);

            list.Gap(10f);
            /****************************************************** Bonus d'attaque ******************************************************/
            list.GapLine();
            GUI.color = Color.green;
            list.Label("KFM_SettingsBonusAttackSection".Translate()+" :");
            GUI.color = Color.white;
            list.GapLine();


            //Autoriser les bonus d'attaque par meute
            list.CheckboxLabeled("KFM_SettingsAllowPackAttackBonus".Translate(), ref allowPackAttackBonus);

            //Autoriser définir directement le rois d'une meute
            list.CheckboxLabeled("KFM_SettingsAllowManualKingSet".Translate(), ref allowManualKingSet);

            //Durée de validitée des bonus d'attaque
            list.Label("KFM_SettingsBonusAttackDuration".Translate( Utils.TranslateTicksToTextIRLSeconds( bonusAttackDurationHour*2500) ));
            bonusAttackDurationHour = (int) list.Slider(bonusAttackDurationHour, 1, 18);

            //Bonus d'attaque guerrier, nb d'ennemis à tuer avant de devenir un guerrier
            list.Label("KFM_SettingsBonusAttackWarriorNbToKill".Translate( (int)warriorNbToKill));
            warriorNbToKill = (int) list.Slider(warriorNbToKill, 1, 150);

            //BOnus d'attaque par ennemis tué
            list.Label("KFM_SettingsBonusAttackByEnemyKilled".Translate( (int)(bonusAttackByEnemyKilled*100)));
            bonusAttackByEnemyKilled = list.Slider(bonusAttackByEnemyKilled, 0.01f, 0.20f);

            //Rois délais minimal avant nouvelle election
            list.Label("KFM_SettingsBonusKingElectionMinHour".Translate(Utils.TranslateTicksToTextIRLSeconds(kingElectionMinHour * 2500)));
            kingElectionMinHour = (int) list.Slider(kingElectionMinHour, 1, 720);

            if (kingElectionMaxHour < kingElectionMinHour)
            {
                kingElectionMaxHour = kingElectionMinHour + 1;
            }

            //Rois délais maximal avant nouvelle election
            list.Label("KFM_SettingsBonusKingElectionMaxHour".Translate(Utils.TranslateTicksToTextIRLSeconds(kingElectionMaxHour * 2500)));
            kingElectionMaxHour = (int) list.Slider(kingElectionMaxHour, kingElectionMinHour, 740);

            //Temps additionnel en cas de nouvelle election suite au décés du rois actuel
            list.Label("KFM_SettingsBonusKingElectionPenalityCausedByKingDeath".Translate(Utils.TranslateTicksToTextIRLSeconds(kingElectionHourPenalityCausedByKingDeath * 2500)));
            kingElectionHourPenalityCausedByKingDeath = (int)list.Slider(kingElectionHourPenalityCausedByKingDeath, 12, 740);

            //Condition de définition d'un chef de meute
            list.Label("KFM_SettingsBonusKingElectionMinMembers".Translate(kingElectionMinMembers));
            kingElectionMinMembers = (int)list.Slider(kingElectionMinMembers, 3, 30);

            //Délais avant auto annulation du mode regroupement des animaux
            list.Label("KFM_SettingsHoursBeforeAutoDisableGroupMode".Translate(Utils.TranslateTicksToTextIRLSeconds(hoursBeforeAutoDisableGroupMode * 2500)));
            hoursBeforeAutoDisableGroupMode = (int) list.Slider(hoursBeforeAutoDisableGroupMode, 2, 24);

            //Bonus d'attaque des rois
            list.Label("KFM_SettingsBonusAttackOfKings".Translate((int)( kingAttackBonus * 100)));
            kingAttackBonus = list.Slider(kingAttackBonus, 0.01f, 1.5f);

            //Bonus d'attaque des warriors
            list.Label("KFM_SettingsBonusAttackOfWarriors".Translate((int)(warriorAttackBonus * 100)));
            warriorAttackBonus = list.Slider(warriorAttackBonus, 0.01f, 1.0f);

            list.Gap(10f);
            //*********************************************  Liste des parametres par meute *************************************
            list.GapLine();
            //Black
            GUI.color = Color.green;
            list.Label("KFM_PackColorblack".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref blackSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref blackManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref blackAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref blackAttackUntilDeath);

            list.GapLine();
            //Blue
            GUI.color = Color.green;
            list.Label("KFM_PackColorblue".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref blueSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref blueManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref blueAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref blueAttackUntilDeath);

            list.GapLine();
            //Gray
            GUI.color = Color.green;
            list.Label("KFM_PackColorgray".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref graySupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref grayManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref grayAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref grayAttackUntilDeath);

            list.GapLine();
            //Green
            GUI.color = Color.green;
            list.Label("KFM_PackColorgreen".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref greenSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref greenManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref greenAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref greenAttackUntilDeath);

            list.GapLine();
            //Orange
            GUI.color = Color.green;
            list.Label("KFM_PackColororange".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref orangeSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref orangeManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref orangeAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref orangeAttackUntilDeath);

            list.GapLine();
            //pink
            GUI.color = Color.green;
            list.Label("KFM_PackColorpink".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref pinkSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref pinkManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref pinkAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref pinkAttackUntilDeath);

            list.GapLine();
            //purple
            GUI.color = Color.green;
            list.Label("KFM_PackColorpurple".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref purpleSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref purpleManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref purpleAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref purpleAttackUntilDeath);

            list.GapLine();
            //red
            GUI.color = Color.green;
            list.Label("KFM_PackColorred".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref redSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref redManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref redAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref redAttackUntilDeath);

            list.GapLine();
            //white
            GUI.color = Color.green;
            list.Label("KFM_PackColorwhite".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref whiteSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref whiteManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref whiteAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref whiteAttackUntilDeath);

            list.GapLine();
            //yellow
            GUI.color = Color.green;
            list.Label("KFM_PackColoryellow".Translate().CapitalizeFirst() + " :");
            GUI.color = Color.white;
            list.GapLine();
            list.CheckboxLabeled("KFM_SettingsSupMode".Translate(), ref yellowSupMode);
            list.CheckboxLabeled("KFM_SettingsManualMode".Translate(), ref yellowManualMode);
            list.CheckboxLabeled("KFM_SettingsAllowPackToAttackStrongerTarget".Translate(), ref yellowAttackStrongerTarget);
            list.CheckboxLabeled("KFM_SettingsAttackUntilDeath".Translate(), ref yellowAttackUntilDeath);

            list.GapLine();
            list.Gap(10f);

            GUI.color = Color.green;
            list.Label("KFM_SettingsIgnoredTargets".Translate() + " :");
            GUI.color = Color.white;
            list.Gap(10f);

            if (list.ButtonText("+"))
                ignoredTargets.Add("");

            if (list.ButtonText("-"))
            {
                if(ignoredTargets.Count != 0)
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


            // Liste d'exception des créatures attaquant à distance
            list.Gap(25f);
            GUI.color = Color.green;
            list.Label("KFM_SettingsIgnoredRangedAttack".Translate());
            GUI.color = Color.white;
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
                list.Label("#"+i.ToString());
                ignoredRangedAttack[i] = list.TextEntry(ignoredRangedAttack[i]);
                list.Gap(4f);
            }

            list.Gap(10f);
            if (list.ButtonText("KFM_SettingsResetIgnoredRangedAttack".Translate()))
                resetDefaultIgnoredRangedAttack();






            // Liste des abtiments par defaut a attaquer
            list.Gap(25f);
            GUI.color = Color.green;
            list.Label("KFM_SettingsDefaultBuildingToAttack".Translate());
            GUI.color = Color.white;
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

            //Meutes
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
         * Check si pour la meute donné le mode d'attaque jusqua la mort est activé
         */
        static public bool isAttackUntilDeathEnabled(Pawn pawn, string APID = "")
        {
            string PID = "";

            if (APID == "" && (pawn == null || pawn.TryGetComp<Comp_Killing>() == null))
                return false;

            if (APID != "")
                PID = APID;
            else
                PID = pawn.TryGetComp<Comp_Killing>().KFM_PID;

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
         * Check si pour la meute donné le mode supervisé est activé
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
         * Check si pour la meute donné le mode manual est activé
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
        * Check si pour la meute donné elle peut attaquer des cibles plus fortes
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