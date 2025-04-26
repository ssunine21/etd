using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public int maxTotalLevel;
        public DataStage stage;
    }

    [Serializable]
    public class DataStage
    {
        
        public Dictionary<GoodType, double> CurrStageEachEnemyRewardDictionary;
        public string MaxStageToString => GetStageLevelExpression(MaxTotalLevel);
        public string CurrStageLevelExpression => GetStageLevelExpression(currTotalLevel);
        public string CurrStageToFullSring => GetStageFullTitle(currTotalLevel);
        public int NextLevel => IsMaxStage ? currTotalLevel + 1 : currTotalLevel;
        public int CurrMainLevel => currTotalLevel / StageSpacing;
        public int MaxSubLevel => MaxTotalLevel % StageSpacing;
        public bool IsMaxStage => currTotalLevel == MaxTotalLevel;
        public bool IsMaxStageTheBossStage => MaxSubLevel == 29;
        public int StageSpacing => 30;
        public int currTotalLevel;
        public int MaxTotalLevel => DataController.Instance.maxTotalLevel;
        
        public BStage[] BDatas => CloudData.CloudData.Instance.bStages;
        public int BDataTotalStageLength => BDatas.Length * StageSpacing;
        private int DifficultyLevel => BDatas[CurrMainLevel].difficultyLevel;
        private int IncrementDifficultyValue => BDatas[CurrMainLevel].incrementDifficultyValue;

        public void Init()
        {
            StageManager.Instance.onBindStartStage += AsyncStageReward;
        }

        [CanBeNull]
        public BStage GetBData(int mainLevel)
        {
            return BDatas.Length <= mainLevel ? null : BDatas[mainLevel];
        }

        public EnemyDifficultyInfo GetEnemyDifficultyInfo(int level)
        {
            var info = new EnemyDifficultyInfo(GetEnemyCombinationIndex(level), DifficultyLevel, IncrementDifficultyValue);
            return info;
        }

        public int GetEnemyCombinationIndex(int level)
        {
            var normalizedLevel = Mathf.Clamp(level / StageSpacing, 0, BDatas.Length - 1);
            return level % StageSpacing == StageSpacing - 1
                ? BDatas[normalizedLevel].bossEnemyCombination
                : BDatas[normalizedLevel].enemyCombinationIndex;
        }

        public void SaveStageLevelData(int level)
        {
            currTotalLevel = Mathf.Clamp(level, 0, BDataTotalStageLength);
            if (currTotalLevel > MaxTotalLevel)
            {
                DataController.Instance.maxTotalLevel = currTotalLevel;
                FirebaseManager.LogEvent("stage", "clear_level", currTotalLevel);
            }
            DataController.Instance.LocalSave();
        }

        public string GetStageLevelExpression(int totalStageLevel)
        {
            return $"{(totalStageLevel / StageSpacing) + 1}-{(totalStageLevel % StageSpacing) + 1}";
        }

        public string GetStageTitle(int mainLevel)
        {
            var localizeTexts = BDatas[Mathf.Clamp(mainLevel, 0, BDatas.Length - 1)].nameLocalizeTypes;
            return LocalizeManager.GetText(localizeTexts[2]);
        }

        public string GetLevelAndStepText(int mainLevel)
        {
            var localizeTexts = BDatas[Mathf.Clamp(mainLevel, 0, BDatas.Length - 1)].nameLocalizeTypes;
            
            return $"{LocalizeManager.GetText(localizeTexts[0])}{LocalizeManager.GetText(localizeTexts[1])}</color>";
        }

        public string GetStageFullTitle(int totalStageLevel)
        {
            var mainLevel = totalStageLevel / StageSpacing;
            
            return $"{GetLevelAndStepText(mainLevel)} {GetStageTitle(mainLevel)} {GetStageLevelExpression(totalStageLevel)}";
        }
        
        private void AsyncStageReward(StageType stageType, int level)
        {
            if (stageType != StageType.Normal) return;
            CurrStageEachEnemyRewardDictionary ??= new Dictionary<GoodType, double>();
            CurrStageEachEnemyRewardDictionary.Clear();
            
            var normalizedLevel = Mathf.Clamp(level / StageSpacing, 0,BDatas.Length - 1);
            CurrStageEachEnemyRewardDictionary = BDatas[normalizedLevel].rewardTypes.ToDictionary(
                key => key,
                key => BDatas[normalizedLevel].rewardValues[
                           BDatas[normalizedLevel].rewardTypes.ToList().IndexOf(key)] /
                       DataController.Instance.enemyCombination.GetEnemyCount(
                           GetEnemyCombinationIndex(level)));
        }
    }
}