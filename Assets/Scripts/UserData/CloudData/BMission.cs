using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BMission
    {
        public int index;
        public MissionType missionType;
        public TimeResetType resetType;
        public int goalCount;
        public GoodType[] rewardGoodTypes;
        public double[] rewardGoodValues;
    }
}