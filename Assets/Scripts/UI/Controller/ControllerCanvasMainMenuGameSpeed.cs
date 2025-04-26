using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMainMenu
    {
        private Sequence _onSequnce;

        private readonly int _gameSpeedDurationTimePerMinutes = 15;

        private void InitGameSpeed()
        {
            View.OnButton.onClick.AddListener(() =>
            {
                var isOn = !DataController.Instance.setting.isGameSpeedUp;
                if (isOn)
                {
                    if (!HasRemoveAds())
                    {
                        if (!IsRemainTime())
                        {
                            var usedGameSpeedFreeOnce = DataController.Instance.player.usedGameSpeedFreeOnce;
                            var confirmText = usedGameSpeedFreeOnce ? $"<sprite=14> {LocalizeManager.GetText(LocalizedTextType.ShowAds)}" : LocalizeManager.GetText(LocalizedTextType.Nickname_FreeOneChance);
                            
                            Get<ControllerCanvasToastMessage>().SetToastMessage(
                                    LocalizeManager.GetText(LocalizedTextType.GameSpeedUpTitle),
                                    LocalizeManager.GetText(LocalizedTextType.GameSpeedUpDescription, _gameSpeedDurationTimePerMinutes, 1 + DataController.Instance.setting.GameSpeed),
                                    LocalizeManager.GetText(LocalizedTextType.Cancel), null,
                                    confirmText, GameSpeedUp)
                                .ShowToastMessage();
                            return;
                        }
                    }
                }
                
                OnOff(isOn);
            });
            
            OnOff(DataController.Instance.setting.isGameSpeedUp);
            TimeTask().Forget();
        }

        private void GameSpeedUp()
        {
            var usedGameSpeedFreeOnce = DataController.Instance.player.usedGameSpeedFreeOnce;
            if (usedGameSpeedFreeOnce)
                GoogleMobileAdsManager.Instance.ShowRewardedAd(StartGameSpeedUp);
            else
            {
                DataController.Instance.player.usedGameSpeedFreeOnce = true;
                StartGameSpeedUp();
            }
        }

        private async UniTaskVoid TimeTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit);
            while (!HasRemoveAds())
            {
                if(IsRemainTime() && DataController.Instance.setting.isGameSpeedUp)
                {
                    var minutes = $"{DataController.Instance.setting.gameSpeedremainTimeForSec / 60:D2}";
                    var seconds = $"{DataController.Instance.setting.gameSpeedremainTimeForSec % 60:D2}";
                    View.SetTimeText($"{minutes}:{seconds}");
                
                    DataController.Instance.setting.gameSpeedremainTimeForSec -= 1;
                    
                    if(!IsRemainTime())
                        OnOff(false);
                }
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            }

            View.SetActiveTimeTMP(false);
        }

        private void StartGameSpeedUp()
        {
            DataController.Instance.setting.gameSpeedremainTimeForSec = 60 * _gameSpeedDurationTimePerMinutes;
            OnOff(true);
            
            DataController.Instance.LocalSave();
        }

        private void UpdateView(bool isOn)
        {
            View
                .SetMaterial(isOn ? null : ResourcesManager.GrayScaleMaterial)
                .SetGrayColor(isOn ? Color.white : ResourcesManager.GrayScaleColor)
                .SetActiveTimeTMP(isOn && !HasRemoveAds());
            OnSequnce(isOn);
        }

        private void OnOff(bool isOn)
        {
            if (isOn)
            {
                if (!HasRemoveAds() && !IsRemainTime())
                {
                    isOn = false;
                }
            }

            DataController.Instance.setting.isGameSpeedUp = isOn;
            if (isOn)
                DataController.Instance.quest.Count(QuestType.ClickGameSpeed);
            
            UpdateView(isOn);
            
            DataController.Instance.buff.OnBindGameSpeed?.Invoke();
        }

        private bool HasRemoveAds()
        {
            return DataController.Instance.good.GetValue(GoodType.RemoveAds) > 0;
        }

        private bool IsRemainTime()
        {
            return DataController.Instance.setting.gameSpeedremainTimeForSec > 0;
        }

        private void OnSequnce(bool flag)
        {
            if (_onSequnce == null && flag)
            {
                var duration = 0.3f;
                var moveOffset = 20f;
                var localPosition0 = View.ArrowImage0.transform.localPosition;
                var localPosition1 = View.ArrowImage1.transform.localPosition;
                _onSequnce = DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() =>
                    {
                        var color = new Color(1, 1, 1, 0);
                        View.ArrowImage0.color = color;
                        View.ArrowImage1.color = color;

                        var offset0 = localPosition0;
                        var offset1 = localPosition1;
                        offset0.x -= moveOffset;
                        offset1.x -= moveOffset;

                        View.ArrowImage0.transform.localPosition = offset0;
                        View.ArrowImage1.transform.localPosition = offset1;
                    })
                    .OnPause(() =>
                    {
                        View.ArrowImage0.color = Color.white;
                        View.ArrowImage1.color = Color.white;
                        View.ArrowImage0.transform.localPosition = localPosition0;
                        View.ArrowImage1.transform.localPosition = localPosition1;
                    })
                    .Append(View.ArrowImage1.transform.DOLocalMoveX(localPosition1.x, duration))
                    .Join(View.ArrowImage1.DOFade(1, duration))
                    .Insert(0.2f, View.ArrowImage0.transform.DOLocalMoveX(localPosition0.x, duration))
                    .Join(View.ArrowImage0.DOFade(1, duration))
                    .AppendInterval(1f)
                    .SetLoops(int.MaxValue)
                    .SetUpdate(true);
            }
            else
            {
                if(flag)
                    _onSequnce.Restart();
                else
                {
                    _onSequnce.Pause();
                }
            }
        }
    }
}
