using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BGrowPass
    {
        public int index;
        public PassType passType;
        public int level;
        public int goal;
        public GoodType nRewardGoodType;
        public double nRewardValue;
        public int nRewardParam0;
        public GoodType pRewardGoodType;
        public double pRewardValue;
        public int pRewardParam0;
    }
}