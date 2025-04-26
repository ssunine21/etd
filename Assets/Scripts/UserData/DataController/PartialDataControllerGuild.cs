using System;
using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataGuild guild;
    }

    [Serializable]
    public class DataGuild
    {
        public bool hasGuild;
        public string nextGuildRaidTicketTimeToString;
        public List<string> getRaidBoxRewardTimeToStrings;
        public string raidClearTimeToString;
        public List<GradeType> raidBoxes;
        public string guildExitTimeToString;
        public double raidDamage;
        public int raidLevel;
        
        private Dictionary<int, BGuild> _cache = new();
        private BInfo BInfoData => CloudData.CloudData.Instance.bInfos[0];
        
        public void Init()
        {
            foreach (var bGuild in CloudData.CloudData.Instance.bGuilds)
            {
                _cache.TryAdd(bGuild.level, bGuild);
            }
        }
        
        public bool CanJoinGuildNow()
        {
            var exitTime = ServerTime.IsoStringToDateTime(guildExitTimeToString);
            #if IS_TEST
            exitTime = exitTime.AddSeconds(30);
            #elif IS_LIVE
            exitTime = exitTime.AddDays(1);
            #endif
            return exitTime <= ServerTime.Date;
        }

        public int GetGuildBossSkilMultiple()
        {
            return BInfoData.guildBossSkilMultiple;
        }

        public void SetGuildExitTime(DateTime time)
        {
            guildExitTimeToString = ServerTime.DateTimeToIsoString(time);
            DataController.Instance.LocalSave();
        }

        public int GetGuildRaidBoxLevelStep()
        {
            return BInfoData.guildRaidBoxLevelStep;
        }

        public void SetRaidBoxRewardTime(GradeType gradeType)
        {
            getRaidBoxRewardTimeToStrings ??= new List<string>();
            var count = getRaidBoxRewardTimeToStrings.Count;
            var gradeTypeToInt = (int)gradeType;
            
            for (var i = count; i <= gradeTypeToInt; ++i)
            {
                getRaidBoxRewardTimeToStrings.Add(ServerTime.DateTimeToIsoString(new DateTime()));
            }

            getRaidBoxRewardTimeToStrings[gradeTypeToInt] = ServerTime.DateTimeToIsoString(ServerTime.Date);
        }

        public string GetRaidBoxRewardTimeToString(GradeType gradeType)
        {
            getRaidBoxRewardTimeToStrings ??= new List<string>();
            var count = getRaidBoxRewardTimeToStrings.Count;
            var gradeTypeToInt = (int)gradeType;
            
            for (var i = count; i <= gradeTypeToInt; ++i)
            {
                getRaidBoxRewardTimeToStrings.Add(ServerTime.DateTimeToIsoString(new DateTime()));
            }
            
            return getRaidBoxRewardTimeToStrings[gradeTypeToInt];
        }

        public void SetRaidDamage(double damage)
        {
            raidDamage = damage;
        }

        public void SetRaidLevel(int level)
        {
            raidLevel = level;
        }

        public void SetGuildRaidData(List<ProbabilityItem> boxes)
        {
            if (boxes != null)
            {
                var clearRaidTime = ServerTime.IsoStringToDateTime(raidClearTimeToString);
                if (clearRaidTime.Date < ServerTime.Date.Date)
                    raidBoxes = boxes.Select(x => x.gradeType).ToList();
                else
                {
                    raidBoxes ??= new List<GradeType>();
                    raidBoxes.AddRange(boxes.Select(x => x.gradeType));
                }
            }
            
            raidClearTimeToString = ServerTime.DateTimeToIsoString(ServerTime.Date);
            DataController.Instance.LocalSave();
        }

        public int GetNeedStageLevel(float value)
        {
            var stageLevel = (int)(value * 7499);
            stageLevel -= stageLevel % 30;
            return stageLevel;
        }

        public int GetMaxMemeberCount(int level)
        {
            return _cache.TryGetValue(level, out var value) ? value.memberCount : 0;
        }

        public int GetMaxExp(int level)
        {
            return _cache.TryGetValue(level, out var value) ? value.exp : 0;
        }

        public int GetGuildCreationCost()
        {
            return CloudData.CloudData.Instance.bInfos[0].guildCreationCost;
        }
        
        public int GetNextGuildRaidTicketTimeInSeconds()
        {
            return BInfoData.nextGuildRaidTicketTimeInSeconds;
        }
        
        public int GetMaxGuildRaidTicketCount()
        {
            return BInfoData.maxGuildRaidTicket;
        }
        
        public void SetNextRaidTicketTimeToString(string dateTime)
        {
            nextGuildRaidTicketTimeToString = dateTime;
        }
    }
}