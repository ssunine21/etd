using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BShop
    {
        public int index;
        public ProductType productType;
        public string productId;
        public GoodType needGoodType;
        public double needPrice;
        public GoodType[] rewardGoodTypes;
        public double[] rewardValues;
        public int[] params0;
        public int maxPurchaseCount;
        public TimeResetType timeResetType;
        public int packageValue;
        public UnlockType unlockType;
        public int unlockValue;
        public int remainTime;
    }
}