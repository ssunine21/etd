using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.CloudData;
using ETD.Scripts.UserData.DataController;
using LitJson;
using UnityEngine;

namespace ETD.Scripts.Common
{
    public class GuildInfo
    {
        public GuildInfo(){}

        public GuildInfo(JsonData guildInfoJson)
        {
            try
            {
                InDate = guildInfoJson[BackendGuildManager.InDateKey].ToString();
                MemberCount = int.Parse(guildInfoJson[BackendGuildManager.MemberCountKey].ToString());
                GuildName = guildInfoJson[BackendGuildManager.GuildNameKey].ToString();
                MarkBackgroundIndex = int.Parse(guildInfoJson[BackendGuildManager.MarkBackgroundImageKey].ToString());
                MarkMainIndex = int.Parse(guildInfoJson[BackendGuildManager.MarkMainImageKey].ToString());
                MarkSubIndex = int.Parse(guildInfoJson[BackendGuildManager.MarkSubImageKey].ToString());
                NeededStage = int.Parse(guildInfoJson[BackendGuildManager.NeededStageKey].ToString());
                Description = guildInfoJson[BackendGuildManager.DescriptionKey].ToString();
                MasterInDate = guildInfoJson[BackendGuildManager.MasterInDate].ToString();

                if (guildInfoJson.ContainsKey(BackendGuildManager.ImmediateRegistrationKey))
                    ImmediateRegistration = guildInfoJson[BackendGuildManager.ImmediateRegistrationKey].ToString() == "True";
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
            }
        }

        public int Level
        {
            get
            {
                var level = 1;
                var exp = TotalExp;
                foreach (var cache in CloudData.Instance.bGuilds.TakeWhile(cache => cache.exp <= exp))
                {
                    exp -= cache.exp;
                    ++level;
                }
                return Mathf.Min(level, CloudData.Instance.bGuilds.Length);
            }
        }
        
        public int GiftBoxStep
        {
            get
            {
                var level = 0;
                var exp = TotalGiftBoxExp;
                var caches = DataController.Instance.guildReward.GetCaches(GuildRewardType.GiftBox);
                foreach (var cache in caches.TakeWhile(cache => cache.exp <= exp))
                {
                    exp -= cache.exp;
                    ++level;
                }

                return Mathf.Min(level, caches.Count - 1);
            }
        }
        public int CurrExp
        {
            get
            {
                var currExp = TotalExp;
                var caches =  CloudData.Instance.bGuilds;
                var maxExp = 0;
                foreach (var cache in caches)
                {
                    if (cache.exp > currExp) return currExp;
                    currExp -= cache.exp;
                    maxExp = Mathf.Max(cache.exp, maxExp);
                }

                return Mathf.Min(currExp, maxExp);
            }
        }
        public int CurrGiftBoxExp
        {
            get
            {
                var currExp = TotalGiftBoxExp;
                var caches =  DataController.Instance.guildReward.GetCaches(GuildRewardType.GiftBox);
                var maxExp = 0;
                foreach (var cache in caches)
                {
                    if (cache.exp > currExp) return currExp;
                    currExp -= cache.exp;
                    maxExp = Mathf.Max(cache.exp, maxExp);
                }

                return Mathf.Min(currExp, maxExp);
            }
        }
        public int CurrGiftBoxPoint => Mathf.Max(0, TotalGiftBoxPoint - (int)DataController.Instance.good.GetValue(GoodType.GuildGiftBoxPoint));
        public int TotalExp => Goods.GetValueOrDefault(GoodType.GuildExp, 0);
        public int TotalGiftBoxExp => Goods.GetValueOrDefault(GoodType.GuildGiftBoxExp, 0);
        public int TotalGiftBoxPoint => Goods.GetValueOrDefault(GoodType.GuildGiftBoxPoint, 0);
        
        public double TotalCombat
        {
            get
            {
                if (_totalCombat <= 0)
                    _totalCombat = MemberItems.Sum(member => member.Combat);

                return _totalCombat;
            }
        }

        public Dictionary<GradeType, int> RaidBoxes
        {
            get
            {
                var result = new Dictionary<GradeType, int>();
                foreach (var memberItem in MemberItems)
                {
                    var clearTime = ServerTime.IsoStringToDateTime(memberItem.RaidClearTimeToString);
                    if (clearTime.Date != ServerTime.Date.Date) continue; 
                    
                    foreach (var boxGradeType in memberItem.RaidBoxes)
                    {
                        if (string.CompareOrdinal(memberItem.RaidClearTimeToString, DataController.Instance.guild.GetRaidBoxRewardTimeToString(boxGradeType)) <= 0) continue;
                        result[boxGradeType] = result.GetValueOrDefault(boxGradeType, 0) + 1;
                    }
                }

                return result;
            }
        }

        public List<string> RaidLogs
        {
            get
            {
                if (_raidLogs != null) return _raidLogs;

                _raidLogs = new List<string>();
                foreach (var memberItem in MemberItems)
                {
                    var clearTime = ServerTime.IsoStringToDateTime(memberItem.RaidClearTimeToString);
                    if (clearTime.Date == ServerTime.Date.Date)
                    {
                        foreach (var boxText in memberItem.RaidBoxes.Select(box => box switch
                                 {
                                     GradeType.C => LocalizedTextType.Guild_NormalRaidBox,
                                     GradeType.B => LocalizedTextType.Guild_RareRaidBox,
                                     GradeType.A => LocalizedTextType.Guild_UniqueRaidBox,
                                     _ => LocalizedTextType.Guild_NormalRaidBox,
                                 }).Select(boxTextType => LocalizeManager.GetText(boxTextType)))
                        {
                            _raidLogs.Add(LocalizeManager.GetText(LocalizedTextType.Guild_RaidLog, memberItem.Nickname, boxText));
                        }
                    }
                }
                return _raidLogs;
            }
        }
    
        public int MemberCount;
        public Dictionary<string, string> ViceMasterList = new();
        public string MasterNickname;
        public string InDate;
        public string GuildName;
        public int GoodsCount;
        public bool ImmediateRegistration;
        public string MasterInDate;
        public int Rank;

        public int MarkBackgroundIndex;
        public int MarkMainIndex;
        public int MarkSubIndex;
        public int NeededStage;
        public string Description;

        public Dictionary<GoodType, int> Goods = new();
        public List<GuildMemberItem> MemberItems = new();
        public GuildMemberItem MyGuildItem => _myGuildItem ??= MemberItems.Find(m => m.InDate == Backend.UserInDate);

        private double _totalCombat;
        private GuildMemberItem _myGuildItem;
        private Dictionary<GradeType, int> _raidBoxes;
        private List<string> _raidLogs;

        public bool IsMaster(string myInDate)
        {
            return MasterInDate.Equals(myInDate);
        }
    }

    public class GuildMemberItem
    {
        public bool IsMaster => Position == "master";
        public int TotalGoodAmount => Goods.Sum(x => x.Value);
        
        public GuildMemberItem() { }
        public GuildMemberItem(JsonData guildMemberJson)
        {
            Nickname = guildMemberJson["nickname"].ToString();
            InDate = guildMemberJson["inDate"].ToString();
            GamerInDate = guildMemberJson["gamerInDate"].ToString();
            LastLogin = guildMemberJson["lastLogin"].ToString();
            Position = guildMemberJson["position"].ToString();
            
            foreach(var backendGoodType in guildMemberJson.Keys)
                if(backendGoodType.Contains("totalGoods"))
                    if (int.TryParse(guildMemberJson[backendGoodType].ToString(), out var goodAmount))
                    {
                        var goodType = DataController.Instance.good.GetGoodTypeFromBackendGoodString(backendGoodType);
                        if(goodType == GoodType.None) continue;
                        Goods.Add(goodType, goodAmount);
                    }
        }

        public string Nickname;
        public string InDate;
        public string GamerInDate;
        public string Position;
        public string LastLogin;
        public int MaxStage;
        public double Combat;
        public double RaidDamage;
        public int RaidLevel;
        public string RaidClearTimeToString;
        public List<GradeType> RaidBoxes;
        public Dictionary<GoodType, int> Goods = new();
    }
}
