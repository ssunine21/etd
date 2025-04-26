using System.Collections.Generic;
using BackEnd;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.DataController;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMyGuild
    {
        private readonly List<ViewSlotGuildMember> _viewSlotGuildMembers = new();
        private const int MemberMenuIndex = 1;
        private const string ViewSlotGuildMemberString = "ViewSlotGuildMember";

        private void InitMember()
        {
            View.SlideButton.AddListener(index => { if(index == MemberMenuIndex) UpdateViewMember();});
        }
        
        private void UpdateViewMember()
        {
            var amIGuildMaster = _myGuildInfo.IsMaster(Backend.UserInDate);
            _viewSlotGuildMembers.GetViewSlots(ViewSlotGuildMemberString, View.ViewSlotGuildMemberParent, _myGuildInfo.MemberItems.Count);
            
            var i = 1;
            foreach (var memberItem in _myGuildInfo.MemberItems)
            {
                var index = memberItem.IsMaster ? 0 : i;
                var slot = _viewSlotGuildMembers[index];
                
                slot.SetMark(memberItem.IsMaster)
                    .SetActiveMasterTag(memberItem.IsMaster)
                    .SetNickname(memberItem.Nickname)
                    .SetCombat(memberItem.Combat.ToDamage())
                    .SetCurrStage(memberItem.MaxStage)
                    .SetContribution(memberItem.TotalGoodAmount)
                    .SetLastLoginTime(memberItem.GamerInDate == Backend.UserInDate 
                        ? LocalizeManager.GetText(LocalizedTextType.Online) 
                        : GetLastLoginText(memberItem.LastLogin))
                    .SetActiveManageWrap(amIGuildMaster)
                    .SetActiveManageButton(memberItem.GamerInDate != Backend.UserInDate)
                    .SetActiveInfoWarp(true)
                    .SetActive(true);

                var nickname = memberItem.Nickname;
                var gamerInDate = memberItem.GamerInDate;
                
                slot.ExpelButton.Button.onClick.RemoveAllListeners();
                slot.ExpelButton.OnClick.AddListener(() => ShowExpelMemberMessage(nickname, gamerInDate));
                slot.NominateButton.Button.onClick.RemoveAllListeners();
                slot.NominateButton.OnClick.AddListener(() => ShowNominateMasterMessage(nickname, gamerInDate));
                ++i;
            }

            for (; i < _viewSlotGuildMembers.Count; ++i)
            {
                _viewSlotGuildMembers[i].SetActive(false);
            }

            View
                .SetMemberCount(_myGuildInfo.MemberCount, DataController.Instance.guild.GetMaxMemeberCount(_myGuildInfo.Level))
                .SetActiveManageButton(amIGuildMaster && _myGuildInfo.InDate != Backend.UserInDate );
        }

        private void ShowExpelMemberMessage(string nickname, string gamerIndate)
        {
            Get<ControllerCanvasToastMessage>().SetToastMessage(
                LocalizeManager.GetText(LocalizedTextType.Warring), LocalizeManager.GetText(LocalizedTextType.Guild_ExpelDesc, nickname),
                LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                LocalizeManager.GetText(LocalizedTextType.Guild_Withdrawal), () => ExpelMember(gamerIndate))
                .ShowToastMessage();
        }

        private void ShowNominateMasterMessage(string nickname, string gamerIndate)
        {
            Get<ControllerCanvasToastMessage>().SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.Warring), LocalizeManager.GetText(LocalizedTextType.Guild_MasterNominateDesc, nickname),
                    LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                    LocalizeManager.GetText(LocalizedTextType.Guild_MasterNominate), () => NominateMember(gamerIndate))
                .ShowToastMessage();
        }
        
        private async void ExpelMember(string gamerIndate)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var isSuccess = await BackendGuildManager.Instance.ExpelMember(gamerIndate);
            if (isSuccess)
            {
                await AsyncMyGuild();
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_ExpelSuccess);
                UpdateViewMember();
                UpdateInfo();
            }
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private async void NominateMember(string gamerIndate)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var isSuccess = await BackendGuildManager.Instance.NominateMaster(gamerIndate);
            if (isSuccess)
            {
                await AsyncMyGuild();
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.Guild_MasterNominateSuccess);
                UpdateViewMember();
                UpdateInfo();
            }
            Get<ControllerCanvasToastMessage>().CloseLoading();
        }

        private string GetLastLoginText(string lastLoginTimeString)
        {
            LocalizedTextType localizedTextType;
            var param = 0;
            var timeSpan = ServerTime.UntilTimeToServerTime(lastLoginTimeString);
            
            var totalDays = timeSpan.TotalDays;
            var totalHours = timeSpan.TotalHours;
            var totalMinutes = timeSpan.TotalMinutes;
            
            if (totalDays < 1)
            {
                if (totalMinutes < 10)
                {
                    return LocalizeManager.GetText(LocalizedTextType.Online);
                }

                var isMinutes = totalHours < 1;
                localizedTextType = isMinutes ? LocalizedTextType.MinutesAgo : LocalizedTextType.HoursAgo;
                param = isMinutes ? (int)totalMinutes : (int)totalHours;
            }
            else
            {
                localizedTextType = LocalizedTextType.DaysAgo;
                param = (int)totalDays;
            }
            
            return LocalizeManager.GetText(localizedTextType, param);
        }
    }
}