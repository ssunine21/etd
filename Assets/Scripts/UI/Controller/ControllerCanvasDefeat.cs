using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasDefeat : ControllerCanvas
    {
        private ViewCanvasDefeat View => ViewCanvas as ViewCanvasDefeat;
        private bool _canMoveStage;

        public ControllerCanvasDefeat(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasDefeat>())
        {
        }

        public void ShowDefeatView(int seconds = 3)
        {
            OpenSetting();
            SetActive(true);
            GameManager.Instance.Pause();
            View.WrapCanvasGroup.alpha = 0;
            View.WrapCanvasGroup.DOFade(1, 1f).OnComplete(() =>
            {
                foreach (var closeButton in View.CloseButtons)
                {
                    closeButton.enabled = true;
                }
                View.TapToCloseTMP.enabled = true;
                AutoCloseDefeatView(seconds).Forget();
            })
            .SetUpdate(true);
        }

        private void OpenSetting()
        {
            View.Canvas.sortingOrder = Get<ControllerCanvasBottomMenu>().IsOpenMenu() ? 50 : 210;
            
            _canMoveStage = true;
            foreach (var closeButton in View.CloseButtons)
            {
                closeButton.enabled = false;
            }
            View.AutoConfirmText.gameObject.SetActive(false);
            View.TapToCloseTMP.enabled = false;
        }
        
        private async UniTaskVoid AutoCloseDefeatView(int seconds)
        {
            await UniTask.Delay(1000);
            View.AutoConfirmText.gameObject.SetActive(true);
            while (seconds > 0)
            {
                View.AutoConfirmText.text =
                    LocalizeManager.GetText(LocalizedTextType.AfterScondsMoveToPreviousStage, seconds);
                await UniTask.Delay(1000);
                seconds--;
            }

            if (_canMoveStage)
                MoveStage();
        }

        public override void Close()
        {
            MoveStage();
        }

        private void MoveStage()
        {
            if (!_canMoveStage) return;
            
            View.AutoConfirmText.gameObject.SetActive(false);
            View.TapToCloseTMP.enabled = false;
            
            _canMoveStage = false;
            GameManager.Instance.Play();
            StageManager.Instance.MoveToNormalStageLevel(DataController.Instance.stage.currTotalLevel - 1, () =>
            {
                View.AutoConfirmText.gameObject.SetActive(false);
                SetActive(false);
                DataController.Instance.player.ResetHp();
            });
        }
        
    }
}