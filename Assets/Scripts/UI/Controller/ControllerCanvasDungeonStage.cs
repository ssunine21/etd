using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BackEnd;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.InGame.Controller.ControllerEnemy;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasDungeonStage : ControllerCanvas
    {
        private ViewCanvasDungeonStage View => ViewCanvas as ViewCanvasDungeonStage;

        private double _goalCount;
        private double _currGoalCount;
        private float _currTime;
        private double _maxGageFillAmountValue;
        private double _currGageFillAmountValue;
        private bool _isPlayDungeon;
        private readonly List<ViewGood> _eanredRewardViewGoods = new();

        public ControllerCanvasDungeonStage(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasDungeonStage>())
        {
            View.EarnedRewardViewGoodPrefab.SetActive(false);
            View.BackButton.onClick.AddListener(() =>
            {
                GameManager.Instance.Pause();
                var desc = StageManager.Instance.PlayingStageType == StageType.DarkDiaDungeon
                    ? $"{LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ExitDescriptionInDarkDia)}" 
                    : $"{LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ExitDescription)}";
                var messageBox = Get<ControllerCanvasToastMessage>();
                    messageBox.SetToastMessage(
                    LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ExitTitle), desc,
                    LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ContinueButton),
                    GameManager.Instance.Play,
                    LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ExitButton),
                    () =>
                    {
                        Get<ControllerCanvasToastMessage>().ShowFadeOutIn(() =>
                        {
                            GameManager.Instance.Play();
                            StageEnd(StageManager.Instance.PlayingStageType, false);
                            StageManager.Instance.ChangeStage(StageType.Normal);
                        });
                    }
                ).ShowToastMessage();
            });

            StageManager.Instance.onBindShowStageClearView += () => StageClear().Forget();
            StageManager.Instance.onBindChangeStageType += InitStageView;
            StageManager.Instance.onBindStageClear += (stageType) =>
            {
                switch (stageType)
                {
                    case StageType.Normal:
                        return;
                    case StageType.EnhanceDungeon:
                        _currGoalCount++;
                        StageClear(true).Forget();
                        return;
                    case StageType.DarkDiaDungeon:
                        _currGoalCount++;
                        break;
                }
                UpdateStageView();
            };
            
            StageManager.Instance.onBindStartStage += (stageType, level) =>
            {
                 if (stageType == StageType.Normal) return;
                switch (stageType)
                {
                    case StageType.GoldDungeon or StageType.EnhanceDungeon or StageType.DarkDiaDungeon:
                        _currGageFillAmountValue = 0;
                        _maxGageFillAmountValue =
                            DataController.Instance.enemyCombination.GetEnemyCount(
                                DataController.Instance.dungeon.GetEnemyCombinationIndex(stageType, level));
                        break;
                    case StageType.DiaDungeon or StageType.GuildRaidDungeon:
                        if (EnemyManager.Instance.TryGetNearbyDamageable(Vector2.zero, out var enemy))
                            _currGageFillAmountValue = _maxGageFillAmountValue = ((ControllerEnemy)enemy).MaxHp;
                        break;
                }
                UpdateGageFillAmount();
                StartStage(stageType, level);
            };
            
            EnemyManager.Instance.onBindDamagedEnemy += (damage) =>
            {
                if (StageManager.Instance.PlayingStageType == StageType.Normal) return;
                if (StageManager.Instance.PlayingStageType is StageType.DiaDungeon or StageType.GuildRaidDungeon)
                {
                    _currGoalCount += damage;
                    _currGageFillAmountValue -= damage;
                    UpdateCurrGoalCount();
                }

                UpdateGageFillAmount();
            };
            
            EnemyManager.Instance.onBindDieEnemy += (enemy) =>
            {
                switch (StageManager.Instance.PlayingStageType)
                {
                    case StageType.Normal:
                        return;
                    case StageType.GoldDungeon:
                        _currTime =
                            Mathf.Min(_currTime + 0.5f,
                                DataController.Instance.dungeon.GetTimeLimit(StageManager.Instance.PlayingStageType,
                                    StageManager.Instance.PlayingStageLevel));
                        _currGoalCount++;
                        _currGageFillAmountValue++;
                        break;
                    case StageType.EnhanceDungeon:
                        _currGageFillAmountValue++;
                        break;
                    case StageType.DarkDiaDungeon:
                        _currTime =
                            Mathf.Min(_currTime + 0.5f,
                                DataController.Instance.dungeon.GetTimeLimit(StageManager.Instance.PlayingStageType,
                                    StageManager.Instance.PlayingStageLevel));
                        _currGageFillAmountValue++;
                        break;
                }

                ShowEffectAndEarnGoodsWhenEnemyDie(enemy);
                
                UpdateGageFillAmount();
                UpdateCurrGoalCount();
            };
        }

        private ViewGood GetViewGood(GoodType goodType)
        {
            foreach (var viewGood in _eanredRewardViewGoods)
            {
                if (viewGood.GoodType == goodType)
                    return viewGood;
            }

            return _eanredRewardViewGoods[0];
        }

        private void InitStageView(StageType stageType, int param0)
        {
            var isNormalStage = stageType == StageType.Normal;
            View.SetActive(!isNormalStage);
            if (isNormalStage) return;

            var timeLimit = DataController.Instance.dungeon.GetTimeLimit(stageType, 0);
            _currGoalCount = stageType switch
            {
                StageType.EnhanceDungeon => DataController.Instance.dungeon.GetMaxGoalCount(stageType),
                _ => 0
            };
            _goalCount = stageType switch
            {
                StageType.DarkDiaDungeon => param0,
                _ => 0
            };
            
            var gageColor = stageType switch
            {
                StageType.GoldDungeon => Color.white,
                StageType.DiaDungeon => new Color(0, 185/255f, 239/255f),
                StageType.EnhanceDungeon => new Color(125/255f, 100/255f, 255/255f),
                StageType.DarkDiaDungeon => new Color(99/255f, 43/255f, 142/255f),
                StageType.GuildRaidDungeon => new Color(255f / 255f, 165f / 255f, 0f / 255f),
                _ => Color.white
            };
            
            var gageFillAmount = stageType switch
            {
                StageType.GoldDungeon => 0,
                StageType.EnhanceDungeon => 0,
                _ => 1
            };
            
            View
                .SetTitle(LocalizeManager.GetDungeonTitle(stageType))
                .SetBadgeSprite(stageType)
                .SetLevelText(LocalizeManager.GetText(LocalizedTextType.Lv))
                .SetGageTexture(stageType, gageColor)
                .SetGageFillAmount(gageFillAmount)
                .SetTimerFillAmount(1)
                .SetTimerText(LocalizeManager.GetText(LocalizedTextType.Seconds, timeLimit), timeLimit > 0)
                .SetTotalCountDescText(LocalizeManager.GetDungeonGoalText(stageType))
                .SetTotalCountText(GetStartingGoalCountText(stageType, _currGoalCount))
                .SetDescription(LocalizeManager.GetDungeonDescription(stageType));
            
            var rewards = DataController.Instance.dungeon.GetLastEarnedRewards(stageType);
            var i = 0;
            foreach (var reward in rewards)
            {
                if(reward.Key == GoodType.None)
                    continue;

                var eanredRewardViewGoods = GetEarnedRewardGood(i);
                eanredRewardViewGoods
                    .SetInit(reward.Key)
                    .SetValue(0)
                    .SetActive(true);
                ++i;
            }
            
            for (; i <_eanredRewardViewGoods.Count; ++i)
            {
                _eanredRewardViewGoods[i].SetActive(false);
            }
            
            View.EarnedRewardViewParent.gameObject.SetActive(stageType != StageType.GuildRaidDungeon);
            
            UpdateStageView();
        }

        private void StartStage(StageType stageType, int level)
        {
            if (_isPlayDungeon) return;
            
            _isPlayDungeon = true;
            var timeLimit = DataController.Instance.dungeon.GetTimeLimit(stageType, level);
            if (timeLimit > 0)
            {
                StartTimer(timeLimit).Forget();
            }
        }

        private async UniTaskVoid StartTimer(float timeLimit)
        {
            _currTime = timeLimit;
            while (_isPlayDungeon && _currTime >= 0)
            {
                if(GameManager.Instance.IsPlaying)
                {
                    var fillAmount = _currTime <= 0 ? 0 : _currTime / timeLimit;
                    View
                        .SetTimerFillAmount(fillAmount)
                        .SetTimerText(LocalizeManager.GetText(LocalizedTextType.Seconds, Mathf.Max(_currTime, 0)));
                    _currTime -= Time.deltaTime;
                }
                await UniTask.Yield(Cts.Token);
            }

            if (_currTime <= 0)
            {
                _currTime = timeLimit;
                GameManager.Instance.Pause();
                StageClear().Forget();
            }
        }

        private void ShowClearView(StageType stageType, bool isClear, bool showNextLevelButton = false)
        {
            Get<ControllerCanvasToastMessage>().CloseLoading();
            _isPlayDungeon = false;
            
            var controllerCanvasClear = Get<ControllerCanvasClear>();
            var title = isClear ? LocalizeManager.GetText(LocalizedTextType.ClearTitle) : LocalizeManager.GetText(LocalizedTextType.FailsTitle);
            var titleColorType = isClear? ColorType.SkyBlue :ColorType.Red;

            var goodItems = new List<GoodItem>();
            if (!isClear)
            {
                controllerCanvasClear.AddValueSlotsWithFail();
            }
            else
            {
                goodItems =
                    (from viewGood in _eanredRewardViewGoods
                        where viewGood.isActiveAndEnabled
                        select new GoodItem(viewGood.GoodType, viewGood.GoodValue * DataController.Instance.dungeon.GetReduceValue())).ToList();

                foreach (var goodValue in goodItems)
                {
                    GoodsEffectManager.Instance.ShowEffect(goodValue.GoodType, Vector2.zero, controllerCanvasClear.MyViewGood, Mathf.Clamp((int)goodValue.Value, 0, 10));
                }
            }

            controllerCanvasClear
                .SetTitle(title, titleColorType)
                .SetMyGood(DataController.Instance.dungeon.GetTicketGoodType(stageType))
                .SetNeededRewards(DataController.Instance.dungeon.GetTicketGoodType(stageType))
                .SetAction(
                    LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_ExitButton),
                    () =>
                    {
                        Get<ControllerCanvasToastMessage>().ShowFadeOutIn(() =>
                        {
                            GameManager.Instance.Play();
                            controllerCanvasClear.Close();
                            StageManager.Instance.ChangeStage(StageType.Normal);
                        });
                        StageEnd(stageType, isClear);
                    },
                    LocalizeManager.GetText(LocalizedTextType.Dungeon_Toast_NextLevel),
                    showNextLevelButton ? NextLevelAction : null
                )
                .SetRewardsAndShow(goodItems).Forget();
            controllerCanvasClear.PlayReduceAnimation().Forget();
        }

        private async UniTaskVoid StageClear(bool showNextLevelButton = false)
        {
            await UniTask.Delay(700, true, PlayerLoopTiming.Update, Cts.Token);
            var stageType = StageManager.Instance.PlayingStageType;

            if (stageType == StageType.DarkDiaDungeon)
            {
                var isClear = _currGoalCount > _goalCount;
                var toast = Get<ControllerCanvasToastMessage>();
                var hasGuild = await BackendGuildManager.Instance.HasGuild();
                
                toast.ShowLoading();
                Get<ControllerCanvasRaid>().TrySettleRaidStage(isClear, (int)_currGoalCount,
                    lootableValue =>
                    {
                        const GoodType goodKey = GoodType.DarkDia;
                        var slot0 = GetEarnedRewardGood(0);
                        slot0.SetInit(goodKey).SetValue(lootableValue).SetActive(true);

                        if (hasGuild)
                        {
                            var slot1 = GetEarnedRewardGood(1);
                            slot1.SetInit(GoodType.GuildGiftBoxPoint).SetValue(DataController.Instance.guildReward.GetGiftBoxPointWithRaid()).SetActive(true);
                            BackendGuildManager.Instance.ContributeGoods(GoodType.GuildGiftBoxPoint, DataController.Instance.guildReward.GetGiftBoxPointWithRaid(), false).Forget();
                        }
                        
                        DataController.Instance.good.Earn(goodKey, lootableValue);
                        DataController.Instance.raid.TryUpdateMyRaidData(new Param
                        {
                            { RaidData.DarkDiaKey, DataController.Instance.good.GetValue(GoodType.DarkDia) }
                        });

                        toast.CloseLoading();
                        
                        Get<ControllerCanvasClear>()
                            .AddValueSlot(null, LocalizeManager.GetText(LocalizedTextType.GoalCount, (float)_goalCount + 1), "")
                            .AddValueSlot(null, LocalizeManager.GetText(LocalizedTextType.MyCount, (float)_currGoalCount), "");
                        
                        ShowClearView(stageType, isClear);
                        DataController.Instance.LocalSave();
                    });
            }
            else if (stageType == StageType.GuildRaidDungeon)
            {
                DataController.Instance.good.TryConsume(DataController.Instance.dungeon.GetTicketGoodType(StageManager.Instance.PlayingStageType), 1);
                var rewards = DataController.Instance.dungeon.GetDungeonReward(stageType, StageManager.Instance.PlayingStageLevel);
                if (rewards != null)
                {
                    Get<ControllerCanvasToastMessage>().ShowLoading(() => ShowClearView(stageType, true));
                    for (var i = 0; i < rewards.Count; ++i)
                        GetEarnedRewardGood(i).SetInit(rewards[i].GoodType).SetValue(rewards[i].Value);

                    foreach (var reward in rewards)
                    {
                        await BackendGuildManager.Instance.ContributeGoods(reward.GoodType, (int)reward.Value, false);
                    }

                    var count = StageManager.Instance.PlayingStageLevel / DataController.Instance.guild.GetGuildRaidBoxLevelStep();
                    var raidBoxes = await DataController.Instance.probability.GetRandomProbabilitys(ProbabilityType.GuildRaidBox, count);

                    DataController.Instance.guild.SetRaidDamage(_currGoalCount);
                    DataController.Instance.guild.SetRaidLevel(StageManager.Instance.PlayingStageLevel);
                    DataController.Instance.guild.SetGuildRaidData(raidBoxes);
                    GetEarnedRewardGood(0).transform.parent.gameObject.SetActive(true);

                    Get<ControllerCanvasToastMessage>().CloseLoading();
                    
                    Get<ControllerCanvasClear>().AddValueSlot(null, LocalizeManager.GetText(LocalizedTextType.Dungeon_GuildRaidDungeonGoal), _currGoalCount.ToDamage());
                    ShowClearView(stageType, true);

                    DataController.Instance.SaveBackendData();
                }
            }
            else
            {
                var isClear = !(stageType == StageType.EnhanceDungeon && DataController.Instance.player.CurrHp <= 0);
                if (isClear)
                {
                    DataController.Instance.good.TryConsume(DataController.Instance.dungeon.GetTicketGoodType(StageManager.Instance.PlayingStageType), 1);
                    foreach (var eanredRewardViewGood in _eanredRewardViewGoods.Where(x => x.isActiveAndEnabled))
                    {
                        var value = eanredRewardViewGood.GoodValue * DataController.Instance.dungeon.GetReduceValue();
                        DataController.Instance.good.Earn(eanredRewardViewGood.GoodType, value);
                    }

                    DataController.Instance.dungeon.SetLastEarnRewards(_eanredRewardViewGoods);
                    DataController.Instance.dungeon.SetMaxGoalCount(StageManager.Instance.PlayingStageType, _currGoalCount);
                }
                else 
                    Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.FailChallengeDescription);

                Get<ControllerCanvasClear>()
                    .SetReduceType(ReduceType.Vip, (float)DataController.Instance.good.GetValue(GoodType.IncreaseDungeonReward))
                    .SetReduceType(ReduceType.Research, DataController.Instance.research.GetValue(ResearchType.IncreaseDungeonReward));

                ShowClearView(stageType, isClear, showNextLevelButton);
            }

            DataController.Instance.LocalSave();
        }

        private void NextLevelAction()
        {
            switch (StageManager.Instance.PlayingStageType)
            {
                case StageType.EnhanceDungeon:
                    if(DataController.Instance.good.GetValue(GoodType.EnhanceDungeonTicket) > 0)
                    {
                        Get<ControllerCanvasToastMessage>().ShowFadeOutIn(() =>
                        {
                            StageManager.Instance.ChangeStage(StageType.EnhanceDungeon);
                            Get<ControllerCanvasClear>().Close();
                        });
                    }
                    else
                    {
                        Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.NotEnoughGoods);
                    }
                    break;
            }
        }

        private void StageEnd(StageType type, bool isClear)
        {
            if (type == StageType.Normal) return;
            StageManager.Instance.onBindStageEnd?.Invoke(type, isClear);
            _isPlayDungeon = false;
        }

        private void UpdateStageView()
        {
            var level = StageManager.Instance.PlayingStageLevel;
            View.SetLevelText(LocalizeManager.GetText(LocalizedTextType.Lv, level));
        }

        private void UpdateGageFillAmount()
        {
            var amount = _currGageFillAmountValue == 0 || _maxGageFillAmountValue == 0
                ? 0
                : (float)_currGageFillAmountValue / (float)_maxGageFillAmountValue;
            View.SetGageFillAmount(amount);
        }

        private void UpdateCurrGoalCount()
        {
            View
                .SetTotalCountText(GetStartingGoalCountText(StageManager.Instance.PlayingStageType, _currGoalCount));
        }
        
        private string GetStartingGoalCountText(StageType stageType, double goalCount)
        {
            return stageType switch
            {
                StageType.GoldDungeon => goalCount.ToString("N0"),
                StageType.DiaDungeon => goalCount.ToDamage(),
                StageType.EnhanceDungeon => DataController.Instance.dungeon.GetMaxGoalCount(stageType).ToString("N0"),
                StageType.DarkDiaDungeon => $"{_goalCount + 1:N0}",
                StageType.GuildRaidDungeon => goalCount.ToDamage(),
                _ => "0"
            };
        }
        
        private void ShowEffectAndEarnGoodsWhenEnemyDie(IDamageable enemy)
        {
            if (StageManager.Instance.PlayingStageType == StageType.Normal) return;
            
            var rewards = StageManager.Instance.GetRewardEachEnemy();
            if (rewards == null) return;
            foreach (var (key, rewardValue) in rewards)
            {
                if(key == GoodType.None) continue;

                var value = rewardValue;
                
                var viewGood = GetViewGood(key);
                StageGoodEffectManager.Instance.ShowEffect(key, enemy.Position, viewGood, 0.3f, 1f,
                    () =>
                    {
                        if (_isPlayDungeon)
                            viewGood.SetValue(viewGood.GoodValue + value);
                    });
            }
        }

        private ViewGood GetEarnedRewardGood(int index)
        {
            var count = _eanredRewardViewGoods.Count;
            for (var i = count; i <= index; ++i)
            {
                _eanredRewardViewGoods.Add(Object.Instantiate(View.EarnedRewardViewGoodPrefab, View.EarnedRewardViewParent));
            }

            return _eanredRewardViewGoods[index];
        }
    }
}