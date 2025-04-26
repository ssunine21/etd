using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Manager
{
    public class BackendGuildManager : Singleton<BackendGuildManager>
    {
        public const string InDateKey = "inDate";
        public const string MemberCountKey = "memberCount";
        public const string GuildNameKey = "guildName";
        public const string ImmediateRegistrationKey = "_immediateRegistration";
        public const string MarkBackgroundImageKey = "markBackgroundImageKey";
        public const string MarkMainImageKey = "markMainImageKey";
        public const string MarkSubImageKey = "markSubImageKey";
        public const string NeededStageKey = "neededStageKey";
        public const string DescriptionKey = "descriptionKey";
        public const string MasterInDate = "masterInDate";
        private const int GoodsCount = 5;
        
        private string _guildListFirstKey = string.Empty;
        private const string GuildExpRankUuid = "0195a4a1-007c-703f-be13-8e3debf87e79";
        
        public override void Init(CancellationTokenSource cts)
        {
            
        }
        
        public async UniTask<bool> HasGuild()
        {
            var compleate = false;
            var isSuccess = false;

            Backend.Guild.GetMyGuildInfoV3(bro =>
            {
                compleate = true;
                isSuccess = bro.IsSuccess();
            });
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }
        
        public async UniTask<bool> ApplyGuild(string guildIndate)
        {
            var compleate = false;
            var isSuccess = false;
            
            Backend.Guild.ApplyGuildV3(guildIndate, bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    switch (bro.GetStatusCode())
                    {
                        case "409":
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_PreApplyMessage);
                            break;
                        default:
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                            break;
                    }
                }

                compleate = true;
            });
            
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }
        

        public async UniTask<bool> TryCreateGuild(string guildName, bool immediateRegistration, Param param)
        {
            var compleate = false;
            var isSuccess = false;
            
            Backend.Guild.CreateGuildV3(guildName, GoodsCount, param, bro =>
            {
                if (!bro.IsSuccess())
                {
                    var errorMessage = bro.StatusCode switch
                    {
                        409 => LocalizedTextType.Guild_NameError0,
                        412 => LocalizedTextType.Guild_NameError1,
                        _ =>  LocalizedTextType.ErrorMessage
                    };
                    
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(errorMessage);
                    return;
                }
                compleate = true;
            });
            
            await UniTask.WaitUntil(() => compleate);
            isSuccess = await SetRegistrationValue(immediateRegistration);
            
            return isSuccess;
        }

        public async UniTask<bool> SetRegistrationValue(bool immediateRegistration)
        {
            var compleate = false;
            var isSuccess = false;
            
            Backend.Guild.SetRegistrationValueV3(immediateRegistration, bro =>
            {
                isSuccess = bro.IsSuccess();
                compleate = true;
            });
            
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }
        
        public async UniTask<JsonData> GetMyGuild()
        {
            var compleate = false;
            var jsonData = new JsonData();
            Backend.Guild.GetMyGuildInfoV3(bro =>
            {
                if (bro.IsSuccess())
                {
                    jsonData = bro.GetFlattenJSON();
                }
                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return jsonData;
        }

        public async UniTask<JsonData> GetGuildList(int limit, bool isNextPage = false)
        {
            var compleate = false;
            var jsonData = new JsonData();
            if (isNextPage && string.IsNullOrEmpty(_guildListFirstKey)) return new JsonData();
            
            _guildListFirstKey = isNextPage ? _guildListFirstKey : string.Empty;
            Backend.Guild.GetGuildListV3(limit, _guildListFirstKey, bro =>
            {
                if (bro.IsSuccess())
                {
                    jsonData = bro.FlattenRows();
                    _guildListFirstKey = bro.FirstKeystring();
                }
                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return jsonData;
        }

        public async UniTask<Dictionary<GoodType, int>> GetGoods(string guildIndate)
        {
            var compleate = false;
            var dic = new Dictionary<GoodType, int>();
            
            Backend.Guild.GetGuildGoodsByIndateV3(guildIndate, (bro) =>
            {
                if(bro.IsSuccess())
                {
                    var jsonData = bro.GetFlattenJSON()["goods"];
                    foreach (var column in jsonData.Keys)
                    {
                        if (column.Contains("totalGoods"))
                        {
                            var goodType = DataController.Instance.good.GetGoodTypeFromBackendGoodString(column);
                            if (int.TryParse(jsonData[column].ToString(), out var value))
                            {
                                dic.TryAdd(goodType, value);
                            }
                        }
                    }
                }

                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return dic;
        }

        public async UniTask<List<GuildMemberItem>> GetGuildMember(string guildIndate)
        {
            var compleate = false;
            var guildMemberList = new List<GuildMemberItem>();

            Backend.Guild.GetGuildMemberListV3(guildIndate, async bro =>
            {
                if (bro.IsSuccess())
                {
                    var guildMemberJson = bro.FlattenRows();
                    for(var i = 0; i < guildMemberJson.Count; i++)
                    {
                        var member = new GuildMemberItem(guildMemberJson[i]);
                        await GetGuildMemberGameData(member);
                        guildMemberList.Add(member);
                    }
                }

                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return guildMemberList;
        }

        public async UniTask<bool> ModifyGuild(Param param)
        {
            var compleate = false;
            var isSuccess = false;
            Backend.Guild.ModifyGuildV4(param, bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    var localizeTextType = bro.GetStatusCode() switch
                    {
                        "428" => LocalizedTextType.Guild_NotAbailableSettlement,
                        _ => LocalizedTextType.ErrorMessage
                    };
                    
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(localizeTextType);
                }        
                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }
        
        public async UniTask<bool> WithdrawGuild()
        {
            var compleate = false;
            var isSuccess = false;
            Backend.Guild.WithdrawGuildV3(bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    var localizedTextType = LocalizedTextType.ErrorMessage;
                    if (bro.GetMessage().Contains("memberExist"))
                        localizedTextType = LocalizedTextType.Guild_WithdrawErrorMessage;
                    
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(localizedTextType);
                }        
                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }

        public async UniTask<bool> ContributeGoods(GoodType goodType, int amount, bool showErrorMessage = true)
        {
            var broGoodType = DataController.Instance.good.GetBackendGoodsTypeFromGoodType(goodType);
            if (broGoodType == goodsType.goods10) return false;
            
            var compleate = false;
            var isSuccess = false;

            if (goodType == GoodType.GuildExp)
            {
                Backend.URank.Guild.ContributeGuildGoods(GuildExpRankUuid, broGoodType, amount, bro => {
                    isSuccess = bro.IsSuccess();
                    if (!bro.IsSuccess())
                    {
                        var localizeTextType = bro.GetStatusCode() switch
                        {
                            "428" => LocalizedTextType.Guild_NotAbailableSettlement,
                            _ => LocalizedTextType.ErrorMessage
                        };

                        if (showErrorMessage)
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(localizeTextType);
                    }

                    compleate = true;
                });
            }
            
            else
            {
                Backend.Guild.ContributeGoodsV4(broGoodType, amount, bro =>
                {
                    isSuccess = bro.IsSuccess();
                    if (!bro.IsSuccess())
                    {
                        var localizeTextType = bro.GetStatusCode() switch
                        {
                            "428" => LocalizedTextType.Guild_NotAbailableSettlement,
                            _ => LocalizedTextType.ErrorMessage
                        };

                        if (showErrorMessage)
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(localizeTextType);
                    }

                    compleate = true;
                });
            }
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }

        public async UniTask<bool> ExpelMember(string gamerInDate)
        {
            var compleate = false;
            var isSuccess = false;
            
            Backend.Guild.ExpelMemberV3(gamerInDate, bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                }    
                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }

        public async UniTask<List<GuildInfo>> GetGuildRank(int limit = 50)
        {
            var compleate = false;
            var guildInfos = new List<GuildInfo>();

            Backend.URank.Guild.GetRankList(GuildExpRankUuid, limit, async bro =>
            {
                var rankListJson = bro.GetFlattenJSON();
                if (bro.IsSuccess())
                {
                    for (var i = 0; i < rankListJson["rows"].Count; ++i)
                    {
                        var info = await GetGuild(rankListJson["rows"][i]["guildInDate"].ToString());
                        if (info != null)
                        {
                            guildInfos.Add(info);
                            int.TryParse(rankListJson["rows"][i]["rank"].ToString(), out info.Rank);
                        }
                    }
                }

                compleate = true;
            });
            await UniTask.WaitUntil(() => compleate);
            return guildInfos;
        }
        
        public async UniTask<GuildInfo> GetMyGuildRank()
        {
            var compleate = false;
            var guildInfo = new GuildInfo();

            Backend.URank.Guild.GetMyGuildRank(GuildExpRankUuid, bro =>
            {
                var rankListJson = bro.GetFlattenJSON();
                if (bro.IsSuccess())
                {
                    for (var i = 0; i < rankListJson["rows"].Count; ++i)
                    {
                        int.TryParse(rankListJson["rows"][i]["rank"].ToString(), out guildInfo.Rank);
                    }
                }

                compleate = true;
            });
            await UniTask.WaitUntil(() => compleate);
            return guildInfo;
        }

        public async UniTask<GuildInfo> GetGuild(string guildIndate)
        {
            var compleate = false;
            GuildInfo guildInfo = null;
            
            Backend.Guild.GetGuildInfoV3(guildIndate, async bro =>
            {
                if (bro.IsSuccess())
                {
                    var json = bro.GetFlattenJSON();
                    guildInfo = new GuildInfo(json["guild"]);
                    guildInfo.MemberItems = await GetGuildMember(guildInfo.InDate);
                }
                
                compleate = true;
            });
            await UniTask.WaitUntil(() => compleate);
            return guildInfo;
        }

        public async UniTask<List<GuildMemberItem>> GetApplicants(int limit = 30)
        {
            var compleate = false;
            var guildMemberItems = new List<GuildMemberItem>();
            
            Backend.Guild.GetApplicantsV3(limit, async bro =>
            {
                if (bro.IsSuccess())
                {
                    var applicantsListJson = bro.FlattenRows();
                    for(var i = 0; i < applicantsListJson.Count; i++)
                    {
                        var memberItem = new GuildMemberItem();
                        if(applicantsListJson[i].ContainsKey("nickname"))
                        {
                            memberItem.Nickname = applicantsListJson[i]["nickname"].ToString();
                        }
                        memberItem.GamerInDate = applicantsListJson[i]["inDate"].ToString();
                        await GetGuildMemberGameData(memberItem);
                        guildMemberItems.Add(memberItem);
                    }
                }      
                compleate = true;
            });
            await UniTask.WaitUntil(() => compleate);
            return guildMemberItems;
        }

        public async UniTask<bool> ApproveApplicant(string gamerIndate)
        {
            var compleate = false;
            var isSuccess = false;
            Backend.Guild.ApproveApplicantV3(gamerIndate, bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    switch (bro.GetStatusCode())
                    {
                        case "412":
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_ApproveErrorMessage0);
                            break;
                        case "429":
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_ApproveErrorMessage1);
                            break;
                        default:
                            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                            break;
                    }
                }
                compleate = true;
            });
            
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }
        
        public async UniTask<bool> RejectApplicant(string gamerIndate)
        {
            var compleate = false;
            var isSuccess = false;
            Backend.Guild.RejectApplicantV3(gamerIndate, bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                }
                compleate = true;
            });
            
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }

        private async UniTask GetGuildMemberGameData(GuildMemberItem member)
        {
            var compleate = false;
            var selectList = new[] { "maxTotalLevel", "maxCombatPower", "guild" };
            Backend.PlayerData.GetOtherData(DataController.GuildDataTableKeyString, member.GamerInDate, selectList, 1, otherInfo =>
            {
                if (otherInfo.IsSuccess())
                {
                    var gameDataJson = otherInfo.FlattenRows();
                    if (gameDataJson.Count > 0)
                    {
                        try
                        {
                            if (double.TryParse(gameDataJson[0]["maxCombatPower"].ToString(), out var combat))
                                member.Combat = combat;

                            if (int.TryParse(gameDataJson[0]["maxTotalLevel"].ToString(), out var maxTotalLevel))
                                member.MaxStage = maxTotalLevel;
                            
                            if(gameDataJson[0].ContainsKey("guild"))
                            {
                                if (gameDataJson[0]["guild"].ContainsKey("raidDamage"))
                                {
                                    if (double.TryParse(gameDataJson[0]["guild"]["raidDamage"].ToString(), out var raidDamage))
                                        member.RaidDamage = raidDamage;
                                }

                                if (gameDataJson[0]["guild"].ContainsKey("raidLevel"))
                                {
                                    if (int.TryParse(gameDataJson[0]["guild"]["raidLevel"].ToString(), out var raidLevel))
                                        member.RaidLevel = raidLevel;
                                }

                                if (gameDataJson[0]["guild"].ContainsKey("raidClearTimeToString"))
                                {
                                    member.RaidClearTimeToString = gameDataJson[0]["guild"]["raidClearTimeToString"].ToString();
                                }

                                if (gameDataJson[0]["guild"].ContainsKey("raidBoxes"))
                                {
                                    member.RaidBoxes ??= new List<GradeType>();
                                    if(gameDataJson[0]["guild"]["raidBoxes"].IsArray)
                                    {
                                        foreach (var raidBoxString in gameDataJson[0]["guild"]["raidBoxes"])
                                        {
                                            member.RaidBoxes.Add((GradeType)Enum.Parse(typeof(GradeType), raidBoxString.ToString()));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            FirebaseManager.LogError(e);
                            compleate = true;
                        }
                    }
                }

                compleate = true;
            });
            await UniTask.WaitUntil(() => compleate);
        }

        public async UniTask<bool> NominateMaster(string gamerIndate)
        {
            var compleate = false;
            var isSuccess = false;
            Backend.Guild.NominateMasterV3(gamerIndate, bro =>
            {
                isSuccess = bro.IsSuccess();
                if (!bro.IsSuccess())
                {
                    ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                }
                compleate = true;
            });
            
            await UniTask.WaitUntil(() => compleate);
            return isSuccess;
        }

        public async UniTask LoadGuildData()
        {
            var compleate = false;
            var guildDataTableKey = DataController.GuildDataTableKeyString;
            Backend.PlayerData.GetMyData(guildDataTableKey, callback =>
            {
                if (callback.IsSuccess())
                {
                    try
                    {
                        var json = BackendReturnObject.Flatten(callback.Rows());
                        if (json.Count > 0)
                        {
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

                            if (serverData != null)
                            {
                                DataController.Instance.guild = serverData.guild;
                                DataController.Instance.guildReward = serverData.guildReward;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        FirebaseManager.LogError(e);
                    }
                    finally
                    {
                        compleate = true;
                    }
                }
                compleate = true;
            });

            await UniTask.WaitUntil(() => compleate);
        }

        public async UniTask SaveGuildData()
        {
            var compleate = false;
            var guildDataTableKey = DataController.GuildDataTableKeyString;

            Backend.PlayerData.GetMyData(guildDataTableKey, callback =>
            {
                try
                {
                    if (callback.IsSuccess())
                    {
                        var param = new Param()
                        {
                            { "guild", DataController.Instance.guild },
                            { "guildReward", DataController.Instance.guildReward },
                            { "maxCombatPower", DataController.Instance.player.MaxCombatPower },
                            { "maxTotalLevel", DataController.Instance.stage.MaxTotalLevel }
                        };

                        if (callback.FlattenRows().Count <= 0)
                        {
                            Backend.PlayerData.InsertData(guildDataTableKey, param, _ => compleate = true);
                        }
                        else
                        {
                            Backend.PlayerData.UpdateMyLatestData(guildDataTableKey, param, _ => compleate = true);
                        }
                    }

                    compleate = true;
                }
                catch (Exception e)
                {
                    FirebaseManager.LogError(e);
                    compleate = true;
                }
            });
            await UniTask.WaitUntil(() => compleate);
        }
    }
}
