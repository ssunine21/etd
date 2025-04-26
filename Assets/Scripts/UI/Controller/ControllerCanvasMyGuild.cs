using System;
using System.Collections.Generic;
using System.Threading;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using LitJson;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMyGuild : ControllerCanvas
    {
        private ViewCanvasMyGuild View => ViewCanvas as ViewCanvasMyGuild;
        private readonly List<ViewGood> _donationViewGoods = new();
        private readonly List<ViewSlotGuildInfo> _rankingSlots = new();
        private readonly List<ViewSlotGuildApplicant> _guildApplicants = new();
        private const int AsyncRankingSec = 600;

        private List<GuildInfo> _guildInfos = new();
        private GuildInfo _myGuildInfo;
        private int _neededStageLevel;
        private int _currRankingSec;
        private int _tempDonationCount;

        private Dictionary<ReddotType, Reddot> _baseReddot = new();

        public ControllerCanvasMyGuild(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasMyGuild>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.DonationViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.ManageViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.RankingViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.RaidBoxViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            
            _tempDonationCount = DataController.Instance.guildReward.donationCount;
            View.DonationViewCanvasPopup.Close();
            View.ManageViewCanvasPopup.Close();
            View.RankingViewCanvasPopup.Close();
            View.ApplicantViewCanvasPopup.Close();
            View.DonationViewGoodPrefab.SetActive(false);
            View.ViewSlotGuildApplicantPrefab.SetActive(false);

            InitMember();
            InitRaid();
            InitShop();

            foreach (var exitButton in View.GuildExitButtons) exitButton.onClick.AddListener(ShowGuildExitPanel);
            View.ShowDonationButton.OnClick.AddListener(() =>
            {
                {
                    View.DonationViewCanvasPopup.Open();
                    UpdateDonationBox(DataController.Instance.guildReward.GetRewardGoodItem(GuildRewardType.Donation, DataController.Instance.guildReward.donationCount));
                }
            });
            
            View.DonationButton.OnClick.AddListener(GetDonationReward);
            View.GuildManageButton.onClick.AddListener(OpenManagePopup);
            View.AutoJoinCheckBox.Toggle.onValueChanged.AddListener(isOn => UpdateJoinTypeCheckBox(true, isOn));
            View.ApproveJoinCheckBox.Toggle.onValueChanged.AddListener(isOn => UpdateJoinTypeCheckBox(false, isOn));
            View.GuildInfoChangeButton.onClick.AddListener(ChangeGuildInfo);
            View.GuildManageStageSlider.onValueChanged.AddListener(UpdateNeededStage);
            View.GiftBoxRewardButton.OnClick.AddListener(GetGiftBoxReward);
            View.SlideButton.AddListener(OpenMenuPanel);
            View.CloseButton.onClick.AddListener(Close);
            View.GiftBoxDescButton.onClick.AddListener(() => ShowDescriptionView(LocalizeManager.GetText(LocalizedTextType.Guild_GiftBoxDesc)));
            
            View.ShowRankingButton.OnClick.AddListener(UpdateRanking);
            
            View.ShowApplicantButton.OnClick.AddListener(() =>
            {
                UpdateApplicantView();
                View.ApplicantViewCanvasPopup.Open();
            });

            View.DonationViewCanvasPopup.OnBindClose += async () =>
            {
                if (_tempDonationCount != DataController.Instance.guildReward.donationCount)
                {
                    Get<ControllerCanvasToastMessage>().ShowLoading();
                    await AsyncMyGuild();
                    UpdateInfo();
                    UpdateGiftBox();
                    Get<ControllerCanvasToastMessage>().CloseLoading();
                }
            };
            
            ServerTime.onBindNextDay += () =>
                DataController.Instance.guildReward.donationCount = 0;

            TimeTask().Forget();
            AsyncMyGuild().Forget();
        }

        public override async void Open()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading(Close);
            var isSetMyGuild = await AsyncMyGuild();
            Get<ControllerCanvasToastMessage>().CloseLoading();

            if (isSetMyGuild)
            {
                if (!DataController.Instance.guild.hasGuild)
                {
                    var giftboxPoint = _myGuildInfo.TotalGiftBoxPoint;
                    foreach (GradeType gradeType in Enum.GetValues(typeof(GradeType)))
                    {
                        DataController.Instance.guild.SetRaidBoxRewardTime(gradeType);
                    }
                    DataController.Instance.good.SetValue(GoodType.GuildGiftBoxPoint, giftboxPoint);
                    DataController.Instance.guild.hasGuild = true;
                }
                
                base.Open();
                
                var amIMaster = _myGuildInfo.IsMaster(Backend.UserInDate);
                UpdateInfo();
                UpdateGiftBox();
                UpdateDonationBox(null);
                if (amIMaster) UpdateApplicantView();
                View.SlideButton.OnClick(0);
            }
        }

        private async UniTaskVoid TimeTask()
        {
            while (!Cts.IsCancellationRequested)
            {
                if (!DataController.Instance.guildReward.IsDonationable)
                {
                    var remainTime = Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextDay);
                    View.SetDonationTime(remainTime);
                }

                _currRankingSec--;
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private async UniTask<bool> AsyncMyGuild()
        {
            await BackendGuildManager.Instance.SaveGuildData();
            var json = await BackendGuildManager.Instance.GetMyGuild();
            if(json.ContainsKey("guild"))
            {
                _myGuildInfo = new GuildInfo(json["guild"]);
                _myGuildInfo.MemberItems = await BackendGuildManager.Instance.GetGuildMember(_myGuildInfo.InDate);
                _myGuildInfo.Goods = await BackendGuildManager.Instance.GetGoods(_myGuildInfo.InDate);
                
                UpdateReddot(ReddotType.GuildDonation);
                UpdateReddot(ReddotType.GuildGiftBox);
                UpdateReddot(ReddotType.GuildRaidBox);
                return true;
            }
            return false;
        }

        private void UpdateInfo()
        {
            View.ViewSlotGuildInfo
                .SetName(_myGuildInfo.GuildName)
                .SetMarkImage(_myGuildInfo.MarkBackgroundIndex, _myGuildInfo.MarkMainIndex, _myGuildInfo.MarkSubIndex)
                .SetLevel(_myGuildInfo.Level)
                .SetExp(_myGuildInfo.CurrExp, DataController.Instance.guild.GetMaxExp(_myGuildInfo.Level))
                .SetMemberCount($"{_myGuildInfo.MemberCount}/{DataController.Instance.guild.GetMaxMemeberCount(_myGuildInfo.Level)}")
                .SetCombat(_myGuildInfo.TotalCombat)
                .SetDescription(_myGuildInfo.Description)
                .SetJoinType(_myGuildInfo.ImmediateRegistration)
                .SetNeededStage(_myGuildInfo.NeededStage);

            var amIMaster = _myGuildInfo.IsMaster(Backend.UserInDate);
            View.GuildManageButton.gameObject.SetActive(amIMaster);
            View.ShowApplicantButton.gameObject.SetActive(amIMaster);
        }

        private void UpdateGiftBox()
        {
            var localizeType = DataController.Instance.guildReward.GetLocalizeTextType(GuildRewardType.GiftBox, _myGuildInfo.GiftBoxStep);
            var currPoint = _myGuildInfo.CurrGiftBoxPoint;
            var maxPoint = DataController.Instance.guildReward.GetNeedValue(GuildRewardType.GiftBox, _myGuildInfo.GiftBoxStep);

            View
                .SetGiftBoxTitle(LocalizeManager.GetText(localizeType))
                .SetGiftKeyPoint(currPoint, maxPoint)
                .SetGiftBoxExp(_myGuildInfo.CurrGiftBoxExp, DataController.Instance.guildReward.GetMaxGiftBoxExp(_myGuildInfo.GiftBoxStep));

            View.GiftBoxRewardButton.Selected(currPoint >= maxPoint);
            UpdateReddot(ReddotType.GuildGiftBox);
        }

        private void UpdateDonationBox(List<GoodItem> goodItems)
        {
            var donationable = DataController.Instance.guildReward.IsDonationable;

            View.ShowDonationButton.Selected(donationable);
            View.DonationButton.Selected(donationable);

            var donationCount = DataController.Instance.guildReward.donationCount;
            var needGoodItem = DataController.Instance.guildReward.GetNeedGoodItem(GuildRewardType.Donation, donationCount);
            if (needGoodItem != null)
            {
                View.DonationViewGood
                    .SetInit(needGoodItem.GoodType)
                    .SetValue(needGoodItem.Value);
            }

            if(goodItems != null)
            {
                var i = 0;
                foreach (var goodItem in goodItems)
                {
                    var viewGood = GetDonationViewGood(i);
                    viewGood
                        .SetInit(goodItem.GoodType, goodItem.Param0)
                        .SetValue(goodItem.Value, goodItem.Param0)
                        .SetActive(true);
                    ++i;
                }

                for (; i < _donationViewGoods.Count; ++i)
                {
                    _donationViewGoods[i].SetActive(false);
                }
            }
            UpdateReddot(ReddotType.GuildDonation);
        }

        private async void UpdateRanking()
        {
            View.MyRankingViewSlot.SetActive(false);
                
            if (_currRankingSec < 0)
            {
                Get<ControllerCanvasToastMessage>().ShowLoading(() => View.RankingViewCanvasPopup.Close());
                _currRankingSec = AsyncRankingSec;
                _guildInfos = await BackendGuildManager.Instance.GetGuildRank();
                var myRankInfo = await BackendGuildManager.Instance.GetMyGuildRank();
                _myGuildInfo.Rank = myRankInfo?.Rank ?? 99;
                Get<ControllerCanvasToastMessage>().CloseLoading();
            }
            
            var i = 0;
            foreach (var guildInfo in _guildInfos)
            {
                var slot = GetRankingSlot(i);
                slot
                    .SetRanking(guildInfo.Rank)
                    .SetActiveRankSlot(true)
                    .SetMarkImage(guildInfo.MarkBackgroundIndex, guildInfo.MarkMainIndex, guildInfo.MarkSubIndex)
                    .SetName(guildInfo.GuildName)
                    .SetCombat(guildInfo.TotalCombat)
                    .SetLevel(guildInfo.Level)
                    .SetMemberCount(guildInfo.MemberCount, DataController.Instance.guild.GetMaxMemeberCount(guildInfo.Level))
                    .SetJoinType(guildInfo.ImmediateRegistration)
                    .SetActive(true);
                ++i;
            }
            for (; i < _rankingSlots.Count; ++i)
            {
                _rankingSlots[i].SetActive(false);
            }
            
            View.MyRankingViewSlot
                .SetRanking(_myGuildInfo.Rank)
                .SetActiveRankSlot(true)
                .SetMarkImage(_myGuildInfo.MarkBackgroundIndex, _myGuildInfo.MarkMainIndex, _myGuildInfo.MarkSubIndex)
                .SetName(_myGuildInfo.GuildName)
                .SetCombat(_myGuildInfo.TotalCombat)
                .SetLevel(_myGuildInfo.Level)
                .SetMemberCount(_myGuildInfo.MemberCount, DataController.Instance.guild.GetMaxMemeberCount(_myGuildInfo.Level))
                .SetJoinType(_myGuildInfo.ImmediateRegistration)
                .SetActive(true);

            View.RankingViewCanvasPopup.Open();
        }
        
        private async void UpdateApplicantView()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var memberItems = await BackendGuildManager.Instance.GetApplicants();
            View.EmptyApplicantListTMPGo.SetActive(memberItems.Count == 0);
            
            var i = 0;
            foreach (var memberItem in memberItems)
            {
                var slot = GetSlotGuildApplicant(i);
                slot
                    .SetNickname(memberItem.Nickname)
                    .SetCombat(memberItem.Combat)
                    .SetStage(memberItem.MaxStage)
                    .SetActive(true);
                ++i;
                
                slot.ApplyButton.onClick.RemoveAllListeners();
                slot.RejectButton.onClick.RemoveAllListeners();
                
                slot.ApplyButton.onClick.AddListener(() => ApproveApplicant(memberItem.GamerInDate));
                slot.RejectButton.onClick.AddListener(() => RejectApplicant(memberItem.GamerInDate));
            }

            for (; i < _guildApplicants.Count; ++i)
            {
                _guildApplicants[i].SetActive(false);
            }
            
            UpdateReddot(ReddotType.GuildApplicant);
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private async void ApproveApplicant(string gamerIndate)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var isSuccess = await BackendGuildManager.Instance.ApproveApplicant(gamerIndate);
            
            await AsyncMyGuild();
            UpdateViewMember();
            UpdateApplicantView();
            
            if (isSuccess)
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_Approved);
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private async void RejectApplicant(string gamerIndate)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var isSuccess = await BackendGuildManager.Instance.RejectApplicant(gamerIndate);
            await AsyncMyGuild();
            UpdateViewMember();
            UpdateApplicantView();
            
            if (isSuccess)
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_Reject);
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private void ShowDescriptionView(string text)
        {
            Get<ControllerCanvasToastMessage>().SetToastMessage(string.Empty, text).ShowToastMessage();
        }

        private ViewSlotGuildApplicant GetSlotGuildApplicant(int index)
        {
            var count = _guildApplicants.Count;
            for (var i = count; i <= index; ++i)
            {
                _guildApplicants.Add(Object.Instantiate(View.ViewSlotGuildApplicantPrefab, View.ApplicantSlotParent));
            }

            return _guildApplicants[index];
        }
        
        private async UniTask<bool> SetGuildList(JsonData jsonData)
        {
            try
            {
                _guildInfos.Clear();
                for (var i = 0; i < jsonData.Count; ++i)
                {
                    GuildInfo guildInfo = null;
                    try
                    {
                        guildInfo = new GuildInfo(jsonData[i]);
                        guildInfo.MemberItems = await BackendGuildManager.Instance.GetGuildMember(guildInfo.InDate);
                    }
                    catch (Exception e)
                    {
                        FirebaseManager.LogError(e);
                    }

                    if (guildInfo != null)
                        _guildInfos.Add(guildInfo);
                }
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                return false;
            }

            return true;
        }

        private ViewSlotGuildInfo GetRankingSlot(int index)
        {
            var count = _rankingSlots.Count;
            for (var i = count; i <= index; ++i)
            {
                _rankingSlots.Add(Object.Instantiate(View.MyRankingViewSlot, View.RankingSlotParent));
            }

            return _rankingSlots[index];
        }

        private ViewGood GetDonationViewGood(int index)
        {
            if (index <= _donationViewGoods.Count)
                _donationViewGoods.Add(Object.Instantiate(View.DonationViewGoodPrefab, View.DonationViewGoodParent));

            return _donationViewGoods[index];
        }

        private void OpenManagePopup()
        {
            UpdateViewGuildManage();
            UpdateJoinTypeCheckBox(_myGuildInfo.ImmediateRegistration, true);
            View.ManageViewCanvasPopup.Open();
        }

        private void UpdateViewGuildManage()
        {
            View.ManageGuildInfo
                .SetMarkImage(_myGuildInfo.MarkBackgroundIndex, _myGuildInfo.MarkMainIndex, _myGuildInfo.MarkSubIndex)
                .SetName(_myGuildInfo.GuildName)
                .SetNeededStage(_myGuildInfo.NeededStage);

            View.DescInputField.text = _myGuildInfo.Description;
        }

        private void UpdateJoinTypeCheckBox(bool isAutoJoin, bool isOn)
        {
            var isAutoJoinOn = isAutoJoin && isOn;

            View.AutoJoinCheckBox.Toggle.enabled = !isAutoJoinOn;
            View.ApproveJoinCheckBox.Toggle.enabled = isAutoJoinOn;

            View.AutoJoinCheckBox.Toggle.SetIsOnWithoutNotify(isAutoJoinOn);
            View.ApproveJoinCheckBox.Toggle.SetIsOnWithoutNotify(!isAutoJoinOn);
        }

        private async void ChangeGuildInfo()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading(() => View.ManageViewCanvasPopup.Close());

            var descText = View.DescInputField.text;
            var immediateRegistration = View.AutoJoinCheckBox.Toggle.isOn;

            var param = new Param()
            {
                { BackendGuildManager.DescriptionKey, descText },
                { BackendGuildManager.NeededStageKey, _neededStageLevel }
            };

            var isModifySuccess = await BackendGuildManager.Instance.ModifyGuild(param);
            if (isModifySuccess)
            {
                var isRegistrationSuccess = await BackendGuildManager.Instance.SetRegistrationValue(immediateRegistration);
                var localizeType = isRegistrationSuccess ? LocalizedTextType.Nickname_ChangeNicknameSuccess : LocalizedTextType.ErrorMessage;

                await AsyncMyGuild();

                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(localizeType);
            }

            UpdateInfo();
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
            View.ManageViewCanvasPopup.Close();
        }

        private void ShowGuildExitPanel()
        {
            var toastCanvas = Get<ControllerCanvasToastMessage>();
            var title = LocalizeManager.GetText(LocalizedTextType.Warring);
            var desc = LocalizeManager.GetText(LocalizedTextType.Guild_WithdrawWarringMessage);
            var confirm = LocalizeManager.GetText(LocalizedTextType.Confirm);
            var cancel = LocalizeManager.GetText(LocalizedTextType.Cancel);
            toastCanvas.SetToastMessage(title, desc,
                    cancel, null,
                    confirm, GuildExit)
                .ShowToastMessage();
        }

        private async void GuildExit()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var isSuccess = await BackendGuildManager.Instance.WithdrawGuild();
            if (isSuccess)
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_WithdrawSuccessMessage);             
                DataController.Instance.guild.hasGuild = false;
                DataController.Instance.guild.SetGuildExitTime(ServerTime.Date);
                DataController.Instance.LocalSave();
                Close();
            }
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private void UpdateNeededStage(float value)
        {
            var stageLevel = DataController.Instance.guild.GetNeedStageLevel(value);
            _neededStageLevel = stageLevel;
            View.ManageGuildInfo.SetNeededStage(stageLevel);
        }

        private async void GetGiftBoxReward()
        {
            var isSuccess = await GetReward(GuildRewardType.GiftBox, _myGuildInfo.GiftBoxStep);
            if (isSuccess)
            {
                Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(DataController.Instance.guildReward.GetRewardGoodItem(GuildRewardType.GiftBox, _myGuildInfo.GiftBoxStep), LocalizeManager.GetText(LocalizedTextType.Guild_GiftBox)).Forget();
                DataController.Instance.good.EarnReward(GoodType.GuildGiftBoxPoint, DataController.Instance.guildReward.GetNeedValue(GuildRewardType.GiftBox, _myGuildInfo.GiftBoxStep));
            }
            
            UpdateGiftBox();
        }

        private async void GetDonationReward()
        {
            var isSuccess = await GetReward(GuildRewardType.Donation, DataController.Instance.guildReward.donationCount);
            if (isSuccess)
            {
                var goodItems = DataController.Instance.guildReward.GetRewardGoodItem(GuildRewardType.Donation, DataController.Instance.guildReward.donationCount);
                Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(goodItems, LocalizeManager.GetText(LocalizedTextType.Guild_Donation)).Forget();
                
                var donationNeededGoodItem = DataController.Instance.guildReward.GetNeedGoodItem(GuildRewardType.Donation, DataController.Instance.guildReward.donationCount);
                DataController.Instance.good.TryConsume(donationNeededGoodItem, false);
                _tempDonationCount = DataController.Instance.guildReward.donationCount;
                
                DataController.Instance.guildReward.donationCount += 1;
                UpdateDonationBox(DataController.Instance.guildReward.GetRewardGoodItem(GuildRewardType.Donation, DataController.Instance.guildReward.donationCount));
                DataController.Instance.LocalSave();
            }
        }

        private async UniTask<bool> GetReward(GuildRewardType rewardType, int step)
        {
            var rewardList = DataController.Instance.guildReward.GetRewardGoodItem(rewardType, step);
            if (rewardList == null)
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
                return false;
            }

            Get<ControllerCanvasToastMessage>().ShowLoading();
            var defaultGoodItems = new List<GoodItem>();
            foreach (var reward in rewardList)
            {
                if (DataController.Instance.good.IsGuildGoods(reward.GoodType))
                {
                    var isSuccess = await BackendGuildManager.Instance.ContributeGoods(reward.GoodType, (int)reward.Value);
                    if (!isSuccess) return false;
                }
                else
                    defaultGoodItems.Add(new GoodItem());
            }

            
            DataController.Instance.good.EarnReward(defaultGoodItems);
            Get<ControllerCanvasToastMessage>().ShowSimpleRewardView(rewardList, LocalizeManager.GetText(LocalizedTextType.Claimed)).Forget();
            Get<ControllerCanvasToastMessage>().CloseLoading();
            return true;
        }
        
        private void OpenMenuPanel(int index)
        {
            var i = 0;
            foreach (var menuPanel in View.MenuPanels)
            {
                menuPanel.gameObject.SetActive(i == index);
                ++i;
            }
        }

        private void UpdateReddot(ReddotType type)
        {
            if (!DataController.Instance.guild.hasGuild) return;
            
            if (!_baseReddot.ContainsKey(type))
                _baseReddot[type] = new Reddot(null, type, new [] { ReddotType.None });

            switch (type)
            {
                case ReddotType.GuildDonation:
                    _baseReddot[type].IsOn = DataController.Instance.guildReward.donationCount < DataController.Instance.guildReward.GetMaxDonationCount();
                    break;
                case ReddotType.GuildGiftBox:
                    var currPoint = _myGuildInfo.CurrGiftBoxPoint;
                    var maxPoint = DataController.Instance.guildReward.GetNeedValue(GuildRewardType.GiftBox, _myGuildInfo.GiftBoxStep);
                    _baseReddot[type].IsOn = currPoint >= maxPoint;
                    break;
                case ReddotType.GuildApplicant:
                    _baseReddot[type].IsOn = _guildApplicants.Find(x => x.isActiveAndEnabled);
                    break;
                case ReddotType.GuildRaidBox:
                    _baseReddot[type].IsOn = _myGuildInfo.RaidBoxes.Count > 0;
                    break;
            }
            
            _baseReddot[type].OnBindShowReddot();
        }
    }
}