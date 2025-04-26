using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UI.Controller
{
    
    public class ControllerCanvasClear : ControllerCanvas
    {
        public ViewGood MyViewGood => View.MyViewGood;
        
        private const string ViewSlotRewardName = "ViewSlotGoodSquare";
        private const string ViewSlotAddValueName = "ViewSlotAddValue";
        private readonly List<ViewGood> _rewardViewGoods = new();
        private readonly List<ViewSlotAddValue> _addValueSlots = new();
        
        private ViewCanvasClear View => ViewCanvas as ViewCanvasClear;
        private List<ViewSlotUI> _rewardSlots;
        private int _valueSlotCount = 1;
        
        private CancellationTokenSource _autoCloseRewardViewToken;
        
        public ControllerCanvasClear(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasClear>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
        }

        public ControllerCanvasClear SetMyGood(GoodType goodType)
        {
            View.MyViewGood.SetInit(goodType);
            SetActiveViewMyGood(goodType != GoodType.None);
            return this;
        }
        
        public async UniTaskVoid SetRewardsAndShow(List<GoodItem> goodItems)
        {
            Open();
            
            var i = 0;
            var viewSlot = _rewardViewGoods.GetViewSlots(ViewSlotRewardName, View.RewardViewGoodParent, goodItems.Count);
            foreach (var viewGood in goodItems)
            {
                viewSlot[i].transform.localScale = new Vector3(1, 0, 1);
                viewSlot[i]
                    .SetInit(viewGood.GoodType)
                    .SetValue(viewGood.Value)
                    .SetActive(true);
                
                viewSlot[i].transform
                    .DOScaleY(1, 0.25f)
                    .SetUpdate(true);

                await UniTask.Delay(150, true, PlayerLoopTiming.Update, Cts.Token);
                ++i;
            }
            AutoCloseRewardPanel().Forget();
        }

        public ControllerCanvasClear SetNeededRewards(GoodType goodType, double value = 1)
        {
            foreach (var neededViewGood in View.NeededViewGoods)
            {
                neededViewGood.SetActive(goodType != GoodType.None);
                neededViewGood
                    .SetInit(goodType)
                    .SetValue(value);
            }

            return this;
        }

        public ControllerCanvasClear SetTitle(string text, ColorType colorType = ColorType.SkyBlue)
        {
            View.ViewSlotTitle.SetText(text).SetTextColor(ResourcesManager.UIColor[colorType]);
            return this;
        }

        public ControllerCanvasClear SetAction(string button0Text, UnityAction action0,  string button1Text = null , UnityAction action1 = null)
        {
            View.Button1.gameObject.SetActive(action1 != null);
            View.Button0.SetActiveButtonText(button0Text);
            View.Button1.SetActiveButtonText(button1Text);
            View.Button0.OnClick.RemoveAllListeners();
            View.Button1.OnClick.RemoveAllListeners();
            View.Button0.OnClick.AddListener(action0);
            View.Button1.OnClick.AddListener(action1);
            return this;
        }

        public override void Close()
        {
            _autoCloseRewardViewToken.Cancel();
            _autoCloseRewardViewToken.Dispose();
            _autoCloseRewardViewToken = null;
            
            base.Close();
        }

        public async UniTaskVoid PlayReduceAnimation()
        {
            const float firstPositionY = 0f;
            const int spacing = -130;
            const int delay = 100;
            const float duration = 0.5f;

            var viewSlot = _addValueSlots.GetViewSlots(ViewSlotAddValueName, View.ViewSlotAddValueParent, _valueSlotCount - 1);
            var i = 0;
            foreach (var valueSlot in viewSlot)
            {
                valueSlot.SetActive(true);
                valueSlot.CanvasGroup.alpha = 0;
                
                var offset = valueSlot.RectTransform.localPosition;
                offset.y = i * spacing + firstPositionY - 500;
                valueSlot.RectTransform.localPosition = offset;
                
                valueSlot.RectTransform.DOLocalMoveY(i * spacing + firstPositionY, duration).SetEase(Ease.InQuart).SetUpdate(true);
                valueSlot.CanvasGroup.DOFade(1, duration).SetEase(Ease.InQuart).SetUpdate(true);
                
                await UniTask.Delay(delay, true, PlayerLoopTiming.Update, Cts.Token);
                ++i;
            }

            _valueSlotCount = 1;
        }

        public ControllerCanvasClear AddValueSlot(Sprite sprite, string title, string desc, bool setActiveBackground = true)
        {
            var viewSlot = _addValueSlots.GetViewSlots(ViewSlotAddValueName, View.ViewSlotAddValueParent, _valueSlotCount )[_valueSlotCount - 1];
            viewSlot.SetIcon(sprite).SetTitleText(title).SetValueText(desc).SetActiveBackground(setActiveBackground);
            _valueSlotCount++;
            return this;
        }

        public ControllerCanvasClear AddValueSlotsWithFail()
        {
            ClearValueSlot();
            AddValueSlot(null,LocalizeManager.GetText(LocalizedTextType.Shop_SummonElemental), LocalizeManager.GetText(LocalizedTextType.Summon))
                .AddValueSlot(null, string.Empty, LocalizeManager.GetText(LocalizedTextType.Enhance), false)
                .AddValueSlot(null, LocalizeManager.GetText(LocalizedTextType.Shop_SummonRune), LocalizeManager.GetText(LocalizedTextType.Summon))
                .AddValueSlot(null, string.Empty, LocalizeManager.GetText(LocalizedTextType.Enhance), false)
                .AddValueSlot(null, LocalizeManager.GetText(LocalizedTextType.GoldUpgrade), string.Empty)
                .AddValueSlot(null, LocalizeManager.GetText(LocalizedTextType.ResearchEnhancement), string.Empty);
            
            return this;
        }

        private ControllerCanvasClear ClearValueSlot()
        {
            _valueSlotCount = 1;
            return this;
        }

        public ControllerCanvasClear SetReduceType(ReduceType reduceType, float value)
        {
            var sprite = reduceType switch
            {
                ReduceType.Vip => DataController.Instance.good.GetImage(GoodType.VIP),
                ReduceType.Research => ResourcesManager.Instance.GetUnlockImage(UnlockType.Research),
                _ => null
            };

            var title = reduceType switch
            {
                ReduceType.Vip => LocalizeManager.GetText(LocalizedTextType.VIPTitle),
                ReduceType.Research => LocalizeManager.GetText(LocalizedTextType.Research_Title),
                _ => string.Empty
            };

            var valueText = reduceType switch
            {
                ReduceType.Vip => $"x{(float)DataController.Instance.good.GetValue(GoodType.IncreaseDungeonReward):P0}",
                ReduceType.Research =>
                    $"x{DataController.Instance.research.GetValue(ResearchType.IncreaseDungeonReward):P0}",
                _ => string.Empty
            };

            AddValueSlot(sprite, title, valueText);
            return this;
        }

        private void SetActiveViewMyGood(bool flag)
        {
            View.MyViewGood.gameObject.SetActive(flag);
        }

        private async UniTaskVoid AutoCloseRewardPanel()
        {
            _autoCloseRewardViewToken = new CancellationTokenSource();
            
            var seconds = 5;
            while (seconds > 0)
            {
                View.SetCloseSecondsText(LocalizeManager.GetText(LocalizedTextType.AutoCloseFewSeconds, seconds));
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, _autoCloseRewardViewToken.Token);
                seconds--;
            }
            
            if (StageManager.Instance.PlayingStageType == StageType.Normal) Close();
            else
            {
                if (!View.Button1.isActiveAndEnabled)
                {
                    View.Button0.OnClick?.Invoke();
                    return;
                }
                
                if(DataController.Instance.setting.dungeonNextChallenge)
                {
                    if (DataController.Instance.good.GetValue(View.NeededViewGoods[0].GoodType) >= View.NeededViewGoods[0].GoodValue)
                        View.Button1.OnClick?.Invoke();
                    else 
                        View.Button0.OnClick?.Invoke();
                }
                else
                {
                    View.Button0.OnClick?.Invoke();
                }
            }
        }
    }
}