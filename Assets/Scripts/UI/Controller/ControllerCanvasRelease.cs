using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasRelease : ControllerCanvas
    {
        private ViewCanvasRelease View => ViewCanvas as ViewCanvasRelease;
        private Sequence _sequnce;
        private readonly Queue<UnlockType> _unlockTypeQueue = new(); 

        public ControllerCanvasRelease(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasRelease>())
        {
            View.Close.onClick.AddListener(() =>
            {
                Close();
                if(_unlockTypeQueue.Count > 0)
                    Show(_unlockTypeQueue.Dequeue());
            });

            Init().Forget();
        }

        private async UniTaskVoid Init()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.WaitUntil(() => !Get<ControllerCanvasOfflineReward>().ActiveSelf, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.WaitUntil(() => !Get<ControllerCanvasAttendance>().ActiveSelf, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.WaitUntil(() => !Get<ControllerCanvasTutorial>().IsTutorialing, PlayerLoopTiming.Update, Cts.Token);

            foreach (UnlockType unlockType in Enum.GetValues(typeof(UnlockType)))
            {
                if (IsShowable(unlockType))
                {
                    Show(unlockType);
                    DataController.Instance.contentUnlock.SetOpen(unlockType);
                }
            }
        }

        private bool IsShowable(UnlockType unlockType)
        {
            return DataController.Instance.quest.currQuestLevel >
                   DataController.Instance.contentUnlock.GetUnlockQuestLevel(unlockType)
                   && !DataController.Instance.contentUnlock.IsOpened(unlockType);
        }
        
        public void Show(UnlockType unlockType)
        {
            var sprite = DataController.Instance.contentUnlock.GetUnlockedSprite(unlockType);
            if (sprite == null) return;
            
            _unlockTypeQueue.Enqueue(unlockType);
            if(_unlockTypeQueue.Count > 1) return;

            View.Icon.sprite = sprite;
            Open();
            View.SetContentsText(LocalizeManager.GetText(unlockType));
            
            if (_sequnce == null)
            {
                const float mainDuration = 0.3f;
                const float strokeScaleOffset = 3.5f;
                const float strokeScaleDuration = 0.6f;
                const float lightDuration = 1.8f;
                
                _sequnce = DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() =>
                    {
                        View.WrapCanvasGroup.alpha = 0;
                        View.WrapCanvasGroup.transform.localScale = Vector3.one * 1.5f;

                        var color = new Color(1, 1, 1, 1);
                        View.Stroke0.color = color;
                        View.Stroke1.color = color;
                        View.Stroke0.transform.localScale = Vector3.one;
                        View.Stroke1.transform.localScale = Vector3.one;

                        var lightImageColor = View.LightImage.color;
                        lightImageColor.a = 0;
                        View.LightImage.color = lightImageColor;
                        View.ContentsTMP.enabled = false;
                        View.TabToCloseTMP.enabled = false;

                        View.Close.enabled = false;
                        
                        View.ShowIconSprite(false).SetLockIconSprite(true);
                    })
                    .Append(View.WrapCanvasGroup.DOFade(1, mainDuration))
                    .Join(View.WrapCanvasGroup.transform.DOScale(Vector3.one, mainDuration))
                    .Join(View.LockIcon.transform.DOShakePosition(lightDuration, 20f, 100))
                    .Join(View.LightImage.DOFade(1, lightDuration).OnComplete(() =>
                    {
                        View.ShowIconSprite(true);
                        View.ContentsTMP.enabled = true;
                    }))
                    .Append(View.LightImage.DOFade(0, 0.1f))
                    .Join(View.Stroke0.transform.DOScale(Vector3.one * strokeScaleOffset, strokeScaleDuration))
                    .Join(View.Stroke1.transform.DOScale(Vector3.one * strokeScaleOffset, strokeScaleDuration))
                    .Join(View.Stroke0.DOFade(0, strokeScaleDuration))
                    .Join(View.Stroke1.DOFade(0, strokeScaleDuration))
                    .OnComplete(() =>
                    {
                        _unlockTypeQueue.Dequeue();
                        View.Close.enabled = true;
                        View.TabToCloseTMP.enabled = true;
                    });
            }
            else 
                _sequnce.Restart();
            
            DataController.Instance.contentUnlock.OnBindInitUnlockDic[unlockType]?.Invoke();
        }
    }
}