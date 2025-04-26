using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using LitJson;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasRanking : ControllerCanvas
    {
        private const string ViewSlotRankingName = nameof(ViewSlotRanking);
        private readonly ViewSlotRanking _viewSlotMyRanking;
        private readonly List<ViewSlotRanking> _viewSlotRankings = new();

        private readonly Dictionary<string, List<JsonData>> _userRankDatas;
        private readonly Dictionary<string, JsonData> _myRankDatas;

        private ViewCanvasRanking View => ViewCanvas as ViewCanvasRanking;

        private const string UserStageRankUuid = "1fdb1ae0-1991-11ef-a73f-3595bb33f18e";
        private const string UserCombatRankUuid = "c98d3eb0-19a5-11ef-9cfa-051dbfd46ad6";

        private int _delayTime = -1;

        public ControllerCanvasRanking(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasRanking>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.SlideButton.AddListener((index) =>
            {
                var rankUuid = index == 0 ? UserStageRankUuid : UserCombatRankUuid;
                UpdateMyRankView(rankUuid);
                UpdateTopRankView(rankUuid);
            });

            _userRankDatas = new Dictionary<string, List<JsonData>>();    
            _myRankDatas = new Dictionary<string, JsonData>();          
            _viewSlotMyRanking = View.MyViewSlotRanking;
            TimeTask().Forget();
        }
        
        public override void Open()
        {
            if (_delayTime < 0)
            {
                var loading = Get<ControllerCanvasToastMessage>();
                loading.ShowLoading();
                AsyncData(isSuccess =>
                {
                    if(isSuccess)
                    {
                        base.Open();
                        View.SlideButton.OnClick(0);
                        _delayTime = 600;
                    }
                    else
                    {
                        loading.ShowTransientToastMessage(LocalizeManager.GetText(LocalizedTextType.ErrorMessage));
                    }
                    
                    loading.CloseLoading();
                });
            }
            else
            {
                base.Open();
                View.SlideButton.OnClick(0);
            }
        }
        
        private void AsyncData(UnityAction<bool> callback = null)
        {
            BackendManager.UpdateMyRanking(UserStageRankUuid, new KeyValuePair<string, double>("maxTotalLevel", DataController.Instance.maxTotalLevel),
                isSuccessStageRanking =>
                {
                    if (isSuccessStageRanking)
                    {
                        BackendManager.UpdateMyRanking(UserCombatRankUuid,
                            new KeyValuePair<string, double>("maxCombatPower", DataController.Instance.maxCombatPower),
                            isSuccessCombatRanking =>
                            {
                                if (isSuccessCombatRanking)
                                {
                                    AsyncTopRank(100, isSuccess =>
                                    {
                                        if (isSuccess)
                                        {
                                            AsyncMyRank(isSuccess =>
                                            {
                                                callback?.Invoke(isSuccess);
                                            });
                                        }
                                        else
                                            callback?.Invoke(false);
                                    });
                                }
                                else
                                    callback?.Invoke(false);
                            });
                    }
                    else 
                        callback?.Invoke(false);
                });
            
        }

        private void AsyncTopRank(int limit, UnityAction<bool> callback = null)
        {
            BackendManager.GetRankList(UserStageRankUuid, limit, (isSuccess, callbackJsonData) =>
            {
                if(isSuccess)
                {
                    AddUserRankData(UserStageRankUuid, callbackJsonData);
                    BackendManager.GetRankList(UserCombatRankUuid, limit, (isSuccess, callbackJsonData) =>
                    {
                        if (isSuccess)
                        {
                            AddUserRankData(UserCombatRankUuid, callbackJsonData);
                            callback?.Invoke(true);
                        }
                        else 
                            callback?.Invoke(false);
                    });
                }
                else 
                    callback?.Invoke(false);
            });
        }

        private void AddUserRankData(string rankUuid, JsonData jsonData)
        {
            if (!_userRankDatas.ContainsKey(rankUuid))
                _userRankDatas.TryAdd(rankUuid, new List<JsonData>());
            for (var i = 0; i < jsonData["rows"].Count; ++i)
            {
                if (_userRankDatas[rankUuid].Count <= i)
                    _userRankDatas[rankUuid].Add(jsonData["rows"][i]);
                else
                    _userRankDatas[rankUuid][i] = jsonData["rows"][i];
            }
        }

        private void AsyncMyRank(UnityAction<bool> callback)
        {
            BackendManager.GetMyRank(UserStageRankUuid, (isSuccess, callbackJsonData) =>
            {
                if(isSuccess)
                {
                    AddMyRankData(UserStageRankUuid, callbackJsonData);
                    BackendManager.GetMyRank(UserCombatRankUuid, (isSuccess, callbackJsonData) =>
                    {
                        if (isSuccess)
                        {
                            AddMyRankData(UserCombatRankUuid, callbackJsonData);
                            callback?.Invoke(true);
                        }
                        else 
                            callback?.Invoke(false);
                    });
                }
                else 
                    callback?.Invoke(false);
            });
        }

        private void AddMyRankData(string rankUuid, JsonData jsonData)
        {
            if (!_myRankDatas.ContainsKey(rankUuid))
                _myRankDatas.TryAdd(rankUuid, new JsonData());

            if (jsonData["rows"].Count > 0)
                _myRankDatas[rankUuid] = jsonData["rows"][0];
        }

        private void UpdateTopRankView(string rankUuid)
        {
            var viewSlots = _viewSlotRankings.GetViewSlots(ViewSlotRankingName, View.SlotParent, _userRankDatas[rankUuid].Count);
            var i = 0;
            foreach (var data in _userRankDatas[rankUuid])
            {
                var score = rankUuid == UserStageRankUuid
                    ? DataController.Instance.stage.GetStageLevelExpression(int.Parse(data["score"].ToString()))
                    : double.Parse(data["score"].ToString()).ToDamage();

                var slot = viewSlots[i];
                slot
                    .SetRank((i + 1).ToString())
                    .SetRankingImage(i)
                    .SetNickanme(data["nickname"].ToString())
                    .SetScore(score)
                    .SetActive(true);
                ++i;
            }
        }

        private void UpdateMyRankView(string rankUuid)
        {
            if(_myRankDatas.TryGetValue(rankUuid, out var data))
            {
                var score = rankUuid == UserStageRankUuid
                    ? DataController.Instance.stage.GetStageLevelExpression(int.Parse(data["score"].ToString()))
                    : double.Parse(data["score"].ToString()).ToDamage();
                
                _viewSlotMyRanking
                    .SetRank(data["rank"].ToString())
                    .SetRankingImage(int.Parse(data["rank"].ToString()) - 1)
                    .SetNickanme(data["nickname"].ToString())
                    .SetScore(score);
            }
        }

        private async UniTaskVoid TimeTask()
        {
            while (!Cts.IsCancellationRequested)
            {
                _delayTime -= 1;
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }
    }
}