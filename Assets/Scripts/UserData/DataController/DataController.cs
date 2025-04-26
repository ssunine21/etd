using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Controller;
using LitJson;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SocialPlatforms;

namespace ETD.Scripts.UserData.DataController
{
    public partial class DataController
    {
        public static string UserDataTableKeyString => UserDataTableKey;
        public static string GuildDataTableKeyString => GuildDataTableKey;
        
        public static bool IsInit { get; private set; }
        public static DataController Instance => _instance ??= new DataController();
        private static DataController _instance;

        private const string FederationLoginTypeKey = "federationLoginTypeKey";
        private const string FederationLoginTokenKey = "federationLoginToken";
        private const string LocalDataSaveKey = "localDataSaveKey";
        private const string UserDataTableKey = "userData";
        private const string GuildDataTableKey = "guildData";

        private bool _isSaving = false;
        
        public void Init()
        {
            _instance.elemental ??= new DataElemental();
            _instance.enemy ??= new DataEnemy();
            _instance.player ??= new DataPlayer();
            _instance.stage ??= new DataStage();
            _instance.rune ??= new DataRune();
            _instance.attribute ??= new DataAttribute();
            _instance.good ??= new DataGood();
            _instance.upgrade ??= new DataUpgrade();
            _instance.elementalCombine ??= new DataElementalCombine();
            _instance.enemyCombination ??= new DataEnemyCombination();
            _instance.enhancement ??= new DataEnhancement();
            _instance.setting ??= new DataSetting();
            _instance.shop ??= new DataShop();
            _instance.quest ??= new DataQuest();
            _instance.dungeon ??= new DataDungeon();
            _instance.difficulty ??= new DataDifficulty();
            _instance.disassembly ??= new DataDisassembly();
            _instance.mission ??= new DataMission();
            _instance.contentUnlock ??= new DataContentUnlock();
            _instance.probability ??= new DataProbability();
            _instance.freeGift ??= new DataFreeGift();
            _instance.tutorial ??= new DataTutorial();
            _instance.offlineReward ??= new DataOfflineReward();
            _instance.buff ??= new DataBuff();
            _instance.attendance ??= new DataAttendance();
            _instance.pass ??= new DataPass();
            _instance.growPass ??= new DataGrowPass();
            _instance.research ??= new DataResearch();
            _instance.raid ??= new DataRaid();
            _instance.vip ??= new DataVip();
            _instance.guild ??= new DataGuild();
            _instance.guildReward ??= new DataGuildReward();
            
            _instance.elemental.Init();
            _instance.rune.Init();
            _instance.good.Init();
            _instance.upgrade.Init();
            _instance.enhancement.Init();
            _instance.attribute.Init();
            _instance.dungeon.Init();
            _instance.stage.Init();
            _instance.mission.Init();
            _instance.quest.Init();
            _instance.contentUnlock.Init();
            _instance.freeGift.Init();
            _instance.tutorial.Init();
            _instance.offlineReward.Init();
            _instance.setting.Init();
            _instance.buff.Init();
            _instance.attendance.Init();
            _instance.pass.Init();
            _instance.growPass.Init();
            _instance.research.Init();
            _instance.guild.Init();
            _instance.guildReward.Init();
            
            //[Raid] initialization must come after [Research] initialization.
            _instance.raid.Init();
            
            //[Player] initialization must come after [Attribute] initialization.
            _instance.player.Init();
            _instance.shop.Init();
            _instance.vip.Init();

            IsInit = true;
        }

        public void ForceInit()
        {
            Init();
            
            LocalSave();
            SaveBackendData();
        }
        
        public void LocalSave()
        { 
            if (!ServerTime.IsInit) return;
            if (!GameManager.Instance.IsPlaying) return;
            
            Instance.setting.updateAt = ServerTime.DateTimeToIsoString(ServerTime.Date);
            var localToJson = JsonUtility.ToJson(Instance);
            PlayerPrefs.SetString(LocalDataSaveKey, localToJson);
        }

        public DataController LocalLoad()
        {
            var localData = JsonUtility.FromJson(PlayerPrefs.GetString(LocalDataSaveKey), typeof(DataController)) as DataController;
            return localData;
        }


        public void SaveBackendData(UnityAction<bool> resultCallback = null)
        {
            if(_isSaving) return;
            
            _isSaving = true;
            resultCallback += (value) => _isSaving = false;
            
            Backend.PlayerData.GetMyData(UserDataTableKey, callback =>
            {
                var isSuccess = false;
                if (callback.IsSuccess())
                {
                    Utility.LogWithColor($"Read Capacity: {callback.GetReadCapacity()}", Color.yellow);
                    var localToJson = JsonUtility.ToJson(Instance);
                    var param = Param.Parse(localToJson);

                    if (callback.FlattenRows().Count <= 0)
                    {
                        Utility.LogWithColor("No Data - Data Insert", Color.yellow);

                        Backend.PlayerData.InsertData(UserDataTableKey, param, insertCallback =>
                        {
                            if (insertCallback.IsSuccess())
                                Utility.LogWithColor($"Write Capacity: {insertCallback.GetWriteCapacity()}", Color.yellow);
                            else
                                Utility.LogError($"Insert fails: {insertCallback}");
                            resultCallback?.Invoke(true);
                        });
                    }
                    else
                    {
                        Utility.LogWithColor("Data - Data Update", Color.yellow);
                        Backend.PlayerData.UpdateMyLatestData(UserDataTableKey, param, updateCallback =>
                        {
                            if (updateCallback.IsSuccess())
                                Utility.LogWithColor($"Write Capacity: {updateCallback.GetWriteCapacity()}", Color.yellow);
                            else
                                Utility.LogError($"Update fails: {updateCallback}");
                            
                            resultCallback?.Invoke(true);
                        });
                    }
                }
                else
                {
                    resultCallback?.Invoke(false);
                }
            });
        }

        public async UniTask LoadBackendData()
        {
            var callback = Backend.PlayerData.GetMyData(UserDataTableKey);
            if (callback.IsSuccess())
            {
                Utility.LogWithColor($"Read Capacity: {callback.GetReadCapacity()}", Color.yellow);
                var json = BackendReturnObject.Flatten(callback.Rows());

                var serverUpdateAt = new DateTime();
                try
                {
                    var updatedAtString = callback.FlattenRows()[0]["updatedAt"].ToString();
                    serverUpdateAt = ServerTime.IsoStringToDateTime(updatedAtString);
                }
                catch (Exception e)
                {
                    FirebaseManager.LogError(e);
                }

                var localData = LocalLoad();
                if (localData != null)
                {
                    var localUpdateAt = ServerTime.IsoStringToDateTime(localData.setting.updateAt);
                    Utility.LogWithColor($"server update at: {serverUpdateAt}", Color.yellow);
                    Utility.LogWithColor($"local update at: {localData.setting.updateAt}", Color.yellow);

                    if (localUpdateAt.Ticks >= serverUpdateAt.Ticks)
                    {
                        _instance = localData;
                        await BackendGuildManager.Instance.LoadGuildData();
                        Utility.LogWithColor("load local data", Color.green);
                        return;
                    }

                    localData.setting.updateAt = ServerTime.DateTimeToIsoString(serverUpdateAt);
                }

                if (json.Count <= 0)
                {
                    return;
                }

                var stringBuilder = new StringBuilder();
                var i = 0;
                foreach (KeyValuePair<string, JsonData> jsonData in json[0])
                {
                    stringBuilder.Append(i == 0 ? "{" : ",");
                    stringBuilder.Append($"\"{jsonData.Key}\":");
                    stringBuilder.Append($"{jsonData.Value.ToJson()}");
                    ++i;
                }

                stringBuilder.Append("}");
                var serverData = JsonUtility.FromJson(stringBuilder.ToString(), typeof(DataController)) as DataController;
                _instance = serverData;
                await BackendGuildManager.Instance.LoadGuildData();
                
                Utility.LogWithColor("load server data", Color.green);
            }
        }

        public static void DeleteFederationLoginInfo()
        {
            PlayerPrefs.DeleteKey(FederationLoginTypeKey);
            PlayerPrefs.DeleteKey(FederationLoginTokenKey);
        }

        public static void SaveFederationLoginInfo(string token, FederationType federationType)
        {
            PlayerPrefs.SetInt(FederationLoginTypeKey, (int)federationType);
            PlayerPrefs.SetString(FederationLoginTokenKey, token);
            PlayerPrefs.Save();
        }

        public static FederationType? GetFederationLoginType()
        {
            var type = PlayerPrefs.GetInt(FederationLoginTypeKey, -1);
            return type == -1 ? null : (FederationType)type;
        }

        public static string GetFederationLoginToken()
        {
            var type = PlayerPrefs.GetString(FederationLoginTokenKey, string.Empty);
            return type;
        }
    }
}