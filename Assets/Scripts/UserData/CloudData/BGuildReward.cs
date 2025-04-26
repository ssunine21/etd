using System;
using ETD.Scripts.Common;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BGuildReward
    {
        public int index;
        public GuildRewardType guildRewardType;
        public int step;
        public int exp;
        public GoodType[] rewardTypes;
        public double[] rewardValues;
        public int[] rewardParams;
        public GoodType needGoodType;
        public int needValue;
        public LocalizedTextType localizeTextType;
        public int maxPurchaseCount;
        public TimeResetType timeResetType;
    }
}