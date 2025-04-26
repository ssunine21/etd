using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BStage
    {
        public int index;
        public int level;
        public int enemyCombinationIndex;
        public int difficultyLevel;
        public int incrementDifficultyValue;
        public int bossEnemyCombination;
        public GoodType[] rewardTypes;
        public double[] rewardValues;
        public LocalizedTextType[] nameLocalizeTypes;
    }
}