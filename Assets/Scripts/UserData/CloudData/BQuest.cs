using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BQuest
    {
        public int index;
        public QuestType questType;
        public int goalCount;
        public GoodType rewardGood;
        public double rewardValue;
    }
}