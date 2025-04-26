using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasToastMessage : ControllerCanvas
    {
        private ViewCanvasToastMessage View => ViewCanvas as ViewCanvasToastMessage;
        private readonly List<ViewGood> _rewardViewGoods = new();
        private const string ViewSlotRewardName = "ViewSlotGoodSquare";

        private Sequence _sequence;
        private Sequence _blinkSequnce;
        private CanvasGroup _transientCanvasGroup; // 투명도 조절용
        private readonly float _originHeight;

        private CancellationTokenSource _autoCloseRewardViewToken;
        private CancellationTokenSource _loadingCancellationTokenSource;

        public ControllerCanvasToastMessage(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasToastMessage>())
        {
            View.SetActive(true);
            View.SimpleRewardViewCanvasPopup.SetViewAnimation(ViewAnimationType.SlideUp);
            View.ViewPopupGoodInfo.SetViewAnimation(ViewAnimationType.SlideUp);
            View.ToastMessageBox.SetViewAnimation(ViewAnimationType.SlideUp);
            View.ViewGoodPrefab.SetActive(false);

            _originHeight = View.TransientViewBackgroundRect.sizeDelta.y;
            
            AllCloseView();
        }
        
        private const float Duration = 0.5f; // 애니메이션 지속 시간
        private const float BlinkSpeed = 0.05f; // 깜빡임 속도

        public void ShowTransientToastMessage(string text)
        {
            if (!_transientCanvasGroup)
            {
                if(View.TransientView.TryGetComponent(out _transientCanvasGroup))
                {
                    _transientCanvasGroup.alpha = 0;
                    View.TransientViewBackgroundRect.sizeDelta = new Vector2(View.TransientViewBackgroundRect.sizeDelta.x, 0);
                }
            }

            SetActiveView(ToastType.TransientMessage, true);
            View.TransientMessage.text = text;

            if (_sequence == null)
            {
                _blinkSequnce = DOTween.Sequence().SetAutoKill(false)
                    .Append(_transientCanvasGroup.DOFade(0, BlinkSpeed))
                    .Append(_transientCanvasGroup.DOFade(1, BlinkSpeed))
                    .Append(_transientCanvasGroup.DOFade(0, BlinkSpeed))
                    .Append(_transientCanvasGroup.DOFade(1, BlinkSpeed));

                _sequence = DOTween.Sequence().SetAutoKill(false)
                    .Append(View.TransientViewBackgroundRect.DOSizeDelta(new Vector2(View.TransientViewBackgroundRect.sizeDelta.x, _originHeight), Duration * 2).SetEase(Ease.OutExpo))
                    .Join(_transientCanvasGroup.DOFade(1, Duration * 0.5f)) // 페이드 인
                    .Join(_blinkSequnce)
                    .AppendInterval(Duration)
                    .Append(View.TransientViewBackgroundRect.DOSizeDelta(new Vector2(View.TransientViewBackgroundRect.sizeDelta.x, 0), Duration).SetEase(Ease.InExpo)) // 높이 축소
                    .Join(_transientCanvasGroup.DOFade(0, Duration * 0.5f)); // 페이드 아웃
            }
            else
            {
                _sequence.Restart();
            }
        }

        public void ShowTransientToastMessage(LocalizedTextType localizedTextType)
        {
            ShowTransientToastMessage(LocalizeManager.GetText(localizedTextType));
        }
        
        public ControllerCanvasToastMessage SetToastMessage(string title, string desc,
            string cancelButtonText = null, UnityAction cancelButtonAction = null,
            string confirmButtonText = null, UnityAction confirmButtonAction = null)
        {
            var activeButtons = cancelButtonText != null || confirmButtonText != null;
            var activeTitle = !string.IsNullOrEmpty(title);

            View
                .SetActiveButtonsGo(activeButtons)
                .SetActiveTitleGo(activeTitle)
                .SetToastBoxMessage(title, desc);

            foreach (var closeButton in View.ToastMessageBox.CloseButtons)
            {
                closeButton.enabled = !activeButtons;
            }

            if (activeButtons)
            {
                View.ToastBoxButton0.gameObject.SetActive(cancelButtonText != null);
                View.ToastBoxButton1.gameObject.SetActive(confirmButtonText != null);
                View.ToastBoxButton0.SetActiveButtonText(cancelButtonText);
                View.ToastBoxButton1.SetActiveButtonText(confirmButtonText);

                View.ToastBoxButton0.OnClick.RemoveAllListeners();
                View.ToastBoxButton1.OnClick.RemoveAllListeners();

                View.ToastBoxButton0.OnClick.AddListener(View.ToastMessageBox.Close);
                View.ToastBoxButton1.OnClick.AddListener(View.ToastMessageBox.Close);
                
                View.ToastBoxButton0.OnClick.AddListener(cancelButtonAction);
                View.ToastBoxButton1.OnClick.AddListener(confirmButtonAction);
            }

            SetToastMessageHorizontalAlignmentOptions(HorizontalAlignmentOptions.Center);
            return this;
        }

        public ControllerCanvasToastMessage SetToastMessageHorizontalAlignmentOptions(HorizontalAlignmentOptions type)
        {
            View.ToastMessageBoxDesc.horizontalAlignment = type;
            return this;
        }
        
        public ControllerCanvasToastMessage ShowToastMessage()
        {
            SetActiveView(ToastType.BoxMessage, true);
            return this;
        }

        public void ShowLoading(UnityAction timeoutCallback = null)
        {
            SetActiveView(ToastType.Loading, true);
            
            View.BigCircle.localRotation = Quaternion.Euler(0, 0, 0);
            View.SmallCircle.localRotation = Quaternion.Euler(0, 0, 0);
            
            View.BigCircle
                .DORotate(new Vector3(0, 0, -360), 1.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuart).SetLoops(int.MaxValue).SetUpdate(true);
            View.SmallCircle
                .DORotate(new Vector3(0, 0, 360), 1.5f, RotateMode.FastBeyond360)
                .SetEase(Ease.InOutQuart).SetLoops(int.MaxValue).SetUpdate(true);

            LoadingTimeOut(timeoutCallback).Forget();
        }

        private async UniTaskVoid LoadingTimeOut(UnityAction action)
        {
            if (_loadingCancellationTokenSource != null) CancelLoadingToken();
            
            _loadingCancellationTokenSource = new CancellationTokenSource();
            await UniTask.Delay(TimeSpan.FromSeconds(10), true, PlayerLoopTiming.Update, _loadingCancellationTokenSource.Token);

            CloseLoading();
            ShowTransientToastMessage(LocalizedTextType.ErrorMessage);
            
            action?.Invoke();
        }

        private void CancelLoadingToken()
        {
            if (_loadingCancellationTokenSource == null) return;
            _loadingCancellationTokenSource.Cancel();
            _loadingCancellationTokenSource.Dispose();
            _loadingCancellationTokenSource = null;
        }

        public void CloseLoading()
        {
            SetActiveView(ToastType.Loading, false);
            View.BigCircle.DOKill();
            View.SmallCircle.DOKill();
        }

        public void ShowSimpleRewardView(GoodItem goodItem, string title)
        {
            var goodItems = new List<GoodItem> { goodItem };
            ShowSimpleRewardView(goodItems, title).Forget();
        }

        public async UniTaskVoid ShowSimpleRewardView(List<GoodItem> goodItems, string title)
        {
            if(goodItems == null || goodItems.Count == 0) return;

            var displayItems = goodItems.Where(item => item.GoodType is not (GoodType.SummonElemental or GoodType.SummonRune)).ToList();
            if (displayItems.Count == 0) return;

            var closeButtons = View.SimpleRewardViewCanvasPopup.CloseButtons;
            foreach (var closeButton in closeButtons) closeButton.enabled = false;
            
            SetActiveView(ToastType.Reward, true);
            View.SetSimpleRewardTitle(title);
            var rewardViewGoods = _rewardViewGoods.GetViewSlots(ViewSlotRewardName, View.SimpleRewardViewGoodParent, goodItems.Count);

            for (var i = 0; i < displayItems.Count; ++i)
            {
                var goodItem = displayItems[i];
                var slot = rewardViewGoods[i];
                
                slot.transform.localScale = new Vector3(1, 0, 1);
                slot.SetInit(goodItem.GoodType, goodItem.Param0).SetValue(goodItem.Value, goodItem.Param0).SetActive(true);
                slot.transform
                    .DOScaleY(1, 0.25f)
                    .SetUpdate(true);

                await UniTask.Delay(150, true, PlayerLoopTiming.Update, Cts.Token);
            }
            
            foreach (var closeButton in closeButtons) closeButton.enabled = true;
        }

        public void ShowFadeOutIn(UnityAction fadeOutCallback = null, UnityAction fadeInCallback = null)
        {
            var fadeDuration = 0.7f;
            GameManager.Instance.Pause();
            
            View.FadeInOutBackground.gameObject.SetActive(true);
            View.FadeInOutBackground.DOFade(1, fadeDuration).OnComplete(() =>
            {
                fadeOutCallback?.Invoke();
                View.FadeInOutBackground.DOFade(0, fadeDuration).OnComplete(() =>
                {
                    GameManager.Instance.Play();
                    View.FadeInOutBackground.gameObject.SetActive(false);
                    fadeInCallback?.Invoke();
                }).SetUpdate(true);
            }).SetUpdate(true);
        }
        
        private void SetActiveView(ToastType toastType, bool flag)
        {            
            CancelLoadingToken();
            switch (toastType)
            {
                case ToastType.BoxMessage:
                    if (flag) View.ToastMessageBox.Open();
                    else View.ToastMessageBox.Close();
                    break;
                case ToastType.Loading:
                    View.LoadingPanel.gameObject.SetActive(flag);
                    break;
                case ToastType.Reward:
                    if (View.SimpleRewardViewCanvasPopup.isActiveAndEnabled == flag) break;
                    if (flag)
                    {
                        AutoCloseRewardPanel().Forget();
                        View.SimpleRewardViewCanvasPopup.Open();
                    }
                    else
                    {
                        View.SimpleRewardViewCanvasPopup.Close();
                        _autoCloseRewardViewToken?.Cancel();
                        _autoCloseRewardViewToken?.Dispose();
                        _autoCloseRewardViewToken = null;
                    }
                    break;
                case ToastType.TransientMessage:
                    View.TransientView.gameObject.SetActive(flag);
                    break;
                case ToastType.GoodInfo:
                    if (flag) View.ViewPopupGoodInfo.Open();
                    else View.ViewPopupGoodInfo.Close();
                    break;
                case ToastType.None:
                default:
                    break;
            }
        }

        private void AllCloseView()
        {
            View.ToastMessageBox.Close();
            View.SimpleRewardViewCanvasPopup.Close();
            View.ViewPopupGoodInfo.Close();
            View.TransientView.gameObject.SetActive(false);
            View.LoadingPanel.gameObject.SetActive(false);
            View.FadeInOutBackground.gameObject.SetActive(false);
        }

        private async UniTaskVoid AutoCloseRewardPanel()
        {
            if (_autoCloseRewardViewToken != null)
            {
                _autoCloseRewardViewToken.Cancel();
                _autoCloseRewardViewToken.Dispose();
                _autoCloseRewardViewToken = null;
            }
            
            _autoCloseRewardViewToken = new CancellationTokenSource();
            
            var seconds = 3;
            while (seconds > 0 && !_autoCloseRewardViewToken.IsCancellationRequested)
            {
                View.SetCloseSecondsText(LocalizeManager.GetText(LocalizedTextType.AutoCloseFewSeconds, seconds));
                seconds--;
                
                await UniTask.Delay(TimeSpan.FromSeconds(1), true, PlayerLoopTiming.Update, _autoCloseRewardViewToken.Token);
            }
            
            SetActiveView(ToastType.Reward, false);
        }
    }
}