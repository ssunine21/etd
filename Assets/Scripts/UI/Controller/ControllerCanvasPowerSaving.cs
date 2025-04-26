using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasPowerSaving : ControllerCanvas
    {
        private ViewCanvasPowerSaving View => ViewCanvas as ViewCanvasPowerSaving;
        private bool _onAutoPowerSaving;
        private int _originFrameRate;

        public ControllerCanvasPowerSaving(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasPowerSaving>())
        {
            
            View.SlideToUnlock.onBindDrag += Close;

            StageManager.Instance.onBindStageClear += (stageType) =>
            {
                if (stageType == StageType.Normal)
                {
                    View.SetCurrLevel(DataController.Instance.stage.CurrStageToFullSring);
                }
            };

            DataController.Instance.setting.onBindInitPowerSaveTime += () => _currAutoPowerSavingTime = 0;
            
            TimeTask().Forget();
            RotateIcon();
        }
        
        public override void Open()
        {
            base.Open();

            View.WrapCanvasGroup.alpha = 0;
            View.WrapCanvasGroup.DOFade(1, 0.25f).SetUpdate(true);

            View.SetCurrLevel(DataController.Instance.stage.CurrStageToFullSring);
            Get<ControllerCanvasMainMenu>().SetActive(false);
            Get<ControllerCanvasBottomMenu>().SetActive(false);
            
            _originFrameRate = Application.targetFrameRate;
            Application.targetFrameRate = 10;
        }

        public override void Close()
        {
            base.Close();

            _currAutoPowerSavingTime = 0f;
            Get<ControllerCanvasMainMenu>().SetActive(true);
            Get<ControllerCanvasBottomMenu>().SetActive(true);
            Application.targetFrameRate = _originFrameRate;
        }

        private float _currAutoPowerSavingTime;
        private async UniTaskVoid TimeTask()
        {
            var autoPowerSaveTimeForSec = 5 * 60;
            var displayRefreshTimeForSec = 1 * 60;
            
            var displayRefreshTime = 0f;
            
            while (!Cts.IsCancellationRequested)
            {
                if(displayRefreshTimeForSec < displayRefreshTime)
                {
                    var batteryLevel = View.BatteryInfo.GetBatteryLevel() * 100f;
                    var batteryColor = batteryLevel switch
                    {
                        <= 20 => Color.red,
                        <= 40 => Color.yellow,
                        _ => Color.white
                    };
                    View
                        .SetTimeText(DateTime.Now.ToString("hh:mm"))
                        .SetDateText(DateTime.Now.ToString("MM/dd/yyyy"))
                        .SetBatteryText($"{batteryLevel:N0}%")
                        .SetBatteryColor(batteryColor);
                }

                if (autoPowerSaveTimeForSec < _currAutoPowerSavingTime)
                {
                    if(!ActiveSelf)
                        Open();
                }

                await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.UnscaledDeltaTime, PlayerLoopTiming.Update, Cts.Token);
                
                displayRefreshTime += 1;

                if (DataController.Instance.setting.isAutoSleep)
                    _currAutoPowerSavingTime += 1;
                else
                    _currAutoPowerSavingTime = 0;
            }
        }

        private void RotateIcon()
        {
            var endValue = new Vector3(0, 0, -180);
            View.RotateIcon
                .DORotate(endValue, 1f)
                .SetEase(Ease.OutCubic)
                .SetLoops(int.MaxValue)
                .SetUpdate(true);
        }
    }
}