using System;
using System.Collections.Generic;
using System.Threading;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using JetBrains.Annotations;
using LitJson;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasGuild : ControllerCanvas
    {
        private ViewCanvasGuild View => ViewCanvas as ViewCanvasGuild;
        private readonly List<GuildInfo> _guildInfos = new();
        private readonly List<ViewSlotUI> _markSlotUIs = new();
        private readonly List<ViewSlotGuildInfo> _viewSlotGuildList = new();
        private readonly List<ViewSlotGuildMember> _viewSlotGuildMembers = new();
        private readonly Dictionary<MarkImageType, int> _markImages = new();
        private readonly ButtonRadioGroup _markSlotGroup = new();
        private const string ViewSlotGuildMemberName = "ViewSlotGuildMember";

        private int _refreshTime = 0;
        private int _neededStageLevel;
        private bool hasReachedBottom;

        public ControllerCanvasGuild(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasGuild>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.GuildInfoViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.MarkCorrectionViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            
            View.MarkCorrectionViewCanvasPopup.Close();
            View.GuildInfoViewCanvasPopup.Close();
            View.ViewSlotUIPrefab.SetActive(false);
            View.ViewSlotGuildListPrefab.SetActive(false);
            View.ViewSlotGuildMember.SetActive(false);
            
            View.RefreshListButton.Button.onClick.AddListener(() => RefreshTime().Forget());
            View.SlideButton.AddListener(index =>
            {
                View.RecommendView.SetActive(index == 0);
                View.CreationView.SetActive(index == 1);
                if (index != 0) InitCreatedInfo();
            });
            View.OpenMarkCorrectionButton.Button.onClick.AddListener(OpenMarkCorrectionView);
            View.AutoJoinCheckBox.Toggle.onValueChanged.AddListener(isOn => UpdateJoinTypeCheckBox(true, isOn));
            View.ApproveJoinCheckBox.Toggle.onValueChanged.AddListener(isOn => UpdateJoinTypeCheckBox(false, isOn));
            View.NeededStageSlider.onValueChanged.AddListener(UpdateNeededStage);
            View.ViewGoodForCreation.SetInit(GoodType.Dia, DataController.Instance.guild.GetGuildCreationCost());
            View.CreationButton.Button.onClick.AddListener(TryCreateGuild);
            
            View.MarkTypeSlideButton.AddListener(UpdateMarkViewSlotUI);
            View.ConfirmButton.onClick.AddListener(OnConfirmMarkImage);
            View.RefreshListButton.Selected(true);
            _markSlotGroup.onBindSelected += OnChangeMarkImageValue;
            
            View.GuildListScrollRect.onValueChanged.AddListener(OnScrollChanged);
            
            foreach (MarkImageType markImageType in Enum.GetValues(typeof(MarkImageType))) 
                SetMarkImage(markImageType, 0);
        }

        public override async void Open()
        {
            await GetGuildList(true);
            
            base.Open();
            View.GuildInfoViewCanvasPopup.SetActive(false);
            View.SlideButton.OnClick(0);
            View.SlideButton.AsyncNormailzedSize();
            DataController.Instance.guild.hasGuild = false;
        }
        
        private void InitCreatedInfo()
        {
            UpdateJoinTypeCheckBox(true, true);
            View.NameInputField.text = string.Empty;
            View.InfoInputField.text = string.Empty;
        }
        
        private void OpenMarkCorrectionView()
        {
            View.MarkCorrectionViewCanvasPopup.Open();
            View.MarkTypeSlideButton.OnClick(0);
            _markSlotGroup.Select(0);
        }

        private async UniTaskVoid RefreshTime()
        {
            if (!View.RefreshListButton.IsSelected) return;
            View.RefreshListButton.Selected(false);
            GetGuildList(true).Forget();
            
            var time = 30;
            while (time > 0)
            {
                View.RefreshListButton.SetInactiveButtonText($"{time}s");
                time--;

                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            }
            View.RefreshListButton.Selected(true);
        }

        private void UpdateGuildList()
        {
            var i = 0;
            foreach (var guildInfo in _guildInfos)
            {
                try
                {
                    var slot = GetViewSlotGuildList(i);
                    if(!slot) continue;
                    slot.SetName(guildInfo.GuildName)
                        .SetCombat(guildInfo.TotalCombat)
                        .SetLevel(guildInfo.Level)
                        .SetJoinType(guildInfo.ImmediateRegistration)
                        .SetMemberCount(guildInfo.MemberCount, DataController.Instance.guild.GetMaxMemeberCount(guildInfo.Level))
                        .SetMarkImage(guildInfo.MarkBackgroundIndex, guildInfo.MarkMainIndex, guildInfo.MarkSubIndex);

                    slot.JoinButton.onClick.RemoveAllListeners();

                    var index = guildInfo;
                    slot.JoinButton.onClick.AddListener(() => ShowSelectedGuildInfo(index));
                    slot.SetActive(true);
                }
                catch (NullReferenceException e)
                {
                    FirebaseManager.LogError(e);
                    continue;
                }

                ++i;
            }

            View.EmptyGuildText.SetActive(i == 0);
            for (; i < _viewSlotGuildList.Count; ++i)
            {
                _viewSlotGuildList[i].SetActive(false);
            }
        }
        
        private void OnScrollChanged(Vector2 position)
        {
            if (View.GuildListScrollRect.content.childCount == 0 || View.GuildListScrollRect.content.rect.height <= 0) return;
            
            if (View.GuildListScrollRect.verticalNormalizedPosition <= 0.01f && !hasReachedBottom)
            {
                hasReachedBottom = true;
                GetGuildList(false).Forget();
            }
            else if (View.GuildListScrollRect.verticalNormalizedPosition > 0.01f)
            {
                hasReachedBottom = false;
            }
        }

        private async UniTask GetGuildList(bool isRefresh)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            if (isRefresh)
                _guildInfos.Clear();
            
            var json = await BackendGuildManager.Instance.GetGuildList(10, !isRefresh);
            var isSuccess = await SetGuildList(json);
            UpdateGuildList();

            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private async UniTask<bool> SetGuildList(JsonData jsonData)
        {
            try
            {
                if (!jsonData.IsArray) return false;
                
                for (var i = 0; i < jsonData.Count; ++i)
                {
                    GuildInfo guildInfo = null;
                    try
                    {
                        guildInfo = new GuildInfo(jsonData[i]);
                        guildInfo.Goods = await BackendGuildManager.Instance.GetGoods(guildInfo.InDate);
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

        private void UpdateJoinTypeCheckBox(bool isAutoJoin, bool isOn)
        {
            var isAutoJoinOn = isAutoJoin && isOn;
            
            View.AutoJoinCheckBox.Toggle.enabled = !isAutoJoinOn;
            View.ApproveJoinCheckBox.Toggle.enabled = isAutoJoinOn;
            
            View.AutoJoinCheckBox.Toggle.SetIsOnWithoutNotify(isAutoJoinOn);
            View.ApproveJoinCheckBox.Toggle.SetIsOnWithoutNotify(!isAutoJoinOn);
        }

        private void UpdateNeededStage(float value)
        {
            var stageLevel = DataController.Instance.guild.GetNeedStageLevel(value);
            _neededStageLevel = stageLevel;
            View.GuildInfo.SetNeededStage(stageLevel);
        }

        private void UpdateMarkViewSlotUI(int markType)
        {
            var sprites = GetMarkSprites((MarkImageType)markType);
            var i = 0;
            foreach (var sprite in sprites)
            {
                var slotUI = GetMarkSlotUI(i);
                if (slotUI == null) break;
                
                slotUI
                    .SetUnitSprite(sprite)
                    .SetActive(true);
                ++i;
            }

            for (; i < _markSlotUIs.Count; ++i)
            {
                _markSlotUIs[i].SetActive(false);
            }
            
            Utility.NormalizeGridLayoutSize(View.ViewSlotUIParent, View.ViewSlotUIParent.constraintCount);
        }
        
        private Sprite[] GetMarkSprites(MarkImageType imageType)
        {
            return imageType switch
            {
                MarkImageType.Background => ResourcesManager.Instance.guildMarkBackgroundSprites,
                MarkImageType.MainSymbol => ResourcesManager.Instance.guildMarkMainSymbolSprites,
                MarkImageType.SubSymbol => ResourcesManager.Instance.guildMarkSubSymbolSprites,
                _ => Array.Empty<Sprite>()
            };
        }

        private ViewSlotUI GetMarkSlotUI(int index)
        {
            try
            {
                var listCount = _markSlotUIs.Count;
                for (var i = listCount; i <= index; ++i)
                {
                    var slotUI = Object.Instantiate(View.ViewSlotUIPrefab, View.ViewSlotUIParent.transform);
                    _markSlotUIs.Add(slotUI);
                    _markSlotGroup.AddListener(slotUI.ActiveButton);
                }

                return _markSlotUIs[index];
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                return null;
            }
        }

        [CanBeNull]
        private ViewSlotGuildInfo GetViewSlotGuildList(int index)
        {
            try
            {
                var listCount = _viewSlotGuildList.Count;
                for (var i = listCount; i <= index; ++i)
                {
                    var slot = Object.Instantiate(View.ViewSlotGuildListPrefab, View.GuildListScrollRect.content);
                    _viewSlotGuildList.Add(slot);
                }

                return _viewSlotGuildList[index];
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
                return null;
            }
        }

        private void OnConfirmMarkImage()
        {
            View.MarkCorrectionViewCanvasPopup.Close();
        }

        private void OnChangeMarkImageValue(int index)
        {
            var markImageType = (MarkImageType)View.MarkTypeSlideButton.SelectedIndex;

            _markImages[markImageType] = index;
            SetMarkImage(markImageType, index);
        }

        private void SetMarkImage(MarkImageType imageType, int index)
        {
            _markImages[imageType] = index;
            View.MarkInfo.SetMarkImage(imageType, index);
            View.GuildInfo.SetMarkImage(imageType, index);
        }

        private async void TryCreateGuild()
        {
            if (!DataController.Instance.guild.CanJoinGuildNow())
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_JoinErrorMessage);
                return;
            }
            
            if (DataController.Instance.good.GetValue(GoodType.Dia) >= 3000)
            {
                var guildName = View.NameInputField.text;
                var param = new Param
                {
                    { BackendGuildManager.MarkBackgroundImageKey, _markImages[MarkImageType.Background] },
                    { BackendGuildManager.MarkMainImageKey, _markImages[MarkImageType.MainSymbol] },
                    { BackendGuildManager.MarkSubImageKey, _markImages[MarkImageType.SubSymbol] },
                    { BackendGuildManager.NeededStageKey, _neededStageLevel },
                    { BackendGuildManager.DescriptionKey, View.InfoInputField.text }
                };

                Get<ControllerCanvasToastMessage>().ShowLoading();
                var immediateRegistration = View.AutoJoinCheckBox.Toggle.isOn;
                var isSuccess = await BackendGuildManager.Instance.TryCreateGuild(guildName, immediateRegistration, param);

                if (isSuccess)
                {
                    View.Close();
                    Get<ControllerCanvasMainMenu>().OpenGuildCanvas();
                    Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_JoinSuccess); 
                    DataController.Instance.good.TryConsume(GoodType.Dia, 3000);
                }
            }
            else
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
            }
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private void ShowSelectedGuildInfo(GuildInfo guildInfo)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            try
            {
                View.SetJoinButtonText(guildInfo.ImmediateRegistration ? LocalizeManager.GetText(LocalizedTextType.Guild_Join) : LocalizeManager.GetText(LocalizedTextType.Guild_ApplyJoin));
                View.SelectedGuildInfo
                    .SetMarkImage(guildInfo.MarkBackgroundIndex, guildInfo.MarkMainIndex, guildInfo.MarkSubIndex)
                    .SetName(guildInfo.GuildName)
                    .SetLevel(guildInfo.Level)
                    .SetMemberCount($"{guildInfo.MemberCount}/{DataController.Instance.guild.GetMaxMemeberCount(guildInfo.Level)}")
                    .SetCombat(guildInfo.TotalCombat)
                    .SetNeededStage(guildInfo.NeededStage)
                    .SetJoinType(guildInfo.ImmediateRegistration)
                    .SetDescription(guildInfo.Description);

                var viewSlots = _viewSlotGuildMembers.GetViewSlots(ViewSlotGuildMemberName, View.GuildMemberParent.transform, guildInfo.MemberItems.Count);
                var i = guildInfo.MemberItems.Count == 1 ? 0 : 1;
                foreach (var guildMember in guildInfo.MemberItems)
                {
                    var index = guildMember.IsMaster ? 0 : i;
                    var viewSlotGuildMember = viewSlots[index];
                    if (viewSlotGuildMember)
                    {
                        viewSlotGuildMember
                            .SetMark(guildMember.IsMaster)
                            .SetActiveMasterTag(guildMember.IsMaster)
                            .SetNickname(guildMember.Nickname)
                            .SetCombat(guildMember.Combat.ToDamage())
                            .SetCurrStage(guildMember.MaxStage)
                            .SetActiveInfoWarp(false)
                            .SetActive(true);
                    }

                    ++i;
                }

                for (; i < _viewSlotGuildMembers.Count; ++i)
                    _viewSlotGuildMembers[i].SetActive(false);
                
                View.JoinButton.onClick.RemoveAllListeners();
                View.JoinButton.onClick.AddListener(() => JoinGuild(guildInfo));

                View.GuildInfoViewCanvasPopup.Open();
                Utility.NormalizeGridLayoutXSize(View.GuildMemberParent, 2);
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
            }
            finally
            {
                Get<ControllerCanvasToastMessage>().CloseLoading();
            }
        }

        private async void JoinGuild(GuildInfo guildInfo)
        {
            if (!DataController.Instance.guild.CanJoinGuildNow())
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_JoinErrorMessage);
                return;
            }
            Get<ControllerCanvasToastMessage>().ShowLoading();

            var guildIndate = guildInfo.InDate;
            var immediateRegistration = guildInfo.ImmediateRegistration;
            var needStage = guildInfo.NeededStage;

            if (DataController.Instance.stage.MaxTotalLevel < needStage)
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_StageLevelInsufficient);
                Get<ControllerCanvasToastMessage>().CloseLoading();
                return;
            }
            
            var isApply = await BackendGuildManager.Instance.ApplyGuild(guildIndate);
            if(isApply)
            {
                LocalizedTextType successTextType;
                if (immediateRegistration)
                {
                    successTextType = LocalizedTextType.Guild_JoinSuccess;
                    View.Close();
                    Get<ControllerCanvasMainMenu>().OpenGuildCanvas();
                }
                else
                {
                    successTextType = LocalizedTextType.Guild_ApplySuccess;
                    View.GuildInfoViewCanvasPopup.Close();
                }
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(successTextType);
            }
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        // [CanBeNull]
        // private ViewSlotGuildMember GetViewSlotGuildMember(int index)
        // {
        //     try
        //     {
        //         var listCount = _viewSlotGuildMembers.Count;
        //         for (var i = listCount; i <= index; ++i)
        //         {
        //             var slot = Object.Instantiate(View.ViewSlotGuildMember, View.GuildMemberParent.transform);
        //             _viewSlotGuildMembers.Add(slot);
        //         }
        //
        //         return _viewSlotGuildMembers[index];
        //     }
        //     catch (Exception e)
        //     {
        //         FirebaseManager.LogError(e);
        //         return null;
        //     }
        // }
    }
}