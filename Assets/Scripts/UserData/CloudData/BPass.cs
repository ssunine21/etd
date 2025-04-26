using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BPass
    {
        public int index;
        public GoodType nRewardGoodType;
        public double nRewardValue;
        public int nRewardParam0;
        public GoodType pRewardGoodType;
        public double pRewardValue;
        public int pRewardParam0;
        public int exp;
        public int[] increasePointAmount;
    }
}