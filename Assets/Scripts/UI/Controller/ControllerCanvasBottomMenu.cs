using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasBottomMenu : ControllerCanvas
    {
        public readonly ButtonRadioGroup bottomButtonGroup;
        
        private ViewCanvasBottomMenu View => ViewCanvas as ViewCanvasBottomMenu;
        private readonly List<LayoutElement> _menuLayoutGroups;

        private readonly List<ControllerCanvas> _activableCanvasList = new();
        
        public ControllerCanvasBottomMenu(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasBottomMenu>())
        {
            SetActive(true);
            
            _menuLayoutGroups = new List<LayoutElement>();
            bottomButtonGroup = new ButtonRadioGroup();
            foreach (var menu in View.BottomMenus)
            {
                bottomButtonGroup.AddListener(menu);
                if (menu.TryGetComponent(out LayoutElement component))
                {
                    _menuLayoutGroups.Add(component);
                }
            }
            
            bottomButtonGroup.onBindSelected += ShowMenu;
            
            DataController.Instance.contentUnlock.AddListenerUnlock(UnlockType.SummonRune, isUnlock => View.BottomMenus[1].Button.enabled = isUnlock);
            DataController.Instance.contentUnlock.AddListenerUnlock(UnlockType.DungeonMenu, isUnlock => View.BottomMenus[3].Button.enabled = isUnlock);
            DataController.Instance.contentUnlock.AddListenerUnlock(UnlockType.ShopMenu, isUnlock => View.BottomMenus[4].Button.enabled = isUnlock);
            
            StageManager.Instance.onBindChangeStageType += UpdateChangeStage;
        }
        
        public GameObject GetTutorialObject(TutorialType tutorialType)
        {
            return tutorialType switch
            {
                TutorialType.EquipElemental => View.BottomMenus[0].gameObject,
                TutorialType.EquipRune => View.BottomMenus[1].gameObject,
                TutorialType.DefaultUpgrade => View.BottomMenus[2].gameObject,
                TutorialType.UpgradeElementalSlot => View.BottomMenus[2].gameObject,
                TutorialType.UpgradeRuneSlot => View.BottomMenus[2].gameObject,
                TutorialType.GoldDungeon => View.BottomMenus[3].gameObject,
                TutorialType.DiaDungeon => View.BottomMenus[3].gameObject,
                TutorialType.EnhanceDungeon => View.BottomMenus[3].gameObject,
                TutorialType.SummonElemental => View.BottomMenus[4].gameObject,
                TutorialType.SummonRune => View.BottomMenus[4].gameObject,
                _ => null
            };
        }

        public bool IsOpenMenu()
        {
            return _activableCanvasList.Any(activableCanvas => activableCanvas.ActiveSelf);
        }

        private void ShowMenu(int selectedIndex)
        {
            if (_activableCanvasList.Count <= 0)
            {
                _activableCanvasList.Add(Get<ControllerCanvasElemental>());
                _activableCanvasList.Add(Get<ControllerCanvasRune>());
                _activableCanvasList.Add(Get<ControllerCanvasUpgrade>());
                _activableCanvasList.Add(Get<ControllerCanvasDungeon>());
                _activableCanvasList.Add(Get<ControllerCanvasShop>());
            }
            
            if (View.BottomMenus[selectedIndex].TryGetComponent(out ContentUnlock contentUnlock))
                if (contentUnlock.goLockPanel.activeSelf)
                    return;

            var index = 0;
            foreach (var activableCanvas in _activableCanvasList)
            {
                var isShow = index == selectedIndex;
                var flexibleWidthSize = 1f;
                if (isShow)
                {
                    if (activableCanvas.ActiveSelf)
                    {
                        View.BottomMenus[selectedIndex].Selected(false);
                        activableCanvas.Close();
                    }
                    else
                    {
                        flexibleWidthSize = 1.4f;
                        activableCanvas.Open();
                    }
                }
                else
                {
                    activableCanvas.Close();
                }
                _menuLayoutGroups[index]
                    .DOFlexibleSize(new Vector2(flexibleWidthSize, 1), 0.1f)
                    .SetEase(Ease.OutQuart)
                    .SetUpdate(true);
                ++index;
            }
        }
        
        private void UpdateChangeStage(StageType stageType, int param = 0)
        {
            View.SetActive(stageType == StageType.Normal);
        }
    }
}