using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Interface;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasSummon : ControllerCanvas
    {
        private ViewCanvasSummon View => ViewCanvas as ViewCanvasSummon; 
        private Sequence _sequence;
        private List<ViewSlotUI> _slots;

        private GoodType _currSummonGoodType;
        private GoodType _goodTypeSummon;
        private double _goodValueSummon;
        private bool _isPlayingCardAnimation;
        private int _currCount;

        private HashSet<int> _earnElementalSSIndexes = new();

        private const float ShakeDuration = 0.5f;
        private const float Strength = 10;
        private const int Vibrato = 30; 
        
        public ControllerCanvasSummon(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasSummon>())
        {
            View.AllOpenButton.onClick.AddListener(() =>
            {
                if (_slots == null || _isPlayingCardAnimation)
                    return;

                View.AllOpenButton.enabled = false;
                foreach (var slot in _slots.Where(slot => slot.isActiveAndEnabled))
                {
                    slot.Button.onClick.Invoke();
                }
            });
            
            View.ExitButton.onClick.AddListener(Close);
            View.SummonButton.onClick.AddListener(() =>
            {
                if(IsAllOpen())
                {
                    if(_goodTypeSummon == GoodType.SummonRune && DataController.Instance.rune.IsMaxRune)
                    {
                        Get<ControllerCanvasToastMessage>().ShowTransientToastMessage(LocalizedTextType.IsRunePull);
                    }
                    else
                    { 
                        if (DataController.Instance.good.TryConsume(_goodTypeSummon, _goodValueSummon))
                        {
                            Summon(_currSummonGoodType, _currCount).Forget();
                        }
                    }
                }
            });
            
            View.EffectSkipCheckBox.Toggle.isOn = DataController.Instance.setting.isSummonEffectSkip;
            View.EffectSkipCheckBox.Toggle.onValueChanged.AddListener(isOn =>
            {
                DataController.Instance.setting.isSummonEffectSkip = isOn;
            });

            Init();
        }
        
        public async UniTaskVoid Summon(GoodType type, int count, int param0 = 0,  bool isResummon = true)
        {
            Get<ControllerCanvasToastMessage>().ShowLoading();
            switch (type)
            {
                case GoodType.SummonElemental or GoodType.SummonElementalS or GoodType.SummonElementalSS:
                {
                    List<KeyValuePair<ElementalType, GradeType>> results;
                    if (type == GoodType.SummonElemental)
                    {
                        results = param0 == 0
                            ? await DataController.Instance.elemental.GetRandomElementalType(count)
                            : DataController.Instance.elemental.GetElementalType(param0, count);
                    }
                    else
                    {
                        results = type switch
                        {
                            GoodType.SummonElementalS => DataController.Instance.elemental.GetElementalType(GradeType.S,
                                count),
                            GoodType.SummonElementalSS => DataController.Instance.elemental.GetElementalType(
                                GradeType.SS, count),
                        };
                    }   
                    
                    foreach (var result in results)
                    {
                        DataController.Instance.elemental.Earn(result);
                        if (result.Value >= GradeType.SS)
                        {
                            _earnElementalSSIndexes.Add(DataController.Instance.elemental.GetBData(result.Key, result.Value).index);
                        }
                    }

                    var isDuplicate = param0 != 0;
                    SummonResult(results, isDuplicate).Forget();
                    DataController.Instance.player.OnBindChangedElemental?.Invoke(0);
                    DataController.Instance.elemental.AddSummonCount(count);
                    DataController.Instance.mission.Count(MissionType.SummonElemental, count);
                    DataController.Instance.quest.Count(QuestType.SummonElemental, count);
                    break;
                }
                case GoodType.SummonRune or GoodType.SummonRuneS or GoodType.SummonRuneSS:
                {
                    List<Rune> results;
                    if (type == GoodType.SummonRune)
                    {
                        results = await DataController.Instance.rune.GetRandomRuneType(count);
                    }
                    else
                    {
                        results = type switch
                        {
                            GoodType.SummonRuneS => DataController.Instance.rune.GetRuneType(GradeType.S, count),
                            GoodType.SummonRuneSS => DataController.Instance.rune.GetRuneType(GradeType.SS, count),
                        };
                    }
                    
                    foreach (var result in results)
                    {
                        if (result.type == RuneType.RandomTag)
                            foreach (var attribute in result.equippingAttr.Where(attribute => attribute.type == AttributeType.RandomTag))
                                result.tags.Add((TagType)(int)attribute.value);
                        
                        DataController.Instance.rune.Earn(result);
                    }
                
                    var keyValuePairs = results.Select((value, index) => new KeyValuePair<RuneType, GradeType>(value.type, value.grade)).ToList();
                    SummonResult(keyValuePairs).Forget();
                    DataController.Instance.player.OnBindChangedRune?.Invoke(0);
                    DataController.Instance.rune.AddSummonCount(count);
                    DataController.Instance.mission.Count(MissionType.SummonRune, count);
                    DataController.Instance.quest.Count(QuestType.SummonRune, count);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _currSummonGoodType = type;
            _currCount = count;

            View.ResummonPanel.SetActive(isResummon);
            
            UpdateView(type, Mathf.Clamp(count, 1, 10));
            
            Get<ControllerCanvasToastMessage>().CloseLoading();
            DataController.Instance.SaveBackendData();
        }

        private async UniTaskVoid SummonResult<T>(List<KeyValuePair<T, GradeType>> results, bool isDuplicateAllowed = false) where T : Enum
        {
            Open();
            View.ChangeAllOpenButtonToExitButton(false);

            var index = 0;

            if (isDuplicateAllowed)
            {
                var dic = new Dictionary<KeyValuePair<T, GradeType>, int>();
                foreach (var result in results)
                {
                    if (dic.ContainsKey(result)) dic[result] += 1;
                    else dic.TryAdd(result, 1);
                }

                foreach (var i in dic)
                {
                    var slot = SetSlot(i.Key, index);
                    slot.CanvasGroup.alpha = 0;
                    slot.EnabledDuplicationCount(i.Value > 1).SetDuplicationCount(i.Value);
                    index++;
                }
            }
            else
            {
                foreach (var result in results)
                {
                    var slot = SetSlot(result, index);
                    slot.CanvasGroup.alpha = 0;
                    slot.EnabledDuplicationCount(false);
                    index++;
                }
            }

            var count = index;

            for (var i = index; i < _slots.Count; ++i)
            {
                _slots[i].SetActive(false);
            }
            
            await UniTask.Yield(Cts.Token);

            var duration = 0.4f;
            _isPlayingCardAnimation = true;

            for (var i = 0; i < count; ++i)
            {          
                var slot = _slots[i];
                var sequnce = DOTween.Sequence();
                if (slot.GradeType >= GradeType.A)
                {
                    sequnce
                        .Append(slot.transform.DOShakeScale(0.5f, Vector3.one * 0.1f, 40))
                        .Join(slot.transform.DOShakeRotation(0.5f, 15, 40));
                }
                
                if (View.EffectSkipCheckBox.IsChecked)
                {
                    slot.CanvasGroup.alpha = 1;
                }
                else
                {        
                    await UniTask.Delay(50, true, PlayerLoopTiming.Update, Cts.Token);
                    var originPos = slot.transform.localPosition;
                    slot.transform.localPosition = new Vector2(originPos.x - 100, originPos.y);
                    sequnce
                        .Join(slot.transform.DOLocalMoveX(originPos.x, duration))
                        .Join(slot.CanvasGroup.DOFade(1, duration))
                        .SetUpdate(true);
                }
                sequnce.Play();
            }
            _isPlayingCardAnimation = false;
        }

        public override void Open()
        {
            if (ActiveSelf)
                return;
            
            base.Open();
        }

        public override void Close()
        {
            foreach (var ssIndex in _earnElementalSSIndexes)
                DataController.Instance.shop.TryUnlockNewPackage(UnlockType.GetElemental, ssIndex);
            
            _earnElementalSSIndexes.Clear();
            base.Close();
        }

        private void Init()
        {
            View.CloseButton.onClick.AddListener(Close);
        }

        private void UpdateView(GoodType type, int count)
        {
            _goodTypeSummon =
                type == GoodType.SummonElemental
                    ? DataController.Instance.good.GetValue(GoodType.SummonElementalTicket) >= count
                        ? GoodType.SummonElementalTicket
                        : GoodType.Dia
                    : DataController.Instance.good.GetValue(GoodType.SummonRuneTicket) >= count
                        ? GoodType.SummonRuneTicket
                        : GoodType.Dia;
            
            _goodValueSummon = _goodTypeSummon == GoodType.Dia ? 100 * count : 1 * count;
            View.SummonViewGood
                .SetInit(_goodTypeSummon)
                .SetValue(_goodValueSummon);

            var ticket = type == GoodType.SummonElemental ? GoodType.SummonElementalTicket : GoodType.SummonRuneTicket;
            View.TopTicketViewGood
                .SetInit(ticket)
                .SetValue(DataController.Instance.good.GetValue(ticket));
        }

        private ViewSlotUI SetSlot<T>(KeyValuePair<T, GradeType> result, int index) where T : Enum
        {
            _slots ??= new List<ViewSlotUI>();
            if (_slots.Count <= index)
            {
                _slots.Add(Object.Instantiate(ResourcesManager.Instance.viewSlotUIPrefab, View.SlotParent));
            }

            var virtualGradeType = (GradeType)Mathf.Clamp((int)result.Value - Random.Range(0, 2), (int)GradeType.A, (int)GradeType.SSS);
            var slot = _slots[index];
            slot.Button.onClick.RemoveAllListeners();
            slot.Button.onClick.AddListener(() => TouchSlot(slot, virtualGradeType, result.Value));
            
            if(typeof(T) == typeof(ElementalType))
            {
                if (Enum.TryParse(typeof(ElementalType), result.Key.ToString(), out var key))
                {
                    var elementalType = (ElementalType)key;
                    slot
                        .SetUnitSprite(DataController.Instance.elemental.GetImage(elementalType, result.Value))
                        .SetCardBackSprite(DataController.Instance.elemental.GetBackCardImage(elementalType));
                }
            }
            else
            {
                if(Enum.TryParse(typeof(RuneType), result.Key.ToString(), out var key))
                {
                    var runeType = (RuneType)key;
                    slot
                        .SetUnitSprite(DataController.Instance.rune.GetImage(runeType, result.Value))
                        .SetCardBackSprite(DataController.Instance.rune.GetBackCardImage(runeType));
                }
            }

            slot
                .SetActiveEffectFront(false)
                .SetActiveEffectBackground(result.Value >= GradeType.A)
                .SetEffectBackgroundColor(ResourcesManager.Instance.GetGradeColor(
                    View.EffectSkipCheckBox.IsChecked ? result.Value : virtualGradeType))
                .SetActiveGradeBorder(false)
                .SetActiveBackCard(true)
                .SetGradeText(result.Value)
                .SetActive(true);

            return slot;
        }

        private void TouchSlot(ViewSlotUI slot, GradeType virtualGradeType, GradeType realGradeType)
        {
            var isUpperA = realGradeType >= GradeType.A;
            var initColor = new Color(1, 1, 1, 0);
            slot.SetActiveEffectFront(isUpperA);

            if (isUpperA && !View.EffectSkipCheckBox.IsChecked)
            {
                slot.SetEffectFrontColor(initColor);
                GradeColorUp(slot, virtualGradeType, realGradeType);
            }
            else
            {
                slot.SetEffectBackgroundColor(ResourcesManager.Instance.GetGradeColor(realGradeType));
                OpenCard(slot);
            }
        }

        private bool IsAllOpen()
        {
            return !_slots.Any(slot => slot.IsEnableBackCard);
        }

        private void GradeColorUp(ViewSlotUI slot, GradeType virtualGradeType, GradeType realGradeType)
        {
            var color = ResourcesManager.Instance.GetGradeColor(virtualGradeType);

            slot.EffectBackground.DOColor(color, ShakeDuration);
            slot.transform.DOShakePosition(ShakeDuration, Strength, Vibrato);
            slot.EffectFront.DOColor(color, ShakeDuration).OnComplete(() =>
            {
                if (virtualGradeType >= realGradeType)
                {
                    if (Random.Range(0f, 1f) <= 0.25f)
                    {
                        slot.transform.DOShakePosition(ShakeDuration, Strength, Vibrato)
                            .OnComplete(() => OpenCard(slot))
                            .SetUpdate(true);
                    }
                    else
                    {
                        OpenCard(slot);
                    }
                }
                else
                {
                    slot.transform
                        .DOShakePosition(ShakeDuration + 1, Strength, Vibrato)
                        .OnComplete(() => GradeColorUp(slot, virtualGradeType + 1, realGradeType))
                        .SetUpdate(true);
                }
            });
        }

        private void OpenCard(ViewSlotUI slot)
        {
            slot
                .SetActiveGradeBorder(true)
                .SetActiveBackCard(false)
                .SetActiveEffectFront(false);

            if (IsAllOpen())
            {
                View.ChangeAllOpenButtonToExitButton(true);
            }
        }
    }
}