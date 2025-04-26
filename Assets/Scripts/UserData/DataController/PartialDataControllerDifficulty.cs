using System;
using ETD.Scripts.UserData.CloudData;
using UnityEngine;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataDifficulty difficulty;
    }

    [Serializable]
    public class DataDifficulty
    {
        private BDifficulty[] BDatas => CloudData.CloudData.Instance.bDifficulties;

        public double GetEnemyHp(EnemyDifficultyInfo info, bool isBoss)
        {
            return isBoss
                ? GetBossHp(info.difficultyLevel, info.incrementDifficultyValue)
                : GetNormalEnemyHp(info.difficultyLevel, info.incrementDifficultyValue);
        }

        public double GetEnemyPower(EnemyDifficultyInfo info, bool isBoss)
        {
            return isBoss
                ? GetBossPower(info.difficultyLevel, info.incrementDifficultyValue)
                : GetNormalEnemyPower(info.difficultyLevel, info.incrementDifficultyValue);
        }
        
        private double GetNormalEnemyHp(int difficultyLevel, int incrementDifficultyValue)
        {
            difficultyLevel = Mathf.Clamp(difficultyLevel, 0, BDatas.Length - 1);
            return BDatas[difficultyLevel].baseHpValue +
                BDatas[difficultyLevel].incrementHpValue * incrementDifficultyValue;
        }

        private double GetNormalEnemyPower(int difficultyLevel, int incrementDifficultyValue)
        {
            difficultyLevel = Mathf.Clamp(difficultyLevel, 0, BDatas.Length - 1);
            return BDatas[difficultyLevel].basePowerValue +
                   BDatas[difficultyLevel].incrementPowerValue * incrementDifficultyValue;
        }
        
        private double GetBossHp(int difficultyLevel, int incrementDifficultyValue)
        {
            difficultyLevel = Mathf.Clamp(difficultyLevel, 0, BDatas.Length - 1);
            return GetNormalEnemyHp(difficultyLevel, incrementDifficultyValue)
                   * BDatas[difficultyLevel].bossIncreaseValue;
        }
        
        private double GetBossPower(int difficultyLevel, int incrementDifficultyValue)
        {
            difficultyLevel = Mathf.Clamp(difficultyLevel, 0, BDatas.Length - 1);
            return GetNormalEnemyPower(difficultyLevel, incrementDifficultyValue)
                   * BDatas[difficultyLevel].bossIncreaseValue;
        }
    }

    public class EnemyDifficultyInfo
    {
        public readonly int enemyCombinationIndex;
        public readonly int difficultyLevel;
        public readonly int incrementDifficultyValue;

        public EnemyDifficultyInfo()
        {
            
        }

        public EnemyDifficultyInfo(int enemyCombinationIndex, int difficultyLevel, int incrementDifficultyValue)
        {
            this.enemyCombinationIndex = enemyCombinationIndex;
            this.difficultyLevel = difficultyLevel;
            this.incrementDifficultyValue = incrementDifficultyValue;
        }
    }
}