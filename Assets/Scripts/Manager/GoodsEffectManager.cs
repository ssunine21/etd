using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Manager
{
    public class GoodsEffectManager : Singleton<GoodsEffectManager>
    {
        private readonly Queue<ViewSlotGoodIcon> _viewSlotGoodIcons = new();
        public override void Init(CancellationTokenSource cts) { }
        
        public void ShowEffect(GoodType goodType, Vector2 start, ViewGood viewGood, int count, UnityAction callback = null, float radiusScale = 0.7f, float scale = 1.8f)
        {
            if (goodType is GoodType.SummonElemental or GoodType.SummonRune) return;
            
            callback += () =>
            {
                if(viewGood)
                {
                    viewGood.ImageTr.DOKill();
                    viewGood.ImageTr.localScale = Vector3.one;
                    viewGood.ImageTr.DOPunchScale(Vector2.one * Random.Range(0.1f, 0.3f), 0.3f)
                        .OnComplete(() => viewGood.ImageTr.localScale = Vector3.one)
                        .SetUpdate(true);
                }
            };
            var end = viewGood ? viewGood.ImageTr.position : Vector3.zero;
            ShowEffect(goodType, start, end, count, radiusScale, callback, scale);
        }

        public void ShowEffect(GoodType goodType, Vector2 start, Vector2 end, int count, float radiusScale = 0.7f, UnityAction callback = null, float scale = 1.8f)
        {
            var viewSlots = _viewSlotGoodIcons.GetViewSlots(ViewSlotGoodIcon.PrefabName, transform, count);
            while (viewSlots.Count > 0)
            {
                var viewSlot = viewSlots.Dequeue();
                var viewTransform = viewSlot.transform;
                viewTransform.position = start;
                viewTransform.localScale = Vector3.one * scale;

                viewSlot
                    .SetGoodSprite(goodType)
                    .SetActive(true);
                viewSlot.transform
                    .DOMove(start + Random.insideUnitCircle * radiusScale, 0.7f)
                    .SetEase(Ease.OutQuad)
                    .OnComplete(() =>
                    {
                        viewSlot.transform
                            .DOMove(end, 0.5f)
                            .SetEase(Ease.InQuart)
                            .OnComplete(() =>
                            {
                                callback?.Invoke();
                                viewSlot.SetActive(false);
                            });
                    })
                    .SetUpdate(true);
            }
        }
    }
}