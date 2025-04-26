using System;
using UnityEngine.Serialization;

namespace ETD.Scripts.UserData.CloudData
{
    [Serializable]
    public class BInfo
    {
        public int labCount;
        public int toStorageDelayTime;
        public float plunderPercentage;
        public float protectedPercentage;
        public int maxRaidTicket;
        public int nextRaidTicketTimeInSeconds;
        public int protectionDurationInHour;
        public int guildCreationCost;
        public int nextGuildRaidTicketTimeInSeconds;
        public int maxGuildRaidTicket;
        public int guildRaidBoxLevelStep;
        public int guildGiftBoxPointWithRaid;
        public int guildBossSkilMultiple;
    }
}