using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller.ControllerEnemy;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMyGuild
    {
        private const int RaidMenuIndex = 2;
        
        private Sequence _sequence;
        private const float InitRotateSpeed = 2f;
        private const float RotateSpeed = 10f;

        private DateTime _nextRaidTicketTime;
        private readonly List<ViewSlotGuildRaidRanking> _viewSlotGuildRaidRankings = new();
        private readonly List<TMP_Text> _logs = new();
        
        private void InitRaid()
        {
            _nextRaidTicketTime = ServerTime.IsoStringToDateTime(DataController.Instance.guild.nextGuildRaidTicketTimeToString);
            View.ViewSlotGuildRaidRankingPrefab.SetActive(false);
            View.RaidBoxViewCanvasPopup.Close();
            View.LogPrefab.gameObject.SetActive(false);
            View.RaidEnterButton.OnClick.AddListener(TryRaid);
            View.RaidDescButton.onClick.AddListener(() => ShowDescriptionView(LocalizeManager.GetText(LocalizedTextType.Guild_RaidDesc)));
            View.RaidBoxDescButton.onClick.AddListener(() => ShowDescriptionView(LocalizeManager.GetText(
                LocalizedTextType.Guild_RaidBoxDesc, DataController.Instance.guild.GetGuildRaidBoxLevelStep(), ServerTime.GetUtcOffset())));
            View.SlideButton.AddListener(index =>
            {
                if(index == RaidMenuIndex)
                {
                    UpdateRaidView();
                    AnimationBoss();
                }
            });
            View.ShowRaidBoxButton.OnClick.AddListener(() =>
            {
                UpdateRaidBoxView();
                View.RaidBoxViewCanvasPopup.Open();
            });

            for (var i = 0; i < View.ViewSlotRaidBoxes.Count; ++i)
            {
                var index = i;
                View.ViewSlotRaidBoxes[i].RewardButton.OnClick.AddListener(() => GetRaidBoxReward(index));
            }

            StageManager.Instance.onBindChangeStageType += UpdateChangeStage;

            RaidTimeTask().Forget();
        }

        private async UniTaskVoid RaidTimeTask()
        {
            while (!Cts.Token.IsCancellationRequested)
            {
                var time = Utility.GetTimeStringToFromTotalSecond(ServerTime.RemainingTimeUntilNextDay);
                View.SetNextBossTime(time);
                TryEarnRaidTicketAndAsyncTime();
                
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, Cts.Token);
            }
        }

        private async void GetRaidBoxReward(int boxIndex)
        {
            var toastCanvas = Get<ControllerCanvasToastMessage>();
            
            toastCanvas.ShowLoading();
            var boxGradeType = (GradeType)boxIndex;
            var boxCount = _myGuildInfo.RaidBoxes[boxGradeType];
            var goodItems = DataController.Instance.guildReward.GetRewardGoodItem(GuildRewardType.RaidBox, boxIndex, boxCount);
            if (goodItems == null) return;

            foreach (var goodItem in goodItems)
                DataController.Instance.good.EarnReward(goodItem.GoodType, goodItem.Value * _myGuildInfo.RaidBoxes[boxGradeType], goodItem.Param0);
            
            DataController.Instance.guild.SetRaidBoxRewardTime(boxGradeType);

            await AsyncMyGuild();
            
            toastCanvas.ShowSimpleRewardView(goodItems, LocalizeManager.GetText(LocalizedTextType.Guild_RaidBox)).Forget();
            toastCanvas.CloseLoading();
            UpdateRaidBoxView();
        }

        private void TryEarnRaidTicketAndAsyncTime()
        {
            var seconds = DataController.Instance.guild.GetNextGuildRaidTicketTimeInSeconds();
            while (true)
            {
                if (IsMaxTicket())
                {
                    SetNextTicketTime(ServerTime.Date.AddSeconds(seconds));
                    break;
                }

                if (!ServerTime.IsRemainingTimeUntilDisable(_nextRaidTicketTime))
                {
                    DataController.Instance.good.Earn(GoodType.GuildRaidTicket, 1);
                    SetNextTicketTime(_nextRaidTicketTime.AddSeconds(seconds));
                }
                else
                    break;
            }

            var timeSpan = ServerTime.RemainingTimeToTimeSpan(_nextRaidTicketTime);
            View
                .SetTicketCount((int)DataController.Instance.good.GetValue(GoodType.GuildRaidTicket), DataController.Instance.guild.GetMaxGuildRaidTicketCount())
                .SetTicketTime(Utility.GetTimeStringToFromTotalSecond(timeSpan))
                .SetActiveTicketTime(!IsMaxTicket());
        }

        private void UpdateRaidView()
        {
            var today = ServerTime.Date.Date;
            var sortedMembers = _myGuildInfo.MemberItems
                .Select(member =>
                {
                    var raidClearTime = ServerTime.IsoStringToDateTime(member.RaidClearTimeToString);
                    var damage = (raidClearTime.Date == today) ? member.RaidDamage : 0d;
                    
                    return new { Member = member, LastDamage = damage };
                })
                .OrderByDescending(m => m.LastDamage);

            var i = 0;
            foreach (var guildMemberItem in sortedMembers)
            {
                if (guildMemberItem.Member.GamerInDate == Backend.UserInDate)
                {
                    var isDamage = guildMemberItem.LastDamage > 0;
                    View.MyViewSlotRanking
                        .SetRankingMark(isDamage ? i : 5)
                        .SetRanking(isDamage ? (i + 1).ToString() : "-")
                        .SetNickname(guildMemberItem.Member.Nickname);
                    if (isDamage)
                        View.MyViewSlotRanking.SetLevelAndTotalDamage(guildMemberItem.Member.RaidLevel, guildMemberItem.LastDamage);
                    else
                        View.MyViewSlotRanking.SetLevelAndTotalDamage("-");
                }

                if(guildMemberItem.LastDamage < 1) continue;
                
                var slot = GetGuildRaidRankingSlot(i);
                slot
                    .SetRankingMark(i)
                    .SetBackgroundColor(i)
                    .SetRanking(i)
                    .SetNickname(guildMemberItem.Member.Nickname)
                    .SetLevelAndTotalDamage(guildMemberItem.Member.RaidLevel, guildMemberItem.LastDamage)
                    .SetActive(true);
                ++i;
            }

            for (; i < _viewSlotGuildRaidRankings.Count; ++i)
            {
                _viewSlotGuildRaidRankings[i].SetActive(false);
            }
        }

        private async void UpdateChangeStage(StageType type, int param0)
        {
            if (StageManager.Instance.PrevPlayingStageType == StageType.GuildRaidDungeon && type == StageType.Normal)
            {
                Get<ControllerCanvasToastMessage>().ShowLoading();
                View.SetActive(true);
                View.SlideButton.OnClick(RaidMenuIndex);
                
                var isSuccess = await AsyncMyGuild();
                if (isSuccess)
                    UpdateRaidView();
                
                Get<ControllerCanvasToastMessage>().CloseLoading();
            }
        }

        private void UpdateRaidBoxView()
        {
            var rewards = _myGuildInfo.RaidBoxes;
            for (var i = 0; i < View.ViewSlotRaidBoxes.Count; i++)
            {
                var viewSlotRaidBox = View.ViewSlotRaidBoxes[i];
                var gradeType = (GradeType)i;

                viewSlotRaidBox.RewardButton.Selected(rewards.TryGetValue(gradeType, out var reward) && reward > 0);
                viewSlotRaidBox
                    .SetBoxTypeTitle(gradeType)
                    .SetRewardCount(reward);
            }

            var logs = _myGuildInfo.RaidLogs;
            var j = 0;
            foreach (var log in logs)
            {
                var slot = GetLogSlot(j);
                slot.gameObject.SetActive(true);
                slot.text = log;
                ++j;
            }

            for (; j < _logs.Count; ++j)
            {
                _logs[j].gameObject.SetActive(false);
            }
            
            UpdateReddot(ReddotType.GuildRaidBox);
        }

        private void AnimationBoss()
        {
            if (_sequence == null)
            {
                const float radius = 270f;
                _sequence = DOTween.Sequence().SetAutoKill(false)
                    .OnStart(() =>
                    {
                        View.ViewBossCanvasGroup.alpha = 0;
                        View.ViewBossInner.localRotation = Quaternion.Euler(Vector3.zero);
                        View.ViewBossBackground.localRotation = Quaternion.Euler(Vector3.zero);
                    })
                    .Append(View.ViewBossCanvasGroup.DOFade(0.8f, 0.3f))
                    .Join(View.ViewBossInner.DOLocalRotate(new Vector3(0, 0, -radius), InitRotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutQuart))
                    .Join(View.ViewBossBackground.DOLocalRotate(new Vector3(0, 0, radius), InitRotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutQuart))
                    .Append(View.ViewBossInner.DOLocalRotate(new Vector3(0, 0, -360), RotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(int.MaxValue))
                    .Join(View.ViewBossBackground.DOLocalRotate(new Vector3(0, 0, 360), RotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(int.MaxValue))
                    .SetUpdate(true);
            }
            else
            {
                _sequence.Restart();
            }
        }

        private ViewSlotGuildRaidRanking GetGuildRaidRankingSlot(int index)
        {
            var count = _viewSlotGuildRaidRankings.Count;
            for (var i = count; i <= index; ++i)
            {
                var slot = Object.Instantiate(View.ViewSlotGuildRaidRankingPrefab, View.ViewSlotGuildRankingParent);
                _viewSlotGuildRaidRankings.Add(slot);
            }

            return _viewSlotGuildRaidRankings[index];
        }

        private bool IsMaxTicket()
        {
            var curr = DataController.Instance.good.GetValue(GoodType.GuildRaidTicket);
            return curr >= DataController.Instance.guild.GetMaxGuildRaidTicketCount();
        }
        
        private void SetNextTicketTime(DateTime dateTime)
        {
            _nextRaidTicketTime = dateTime;
            DataController.Instance.guild.SetNextRaidTicketTimeToString(ServerTime.DateTimeToIsoString(_nextRaidTicketTime));
            DataController.Instance.LocalSave();
        }

        private void TryRaid()
        {
            if (!DataController.Instance.good.IsEnoughGood(GoodType.GuildRaidTicket, 1))
            {
                Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
                return;
            }

            Get<ControllerCanvasToastMessage>().ShowFadeOutIn(() =>
            {
                Close();
                ControllerEnemyGuildBoss.skillDelayTime = 0;
                StageManager.Instance.ChangeStage(StageType.GuildRaidDungeon);
            });
        }

        private TMP_Text GetLogSlot(int index)
        {
            var count = _logs.Count;
            for (var i = count; i <= index; ++i)
            {
                _logs.Add(Object.Instantiate(View.LogPrefab, View.LogParent));
            }

            return _logs[index];
        }
    }
}