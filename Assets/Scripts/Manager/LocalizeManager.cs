using System;
using System.Collections.Generic;
using System.Text;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;

namespace ETD.Scripts.Manager
{
    public static class LocalizeManager
    {
        private static Dictionary<LocalizedTextType, string[]> LocalizedTextDic
        {
            get
            {
                if (_localizedTextDic != null) return _localizedTextDic;
                _localizedTextDic = new Dictionary<LocalizedTextType, string[]>();
                foreach (var value in CloudData.Instance.bLocalizedTexts)
                {
                    _localizedTextDic.TryAdd(value.localizedTextType, value.countries);
                }

                return _localizedTextDic;
            }
        }

        private static Dictionary<LocalizedTextType, string[]> _localizedTextDic;

        private static string GetLocalizedText(LocalizedTextType key)
        {
            try
            {
                return LocalizedTextDic.ContainsKey(key)
                    ? LocalizedTextDic[key][GameManager.systemLanguageNumber]
                    : string.Empty;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        public static string GetText(AttributeType type)
        {
            return type switch
            {
                AttributeType.Atk => GetText(LocalizedTextType.AttributeType_Atk),
                AttributeType.FAtk => GetText(LocalizedTextType.AttributeType_FAtk),
                AttributeType.IAtk => GetText(LocalizedTextType.AttributeType_IAtk),
                AttributeType.WAtk => GetText(LocalizedTextType.AttributeType_WAtk),
                AttributeType.LgAtk => GetText(LocalizedTextType.AttributeType_LgAtk),
                AttributeType.LAtk => GetText(LocalizedTextType.AttributeType_LAtk),
                AttributeType.DAtk => GetText(LocalizedTextType.AttributeType_DAtk),
                AttributeType.AddAtk => GetText(LocalizedTextType.AttributeType_AddAtk),
                AttributeType.AddFAtk => GetText(LocalizedTextType.AttributeType_AddFAtk),
                AttributeType.AddIAtk => GetText(LocalizedTextType.AttributeType_AddIAtk),
                AttributeType.AddWAtk => GetText(LocalizedTextType.AttributeType_AddWAtk),
                AttributeType.AddLgAtk => GetText(LocalizedTextType.AttributeType_AddLgAtk),
                AttributeType.AddLAtk => GetText(LocalizedTextType.AttributeType_AddLAtk),
                AttributeType.AddDAtk => GetText(LocalizedTextType.AttributeType_AddDAtk),
                AttributeType.Projectile => GetText(LocalizedTextType.AttributeType_Projectile),
                AttributeType.Expansion => GetText(LocalizedTextType.AttributeType_Expansion),
                AttributeType.Chain => GetText(LocalizedTextType.AttributeType_Chain),
                AttributeType.Amplify => GetText(LocalizedTextType.AttributeType_Amplify),
                AttributeType.AttackSpeed => GetText(LocalizedTextType.AttributeType_AttackSpeed),
                AttributeType.Duration => GetText(LocalizedTextType.AttributeType_Duration),
                AttributeType.RandomTag => GetText(LocalizedTextType.AttributeType_RandomTag),
                AttributeType.CriticalRate => GetText(LocalizedTextType.AttributeType_CriticalRate),
                AttributeType.CriticalDamage => GetText(LocalizedTextType.AttributeType_CriticalDamage),
                AttributeType.AddMaxHp => GetText(LocalizedTextType.AttributeType_AddMaxHp),
                AttributeType.AddGoldGain => GetText(LocalizedTextType.AttributeType_AddGoldGain),
                AttributeType.GameSpeed => GetText(LocalizedTextType.AttributeType_GameSpeed),
                _ => ""
            };
        }

        public static string GetText(TagType type)
        {
            return type switch
            {
                TagType.Projectile => GetText(LocalizedTextType.TagType_Projectile),
                TagType.Expansion => GetText(LocalizedTextType.TagType_Expansion),
                TagType.Chain => GetText(LocalizedTextType.TagType_Chain),
                TagType.Duration => GetText(LocalizedTextType.TagType_Duration),
                TagType.Fire => GetText(LocalizedTextType.TagType_Fire),
                TagType.Water => GetText(LocalizedTextType.TagType_Water),
                TagType.Wind => GetText(LocalizedTextType.TagType_Wind),
                TagType.Lighting => GetText(LocalizedTextType.TagType_Lighting),
                TagType.Light => GetText(LocalizedTextType.TagType_Light),
                TagType.Darkness => GetText(LocalizedTextType.TagType_Darkness),
                _ => ""
            };
        }
        
        public static string GetText(RuneType type)
        {
            return type switch
            {
                RuneType.Projectile => GetText(LocalizedTextType.RuneType_Projectile),
                RuneType.Expansion => GetText(LocalizedTextType.RuneType_Expansion),
                RuneType.Chain => GetText(LocalizedTextType.RuneType_Chain),
                RuneType.Amplify => GetText(LocalizedTextType.RuneType_Amplify),
                RuneType.AttackSpeed => GetText(LocalizedTextType.RuneType_AttackSpeed),
                RuneType.Duration => GetText(LocalizedTextType.RuneType_Duration),
                RuneType.RandomTag => GetText(LocalizedTextType.RuneType_RandomTag),
                _ => ""
            };
        }
        
        public static string GetText(QuestType type)
        {
            return type switch
            {
                QuestType.KillEnemy => GetText(LocalizedTextType.Quest_KillEnemy),
                QuestType.SummonElemental => GetText(LocalizedTextType.Quest_SummonElemental),
                QuestType.SummonRune => GetText(LocalizedTextType.Quest_SummonRune),
                QuestType.ClearNormalStage => GetText(LocalizedTextType.Quest_ClearNormalStage),
                QuestType.ClearGoldDungeon => GetText(LocalizedTextType.Quest_ClearGoldDungeon),
                QuestType.ClearDiaDungeon => GetText(LocalizedTextType.Quest_ClearDiaDungeon),
                QuestType.ClearEnhanceDungeon => GetText(LocalizedTextType.Quest_ClearEnhanceDungeon),
                QuestType.UpgradeElementalSlot => GetText(LocalizedTextType.Quest_UpgradeElementalSlot),
                QuestType.EquipElemental => GetText(LocalizedTextType.Quest_EquipElemental),
                QuestType.UpgradeRuneSlot => GetText(LocalizedTextType.Quest_UpgradeRuneSlot),
                QuestType.EquipRune => GetText(LocalizedTextType.Quest_EquipRune),
                QuestType.UpgradeProjector => GetText(LocalizedTextType.Quest_UpgradeProjector),
                QuestType.EquipElementalWithProjector => GetText(LocalizedTextType.Quest_EquipElementalWithProjector),
                QuestType.EquipRuneWithProjector => GetText(LocalizedTextType.Quest_EquipRuneWithProjector),
                QuestType.UpgradeATK => GetText(LocalizedTextType.Quest_UpgradeATK),
                QuestType.UpgradeMaxHp => GetText(LocalizedTextType.Quest_UpgradeMaxHp),
                QuestType.UpgradeIncreaseHealAmount => GetText(LocalizedTextType.Quest_UpgradeIncreaseHealAmount),
                QuestType.EnhanceElemental => GetText(LocalizedTextType.Quest_EnhanceElemental),
                QuestType.EnhanceRune => GetText(LocalizedTextType.Quest_EnhanceRune),
                QuestType.DisassembleRune => GetText(LocalizedTextType.Quest_DisassembleRune),
                QuestType.ClickGameSpeed => GetText(LocalizedTextType.Quest_GameSpeed),
                QuestType.ClickGameSpeedBuff => GetText(LocalizedTextType.Quest_GameSpeedBuff),
                QuestType.AnyResearchCompleate => GetText(LocalizedTextType.Quest_AnyResearchCompleate),
                _ => ""
            };
        }

        public static string GetDungeonTitle(StageType type)
        {
            return type switch
            {
                StageType.GoldDungeon => GetText(LocalizedTextType.Dungeon_GoldDungeon),
                StageType.DiaDungeon => GetText(LocalizedTextType.Dungeon_DiaDungeon),
                StageType.EnhanceDungeon => GetText(LocalizedTextType.Dungeon_EnhanceStoneDungeon),
                StageType.DarkDiaDungeon => GetText(LocalizedTextType.Dungeon_DarkDiaDungeon),
                StageType.GuildRaidDungeon => GetText(LocalizedTextType.Dungeon_GuildRaidDungeon),
                _ => ""
            };
        }
        
        public static string GetDungeonDescription(StageType type)
        {
            return type switch
            {
                StageType.GoldDungeon => GetText(LocalizedTextType.Dungeon_GoldDungeonDescription),
                StageType.DiaDungeon => GetText(LocalizedTextType.Dungeon_DiaDungeonDescription),
                StageType.EnhanceDungeon => GetText(LocalizedTextType.Dungeon_EnhanceStoneDungeonDescription),
                StageType.DarkDiaDungeon => GetText(LocalizedTextType.Dungeon_DarkDiaDungeonDescription),
                StageType.GuildRaidDungeon => GetText(LocalizedTextType.Dungeon_GuildRaidDungeonDescription),
                _ => ""
            };
        }

        public static string GetText(GoodType type, float param0 = 0)
        {
            return type switch
            {
                GoodType.VIP => GetText(LocalizedTextType.Good_VIP, param0),
                GoodType.IncreaseStageReward =>GetText(LocalizedTextType.Good_IncreaseStageReward, param0),
                GoodType.IncreaseGameSpeed =>GetText(LocalizedTextType.Good_IncreaseGameSpeed, param0),
                GoodType.IncreaseDungeonReward =>GetText(LocalizedTextType.Good_IncreaseDungeonReward, param0),
                GoodType.IncreaseLabSpeed =>GetText(LocalizedTextType.Good_IncreaseLabSpeed, param0),
                _ => ""
            };
        }

        public static string GetDungeonGoalText(StageType type)
        {
            return type switch
            {
                StageType.GoldDungeon => GetText(LocalizedTextType.Dungeon_GoldDungeonGoal),
                StageType.DiaDungeon => GetText(LocalizedTextType.Dungeon_DiaDungeonGoal),
                StageType.EnhanceDungeon => GetText(LocalizedTextType.Dungeon_EnhanceStoneDungeonGoal),
                StageType.DarkDiaDungeon => GetText(LocalizedTextType.Dungeon_DarkDiaDungeonGoal),
                StageType.GuildRaidDungeon => GetText(LocalizedTextType.Dungeon_GuildRaidDungeonGoal),
                _ => ""
            };
        }

        public static string GetDungeonGoalTextWithCount(StageType type, double param0)
        {
            return type switch
            {
                StageType.GoldDungeon => $"{GetText(LocalizedTextType.Dungeon_GoldDungeonGoal)} : {param0}",
                StageType.DiaDungeon => $"{GetText(LocalizedTextType.Dungeon_DiaDungeonGoal)} : {param0.ToDamage()}",
                StageType.EnhanceDungeon => $"{GetText(LocalizedTextType.Dungeon_EnhanceStoneDungeonGoal)} : {Mathf.Max(0, (int)param0 - 1)}",
                _ => ""
            };
        }

        public static string GetText(TimeResetType resetType, MissionType type, float param0 = 0, float param1 = 0)
        {
            var dailyString = resetType switch
            {
                TimeResetType.Daily => $"<color=yellow>[{GetText(LocalizedTextType.Mission_Daily)}]</color>",
                TimeResetType.Weekly => $"<color=orange>[{GetText(LocalizedTextType.Mission_Weekly)}]</color>",
                TimeResetType.Repeat => $"<color=green>[{GetText(LocalizedTextType.Mission_Repeat)}]</color>",
                _ => ""
            };
            
            var missionString = type switch
            {
                MissionType.DailyLogin => GetText(LocalizedTextType.Mission_DailyLogin, param0, param1),
                MissionType.KillEnemy => GetText(LocalizedTextType.Mission_KillEnemy, param0, param1),
                MissionType.ClearGoldDungeon => GetText(LocalizedTextType.Mission_ClearGoldDungeon, param0, param1),
                MissionType.ClearDiaDungeon => GetText(LocalizedTextType.Mission_ClearDiaDungeon, param0, param1),
                MissionType.ClearEnhanceDungeon => GetText(LocalizedTextType.Mission_ClearEnhanceDungeon, param0, param1),       
                MissionType.ClearNormalStage => GetText(LocalizedTextType.Mission_ClearNormalStage, param0, param1),
                MissionType.ClearAnyDungeon => GetText(LocalizedTextType.Mission_ClearAnyDungeon, param0, param1),
                MissionType.SummonElemental => GetText(LocalizedTextType.Mission_SummonElemental, param0, param1),
                MissionType.SummonRune => GetText(LocalizedTextType.Mission_SummonRune, param0, param1),
                MissionType.Enhance => GetText(LocalizedTextType.Mission_Enhance, param0, param1),
                MissionType.Upgrade => GetText(LocalizedTextType.Mission_Upgrade, param0, param1),
                _ => ""
            };
            var text = $"{dailyString}\n{missionString}";
            return text;
        }

        public static string GetText(UnlockType unlockType)
        {
            return unlockType switch
            {
                UnlockType.Research => GetText(LocalizedTextType.Research_Title),
                UnlockType.Raid => GetText(LocalizedTextType.PlunderBattle),
                UnlockType.Pass => GetText(LocalizedTextType.Pass_Title),
                UnlockType.GrowPass => GetText(LocalizedTextType.GrowPassTitle),
                UnlockType.Guild => GetText(LocalizedTextType.Guild_Title),
                _ => string.Empty
            };
        }

        public static string GetElementalDescriptionText(LocalizedTextType localizeType,
            float attackSpeed = 0, float duration = 0, float attackPerSecond = 0, float attackCount = 0, float coefficient = 0)
        {
            return localizeType switch
            {
                LocalizedTextType.Elemental_AFC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AFB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AFA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AFS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AFSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AIC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AIB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_AIA => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AIS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AISS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AWC => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AWB => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AWA => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AWS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_AWSS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ALgC => GetText(localizeType, attackSpeed, attackCount, coefficient),
                LocalizedTextType.Elemental_ALgB => GetText(localizeType, attackSpeed, attackCount, coefficient),
                LocalizedTextType.Elemental_ALgA => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ALgS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ALgSS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ALC => GetText(localizeType, attackSpeed, attackCount, coefficient),
                LocalizedTextType.Elemental_ALB => GetText(localizeType, attackSpeed, attackCount, coefficient),
                LocalizedTextType.Elemental_ALA => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ALS => GetText(localizeType, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ALSS => GetText(localizeType,  coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ADC => GetText(localizeType,  coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ADB => GetText(localizeType,  coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ADA => GetText(localizeType,  coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ADS => GetText(localizeType,  coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_ADSS => GetText(localizeType,  coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_LFC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LFB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LFA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LFS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LFSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LIC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LIB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LIA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LIS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LISS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LWC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LWB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LWA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LWS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LWSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLgC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLgB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLgA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLgS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLgSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LLSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LDC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LDB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LDA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LDS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_LDSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_PFC => GetText(localizeType, attackSpeed, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PFB => GetText(localizeType, attackSpeed, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PFA => GetText(localizeType, attackSpeed, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PFS => GetText(localizeType, attackSpeed, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PFSS => GetText(localizeType, attackSpeed, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PIC => GetText(localizeType, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PIB => GetText(localizeType, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PIA => GetText(localizeType, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PIS => GetText(localizeType, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PISS => GetText(localizeType, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PWC => GetText(localizeType, attackCount, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PWB => GetText(localizeType, attackCount, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PWA => GetText(localizeType, attackCount, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PWS => GetText(localizeType, attackCount, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PWSS => GetText(localizeType, attackCount, coefficient, attackPerSecond, duration),
                LocalizedTextType.Elemental_PLgC => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_PLgB => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_PLgA => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_PLgS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_PLgSS => GetText(localizeType, attackCount, coefficient),
                LocalizedTextType.Elemental_PLC => GetText(localizeType, coefficient),
                LocalizedTextType.Elemental_PLB => GetText(localizeType, coefficient),
                LocalizedTextType.Elemental_PLA => GetText(localizeType, coefficient),
                LocalizedTextType.Elemental_PLS => GetText(localizeType, coefficient),
                LocalizedTextType.Elemental_PLSS => GetText(localizeType, coefficient),
                LocalizedTextType.Elemental_PDC => GetText(localizeType, attackCount, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PDB => GetText(localizeType, attackCount, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PDA => GetText(localizeType, attackCount, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PDS => GetText(localizeType, attackCount, coefficient, attackPerSecond),
                LocalizedTextType.Elemental_PDSS => GetText(localizeType, attackCount, coefficient, attackPerSecond),
                _ => "",
            };
        }

        public static string GetLocalizedTextConcat(LocalizedTextType[] types, string joinText = null)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < types.Length; ++i)
            {
                sb.Append(GetText(types[i]));
                if (!string.IsNullOrEmpty(joinText) && i < types.Length - 1) sb.Append(joinText);
            }

            return sb.ToString();
        }

        public static string GetText(LocalizedTextType type, float param0 = 0, float param1 = 0, float param2 = 0, float param3 = 0, float param4 = 0)
        {
            return string.Format(GetLocalizedText(type), param0, param1, param2, param3, param4);
        }
        
        public static string GetText(LocalizedTextType type, string param0, string param1 = "")
        {
            return string.Format(GetLocalizedText(type), param0, param1);
        }
    }
}