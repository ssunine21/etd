using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasHp : ControllerCanvas
    {
        private ViewCanvasHp View => ViewCanvas as ViewCanvasHp;

        public ControllerCanvasHp(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasHp>())
        {
            SetActive(true);
            DataController.Instance.player.OnBindChangedHp += UpdateHpView;
            
            MainTask().Forget();
        }

        private async UniTaskVoid MainTask()
        { 
            var timeSinceLastRegen = 0f;
            while (!Cts.IsCancellationRequested)
            {
                await UniTask.Yield();
                if (DataController.Instance.player.CurrHp < DataController.Instance.player.MaxHp && GameManager.Instance.IsPlaying)
                {
                    if (timeSinceLastRegen >= 1)
                    {
                        DataController.Instance.player.CurrHp += 
                            1 + DataController.Instance.upgrade.GetValue(UpgradeType.IncreaseHealAmountPerSecond) 
                              + DataController.Instance.research.GetValue(ResearchType.IncreaseHealAmountPerSecond);
                        timeSinceLastRegen = 0;
                        DataController.Instance.player.OnBindChangedHp?.Invoke();
                    }
                }
                timeSinceLastRegen += Time.deltaTime;
            }
        }

        private void UpdateHpView()
        {
            var curr = DataController.Instance.player.CurrHp;
            var max = DataController.Instance.player.MaxHp;

            var endValue = curr <= 0 ? 0f : (float)(curr / max);
            View.HpFillImage.fillAmount = endValue;
        }
    }
}