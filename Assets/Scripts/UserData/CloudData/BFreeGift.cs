using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BFreeGift
    {
        public int index;
        public GoodType rewardGoodTypes;
        public double rewardValues;
        public int param0;
        public int maxPurchaseCount;
        public FreeGiftType freeGiftType;
        public TimeResetType timeResetType;
    }
}