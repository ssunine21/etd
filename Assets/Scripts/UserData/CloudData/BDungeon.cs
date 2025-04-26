using System;
using ETD.Scripts.Common;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BDungeon
    {
        public int index;
        public StageType stageType;
        public int level;
        public int enemyCombinationIndex;
        public int difficultyLevel;
        public int incrementDifficultyValue;
        public GoodType[] rewardTypes;
        public double[] rewardValues;
        public float timeLimit;
    }
}