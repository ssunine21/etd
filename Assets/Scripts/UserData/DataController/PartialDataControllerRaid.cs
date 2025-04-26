using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using BackEnd.BackndNewtonsoft.Json;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.CloudData;
using LitJson;
using UnityEngine;
using UnityEngine.Events;
using JsonConvert = Newtonsoft.Json.JsonConvert;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public DataRaid raid;
    }

    [Serializable]
    public class DataRaid
    {
        public string nextRaidTicketTimeToString;
        public int CurrRaidLevel { get; set; }
        
        private const string RaidDataTableKey = "raidData";
        private const string IsRaidKey = "isRaid";
        private const string LevelKey = "level";
        private const string PlaceNumberKey = "placeNumber";
        private BRaid[] BDatas => CloudData.CloudData.Instance.bRaids;
        private BInfo BInfoData => CloudData.CloudData.Instance.bInfos[0];

        public void Init()
        {
            if (DataController.Instance.research.OnBindResearch != null)
            {
                DataController.Instance.research.OnBindResearch[ResearchType.IncreaseSaveStorage] += () =>
                {
                    TryUpdateMyRaidData(new Param
                    {
                        {
                            RaidData.StorageLevelKey, DataController.Instance.research.GetCurrLevel(ResearchType.IncreaseSaveStorage)
                        }
                    });
                };
            }

            if (DataController.Instance.good.OnBindChangeGood != null)
            {
                DataController.Instance.good.OnBindChangeGood += goodType =>
                {
                    if (goodType == GoodType.DarkDia)
                    {
                        TryUpdateMyRaidData(new Param
                        {
                            { RaidData.DarkDiaKey, DataController.Instance.good.GetValue(GoodType.DarkDia) },
                        });
                    }
                };
            }
        }

        public int GetMaxTicketCount()
        {
            return BInfoData.maxRaidTicket;
        }

        public int GetNextTicketTimeInSeconds()
        {
            return BInfoData.nextRaidTicketTimeInSeconds;
        }

        public int GetMaxLevel()
        {
            return BDatas[0].maxLevel;
        }

        public int GetTotalPlaceCount()
        {
            return BDatas[0].placeCounts.Sum();
        }

        public int GetPlaceCount(int level)
        {
            level = Mathf.Clamp(level, 0, BDatas[0].placeCounts.Length - 1);
            return BDatas[0].placeCounts[level];
        }

        public int GetTotalDuration(int level)
        {
            level = Mathf.Clamp(level, 0, BDatas[0].totalDurations.Length - 1);
            return BDatas[0].totalDurations[level];
        }

        public void SetNextRaidTicketTimeToString(string dateTime)
        {
            nextRaidTicketTimeToString = dateTime;
        }

        public TimeSpan GetProgressTimeSpan(RaidData raidData)
        {
            if (raidData == null) return TimeSpan.Zero;
            var timespan = raidData.IsRaid
                ? ServerTime.Date - raidData.RaidStartTime
                : raidData.RaidEndTime - raidData.RaidStartTime;
            var maxValue = raidData.RaidEndTime - raidData.RaidStartTime;

            return timespan > maxValue ? maxValue : timespan;
        }
        

        public TimeSpan GetRemainingTimeSpan(RaidData raidData)
        {
            if(raidData == null) return TimeSpan.Zero;
            var timespan = raidData.IsRaid
                ? raidData.RaidEndTime - ServerTime.Date
                : raidData.RaidStartTime.AddSeconds(GetTotalDuration(raidData.Level)) - raidData.RaidEndTime;

            return timespan < TimeSpan.Zero ? TimeSpan.Zero : timespan;
        }
        
        public double GetValueWithProgressSeconds(RaidData raidData)
        {
            var progressSeconds = DataController.Instance.raid.GetProgressTimeSpan(raidData).TotalSeconds;
            var result = DataController.Instance.raid.GetValueWithProgressSeconds(raidData.Level, progressSeconds);
            
            return result;
        }
        
        public double GetValueWithProgressSeconds(int level, double progressSeconds)
        {
            var value = GetValuePerSeconds(level);
            var result = Math.Floor(value * progressSeconds);
            return result;
        }

        public double GetValuePerSeconds(int level)
        {
            level = Mathf.Clamp(level, 0, BDatas[0].goodValuesPerSeconds.Length - 1);
            var goodValuesPerSeconds = BDatas[0].goodValuesPerSeconds[level];

            return goodValuesPerSeconds;
        }

        public int GetStartDifficultyLevel()
        {
            return GetStartDifficultyLevel(CurrRaidLevel);
        }

        public int GetStartDifficultyLevel(int level)
        {
            return BDatas[0].difficultyLevels[Mathf.Clamp(level, 0, BDatas[0].difficultyLevels.Length - 1)];
        }
        
        public double GetLootableValue(RaidData raidData)
        {
            var userDarkDia = raidData.DarkDia;
            var safeStorage = DataController.Instance.research.GetValue(ResearchType.IncreaseSaveStorage, raidData.StorageLevel);
            var raidDarkDia = GetValueWithProgressSeconds(raidData);

            var maxValue = Math.Max(0, userDarkDia - safeStorage) + raidDarkDia;
            var lostPercent = 1 - (DataController.Instance.player.IsProtected() 
                ? DataController.Instance.raid.GetProtectedPercentage() : DataController.Instance.raid.GetPlunderPercentage());
            var result = maxValue * lostPercent;
            return result;
        }

        public GoodType GetGoodType()
        {
            return BDatas[0].goodType;
        }

        public bool TryGetMyRaidData(out RaidData raidData, int tryCount = 3)
        {          
            raidData = null;
            
            var bro = Backend.GameData.GetMyData(RaidDataTableKey, new Where());
            if (bro.IsSuccess() == false) return false;
            
            var raidDataListJson = bro.FlattenRows();
            
            if (raidDataListJson.Count <= 0)
            {
                if (tryCount <= 0) return false;
                TryInsertMyData();
                return TryGetMyRaidData(out raidData, tryCount - 1);
            }

            raidData = new RaidData(raidDataListJson[0]);
            return true;
        }
        
        private bool TryInsertMyData()
        {
            var bro = Backend.GameData.Insert(RaidDataTableKey);
            return bro.IsSuccess();
        }

        public void GetRaidData(int level, int placeNumber, UnityAction<RaidData> callback)
        {
            var raidDatas = new List<RaidData>();
            var where = new Where();
            where.Equal(IsRaidKey, true);
            where.Equal(LevelKey, level);
            where.Equal(PlaceNumberKey, placeNumber);
            
            SendQueue.Enqueue(Backend.GameData.Get, RaidDataTableKey, where, bro =>
            {
                if (bro.IsSuccess() == false)
                    callback?.Invoke(null);

                var raidDataListJson = bro.FlattenRows();
                if (raidDataListJson.Count <= 0)
                    callback?.Invoke(null);

                for (var i = 0; i < raidDataListJson.Count; ++i)
                {
                    raidDatas.Add(new RaidData(raidDataListJson[i]));
                }
                
                callback?.Invoke(raidDatas[0]);
            });
        }
        
        public void GetRaidDatas(UnityAction<List<RaidData>> callback)
        {
            var raidDatas = new List<RaidData>();
            var where = new Where();
            var totalPlaceCount = GetTotalPlaceCount();
            
            where.Equal(IsRaidKey, true);
            
            SendQueue.Enqueue(Backend.GameData.Get, RaidDataTableKey, where, totalPlaceCount, bro =>
            {
                if (bro.IsSuccess() == false)
                    callback?.Invoke(null);

                var raidDataListJson = bro.FlattenRows();
                for (var i = 0; i < raidDataListJson.Count; ++i)
                {
                    var newRaidData = new RaidData(raidDataListJson[i]);
                    raidDatas.Add(newRaidData);
                }
                callback?.Invoke(raidDatas);
            });
        }

        public bool TryUpdateMyRaidData(Param param)
        {
            return TryGetMyRaidData(out var raidData) && TryUpdate(raidData, param);
        }

        public bool TryUpdate(RaidData raidData, Param param)
        {
            return raidData != null && TryUpdate(raidData.InDate, raidData.OnwerInDate, param);
        }

        public bool TryUpdate(string inDate, string ownerInDate, Param param)
        {
            var bro = Backend.GameData.UpdateV2(RaidDataTableKey, inDate, ownerInDate, param);

            if (!bro.IsSuccess())
            {
                Utility.LogError(bro.GetMessage());
            }
            return bro.IsSuccess();
        }

        public bool TryInitRaidData(RaidData raidData)
        {
            var param = new Param
            {
                {RaidData.IsRaidKey, false},
                {RaidData.LostValueKey, 0},
                {RaidData.LastAttackerKey, string.Empty},
                {RaidData.MaxStageKey, 0},
                {RaidData.RaidEndTimeToStringKey, string.Empty},
            };

            return TryUpdate(raidData, param);
        }
        
        public float GetPlunderPercentage()
        {
            return BInfoData.plunderPercentage;
        }

        public float GetProtectedPercentage()
        {
            return BInfoData.protectedPercentage;
        }
    }

    public class RaidData
    {
        public readonly string Nickname;
        public readonly string InDate;
        public readonly string OnwerInDate;
        
        public readonly bool IsRaid;
        public readonly int Level;
        public readonly int PlaceNumber;
        public readonly double LostValue;
        public readonly string LastAttacker;
        public readonly int MaxStage;
        public readonly double DarkDia;
        public readonly int StorageLevel;
        public readonly DateTime RaidStartTime;
        public readonly DateTime RaidEndTime;
        public readonly DateTime ProtectEndTime;
        public readonly DateTime InBattleStartTime;
        
        public readonly string RaidStartTimeToString;
        public readonly string RaidEndTimeToString;
        public readonly string ProtectEndTimeToString;
        public readonly string InBattleStartTimeToString;
        
        public RaidData(){}

        public RaidData(JsonData json)
        {
            Nickname = json[NicknameKey].ToString();
            InDate = json[InDateKey].ToString();
            OnwerInDate = json[OnwerInDateKey].ToString();
            IsRaid = bool.Parse(json[IsRaidKey].ToString());
            Level = int.Parse(json[LevelKey].ToString());
            PlaceNumber = int.Parse(json[PlaceNumberKey].ToString());
            LostValue = double.Parse(json[LostValueKey].ToString());
            LastAttacker = json[LastAttackerKey].ToString();
            MaxStage = int.Parse(json[MaxStageKey].ToString());
            RaidStartTimeToString = json[RaidStartTimeToStringKey].ToString();
            RaidEndTimeToString = json[RaidEndTimeToStringKey].ToString();
            ProtectEndTimeToString = json[ProtectEndTimeToStringKey].ToString();
            DarkDia = double.Parse(json[DarkDiaKey].ToString());
            StorageLevel = int.Parse(json[StorageLevelKey].ToString());
            InBattleStartTimeToString = json.ContainsKey(InBattleStartTimeKey) ? json[InBattleStartTimeKey].ToString() : "";

            RaidStartTime = ServerTime.IsoStringToDateTime(RaidStartTimeToString);
            RaidEndTime = ServerTime.IsoStringToDateTime(RaidEndTimeToString);
            ProtectEndTime = ServerTime.IsoStringToDateTime(ProtectEndTimeToString);
            InBattleStartTime = ServerTime.IsoStringToDateTime(InBattleStartTimeToString);
        }

        public bool Equals(string onwerInDate)
        {
            return OnwerInDate == onwerInDate;
        }

        public const string NicknameKey = "nickname";
        public const string InDateKey = "inDate";
        public const string OnwerInDateKey = "owner_inDate";
        public const string IsRaidKey = "isRaid";
        public const string LevelKey = "level";
        public const string PlaceNumberKey = "placeNumber";
        public const string LostValueKey = "lostValue";
        public const string LastAttackerKey = "lastAttacker";
        public const string MaxStageKey = "maxStage";
        public const string RaidStartTimeToStringKey = "raidStartTime";
        public const string RaidEndTimeToStringKey = "raidEndTime";
        public const string ProtectEndTimeToStringKey = "protectEndTime";
        public const string DarkDiaKey = "darkDia";
        public const string StorageLevelKey = "storageLevel";
        public const string InBattleStartTimeKey = "inBattleStartTime";
    }
}