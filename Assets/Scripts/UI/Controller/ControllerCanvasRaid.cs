using System;
using System.Collections.Generic;
using System.Threading;
using BackEnd;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasRaid : ControllerCanvas
    {
        private readonly List<ViewSlotRaidPlace> _viewSlotRaidPlaces = new();

        private readonly ButtonRadioGroup _levelButtonGroup;
        private readonly ButtonRadioGroup _tabButtonGroup;
        private readonly Vector3 _darkDiaStateImageOriginPos;
        private readonly Sequence _stateAnimationSequnce;
        
        private ViewCanvasRaid View => ViewCanvas as ViewCanvasRaid;
        private RaidData _currRaidData;
        private RaidData _myRaidData;
        private TimeSpan _remainTimeSpen;
        private DateTime _nextRaidTicketTime;
        private Dictionary<int, List<RaidData>> _raidUserDatasDic;

        private int _currLevel;
        private int _currPlaceNumber;
        private int _refreshTime;

        public ControllerCanvasRaid(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasRaid>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.ViewRaidInfoPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            
            _tabButtonGroup = new ButtonRadioGroup();
            _tabButtonGroup.onBindSelected += (index) =>
            {
                switch (index)
                {
                    case 0:
                    {
                        if (_levelButtonGroup != null)
                            UpdateViewSlotRaidPlace(_levelButtonGroup.SelectedIndex);
                        break;
                    }
                    case 1:
                        UpdateMyState();
                        break;
                }

                for (var i = 0; i < View.TabPanels.Length; ++i)
                {
                    View.TabPanels[i].SetActive(i == index);
                }
            };
            
            _levelButtonGroup = new ButtonRadioGroup();
            _levelButtonGroup.onBindSelected += UpdateViewSlotRaidPlace;

            InitialTabButtons();
            InitialLevelButtons();
            
            _darkDiaStateImageOriginPos = View.DarkDiaStateStateImage.transform.localPosition;
            _stateAnimationSequnce = DOTween.Sequence().SetAutoKill(false)
                .AppendInterval(2f)
                .Append(View.DarkDiaStateStateImage.transform.DOLocalMoveY(50, 0.8f).SetEase(Ease.InOutCirc))
                .Append(View.DarkDiaStateStateImage.transform.DOLocalMoveY(0, 0.2f).SetEase(Ease.InQuint)
                    .OnComplete(() =>
                    {
                        if (ActiveSelf && _tabButtonGroup.SelectedIndex == 1 && !View.ViewRaidInfoPopup.IsActive)
                            GoodsEffectManager.Instance.ShowEffect(GoodType.DarkDia, View.DarkDiaStateStateImage.transform.position, View.DarkDiaValueImageTr.position, Random.Range(3, 5));
                    }))
                .SetLoops(int.MaxValue, LoopType.Restart)
                .OnComplete(() => { View.DarkDiaStateStateImage.transform.localPosition = _darkDiaStateImageOriginPos; });

            View.CloseButton.onClick.AddListener(Close);
            View.RaidButton.onClick.AddListener(() =>
            {
                var toastCanvas = Get<ControllerCanvasToastMessage>();
                if (GetMyRaidState() is RaidState.Raiding or RaidState.Raided or RaidState.RaidCompleate)
                {
                    toastCanvas.ShowTransientToastMessage(LocalizedTextType.AlreadyRaid);
                    return;
                }
                
                toastCanvas.ShowLoading();
                TryRaid(isSuccess =>
                {
                    if(isSuccess)
                    {
                        View.ViewRaidInfoPopup.Close();    
                    }        
                    toastCanvas.CloseLoading();                    
                });
            });
            View.RaidCancelButton.onClick.AddListener(TrySettleRaidValue);
            View.OpenGuidePopupViewButton.onClick.AddListener(OpenGuildView);
            View.RefreshButton.onClick.AddListener(() =>
            {
                AsyncRaidDatas().Forget();
                _refreshTime = 30;
                DiscountAndUpdateRefreshTime();
            });
            
            StageManager.Instance.onBindChangeStageType += UpdateChangeStage;
            DataController.Instance.contentUnlock.OnBindInitUnlockDic[UnlockType.Raid] += InitDarkDiaChargeDateTime;
            _nextRaidTicketTime = ServerTime.IsoStringToDateTime(DataController.Instance.raid.nextRaidTicketTimeToString);
            
            UpdateMyState();
            TimeTask().Forget();
        }

        private async UniTaskVoid TimeTask()
        {
            while (!Cts.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
                var myState = GetMyRaidState();
                if(myState == RaidState.Raiding)
                {
                    _remainTimeSpen = DataController.Instance.raid.GetRemainingTimeSpan(_myRaidData);
                    if (_remainTimeSpen.TotalSeconds > 0)
                    {
                        var timeText = Utility.GetTimeStringToFromTotalSecond(_remainTimeSpen);
                        var value = DataController.Instance.raid.GetValueWithProgressSeconds(_myRaidData);
                        var valueText = $"<sprite=2> <color=orange>{value.ToGoodString(GoodType.DarkDia)}</color>";

                        View
                            .SetCurrValueTMP(valueText)
                            .SetCurrTimeTMP(timeText);
                        GetMyProccessFillAmount();
                    }
                }
                
                View.Reddot.ShowReddot(myState is RaidState.RaidCompleate or RaidState.Raided);
                
                TryEarnRaidTicketAndAsyncTime();
                UpdateTicketPanel();
                
                DiscountAndUpdateRefreshTime();
                UpdateStateAnimation(myState);
                UpdateProtectTime();
            }
        }

        private void InitDarkDiaChargeDateTime()
        {
            if (string.IsNullOrEmpty(DataController.Instance.raid.nextRaidTicketTimeToString))
            {
                DataController.Instance.good.SetValue(GoodType.RaidTicket, DataController.Instance.raid.GetMaxTicketCount());
                SetNextTicketTime(ServerTime.Date);
            }
        }
        
        public override void Open()
        {
            AsyncRaidDatas(() =>
            {
                base.Open();
                _levelButtonGroup.Select(0);
                _tabButtonGroup.Select(0);
            }).Forget();
        }

        private void OpenGuildView()
        {
            Get<ControllerCanvasToastMessage>()
                .SetToastMessage(null, LocalizeManager.GetText(LocalizedTextType.RaidGuideDesc))
                .SetToastMessageHorizontalAlignmentOptions(HorizontalAlignmentOptions.Left)
                .ShowToastMessage();
        }

        private void UpdateStateAnimation(RaidState myState)
        {
            if (myState != RaidState.Raiding)
                _stateAnimationSequnce.Complete();
            
            switch (myState)
            {
                case RaidState.Empty:
                    View.DarkDiaStateStateImage.material = ResourcesManager.Instance.grayScaleMaterial;
                    break;
                case RaidState.Raiding:
                    View.DarkDiaStateStateImage.material = null;
                    if (!_stateAnimationSequnce.IsPlaying())
                        _stateAnimationSequnce.Restart();
                    break;
                case RaidState.RaidCompleate:
                    View.DarkDiaStateStateImage.material = null;
                    break;
                case RaidState.Raided:
                    View.DarkDiaStateStateImage.material = ResourcesManager.Instance.grayScaleMaterial;
                    break;
            }
        }
        
        private void UpdateProtectTime()
        {
            var text = string.Empty;
            if(DataController.Instance.player.IsProtected())
            {
                var timeSpan = DataController.Instance.player.GetProtectRemainTimeSpan();
                var timeText = Utility.GetTimeStringToFromTotalSecond(timeSpan);
                text = $"<color=green>({LocalizeManager.GetText(LocalizedTextType.Protect)}) - {timeText}</color>";
            }
            View.SetProtectedText(text);
        }

        private void TryEarnRaidTicketAndAsyncTime()
        {
            var seconds = DataController.Instance.raid.GetNextTicketTimeInSeconds();
            while (true)
            {
                if(IsMaxTicket())
                {
                    SetNextTicketTime(ServerTime.Date.AddSeconds(seconds));
                    return;
                }

                if (!ServerTime.IsRemainingTimeUntilDisable(_nextRaidTicketTime))
                {
                    DataController.Instance.good.Earn(GoodType.RaidTicket, 1);
                    SetNextTicketTime(_nextRaidTicketTime.AddSeconds(seconds));
                }
                else
                    return;
            }
        }

        private void UpdateTicketPanel()
        {
            var timeSpan = ServerTime.RemainingTimeToTimeSpan(_nextRaidTicketTime);
            View
                .SetNextRaidTicketTimeText(Utility.GetTimeStringToFromTotalSecond(timeSpan))
                .SetCurrRaidTicketText(
                    $"{DataController.Instance.good.GetValue(GoodType.RaidTicket)}/{DataController.Instance.raid.GetMaxTicketCount()}")
                .SetActiveTicketTimePanel(!IsMaxTicket());
            
        }

        private bool IsMaxTicket()
        {
            var curr = DataController.Instance.good.GetValue(GoodType.RaidTicket);
            return curr >= DataController.Instance.raid.GetMaxTicketCount();
        }
        
        private void SetNextTicketTime(DateTime dateTime)
        {
            _nextRaidTicketTime = dateTime;
            DataController.Instance.raid.SetNextRaidTicketTimeToString(ServerTime.DateTimeToIsoString(_nextRaidTicketTime));
        }

        private void TryRaid(UnityAction<bool> isSuccess)
        {
            if (DataController.Instance.good.IsEnoughGood(GoodType.RaidTicket, 1))
            {
                DataController.Instance.raid.GetRaidData(_currLevel, _currPlaceNumber, raidData =>
                {
                    _currRaidData = raidData;
                    DataController.Instance.raid.CurrRaidLevel = _currLevel;
                    
                    //해당 자리에 데이터 없으면(약탈 안되면) 내 데이터 가져오기(점령하기)
                    if(_currRaidData == null)
                        if (!DataController.Instance.raid.TryGetMyRaidData(out _currRaidData))
                        {
                            Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                            isSuccess?.Invoke(false);
                            return;
                        }
                    
                    var inBattleTime = _currRaidData.InBattleStartTime.AddMinutes(3);
                    if (ServerTime.IsRemainingTimeUntilDisable(inBattleTime))
                    {
                        Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.AnotherUserTaking);
                        isSuccess?.Invoke(false);
                        return;
                    }
                    
                    var param = new Param()
                    {
                        { RaidData.InBattleStartTimeKey, ServerTime.DateTimeToIsoString(ServerTime.Date) },
                        { RaidData.LevelKey, _currLevel },
                        { RaidData.PlaceNumberKey, _currPlaceNumber },
                    };

                    if (!_currRaidData.Equals(Backend.UserInDate))
                    {
                        if (!DataController.Instance.raid.TryUpdateMyRaidData(param))
                        {
                            Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                            isSuccess?.Invoke(false);
                            return;
                        }
                    }
                    
                    if (DataController.Instance.raid.TryUpdate(_currRaidData, param))
                    {
                        if (DataController.Instance.good.TryConsume(GoodType.RaidTicket, 1))
                        {
                            Get<ControllerCanvasToastMessage>().ShowFadeOutIn(() =>
                            {
                                Close();
                                StageManager.Instance.ChangeStage(StageType.DarkDiaDungeon, _currRaidData?.MaxStage ?? 0);
                            });
                            isSuccess?.Invoke(true);
                        }
                        else
                            isSuccess?.Invoke(false);
                        return;
                    }

                    Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                    isSuccess?.Invoke(false);
                });
            }
            else
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
                isSuccess?.Invoke(false);
            }
        }

        public void TrySettleRaidStage(bool isClear, int currCount, UnityAction<double> callback)
        {
            if (isClear)
            {
                var myParam = new Param
                {
                    { RaidData.IsRaidKey, true },
                    { RaidData.RaidEndTimeToStringKey, ServerTime.DateTimeToIsoString(ServerTime.Date.AddSeconds(DataController.Instance.raid.GetTotalDuration(_currRaidData.Level)))},
                    { RaidData.RaidStartTimeToStringKey, ServerTime.DateTimeToIsoString(ServerTime.Date) },
                    { RaidData.MaxStageKey, currCount },
                    { RaidData.LostValueKey, 0 },
                    { RaidData.NicknameKey, Backend.UserNickName },
                    { RaidData.LastAttackerKey, string.Empty },
                    { RaidData.DarkDiaKey, DataController.Instance.good.GetValue(GoodType.DarkDia)},
                    { RaidData.StorageLevelKey, DataController.Instance.research.GetCurrLevel(ResearchType.IncreaseSaveStorage)}
                };

                if (_currRaidData.Equals(Backend.UserInDate))
                {
                    DataController.Instance.raid.TryUpdate(_currRaidData, myParam);
                    callback?.Invoke(0);
                }
                else if (DataController.Instance.raid.TryGetMyRaidData(out var myRaidData))
                {
                    var lootableValue = DataController.Instance.raid.GetLootableValue(_currRaidData);
                    var playerParam = new Param
                    {
                        { RaidData.IsRaidKey, false },
                        { RaidData.RaidEndTimeToStringKey, ServerTime.DateTimeToIsoString(ServerTime.Date) },
                        { RaidData.LostValueKey, lootableValue},
                        { RaidData.LastAttackerKey, Backend.UserNickName }
                    };

                    DataController.Instance.raid.TryUpdate(myRaidData.InDate, myRaidData.OnwerInDate, myParam);
                    DataController.Instance.raid.TryUpdate(_currRaidData, playerParam);
                    callback?.Invoke(lootableValue);
                }
                return;
            }
            callback?.Invoke(0);
        }

        private void TrySettleRaidValue()
        {
            var toastMessage = Get<ControllerCanvasToastMessage>();
            var state = GetMyRaidState();
            if (state is RaidState.None or RaidState.Empty)
            {
                toastMessage.ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                return;
            }
            
            var earnDarkDiaValue = DataController.Instance.raid.GetValueWithProgressSeconds(_myRaidData);
            var lostDarkDiaValue = _myRaidData.LostValue;

            var title = LocalizeManager.GetText(LocalizedTextType.Settlement);
            var desc =
                (state == RaidState.Raided
                    ? $"{LocalizeManager.GetText(LocalizedTextType.LostReward)} : <color={Utility.GetRedColorToHex()}><sprite=2> {lostDarkDiaValue.ToGoodString(GoodType.DarkDia)}</color>\n"
                    : "")
                + $"{LocalizeManager.GetText(LocalizedTextType.EarnReward)} : <color=orange><sprite=2> {earnDarkDiaValue.ToGoodString(GoodType.DarkDia)}</color>";

            desc = $"<size=120%>{desc}</size>";
            toastMessage.SetToastMessage(
                title, desc, LocalizeManager.GetText(LocalizedTextType.Cancel),null,
                LocalizeManager.GetText(LocalizedTextType.Settle), () =>
                {
                    toastMessage.ShowLoading();
                    switch (state)
                    {
                        case RaidState.None:
                        case RaidState.Empty: return;
                        case RaidState.Raided:
                        case RaidState.Raiding:
                        case RaidState.RaidCompleate:
                            if (DataController.Instance.raid.TryInitRaidData(_myRaidData))
                            {
                                var goodType = GoodType.DarkDia;
                                var viewGood = Get<ControllerCanvasMainMenu>().GetViewGood(goodType);
                                GoodsEffectManager.Instance.ShowEffect(goodType, Vector2.zero, viewGood, 10);
                                DataController.Instance.good.Earn(goodType, earnDarkDiaValue - lostDarkDiaValue);
                                
                                AsyncRaidDatas().Forget();
                            }
                            break;
                    }
                    UpdateMyState();
                    toastMessage.CloseLoading();
                }).ShowToastMessage();
        }

        private void OpenPopupView(int level, int placeNumber)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            DataController.Instance.raid.GetRaidData(level, placeNumber, raidUserData =>
            {
                var isEmpty = raidUserData == null;
                View.SetEmpty(isEmpty);

                var goodType = DataController.Instance.raid.GetGoodType();
                var titleText = $"{LocalizeManager.GetText(LocalizedTextType.Lv, level + 1)}-{placeNumber + 1}";
                var stateText = isEmpty
                    ? LocalizeManager.GetText(LocalizedTextType.Empty)
                    : LocalizeManager.GetText(LocalizedTextType.Raiding);
                var buttonText = isEmpty
                    ? LocalizeManager.GetText(LocalizedTextType.Raid)
                    : LocalizeManager.GetText(LocalizedTextType.Plunder);
                string valueText;
                var timeText = string.Empty;
                var userInfoText = string.Empty;

                if (isEmpty)
                {
                    var remainingTimeToSeconds = DataController.Instance.raid.GetTotalDuration(level);
                    var remainTimeToDhms = Utility.GetTimeStringToFromTotalSecond(remainingTimeToSeconds);
                    var maxValueWithinRemainingTime =
                        DataController.Instance.raid.GetValueWithProgressSeconds(level, remainingTimeToSeconds);

                    timeText = $"{LocalizeManager.GetText(LocalizedTextType.Duration)} : {remainTimeToDhms}";
                    valueText =
                        $"<size=130%><sprite=2> <b><color=orange>{maxValueWithinRemainingTime.ToGoodString(goodType)}</color></b></size> " +
                        $"{LocalizeManager.GetText(LocalizedTextType.Earnable)}";
                }
                else
                {
                    var lootableValue = DataController.Instance.raid.GetLootableValue(raidUserData);
                    userInfoText = GetUserInfoText(raidUserData);
                    valueText =
                        $"<size=130%><sprite=2> <b><color=orange>{lootableValue.ToGoodString(goodType)}</color></b></size> " +
                        $"{LocalizeManager.GetText(LocalizedTextType.Plunderable)}";
                }

                View
                    .SetTitleText(titleText)
                    .SetStateText(stateText)
                    .SetUserInfoText(userInfoText)
                    .SetValueText(valueText)
                    .SetEnterButtonText(buttonText)
                    .SetDurationText(timeText);

                _currLevel = level;
                _currPlaceNumber = placeNumber;

                View.ViewRaidInfoPopup.Open();
                Get<ControllerCanvasToastMessage>().CloseLoading();
            });
        }

        private void UpdateMyState()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();

            if (!DataController.Instance.raid.TryGetMyRaidData(out _myRaidData))
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                Get<ControllerCanvasToastMessage>().CloseLoading();
                Close();
                return;
            }

            var stateText = string.Empty;
            var buttonText = string.Empty;
            var attackerNickname = string.Empty;
            
            var value = DataController.Instance.raid.GetValueWithProgressSeconds(_myRaidData);
            var valueColor = "orange";
            
            View.RaidCancelButton.gameObject.SetActive(true);
            _remainTimeSpen = GetMyRaidState() == RaidState.Empty 
                ? TimeSpan.Zero : DataController.Instance.raid.GetRemainingTimeSpan(_myRaidData);

            var myState = GetMyRaidState();
            SetOnOffImage(myState);
            switch (myState)
            {
                case RaidState.Raiding:
                    stateText = LocalizeManager.GetText(LocalizedTextType.Raiding);
                    buttonText = LocalizeManager.GetText(LocalizedTextType.Stop);
                    break;
                case RaidState.RaidCompleate:
                    stateText = LocalizeManager.GetText(LocalizedTextType.Raiding);
                    buttonText = LocalizeManager.GetText(LocalizedTextType.Settle);
                    break;
                case RaidState.Empty:
                    value = 0;
                    stateText = LocalizeManager.GetText(LocalizedTextType.Empty);
                    View.RaidCancelButton.gameObject.SetActive(false);
                    break;
                case RaidState.Raided:
                    stateText = LocalizeManager.GetText(LocalizedTextType.Raided);
                    buttonText = LocalizeManager.GetText(LocalizedTextType.Settle);
                    value -= _myRaidData.LostValue;
                    valueColor = Utility.GetRedColorToHex();
                    attackerNickname = LocalizeManager.GetText(LocalizedTextType.AttackerDesc,$"<color={valueColor}>{_myRaidData.LastAttacker}</color>");
                    break;
            }
            View.Reddot.ShowReddot(myState is RaidState.RaidCompleate or RaidState.Raided);
            UpdateStateAnimation(myState);

            var isMinus = value < 0;
            value = Math.Abs(value);
            var timeText = Utility.GetTimeStringToFromTotalSecond(_remainTimeSpen);
            var fillAmount = GetMyProccessFillAmount();
            var valueText = 
                $"<sprite=2> <color={valueColor}>" +
                (isMinus ? "-" : "") +
                $"{value.ToGoodString(GoodType.DarkDia)}</color>";

            View.SetMyStateTMP(stateText)
                .SetCurrValueTMP(valueText)        
                .SetCurrTimeTMP(timeText)
                .SetMyRaidCancelButtonTMP(buttonText)
                .SetCurrFillAmount(fillAmount)
                .SetAttackerNicknameText(attackerNickname);
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private float GetMyProccessFillAmount()
        {
            if (GetMyRaidState() == RaidState.Empty) return 0;
            float fillAmount;
            try
            {
                fillAmount = (float)DataController.Instance.raid.GetProgressTimeSpan(_myRaidData).TotalSeconds 
                             / DataController.Instance.raid.GetTotalDuration(_myRaidData?.Level ?? 0);
            }
            catch (Exception e)
            {
                fillAmount = 0;
            }

            return fillAmount;
        }

        private async UniTaskVoid AsyncRaidDatas(UnityAction callback = null)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            _raidUserDatasDic ??= new Dictionary<int, List<RaidData>>();

            foreach (var key in _raidUserDatasDic.Keys)        
            {
                _raidUserDatasDic[key].Clear();
            }
            
            DataController.Instance.raid.GetRaidDatas(raidDatas =>
            {
                foreach (var raidData in raidDatas)
                {
                    var level = raidData.Level;
                    if(!_raidUserDatasDic.ContainsKey(level)) _raidUserDatasDic.Add(level, new List<RaidData>());

                    var isChange = false;
                    for (var i = 0; i < _raidUserDatasDic[level].Count; ++i)
                    {
                        if (_raidUserDatasDic[level][i].PlaceNumber == raidData.PlaceNumber)
                        {
                            isChange = true;
                            if (raidData.RaidEndTime >= _raidUserDatasDic[level][i].RaidEndTime)
                            {
                                _raidUserDatasDic[level][i] = raidData;
                            }

                            break;
                        }
                    }

                    if (!isChange)
                        _raidUserDatasDic[level].Add(raidData);
                }
            
                callback?.Invoke();
                Get<ControllerCanvasToastMessage>().CloseLoading();
            });
        }
        
        private void UpdateViewSlotRaidPlace(int level)
        {
            if(!_raidUserDatasDic.TryGetValue(level, out var raidUserDatas))
            {
                raidUserDatas = new List<RaidData>();
            }
            
            var placeCount = DataController.Instance.raid.GetPlaceCount(level);
            var slotCount = _viewSlotRaidPlaces.Count;
            for (; slotCount < placeCount; ++slotCount)
            {
                var viewSlot = InitialPlace();
                var index = slotCount;
                viewSlot.EnterButton.onClick.AddListener(() =>
                {
                    OpenPopupView(_levelButtonGroup.SelectedIndex, index);
                });
                _viewSlotRaidPlaces.Add(viewSlot);
            }

            var i = 0;
            for (; i < placeCount; ++i)
            {
                var viewSlot = _viewSlotRaidPlaces[i];
                var raidUser = raidUserDatas.Find(x => x.PlaceNumber == i);
                
                var goodType = DataController.Instance.raid.GetGoodType();
                var isEmpty = raidUser == null;
                var myPlace = !isEmpty && raidUser.OnwerInDate == Backend.UserInDate;
                var stateText = $"{LocalizeManager.GetText(LocalizedTextType.Lv, level + 1)}-{i + 1}\n";
                string firstText;
                string secondText;
                string buttonText;
            
                if (isEmpty)
                {
                    stateText += LocalizeManager.GetText(LocalizedTextType.Empty);

                    var remainingTimeToSeconds = DataController.Instance.raid.GetTotalDuration(level);
                    var remainTimeToDhms = Utility.GetTimeStringToFromTotalSecond(remainingTimeToSeconds);
                    var maxValueWithinRemainingTime =
                        DataController.Instance.raid.GetValueWithProgressSeconds(level, remainingTimeToSeconds);
                    
                    firstText = $"{LocalizeManager.GetText(LocalizedTextType.Duration)} : {remainTimeToDhms}";
                    secondText = $"<size=110%><sprite=2> <b><color=orange>{maxValueWithinRemainingTime.ToGoodString(goodType)}</color></b></size> " +
                                 LocalizeManager.GetText(LocalizedTextType.Earnable);
                    buttonText = LocalizeManager.GetText(LocalizedTextType.Raid);
                }
                else
                {
                    stateText += LocalizeManager.GetText(LocalizedTextType.Raiding);
                    
                    var lootableValue = DataController.Instance.raid.GetLootableValue(raidUser);
                    firstText = GetUserInfoText(raidUser);
                    secondText = $"<size=110%><sprite=2></size> <b><color=orange>{lootableValue.ToGoodString(goodType)}</color></b> " +
                                 LocalizeManager.GetText(LocalizedTextType.Plunderable);
                    buttonText = LocalizeManager.GetText(LocalizedTextType.Plunder);
                }

                var color = viewSlot.BackgroundImage.color;
                color.a = myPlace ? 0.5f : 1f;
                viewSlot.BackgroundImage.color = color;
                viewSlot.EnterButton.enabled = !myPlace;
                
                viewSlot
                    .SetStateText(stateText)
                    .SetMiddleFirstText(firstText)
                    .SetMiddleSecondText(secondText)
                    .SetButtonText(buttonText)
                    .SetPointOn(isEmpty)
                    .SetActive(true);
            }

            for (; i < _viewSlotRaidPlaces.Count; ++i)
            {
                _viewSlotRaidPlaces[i].SetActive(false);
            }
        }

        private string GetUserInfoText(RaidData raidData)
        {
            var nickname = raidData.Nickname;
            var isProtected = ServerTime.IsRemainingTimeUntilDisable(raidData.ProtectEndTime);
                    
            return $"{nickname}" + (isProtected ? $"<color=green>({LocalizeManager.GetText(LocalizedTextType.Protect)})</color>" : "") + 
                   $" - {LocalizeManager.GetText(LocalizedTextType.Stair, raidData.MaxStage)}";
        }

        private void InitialTabButtons()
        {
            foreach (var activeButton in View.TabActiveButtons)
            {
                _tabButtonGroup.AddListener(activeButton);
            }
        }

        private void InitialLevelButtons()
        {
            var maxLevel = DataController.Instance.raid.GetMaxLevel();
            for (var i = 0; i < maxLevel; ++i)
            {
                var activeButton = Object.Instantiate(View.LevelActiveButtonPrefab, View.LevelPanel);
                var levelText = $"{LocalizeManager.GetText(LocalizedTextType.Lv, i + 1)}";
                activeButton
                    .SetActiveButtonText(levelText)
                    .SetInactiveButtonText(levelText);
                
                activeButton.gameObject.SetActive(true);
                _levelButtonGroup.AddListener(activeButton);
            }
            
        }

        private ViewSlotRaidPlace InitialPlace()
        {
            return Object.Instantiate(View.ViewSlotRaidPlacePrefab, View.PlacePanel);
        }

        private void UpdateChangeStage(StageType type, int param0)
        {
            if (StageManager.Instance.PrevPlayingStageType == StageType.DarkDiaDungeon && type == StageType.Normal)
            {
                View.SetActive(true);
                _tabButtonGroup.Select(1);
                AsyncRaidDatas().Forget();
            }
        }

        private RaidState GetMyRaidState()
        {
            if (_myRaidData == null) return RaidState.None;
            
            if (_myRaidData.IsRaid)
            {
                var timeSpan = DataController.Instance.raid.GetRemainingTimeSpan(_myRaidData);
                return timeSpan.TotalSeconds <= 0 ? RaidState.RaidCompleate : RaidState.Raiding;
            }
            if (string.IsNullOrEmpty(_myRaidData.RaidEndTimeToString)) return RaidState.Empty;
            if (!string.IsNullOrEmpty(_myRaidData.LastAttacker)) return RaidState.Raided;
            
            return RaidState.None;
        }

        private void SetOnOffImage(RaidState state)
        {
            switch (state)
            {
                case RaidState.Raiding:
                case RaidState.RaidCompleate:
                    View.MyPointOnImage.enabled = true;
                    View.MyPointOffImage.enabled = false;
                    break;
                case RaidState.Raided:
                    View.MyPointOnImage.enabled = false;
                    View.MyPointOffImage.enabled = true;
                    break;
                default:
                    View.MyPointOnImage.enabled = false;
                    View.MyPointOffImage.enabled = false;
                    break;
            }
        }

        private void DiscountAndUpdateRefreshTime()
        {
            _refreshTime -= 1;
            _refreshTime = Mathf.Clamp(_refreshTime, 0, 30);
            View.SetLockRefreshButton(_refreshTime > 0)
                .SetRefreshTimeText($"{_refreshTime}s");
        }
    }

    enum RaidState
    {
        None = -1,
        Raiding,
        RaidCompleate,
        Raided,
        Empty
    }
}