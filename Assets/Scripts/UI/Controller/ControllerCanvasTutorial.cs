using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    [Flags]
    public enum TutorialCanvasType
    {
        None = 0 ,
        GuideView = 1 << 0,
        ScriptView = 1 << 1,
        ObjectView = 1 << 2,
    }
    
    public class ControllerCanvasTutorial : ControllerCanvas
    {
        public bool IsTutorialing;
        
        private ViewCanvasTutorial View => ViewCanvas as ViewCanvasTutorial;
        private GameObject _guideArrow;
        private readonly List<GameObject> _copyObjects = new();
        private readonly UnityEvent _onTextShowed;

        private TutorialType _currTutorialType;
        private TutorialCanvasType _currTutorialCanvasType;
        private int _currPageOrder;
        private int _endPageOrder;

        private readonly float backgroundFadeValue = 0.8f;

        public ControllerCanvasTutorial(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasTutorial>())
        {
            View.NextScriptButton.onClick.AddListener(() =>
            {
                if(View.Typewriter.isShowingText)
                    View.Typewriter.SkipTypewriter();
                else
                {
                    if (_endPageOrder == -1 || _currPageOrder < _endPageOrder)
                        UpdateScriptView(_currTutorialType, _currPageOrder + 1);
                    else
                    {
                        View.ScriptViewCanvasPopupWrap.Close();
                        _onTextShowed?.Invoke();
                    }
                }
            });
            
            View.Typewriter.onTypewriterStart.AddListener(() => View.ScriptArrow.enabled = false);
            View.Typewriter.onTextShowed.AddListener(() =>
            {
                View.ScriptArrow.enabled = true;
                View.ScriptArrow.DOKill();
                View.ScriptArrow.transform
                    .DOLocalJump(View.ScriptArrow.transform.localPosition, 7, 1, 1f)
                    .SetLoops(int.MaxValue)
                    .SetUpdate(true);
            });

            DataController.Instance.quest.OnBindCount += () =>
            {
                if (DataController.Instance.quest.IsCanBeCleared && DataController.Instance.quest.currQuestLevel < 16)
                {
                    ShowGuideArrow(Get<ControllerCanvasMainMenu>().GetGuideArrowParent(TutorialType.GameStart));
                }
            };

            DataController.Instance.quest.OnBindClear += (questLevel) => StartTutorial(questLevel).Forget();
            
            _onTextShowed = new UnityEvent();
            
            TutorialTask().Forget();
        }

        private async UniTaskVoid TutorialTask()
        {
            await UniTask.WaitUntil(() => ServerTime.IsInit, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.Delay(1000, true, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.WaitUntil(() => !Get<ControllerCanvasOfflineReward>().ActiveSelf, PlayerLoopTiming.Update, Cts.Token);
            await UniTask.WaitUntil(() => !Get<ControllerCanvasAttendance>().ActiveSelf, PlayerLoopTiming.Update, Cts.Token);
            
            var currQuestLevel = DataController.Instance.quest.currQuestLevel;
            if (DataController.Instance.tutorial.TryGetQuestType(currQuestLevel, out var tutorialType))
            {
                var tutorialBData = DataController.Instance.tutorial.Get(tutorialType);
                if (tutorialBData.isOnAwake && (!DataController.Instance.quest.IsCanBeCleared || currQuestLevel == 0))
                    StartTutorial(tutorialType);
            }
        }

        private async UniTaskVoid StartTutorial(int currQuestLevel)
        {
            CloseGuideArrow();
            if (DataController.Instance.contentUnlock.IsShowAnimation(currQuestLevel))
            {
                var controllerCanvasRelease = Get<ControllerCanvasRelease>();
                await UniTask.WaitUntil(() => controllerCanvasRelease.ActiveSelf);
                await UniTask.WaitUntil(() => !controllerCanvasRelease.ActiveSelf);
            }
            
            if (DataController.Instance.tutorial.TryGetQuestType(currQuestLevel, out var tutorialType))
            {
                StartTutorial(tutorialType);
            }
        }

        private void SetElementalSlotTextColor(Transform tr)
        {
            if (tr)
            {
                if (tr.TryGetComponent<TMP_Text>(out var tmpText))
                    tmpText.color = new Color(149f / 255f, 202f / 255f, 208f / 255f);
            }
        }

        public void StartTutorial(TutorialType tutorialType)
        {
            ClearCopyObject();
            switch (tutorialType) 
            {
                case TutorialType.DefaultUpgrade:
                    ShowScript(tutorialType, async () =>
                    {
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(tutorialType),
                            async () =>
                            {
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(Get<ControllerCanvasUpgrade>().GetTutorialObject(tutorialType), Close);
                            });
                    });
                    break;
                case TutorialType.GoldDungeon:
                    ShowScript(tutorialType, async () =>
                    { 
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(tutorialType), async () =>
                            {
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(Get<ControllerCanvasDungeon>().GetTutorialObject(tutorialType), async () =>
                                    { 
                                        ShowVoid();
                                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                        ShowObjectWithGuideArrow(Get<ControllerCanvasDungeon>().GetTutorialObject(tutorialType), Close);
                                    });
                            });
                    });
                    break;
                case TutorialType.DiaDungeon:
                    ShowScript(tutorialType, async () =>
                    { 
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(tutorialType), async () =>
                        {
                            ShowVoid();
                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                            ShowObjectWithGuideArrow(Get<ControllerCanvasDungeon>().GetTutorialObject(tutorialType), async () =>
                            { 
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(Get<ControllerCanvasDungeon>().GetTutorialObject(tutorialType), Close);
                            });
                        });
                    });
                    break;
                case TutorialType.EnhanceDungeon:
                    ShowScript(tutorialType, async () =>
                    { 
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(tutorialType), async () =>
                        {
                            ShowVoid();
                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                            ShowObjectWithGuideArrow(Get<ControllerCanvasDungeon>().GetTutorialObject(tutorialType), async () =>
                            { 
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(Get<ControllerCanvasDungeon>().GetTutorialObject(tutorialType), Close);
                            });
                        });
                    });
                    break;
                case TutorialType.SummonRune:
                    ShowScript(tutorialType, async () =>
                    {
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.SummonRune), async () =>
                        {
                            var canvasShop = Get<ControllerCanvasShop>();
                            canvasShop.ScrollToSummonRuneRect().Forget();
                                
                            ShowVoid();
                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                            ShowObjectWithGuideArrow(canvasShop.GetTutorialObject(TutorialType.SummonRune), async () =>
                            {
                                Close();
                                await UniTask.WaitUntil(() => Get<ControllerCanvasSummon>().ActiveSelf);
                                await UniTask.WaitUntil(() => !Get<ControllerCanvasSummon>().ActiveSelf);
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(
                                    Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.SummonRune),
                                    () => ShowQuestPanelWithGuideArrow().Forget());
                            });
                        });
                    });
                    break;
                case TutorialType.EquipRune:
                    ShowScript(tutorialType, async () =>
                    {
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.EquipRune),
                            async () =>
                            {
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowScript( tutorialType, () =>
                                {
                                    ShowScriptAndObjectWithGuideArrow(tutorialType, Get<ControllerCanvasRune>().GetTutorialObject(TutorialType.Tag),
                                        async () =>
                                        {
                                            ShowVoid();
                                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                            ShowObjectWithGuideArrow(Get<ControllerCanvasRune>().GetTutorialObject(TutorialType.EquipRune),
                                                () => ShowObjectWithGuideArrow(Get<ControllerCanvasRune>().GetTutorialObject(TutorialType.EquipRune),
                                                    () => ShowObjectWithGuideArrow(Get<ControllerCanvasRune>().GetTutorialObject(TutorialType.EquipRuneSlot), 
                                                        async () =>
                                                        {
                                                            ShowVoid();
                                                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                                            ShowScriptAndObjectWithGuideArrow(tutorialType, Get<ControllerCanvasRune>().GetTutorialObject(TutorialType.Tag),
                                                                () =>
                                                                {
                                                                    ClearCopyObject();
                                                                    Close();
                                                                }, 4, 5);
                                                        })));
                                        }, 2, 3);
                                }, 1, 1);
                            });
                    }, 0, 0);
                    break;
                case TutorialType.UpgradeRuneSlot:
                    ShowScript(tutorialType, async () =>
                    { 
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.UpgradeRuneSlot), async () =>
                        {
                            ShowVoid();
                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                            ShowObjectWithGuideArrow(Get<ControllerCanvasUpgrade>().GetTutorialObject(TutorialType.UpgradeRuneSlot), async () =>
                            {
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(Get<ControllerCanvasUpgrade>().GetTutorialObject(TutorialType.UpgradeRuneSlot), async () =>
                                {
                                    ShowVoid();
                                    await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);

                                    ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.UpgradeRuneSlot),
                                        () => ShowQuestPanelWithGuideArrow().Forget());
                                });
                            });
                        });
                    });
                    break;
                case TutorialType.SummonElemental:
                    ShowScript(tutorialType, async () =>
                    {
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.SummonElemental), async () =>
                            {
                                var canvasShop = Get<ControllerCanvasShop>();
                                canvasShop.ScrollToSummonElementalRect().Forget();
                                
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(canvasShop.GetTutorialObject(TutorialType.SummonElemental), async () =>
                                    {
                                        Close();
                                        await UniTask.WaitUntil(() => Get<ControllerCanvasSummon>().ActiveSelf);
                                        await UniTask.WaitUntil(() => !Get<ControllerCanvasSummon>().ActiveSelf);
                                        ShowVoid();
                                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                        ShowObjectWithGuideArrow(
                                            Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.SummonElemental),
                                            () => ShowQuestPanelWithGuideArrow().Forget());
                                    });
                            });
                    });
                    break;
                case TutorialType.UpgradeElementalSlot:
                    ShowScript(tutorialType, async () =>
                    { 
                        ShowVoid();
                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.UpgradeElementalSlot), async () =>
                            {
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                ShowObjectWithGuideArrow(Get<ControllerCanvasUpgrade>().GetTutorialObject(TutorialType.UpgradeElementalSlot), async () =>
                                    {
                                        ShowVoid();
                                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                        ShowObjectWithGuideArrow(Get<ControllerCanvasUpgrade>().GetTutorialObject(TutorialType.UpgradeElementalSlot), async () =>
                                        {
                                            ShowVoid();
                                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);

                                            ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.UpgradeElementalSlot),
                                                () => ShowQuestPanelWithGuideArrow().Forget());
                                        });
                                    });
                            });
                    });
                    break;
                case TutorialType.EquipElemental:
                    ShowScript(tutorialType, () =>
                        ShowObjectWithGuideArrow(Get<ControllerCanvasBottomMenu>().GetTutorialObject(TutorialType.EquipElemental),
                            async () =>
                            {
                                ShowVoid();
                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                GuideDescriptionElementalSlot(
                                    async () =>
                                    {
                                        ClearCopyObject();
                                        ShowVoid();
                                        await UniTask.Delay(300, true, PlayerLoopTiming.Update, Cts.Token);
                                        ShowScript(tutorialType, async () =>
                                        {
                                            ShowVoid();
                                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                            ShowObjectWithGuideArrow(Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.SelectElementalSlot),
                                                () => ShowObjectWithGuideArrow(Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.SelectElementalSlot),
                                                    () => ShowObjectWithGuideArrow(Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.EquipLinkSlot),
                                                        async () =>
                                                    {
                                                        ShowVoid();
                                                        await UniTask.Delay(300, true, PlayerLoopTiming.Update, Cts.Token);
                                                        ShowScript(tutorialType, () =>
                                                        {
                                                            ShowScriptAndObjectWithGuideArrow(tutorialType, Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.Tag), () =>
                                                            {
                                                                ClearCopyObject();
                                                                Close();
                                                                DataController.Instance.setting.isGuideElemental = true;
                                                            }, 4, 5);
                                                        }, 3, 3);
                                                    })));
                                        }, 2, 2);
                                        
                                       
                                    });
                            }), 0, 1);
                    break;
                case TutorialType.Research:
                    ShowScript(tutorialType, () =>
                    {
                        ShowObjectWithGuideArrow(Get<ControllerCanvasMainMenu>().GetTutorialObject(tutorialType), async () =>
                        {
                            ClearCopyObject();
                            ShowVoid();
                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                            var labCanvas = Get<ControllerCanvasLab>();
                            var buttonObject0 = labCanvas.GetTutorialObject(tutorialType, 0).GetComponent<Button>();
                            var buttonObject1 = labCanvas.GetTutorialObject(tutorialType, 1).GetComponent<Button>();
                            buttonObject0.enabled = false;
                            buttonObject1.enabled = false;
                            ShowScriptAndObjectWithGuideArrow(tutorialType, buttonObject0.gameObject,
                                async () =>
                                {                        
                                    ClearCopyObject();
                                    ShowVoid();
                                    
                                    await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                    ShowScriptAndObjectWithGuideArrow(tutorialType, buttonObject1.gameObject,
                                    () =>
                                    {
                                        buttonObject0.enabled = true;
                                        buttonObject1.enabled = true;
                                        ShowObjectWithGuideArrow(buttonObject1.gameObject, async () =>
                                        {
                                            ShowVoid();
                                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                            ShowScriptAndObject(tutorialType, labCanvas.GetTutorialObject(tutorialType, 3), async () =>
                                            {
                                                ShowVoid();
                                                await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                                ShowObjectWithGuideArrow(labCanvas.GetTutorialObject(tutorialType, 2), async () =>
                                                    {
                                                        ShowVoid();
                                                        await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                                                        ShowScriptAndObject(tutorialType, labCanvas.GetTutorialObject(tutorialType, 4), () =>
                                                        {
                                                            ClearCopyObject();
                                                            Close();
                                                        }, 4, 4);
                                                    });
                                            }, 3, 3);
                                        });
                                    }, 2, 2);
                                    
                                }, 1, 1);
                        });
                    }, 0, 0);
                    break;
                case TutorialType.Raid:
                    ShowScript(tutorialType, () =>
                    {
                        ShowObjectWithGuideArrow(Get<ControllerCanvasMainMenu>().GetTutorialObject(tutorialType), async () =>
                        {
                            ClearCopyObject();
                            ShowVoid();
                            await UniTask.Delay(500, true, PlayerLoopTiming.Update, Cts.Token);
                            ShowScript(tutorialType, async () =>
                            {
                                ClearCopyObject();
                                Close();
                            }, 1, 3);
                        });
                    }, 0, 0);
                    break;
                case TutorialType.GameStart:
                    ShowScript(tutorialType, () =>
                    {
                        Close();
                        ShowGuideArrow(Get<ControllerCanvasMainMenu>().GetGuideArrowParent(TutorialType.GameStart));
                    });
                    break;
                case TutorialType.GameSpeed:
                    ShowScript(tutorialType, () => ShowObjectWithGuideArrow(Get<ControllerCanvasMainMenu>().GetTutorialObject(TutorialType.GameSpeed), Close));
                    break;
                case TutorialType.Buff:
                    ShowScript(tutorialType, () => ShowObjectWithGuideArrow(Get<ControllerCanvasMainMenu>().GetTutorialObject(TutorialType.Buff), Close));
                    break;
                default:
                    break;
            }
        }

        private void GuideDescriptionElementalSlot(UnityAction callback)
        {
            callback += ClearCopyObject;
            
            ShowScript(TutorialType.DescriptionSlot, 
                () =>  ShowScriptAndObject(TutorialType.DescriptionActiveSlot,
                    Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.DescriptionActiveSlot),
                    () => ShowScriptAndObject(TutorialType.DescriptionLinkSlot,
                        Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.DescriptionLinkSlot),
                        () => ShowScriptAndObject(TutorialType.DescriptionPassiveSlot,
                            Get<ControllerCanvasElemental>().GetTutorialObject(TutorialType.DescriptionPassiveSlot),
                            () => callback?.Invoke()))));
        }

        private void ShowVoid()
        {
            Open(TutorialCanvasType.ObjectView);
            View.Background.color = new Color(0, 0, 0, 0);
        }

        private void ShowScriptAndObject(TutorialType type, GameObject originGo, UnityAction onTextShowed = null, int startOrder = 0, int endOrder = -1)
        {
            ShowScript(type, onTextShowed, startOrder, endOrder, TutorialCanvasType.ObjectView);
            ShowObject(originGo, TutorialCanvasType.ScriptView);
        }

        private void ShowScriptAndObjectWithGuideArrow(TutorialType type, GameObject originGo, UnityAction onTextShowed = null, int startOrder = 0, int endOrder = -1)
        {
            ShowScript(type, onTextShowed, startOrder, endOrder, TutorialCanvasType.ObjectView);
            ShowObjectWithGuideArrow(originGo,null, TutorialCanvasType.ScriptView);
        }
        
        private void ShowScriptAndObject(TutorialType type, GameObject[] originGOs, UnityAction onTextShowed = null, int startOrder = 0, int endOrder = -1)
        {
            ShowScript(type, onTextShowed, startOrder, endOrder, TutorialCanvasType.ObjectView);
            foreach (var originGO in originGOs)
            {
                ShowObject(originGO, TutorialCanvasType.ScriptView);
            }
        }
        
        private void ShowScript(TutorialType type, UnityAction onTextShowed = null, int startOrder = 0, int endOrder = -1, TutorialCanvasType addCanvasType = TutorialCanvasType.None)
        {
            _onTextShowed.RemoveAllListeners();
            
            if (onTextShowed != null)
                _onTextShowed.AddListener(onTextShowed);
            
            Open(TutorialCanvasType.ScriptView | addCanvasType);

            _endPageOrder = endOrder;
            UpdateScriptView(type, startOrder);
            
            View.Background.color = new Color(0, 0, 0, 0);
            View.Background.DOFade(backgroundFadeValue, 0.3f);
        }
        
        private void ShowObject(GameObject originGo, TutorialCanvasType addCanvasType = TutorialCanvasType.None)
        {
            if (originGo == null)
            {
                Close();
                return;
            }

            Open(TutorialCanvasType.ObjectView | addCanvasType);
            var copyObject = Object.Instantiate(originGo, View.transform);
            _copyObjects.Add(copyObject);
            copyObject.transform.position = originGo.transform.position;

            if (originGo.TryGetComponent<RectTransform>(out var rect))
            {
                if (copyObject.TryGetComponent<RectTransform>(out var copyRect))
                {
                    copyRect.sizeDelta = rect.sizeDelta;
                }
            }
            
            SetElementalSlotTextColor(copyObject.transform.Find("ElementalTypeText"));
         
            View.Background.color = new Color(0, 0, 0, 0);
            View.Background.DOFade(backgroundFadeValue, 0.3f);
        }

        private async UniTaskVoid ShowQuestPanelWithGuideArrow(int delay = 500)
        {
            ShowVoid();
            await UniTask.Delay(delay, true, PlayerLoopTiming.Update, Cts.Token);
            var original = Get<ControllerCanvasMainMenu>().GetTutorialObject(TutorialType.ClickQuestPanel);
            var arrow = original.transform.Find("Arrow");
            if(arrow)
                arrow.gameObject.SetActive(false);
            ShowObjectWithGuideArrow(Get<ControllerCanvasMainMenu>().GetTutorialObject(TutorialType.ClickQuestPanel));
            if(arrow)
                arrow.gameObject.SetActive(true);
        }

        private void ShowObjectWithGuideArrow(GameObject originGo, UnityAction callback = null,
            TutorialCanvasType addCanvasType = TutorialCanvasType.None)
        {
            if (originGo == null)
            {
                Close();
                return;
            }

            Open(TutorialCanvasType.ObjectView | addCanvasType);
            ClearCopyObject();
            
            var copyObject = Object.Instantiate(originGo, View.transform);
            _copyObjects.Add(copyObject);
            copyObject.transform.position = originGo.transform.position;
            copyObject.SetActive(true);

            if (originGo.TryGetComponent<RectTransform>(out var rect))
            {
                if (copyObject.TryGetComponent<RectTransform>(out var copyRect))
                {
                    copyRect.sizeDelta = rect.sizeDelta;
                }
            }

            var buttons = originGo.GetComponentsInChildren<Button>();
            if (buttons != null)
            {
                var copiedButtons = copyObject.GetComponentsInChildren<Button>();
                if (buttons.Length == copiedButtons.Length)
                {
                    for (var i = 0; i < buttons.Length; ++i)
                    {
                        copiedButtons[i].onClick.RemoveAllListeners();
                        copiedButtons[i].onClick.AddListener(buttons[i].onClick.Invoke);
                        copiedButtons[i].onClick.AddListener(() =>
                        {
                            ClearCopyObject();
                            callback?.Invoke();
                        });
                    }
                }
            }
            
            SetElementalSlotTextColor(copyObject.transform.Find("ElementalTypeText"));
            var guideArrowParent = copyObject.transform.Find("GuideArrowParent");
            if (guideArrowParent)
            {
                if (guideArrowParent.TryGetComponent<RectTransform>(out var rectTransform))
                    ShowGuideArrow(rectTransform);
            }

            View.Background.color = new Color(0, 0, 0, 0);
            View.Background.DOFade(backgroundFadeValue, 0.3f);
        }

        private void ClearCopyObject()
        {
            foreach (var copyObject in _copyObjects)
            {
                Object.DestroyImmediate(copyObject);
            }
            _copyObjects.Clear();
        }

        private void UpdateScriptView(TutorialType type, int order = 0)
        {
            var scriptLocalizedType = DataController.Instance.tutorial.GetLocalizedTextType(type, order);
            if (!scriptLocalizedType.HasValue)
            {             
                View.ScriptViewCanvasPopupWrap.Close();
                _onTextShowed?.Invoke();
                return;
            }

            _currTutorialType = type;
            _currPageOrder = order;
            
            var script = LocalizeManager.GetText(scriptLocalizedType.Value);
            View.Typewriter.ShowText(script);
            View.Typewriter.StartShowingText();
        }

        
        public void QuestGuide(QuestType questType)
        {
            switch (questType)
            {
                case QuestType.SummonElemental:
                    var elementalCanvas = Get<ControllerCanvasShop>();
                    OpenButtomMenu(elementalCanvas.MenuIndex);
                    elementalCanvas.ScrollToSummonElementalRect().Forget();
                    break;
                case QuestType.SummonRune:
                    var runeCanvas = Get<ControllerCanvasShop>();
                    OpenButtomMenu(runeCanvas.MenuIndex);
                    runeCanvas.ScrollToSummonRuneRect().Forget();
                    break;
                case QuestType.ClearGoldDungeon:
                case QuestType.ClearDiaDungeon:
                case QuestType.ClearEnhanceDungeon:      
                    var controllerCanvasDungeon = Get<ControllerCanvasDungeon>();
                    OpenButtomMenu(controllerCanvasDungeon.MenuIndex);
                    ShowGuideArrow(controllerCanvasDungeon.GetGuideArrowParent(questType));
                    break;
                case QuestType.UpgradeElementalSlot:
                case QuestType.UpgradeRuneSlot:
                case QuestType.UpgradeProjector:
                    var controllerCanvasUpgrade0 = Get<ControllerCanvasUpgrade>();
                    OpenButtomMenu(controllerCanvasUpgrade0.MenuIndex);
                    controllerCanvasUpgrade0.SelectMenu(1);
                    ShowGuideArrow(controllerCanvasUpgrade0.GetGuideArrowParent(questType));
                    break;
                case QuestType.UpgradeATK:
                case QuestType.UpgradeMaxHp:
                case QuestType.UpgradeIncreaseHealAmount:
                    var controllerCanvasUpgrade1 = Get<ControllerCanvasUpgrade>();
                    OpenButtomMenu(controllerCanvasUpgrade1.MenuIndex);
                    controllerCanvasUpgrade1.SelectMenu(0);
                    ShowGuideArrow(controllerCanvasUpgrade1.GetGuideArrowParent(questType));
                    break;
                case QuestType.EnhanceElemental:
                    var controllerCanvasElemental = Get<ControllerCanvasElemental>();
                    OpenButtomMenu(controllerCanvasElemental.MenuIndex);
                   // ShowGuideArrow(controllerCanvasElemental.GetGuideArrowParent(questType));
                    break;
                case QuestType.EquipRune:
                    OpenButtomMenu(Get<ControllerCanvasRune>().MenuIndex);
                    if(IsEnableQuestLevel(TutorialType.EquipRune))
                        StartTutorial(TutorialType.EquipRune);
                    break;
                case QuestType.EnhanceRune:
                    var controllerCanvasRune0 = Get<ControllerCanvasRune>();
                    OpenButtomMenu(controllerCanvasRune0.MenuIndex);
                    ShowGuideArrow(controllerCanvasRune0.GetGuideArrowParent(questType));
                    break;
                case QuestType.DisassembleRune:
                    var controllerCanvasRune1 = Get<ControllerCanvasRune>();
                    OpenButtomMenu(controllerCanvasRune1.MenuIndex);
                    ShowGuideArrow(controllerCanvasRune1.GetGuideArrowParent(questType));
                    break;
                case QuestType.KillEnemy:
                case QuestType.ClearNormalStage:
                default:
                    break;
            }
        }

        public void Open(TutorialCanvasType canvasType)
        {
            IsTutorialing = true;
            View.SetActive(true);

            View.ScriptViewCanvasPopupWrap.gameObject.SetActive((canvasType & TutorialCanvasType.ScriptView) == TutorialCanvasType.ScriptView);
            View.Background.gameObject.SetActive((canvasType & TutorialCanvasType.ObjectView) == TutorialCanvasType.ObjectView
                                                 || (canvasType & TutorialCanvasType.ScriptView) == TutorialCanvasType.ScriptView);

            if ((canvasType & TutorialCanvasType.ScriptView) == TutorialCanvasType.ScriptView)
                View.ScriptViewCanvasPopupWrap.Open();
        }

        public override void Close()
        {
            base.Close();
            IsTutorialing = false;
        }
        
        private void OpenButtomMenu(int menuIndex)
        {
            Get<ControllerCanvasBottomMenu>().bottomButtonGroup.Select(menuIndex);
        }

        private bool IsEnableQuestLevel(TutorialType type)
        {
            return DataController.Instance.quest.currQuestLevel == DataController.Instance.tutorial.GetQuestLevel(type);
        }

        private void ShowGuideArrow(RectTransform parent)
        {
            if (!parent) return;

            if (!_guideArrow)
            {
                _guideArrow = Object.Instantiate(View.GuideArrowPrefab);
            }

            _guideArrow.SetActive(true);
            _guideArrow.transform.SetParent(parent);
            _guideArrow.transform.localPosition = Vector3.zero;
            _guideArrow.transform.localScale = Vector3.one;
            _guideArrow.transform.localRotation = Quaternion.Euler(Vector3.zero);

            if (_guideArrow.TryGetComponent<RectTransform>(out var childRect))
            {
                childRect.sizeDelta = parent.sizeDelta;
            }
        }

        private void CloseGuideArrow()
        {
            if(_guideArrow) 
                _guideArrow.SetActive(false);
        }
    }
}