using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasMission : ControllerCanvas
    {

        private readonly Dictionary<MissionType, ViewSlotMissionReward> _viewSlotRewards = new();
        private readonly List<ViewSlotMissionTotalReward> _viewSlotTotalRewards = new();
        private readonly Dictionary<TimeResetType, Dictionary<MissionType, Reddot>> _reddots = new();
        
        private ViewCanvasMission View => ViewCanvas as ViewCanvasMission;

        public ControllerCanvasMission(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasMission>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.SlideButton.AddListener(ChangePanel);

            foreach (var bData in DataController.Instance.mission.BDatas)
            {
                _reddots.TryAdd(bData.resetType, new Dictionary<MissionType, Reddot>());
                _reddots[bData.resetType].TryAdd(bData.missionType, new Reddot(
                    bData.resetType switch
                    {
                        TimeResetType.Daily => ReddotType.MissionDaily,
                        TimeResetType.Weekly => ReddotType.MissionWeekly,
                        TimeResetType.Repeat => ReddotType.MissionRepeat,
                        _ => ReddotType.None
                    }
                ));
            }

            InitReddot();
            
            View.AllEarnButton.onClick.AddListener(() =>
            {
                foreach (MissionType missionType in Enum.GetValues(typeof(MissionType)))
                {
                    if (_viewSlotRewards.TryGetValue(missionType, out var slot))
                    {
                        if (slot.isActiveAndEnabled)
                        {
                            slot.ViewGoods[0].Button.onClick.Invoke();
                            if (DataController.Instance.good.GetValue(GoodType.RemoveAds) > 0 && slot.ViewGoods.Length > 1)
                                slot.ViewGoods[1].Button.onClick.Invoke();
                        }
                    }
                }

                foreach (var slot in _viewSlotTotalRewards.Where(x => x.isActiveAndEnabled))
                {
                    slot.ViewGood.Button.onClick.Invoke();
                }

                View.ShowAllEarnButtonReddot(false);
            });
            
            DataController.Instance.mission.onBindMissionCount += (missionType) =>
            {
                UpdateRewardCount(missionType);
                AsyncTotalReward(GetTimeResetFromSlideButtonIndex());
                SetReddot(missionType);
            };
            DataController.Instance.mission.onBindGetRewarded += (resetType, missionType, isAds) =>
            {
                UpdateSlot(missionType);
                AsyncTotalReward(resetType);
                SetReddot(missionType);
            };
            
            TimeTask().Forget();
        }

        public override void Open()
        {
            base.Open();
            View.SlideButton.OnClick(0);
            View.SlideButton.AsyncNormailzedSize();
        }

        private void ChangePanel(int index)
        {
            var resetType = GetTimeResetFromSlideButtonIndex();
            AsyncTotalReward(resetType);
            AsyncReward(resetType);

            var timeSpan = GetTimeResetFromSlideButtonIndex() == TimeResetType.Daily
                ? ServerTime.RemainingTimeUntilNextDay
                : ServerTime.RemainingTimeUntilNextWeek;
            View.ViewSlotTime.SetTimeText(Utility.GetTimeStringToFromTotalSecond(timeSpan));

            View.ShowAllEarnButtonReddot(_reddots[resetType].Values.Any(reddot => reddot.IsOn));
        }
        
        private TimeResetType GetTimeResetFromSlideButtonIndex()
        {
            return View.SlideButton.SelectedIndex switch
            {
                0 => TimeResetType.Daily,
                1 => TimeResetType.Weekly,
                2 => TimeResetType.Repeat,
                _ => TimeResetType.Daily
            };
        }


        private void UpdateTotalRewardCount()
        {
            var resetType = GetTimeResetFromSlideButtonIndex();
            View.SetTotalAmount(
                DataController.Instance.mission.GetClearCount(resetType), 
                DataController.Instance.mission.GetMissionCount(resetType));
        }
        
        private void AsyncTotalReward(TimeResetType resetType)
        {
            UpdateTotalRewardCount();
            var i = 0;

            var datas = DataController.Instance.mission.GetTotalRewardCaches(resetType);
            if (datas == null) return;
            
            foreach (var bData in datas.Values)
            {
                var totalRewardSlot = GetSlotTotalReward(i);
                totalRewardSlot.gameObject.SetActive(true);
                totalRewardSlot.ViewGood
                    .SetInit(bData.rewardGoodTypes[0])
                    .SetValue(bData.rewardGoodValues[0])
                    .SetActiveCheckPanel(DataController.Instance.mission.IsGetReward(bData.resetType, bData.missionType, false));
                totalRewardSlot.SetGoalCountText(bData.goalCount.ToString());
                totalRewardSlot.GetComponent<RectTransform>().anchoredPosition =
                    new Vector2(GetTotalRewardXPos(bData.resetType, bData.goalCount), 0);

                totalRewardSlot.ViewGood.ReddotView.ShowReddot(
                    DataController.Instance.mission.CanEarned(bData.resetType, bData.missionType, false));
                ++i;
            }
        }

        private void AsyncReward(TimeResetType resetType)
        {
            SetActiveRewardSlot(false);
            foreach (var bData in DataController.Instance.mission.GetRewardCaches(resetType))
            {
                UpdateSlot(bData.Value.missionType);
            }
            View.ShowTotalRewardView(resetType != TimeResetType.Repeat);
        }

        private void UpdateSlot(MissionType missionType)
        {
            UpdateRewardCount(missionType);
            UpdateRewardViewGoods(missionType);
            
            var slot = GetSlotReward(missionType);
            var resetType = GetTimeResetFromSlideButtonIndex();
            slot.gameObject.SetActive(true);
            slot.ViewGoods[1].gameObject.SetActive(resetType != TimeResetType.Repeat);

            var isGetReward = DataController.Instance.mission.IsGetReward(resetType, missionType, false);
            
            if (DataController.Instance.mission.CanEarned(resetType, missionType, false))
            {
                slot.transform.SetAsFirstSibling();
            }
            else if (isGetReward)
            {
                slot.transform.SetAsLastSibling();
            }
        }
        
        private void UpdateRewardCount(MissionType missionType)
        {
            var resetType = GetTimeResetFromSlideButtonIndex();
            var currClearCount = DataController.Instance.mission.GetCurrCount(resetType, missionType);
            var goalCount = DataController.Instance.mission.GetGoalCount(resetType, missionType);
            
            GetSlotReward(missionType)
                .SetCountText(currClearCount, goalCount)
                .SetFillAmount(currClearCount, goalCount);
        }

        private void UpdateRewardViewGoods(MissionType missionType)
        {
            var bData = DataController.Instance.mission.GetRewardCache(GetTimeResetFromSlideButtonIndex(), missionType);
            if (bData == null) return;
            
            var slot = GetSlotReward(missionType);
            var resetType = GetTimeResetFromSlideButtonIndex();
            slot.SetTitle(LocalizeManager.GetText(resetType, missionType, bData.goalCount));
            for (var rewardIndex = 0; rewardIndex < bData.rewardGoodTypes.Length; ++rewardIndex)
            {
                var isAds = rewardIndex == 1;
                slot.ViewGoods[rewardIndex]
                    .SetInit(bData.rewardGoodTypes[rewardIndex])
                    .SetValue(bData.rewardGoodValues[rewardIndex])
                    .ShowAdsPanel(rewardIndex == 1)
                    .SetActiveCheckPanel(DataController.Instance.mission.IsGetReward(resetType, missionType, isAds));

                var canReward = DataController.Instance.mission.CanEarned(resetType, missionType, isAds);
                slot.ViewGoods[rewardIndex].ReddotView.ShowReddot(canReward);
            }
        }
        
        private void SetActiveRewardSlot(bool flag)
        {
            foreach (var slotReward in _viewSlotRewards.Values)
            {
                slotReward.gameObject.SetActive(flag);
            }
        }

        private float GetTotalRewardXPos(TimeResetType resetType, int goal)
        {
            var missionCount = DataController.Instance.mission.GetMissionCount(resetType);
            var posRatio = missionCount == 0 || goal == 0 ? 0 : (float)goal / missionCount;

            var pos = View.TotalGageWidth * posRatio;
            return pos;
        }

        private MissionType GetTotalRewardMissionType(TimeResetType resetType, int index)
        {
            return resetType switch
            {
                TimeResetType.Daily => index switch
                {
                    0 => MissionType.DailyMission0,
                    1 => MissionType.DailyMission1,
                    2 => MissionType.DailyMission2,
                    _ => MissionType.None
                },
                TimeResetType.Weekly => index switch
                {
                    0 => MissionType.WeeklyMission0,
                    1 => MissionType.WeeklyMission1,
                    2 => MissionType.WeeklyMission2,
                    _ => MissionType.None
                },
                _ => MissionType.None
            };
        }

        private ViewSlotMissionReward GetSlotReward(MissionType missionType)
        {
            if (_viewSlotRewards.TryGetValue(missionType, out var reward)) return reward;
            var newSlot = Object.Instantiate(View.ViewSlotMissionRewardPrefab, View.RewardSlotParent);
            _viewSlotRewards.TryAdd(missionType, newSlot);

            newSlot.ViewGoods[0].AddListener(() => TryEarn(missionType, false));
            if(newSlot.ViewGoods.Length > 1)
                newSlot.ViewGoods[1].AddListener(() => TryEarn(missionType, true));
            
            return _viewSlotRewards[missionType];
        }
        
        private ViewSlotMissionTotalReward GetSlotTotalReward(int index)
        {
            var count = _viewSlotTotalRewards.Count;
            for (var i = count; i <= index; ++i)
            {
                var newSlot = Object.Instantiate(View.ViewSlotMissionTotalRewardPrefab, View.TotalRewardSlotParent);
                var iIndex = i;
                newSlot.ViewGood.AddListener(() =>
                    {
                        var missionType = GetTotalRewardMissionType(GetTimeResetFromSlideButtonIndex(), iIndex);
                        if (missionType != MissionType.None)
                            TryEarn(missionType, false);
                    });
                
                _viewSlotTotalRewards.Add(newSlot);
            }
            return _viewSlotTotalRewards[index];
        }

        private void TryEarn(MissionType missionType, bool isAds)
        {
            var resetType = GetTimeResetFromSlideButtonIndex();
            if (DataController.Instance.mission.CanEarned(resetType, missionType, isAds))
            {
                if (isAds)
                    GoogleMobileAdsManager.Instance.ShowRewardedAd(() => Earn(resetType, missionType, true));
                else
                    Earn(resetType, missionType, false);
            }
        }

        private void Earn(TimeResetType resetType, MissionType missionType, bool isAds)
        {
            var goodPair = DataController.Instance.mission.GetGoodReward(resetType, missionType, isAds);
            var viewGood = Get<ControllerCanvasMainMenu>().GetViewGood(goodPair.Key);

            GoodsEffectManager.Instance.ShowEffect(goodPair.Key, Vector2.zero, viewGood, 10);
            DataController.Instance.mission.SetIsRewardEarned(resetType, missionType, isAds, true);
            DataController.Instance.mission.onBindGetRewarded?.Invoke(resetType, missionType, isAds);

            if (!DataController.Instance.mission.IsTotalMission(missionType) && !isAds)
                DataController.Instance.mission.CountTotalMission(resetType);

            DataController.Instance.good.Earn(goodPair.Key, goodPair.Value);
            DataController.Instance.LocalSave();
        }

        private void InitReddot()
        {
            foreach (MissionType missionType in Enum.GetValues(typeof(MissionType)))
            {
                SetReddot(missionType);
            }
        }

        private void SetReddot(MissionType missionType)
        {
            if (missionType == MissionType.None) return;
            
            foreach (var reddotKeyPair in _reddots)
            {
                var reddot = reddotKeyPair.Value;
                if(!reddot.ContainsKey(missionType)) continue;
                
                var canEarned = DataController.Instance.mission.CanEarned(reddotKeyPair.Key, missionType, false);
                reddot[missionType].IsOn = canEarned;
                reddot[missionType].OnBindShowReddot();
            }
        }
        
        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            
            while (true)
            {
                var timeSpan = GetTimeResetFromSlideButtonIndex() == TimeResetType.Daily
                    ? ServerTime.RemainingTimeUntilNextDay
                    : ServerTime.RemainingTimeUntilNextWeek;
                View.ViewSlotTime.SetTimeText(Utility.GetTimeStringToFromTotalSecond(timeSpan));
                
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            }
        }
    }
}