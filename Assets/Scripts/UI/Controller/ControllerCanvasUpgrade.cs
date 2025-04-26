using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasUpgrade : ControllerCanvas
    {
        public int MenuIndex => 2;

        private ViewCanvasUpgrade View => ViewCanvas as ViewCanvasUpgrade;

        private List<ViewSlotUpgrade> _viewSlotUpgrades;
        private Sequence _openSequence;
        private Sequence _closeSequence;
        private bool _isPlayAnimation;

        public ControllerCanvasUpgrade(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasUpgrade>())
        {
            SetViewAnimation(ViewAnimationType.SlideUp);
            View.OnBindOpen += () => View.SlideButton.OnClick(0);

            View.SlideButton.AddListener(_ => UpdateAllSlotView());
            Init();
        }
        
        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.DefaultUpgrade => _viewSlotUpgrades.First(slot => slot.UpgradeType == UpgradeType.IncreaseBaseAtkPower).gameObject,
                TutorialType.UpgradeElementalSlot => View.SlideButton.SelectedIndex == 0 ? 
                    View.SlideButton.Buttons[1].gameObject : _viewSlotUpgrades.First(slot => slot.UpgradeType == UpgradeType.IncreaseElementalUnit).gameObject,
                TutorialType.UpgradeRuneSlot => View.SlideButton.SelectedIndex == 0 ? 
                    View.SlideButton.Buttons[1].gameObject : _viewSlotUpgrades.First(slot => slot.UpgradeType == UpgradeType.IncreaseRuneUnit).gameObject,
                _ => null
            };
        }

        private void Init()
        {
            _viewSlotUpgrades ??= new List<ViewSlotUpgrade>();
            for (var i = 0; i < DataController.Instance.upgrade.CloudDataOrderBy.Count; ++i)
            {
                var index = i;
                var slot = Object.Instantiate(View.ViewSlotUpgradePrefab, View.SlotScrollRect.content);
                ((CustomButton)slot.UpgradeButton.Button).onBindPointerUp += DataController.Instance.player.UpdateAttributeAll;
                ((CustomButton)slot.UpgradeButton.Button).onBindContinuousClick += (clickCount) =>
                {
                    var increaseCount = clickCount > 10 ? clickCount : 1;
                    if (TryUpgrade(index, increaseCount))
                    {
                        if (slot.UpgradeType == UpgradeType.IncreaseProjector)
                            UpdateAllSlotView();
                        else
                            UpdateSlotView(index);

                        UpgradeAnimation(index).Forget();
                    }
                };
                
                slot.UpgradeButton.OnClick.AddListener(() =>
                {
                    if (TryUpgrade(index))
                    {
                        if(slot.UpgradeType == UpgradeType.IncreaseProjector)
                            UpdateAllSlotView();
                        else 
                            UpdateSlotView(index);
                        
                        UpgradeAnimation(index).Forget();
                    }
                });
                
                slot.gameObject.SetActive(true);
                _viewSlotUpgrades.Add(slot);
            }
            
            DataController.Instance.contentUnlock.AddListenerUnlock(UnlockType.SummonElemental, (isUnlock) =>
            {
                View.SlideButton.Buttons[1].enabled = isUnlock;
            });
            
            UpdateAllSlotView();
        }

        private bool TryUpgrade(int index, int increaseCount = 1)
        {
            var data = DataController.Instance.upgrade.CloudDataOrderBy[index];
            var price = DataController.Instance.upgrade.GetUpgradePrice(data.upgradeType) * increaseCount;

            if (!DataController.Instance.good.TryConsume(data.goodType, price)) return false;
            DataController.Instance.upgrade.Upgrade(DataController.Instance.upgrade.CloudDataOrderBy[index].upgradeType, increaseCount);
            return true;
        }

        private void UpdateAllSlotView()
        {
            for (var i = 0; i < _viewSlotUpgrades.Count; ++i)
            {
                UpdateSlotView(i);
            }
        }

        private async UniTaskVoid UpgradeAnimation(int index)
        {
            if (_isPlayAnimation)
                return;

            _isPlayAnimation = true;
            var slot = _viewSlotUpgrades[index];
            var newPosition = new Vector2(500, 0);
            var oldPosition = new Vector2(-1000, 0);
            var duration = 0.23f;
            
            slot.LightBig.gameObject.SetActive(true);
            slot.LightBig.DOLocalMove(newPosition, duration).OnComplete(() =>
            {
                slot.LightBig.localPosition = oldPosition;
                slot.LightBig.gameObject.SetActive(false);
                _isPlayAnimation = false;
            });
        }

        private void UpdateSlotView(int index)
        {
            if (DataController.Instance.upgrade.CloudDataOrderBy.Count <= index) return;

            var cloudData = DataController.Instance.upgrade.CloudDataOrderBy[index];
            if (_viewSlotUpgrades.Count <= index) return;

            _viewSlotUpgrades[index].UpgradeType = cloudData.upgradeType;
            
            _viewSlotUpgrades[index]
                .SetIcon(DataController.Instance.upgrade.GetImage(cloudData.upgradeType))
                .SetContentsText(LocalizeManager.GetText(cloudData.localizedTextType))
                .SetLevel($"{DataController.Instance.upgrade.GetLevel(cloudData.upgradeType)}")
                .SetValue($"+{DataController.Instance.upgrade.GetValueText(cloudData.upgradeType)}")
                .SetGoodType(cloudData.goodType)
                .SetGoodValue(DataController.Instance.upgrade.GetUpgradePrice(cloudData.upgradeType))
                .SetActive(cloudData.goodType == GoodType.Gold ? View.SlideButton.SelectedIndex == 0 : View.SlideButton.SelectedIndex == 1)
                .SetMaxLevel(DataController.Instance.upgrade.IsMaxLevel(cloudData.upgradeType));

            if (cloudData.upgradeType == UpgradeType.IncreaseRuneUnit
                && !DataController.Instance.contentUnlock.IsUnLock(UnlockType.SummonRune))
            {
                var text = LocalizeManager.GetText(
                    LocalizedTextType.UnlockCondition,
                    DataController.Instance.contentUnlock.GetUnlockQuestLevel(UnlockType.SummonRune));

                _viewSlotUpgrades[index].SetLockPanel(true, text);
            }
            else if (IsLockableUpgradeType(cloudData.upgradeType))
            {
                var isLock = IsLock(cloudData.upgradeType);
                _viewSlotUpgrades[index].SetLockPanel(isLock, LocalizeManager.GetText(LocalizedTextType.UnlockUpgradeProjector));
            }
        }

        private bool IsLock(UpgradeType type)
        {
            var projectorLevel = DataController.Instance.upgrade.GetLevel(UpgradeType.IncreaseProjector);
            var level = DataController.Instance.upgrade.GetLevel(type);

            switch (type)
            {
                case UpgradeType.IncreaseElementalUnit:
                    return 2 * (projectorLevel + 1) <= level;
                case UpgradeType.IncreaseRuneUnit:
                    return 3 * (projectorLevel + 1) <= level;
                default:
                    return false;
            }
        }

        private bool IsLockableUpgradeType(UpgradeType type)
        {
            if (DataController.Instance.upgrade.IsMaxLevel(type)) return false;
            
            return type is UpgradeType.IncreaseElementalUnit or UpgradeType.IncreaseRuneUnit or UpgradeType.IncreaseProjector;
        }

        public void SelectMenu(int index)
        {
            View.SlideButton.OnClick(Mathf.Clamp(index, 0, 2));
        }

        public RectTransform GetGuideArrowParent(QuestType questType)
        {
            var upgradeType = questType switch
            {
                QuestType.UpgradeATK => UpgradeType.IncreaseBaseAtkPower,
                QuestType.UpgradeMaxHp => UpgradeType.IncreaseMaxHp,
                QuestType.UpgradeIncreaseHealAmount => UpgradeType.IncreaseHealAmountPerSecond,
                QuestType.UpgradeElementalSlot => UpgradeType.IncreaseElementalUnit,
                QuestType.UpgradeRuneSlot => UpgradeType.IncreaseRuneUnit,
                QuestType.UpgradeProjector => UpgradeType.IncreaseProjector,
                _ => UpgradeType.IncreaseBaseAtkPower
            };

            return _viewSlotUpgrades.Find((slot) => slot.UpgradeType == upgradeType).GuideArrowRectTransform;
        }
    }
}