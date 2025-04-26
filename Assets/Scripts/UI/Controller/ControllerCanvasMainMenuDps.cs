using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public partial class ControllerCanvasMainMenu
    {
        private bool _isOnDpsPanel;
        private bool _isShowAllView;
        private int _currTimeSec;

        private bool IsTimeOver => _currTimeSec > 1799;
        
        private void InitDPS()
        {
            View.DpsArrowButton.onClick.AddListener(() => OnOffPresetPanel(!_isOnDpsPanel));
            View.DpsSwitchButton.onClick.AddListener(() =>
            {
                _isShowAllView = !_isShowAllView;
                UpdateDPSView();
            });
            View.DpsResetButton.onClick.AddListener(() =>
            {
                for (var i = 0; i < DataController.Instance.player.dpsContainer.Length; ++i)
                {
                    DataController.Instance.player.dpsContainer[i] = 0;
                }

                DataController.Instance.player.totalDps = 0;
                _currTimeSec = 0;
                SetProjectorDPS();
                SetAllDPS();
            });

            OnOffPresetPanel(false);
            UpdateDPSView();
            
            DpsTimeTask().Forget();
            DpsMainTask().Forget();

            DataController.Instance.player.OnBindChangedElemental += (projectorIndex) =>
            {
                var elementals = DataController.Instance.player.GetEquippedElementals(projectorIndex);
                var i = 0;
                if (!_isShowAllView) return;

                foreach (var elemental in elementals)
                {
                    var index = projectorIndex * 3 + i;
                    if (elemental != null)
                    {
                        var sprite = DataController.Instance.elemental.GetImage(elemental.type, elemental.grade);
                        View.ViewSlotDps[index]
                            .SetIcon(sprite);
                    }
                    ++i;
                }
            };
        }

        private async UniTaskVoid DpsTimeTask()
        {
            while (true)
            {
                await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
                if (IsTimeOver) continue;
         
                _currTimeSec++;
                UpdateTime();
            }
        }

        private async UniTaskVoid DpsMainTask()
        {
            while (true)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, Cts.Token);
                if(IsTimeOver) continue;
                
                if (_isShowAllView)
                    SetAllDPS();
                else
                    SetProjectorDPS();
            }
        }

        private void SetProjectorDPS()
        {
            var i = 0;
            for (; i < 3; ++i)
            {
                double dps = 0;
                for (var j = 0; j < 3; ++j)
                {
                    var index = i * 3 + j;
                    dps += DataController.Instance.player.dpsContainer[index];
                }

                var percent = (DataController.Instance.player.totalDps == 0 || dps == 0)
                    ? 0f
                    : (float)dps / (float)DataController.Instance.player.totalDps;
                
                View.ViewSlotDps[i]
                    .SetValueText(dps.ToDamage())
                    .SetFillAmount(percent)
                    .SetHeight(100f)
                    .SetProjectorColor(DataController.Instance.player.GetProjectorUnitColor(i))
                    .SetProjector(true);
            }

            i = 0;
            for (; i <= DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseProjector); ++i)
                View.ViewSlotDps[i].SetActive(true);

            for (; i < View.ViewSlotDps.Length; ++i)
            {
                View.ViewSlotDps[i].SetActive(false);
            }
        }

        private void SetAllDPS()
        {
            var i = 0;
            foreach (var dps in DataController.Instance.player.dpsContainer)
            {
                
                var percent = (DataController.Instance.player.totalDps == 0 || dps == 0)
                    ? 0f
                    : (float)dps / (float)DataController.Instance.player.totalDps;

                View.ViewSlotDps[i]
                    .SetValueText(dps.ToDamage())
                    .SetFillAmount(percent)
                    .SetHeight(50f)
                    .SetProjector(false);
                ++i;
            }
            
            for(var j = 0; j < 3; ++j)
            {
                var index0 = 0;
                foreach (var elemental in DataController.Instance.player.GetEquippedElementals(j))
                {
                    var index1 = j * 3 + index0;
                    if (elemental != null)
                    {
                        var sprite = DataController.Instance.elemental.GetImage(elemental.type, elemental.grade);
                        View.ViewSlotDps[index1].SetIcon(sprite);
                    }
                    View.ViewSlotDps[index1].SetActive(elemental != null);
                    ++index0;
                }
            }
        }

        private void UpdateDPSView()
        {
            if (_isShowAllView)
            {
                SetAllDPS();
            }
            else
            {
                SetProjectorDPS();
            }
        }

        private void UpdateTime()
        {
            var minutes = $"{_currTimeSec / 60:D2}";
            var seconds = $"{_currTimeSec % 60:D2}";
            View.SetPresetTimeText($"{minutes}:{seconds}");
        }
        
        private void OnOffPresetPanel(bool isOn)
        {
            const float duration = 0.5f;
            var toX = isOn ? -120f : 195f;
            var toZ = isOn ? 0f : 180f;
            View.DpsPanel.DOAnchorPosX(toX, duration).SetEase(Ease.OutBack).SetUpdate(true);
            View.DpsArrowButton.transform
                .DORotate(new Vector3(0, 0, toZ), duration * 1.5f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
            
            View.DpsSwitchButton.gameObject.SetActive(isOn);
            View.DpsResetButton.gameObject.SetActive(isOn);
            View.DpsTimeGo.SetActive(isOn);

            _isOnDpsPanel = isOn;
        }
    }
}