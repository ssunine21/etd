using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BackEnd;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMainMenu : ControllerCanvas
    {
        public static UnityAction<ReddotType, bool> onBindReddot;
        
        private ViewCanvasMainMenu View => ViewCanvas as ViewCanvasMainMenu;

        private Sequence _menuAnimationSequence;
        private Sequence _presetMenuAnimationSequence;

        private readonly ButtonRadioGroup _presetRadioGroup = new();
        private readonly Dictionary<ReddotType, ReddotView> _reddotViews = new();

        public ControllerCanvasMainMenu(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasMainMenu>())
        {
            SetActive(true);
            foreach (var reddotView in View.GetComponentsInChildren<ReddotView>())
            {
                if (reddotView.BaseReddotType != ReddotType.None)
                    _reddotViews.TryAdd(reddotView.BaseReddotType, reddotView);
            }
            
            View.OpenInnerMenu.onClick.AddListener(() =>
            {
                View.OpenInnerMenu.gameObject.SetActive(false);
                if (_menuAnimationSequence == null)
                {
                    const float totalDuration = 0.4f;
                    var toSize = new Vector2(View.InnerMenuWrapRectTr.sizeDelta.x,
                        ((View.InnerMenuButtons.Length - 1) * 120 + 130));
                    var timeInterval = totalDuration / (View.InnerMenuButtons.Length - 1);

                    _menuAnimationSequence = DOTween.Sequence().SetAutoKill(false)
                        .OnPlay(() =>
                        {
                            View.InnerMenuWrapRectTr.gameObject.SetActive(true);
                            View.InnerMenuWrapRectTr.sizeDelta =
                                new Vector2(View.InnerMenuWrapRectTr.sizeDelta.x, 50f);

                            for (var i = 1; i < View.InnerMenuButtons.Length; ++i)
                            {
                                View.InnerMenuButtons[i].transform.localScale = Vector3.zero;  
                            }
                        })
                        .Append(View.InnerMenuWrapRectTr.DOSizeDelta(toSize, totalDuration).SetEase(Ease.OutBack))
                        .SetUpdate(true);
                    
                    var duration = totalDuration / View.InnerMenuButtons.Length;
                    for (var i = 1; i < View.InnerMenuButtons.Length; ++i)
                    {
                        _menuAnimationSequence.Insert(timeInterval,
                            View.InnerMenuButtons[i].transform.DOScale(Vector3.one, duration * i)).SetUpdate(true);
                    }
                }
                else
                    _menuAnimationSequence.Restart();
            });
            
            View.OpenPresetMenu.onClick.AddListener(() =>
            {
                if (View.PresetMenuWrapRectTr.gameObject.activeSelf)
                {
                    View.PresetMenuWrapRectTr.gameObject.SetActive(false);
                    View.ChessImage.enabled = true;
                    View.PresetCloseImage.enabled = false;
                }
                else
                {
                    
                    if (_presetMenuAnimationSequence == null)
                    {
                        const float totalDuration = 0.4f;
                        var toSize = new Vector2(View.PresetMenuWrapRectTr.sizeDelta.x,
                            (View.PresetButtons.Length - 1) * 110 + 110);
                        var timeInterval = totalDuration / (View.PresetButtons.Length - 1);

                        _presetMenuAnimationSequence = DOTween.Sequence().SetAutoKill(false)
                            .OnPlay(() =>
                            {
                                View.ChessImage.enabled = false;
                                View.PresetCloseImage.enabled = true;
                                View.PresetMenuWrapRectTr.gameObject.SetActive(true);
                                View.PresetMenuWrapRectTr.sizeDelta =
                                    new Vector2(View.PresetMenuWrapRectTr.sizeDelta.x, 50f);

                                for (var i = 1; i < View.PresetButtons.Length; ++i)
                                {
                                    View.PresetButtons[i].transform.localScale = Vector3.zero;
                                }
                            })
                            .Append(View.PresetMenuWrapRectTr.DOSizeDelta(toSize, totalDuration).SetEase(Ease.OutBack))
                            .SetUpdate(true);

                        var duration = totalDuration / View.PresetButtons.Length;
                        for (var i = 1; i < View.PresetButtons.Length; ++i)
                        {
                            _presetMenuAnimationSequence.Insert(timeInterval,
                                View.PresetButtons[i].transform.DOScale(Vector3.one, duration * i)).SetUpdate(true);
                        }
                    }
                    else
                        _presetMenuAnimationSequence.Restart();
                }
            });

            var i = 0;
            foreach (var presetButton in View.PresetButtons)
            {
                var index = i;
                _presetRadioGroup.AddListener(presetButton, () => DataController.Instance.player.ChangePreset(index));
                ++i;
            }
            
            View.ProfileButton.onClick.AddListener(() => { });
            View.SetNickname(Backend.UserNickName);
            
            View.CloseInnerMenu.onClick.AddListener(() =>
            {
                View.InnerMenuWrapRectTr.gameObject.SetActive(false);
                View.OpenInnerMenu.gameObject.SetActive(true);
            });
            View.OpenSetting.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasSetting>()));
            View.OpenPowerSaving.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasPowerSaving>()));
            View.OpenRanking.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasRanking>()));
            View.OpenMission.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasMission>()));
            View.OpenMail.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasMail>()));
            View.OpenFreeGifts.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasFreeGifts>()));
            View.OpenNewPackage.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasNewPackage>()));
            View.ProfileButton.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasProfile>()));
            View.OpenBuffs.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasBuff>()));
            View.OpenAttendance.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasAttendance>()));
            View.OpenFirstPurchase.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasFirstPurchase>()));
            View.OpenPass.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasPass>()));
            View.OpenGrowPass.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasGrowPass>()));
            View.OpenLab.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasLab>()));
            View.OpenRaid.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasRaid>()));
            View.OpenVip.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasVip>()));
            View.OpenGuild.onClick.AddListener(OpenGuildCanvas);

            View.OpenAttendance.gameObject.SetActive(!DataController.Instance.attendance.AllReceived() && DataController.Instance.contentUnlock.IsUnLock(UnlockType.Attendance));
            View.OpenFirstPurchase.gameObject.SetActive(DataController.Instance.shop.CanReceiveFirstPurchaseReward() && DataController.Instance.contentUnlock.IsUnLock(UnlockType.FirstPurchase));
            View.OpenLab.gameObject.SetActive(DataController.Instance.contentUnlock.IsUnLock(UnlockType.Research));
            View.OpenRaid.gameObject.SetActive(DataController.Instance.contentUnlock.IsUnLock(UnlockType.Raid));
            View.OpenGuild.gameObject.SetActive(DataController.Instance.contentUnlock.IsUnLock(UnlockType.Guild));

            UpdateVipLevel();
            
            DataController.Instance.quest.OnBindClear += (clearLevel) =>
            {
                if (clearLevel == DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.Attendance))
                    View.OpenAttendance.gameObject.SetActive(true);
                if (clearLevel == DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.FirstPurchase))
                    View.OpenFirstPurchase.gameObject.SetActive(true);
                if (clearLevel == DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.Research))
                    View.OpenLab.gameObject.SetActive(true);
                if (clearLevel == DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.Raid))
                    View.OpenRaid.gameObject.SetActive(true);
                if (clearLevel == DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.Rating))
                    ShowRatingMessage();
                if (clearLevel == DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.Guild))
                    View.OpenGuild.gameObject.SetActive(true);
            };

            for (var j = 0; j < View.PresetButtons.Length; ++j)
            {
                View.PresetButtons[j].OnOffView(j == DataController.Instance.player.currPresetIndex);
            }

            #if IS_LIVE
            View.OpenTestPanel.gameObject.SetActive(false);
            var list = View.InnerMenuButtons.ToList();
            list.RemoveAt(list.Count - 1);
            View.SetInnerMenu(list.ToArray());
            #elif IS_TEST
            View.OpenTestPanel.gameObject.SetActive(true);
            View.OpenTestPanel.onClick.AddListener(() => OpenCanvas(Get<ControllerCanvasTest>()));
            #endif
            
            onBindReddot += (reddotType, flag) =>
            {
                if(_reddotViews.TryGetValue(reddotType, out var view))
                    view.ShowReddot(flag);
            };

            InitQuest();
            InitDPS();
            InitGameSpeed();
            
            StageManager.Instance.onBindChangeStageType += UpdateChangeStage;
            EnemyManager.Instance.onBindDieEnemy += ShowEffectAndEarnGoodsWhenEnemyDie;
            DataController.Instance.player.OnChangedNickname += (nickname) => View.SetNickname(nickname);
            DataController.Instance.player.OnBindChangeTotalCombat += UpdateCombatPower;
            DataController.Instance.shop.onBindNewPackageTime += SetNewPackageButton;
            DataController.Instance.shop.OnBindVipPurchased += UpdateVipLevel;
            
            UpdateCombatPower(DataController.Instance.maxCombatPower);
            
            View.CloseInnerMenu.onClick.Invoke();
        }
        
        public RectTransform GetGuideArrowParent(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.GameStart => View.QuestGuideArrowRectTransform,
                TutorialType.GameSpeed => View.GameSpeedGuideArrowRectTransform,
                _ => null
            };
        }

        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.GameSpeed => View.OnButton.gameObject,
                TutorialType.Buff => View.OpenBuffs.gameObject,
                TutorialType.ClickQuestPanel => View.QuestPanel.gameObject,
                TutorialType.Research => View.OpenLab.gameObject,
                TutorialType.Raid => View.OpenRaid.gameObject,
                _ => null
            };
        }

        public ViewGood GetViewGood(GoodType goodType)
        {
            foreach (var viewGood in View.ViewGoods)
            {
                if (viewGood.GoodType == goodType)
                    return viewGood;
            }

            return View.ViewGoods[0];
        }

        public async void OpenGuildCanvas()
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            var hasGuild = await BackendGuildManager.Instance.HasGuild();      
            Get<ControllerCanvasToastMessage>().CloseLoading();

            if (hasGuild)
                OpenCanvas(Get<ControllerCanvasMyGuild>());
            else
            {
                OpenCanvas(Get<ControllerCanvasGuild>());
            }
        }

        private void ShowRatingMessage()
        {
            Get<ControllerCanvasToastMessage>().SetToastMessage(
                LocalizeManager.GetText(LocalizedTextType.Rating_Title),
                LocalizeManager.GetText(LocalizedTextType.Rating_Desc),
                LocalizeManager.GetText(LocalizedTextType.ItsOk), null, 
                LocalizeManager.GetText(LocalizedTextType.Sure), 
                () =>
                {
                    Get<ControllerCanvasToastMessage>().ShowLoading();
                    GameManager.Instance.RequestAppReview(isSuccess =>
                    {
                        Get<ControllerCanvasToastMessage>().CloseLoading();
                    });
                }).ShowToastMessage();
        }
        
        private void OpenCanvas(ControllerCanvas canvas)
        {
            View.CloseInnerMenu.onClick.Invoke();
            canvas.Open();
        }

        private void UpdateVipLevel()
        {
            View.SetVipLevelText(LocalizeManager.GetText(LocalizedTextType.Good_VIP, (int)DataController.Instance.good.GetValue(GoodType.VIP)));
        }
        
        private void UpdateChangeStage(StageType stageType, int param0)
        {
            View.SetActive(stageType == StageType.Normal);
        }

        private void UpdateCombatPower(double value)
        {
            View.SetCombatPowerString(value.ToDamage());
        }

        private void SetNewPackageButton(string remainingTime)
        {
            View.OpenNewPackage.gameObject.SetActive(!string.IsNullOrEmpty(remainingTime));
            View.SetNewPackageTime(remainingTime);
        }
        
        private void ShowEffectAndEarnGoodsWhenEnemyDie(IDamageable enemy)
        {
            if (StageManager.Instance.PlayingStageType != StageType.Normal) return;
            
            StageGoodEffectManager.Instance.ShowEffect(GoodType.Gold, enemy.Position, GetViewGood(GoodType.Gold), 1, 0.9f);
            
            var rewards = StageManager.Instance.GetRewardEachEnemy();
            foreach (var (key, rewardValue) in rewards)
            {
                var reduceValue =
                    1 + DataController.Instance.research.GetValue(ResearchType.IncreaseNormalStageReward)
                      + DataController.Instance.good.GetValue(GoodType.IncreaseStageReward)
                    + (key == GoodType.Gold ? DataController.Instance.buff.GetValueIfBuffOn(AttributeType.AddGoldGain) : 0);

                var value = rewardValue * reduceValue;
                
                if (key == GoodType.Dia)
                {
                    if (Random.Range(0f, 1f) > 0.05f)
                        continue;
                }
                
                DataController.Instance.good.Earn(key, value);
            }
            
            DataController.Instance.LocalSave();
        }
    }
}