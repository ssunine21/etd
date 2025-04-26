using System.Collections.Generic;
using System.Threading;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Manager
{
    public class StageGoodEffectManager : Singleton<StageGoodEffectManager>
    {
        private readonly Queue<ViewSlotGoodIcon> _viewSlotGoodIcons = new();
        
        public override void Init(CancellationTokenSource cts) { }
        
        public void ShowEffect(GoodType goodType, Vector2 start, ViewGood viewGood, float radiusScale = 0.3f, float scale = 1.5f, UnityAction callback = null)
        {
            callback += () =>
            {
                viewGood.ImageTr.DOKill();
                viewGood.ImageTr.localScale = Vector3.one;
                viewGood.ImageTr.DOPunchScale(Vector2.one * 0.1f, 0.3f)
                    .OnComplete(() => viewGood.ImageTr.localScale = Vector3.one);
            };
            ShowEffect(goodType, start, viewGood.ImageTr.position, radiusScale, callback, scale);
        }

        private void ShowEffect(GoodType goodType, Vector2 start, Vector2 end, float radiusScale = 0.3f, UnityAction callback = null, float scale = 1)
        {
            var viewSlot = _viewSlotGoodIcons.GetViewSlots(ViewSlotGoodIcon.PrefabName, transform, 1).Dequeue();

            var viewTransform = viewSlot.transform;
            viewTransform.position = start;
            viewTransform.localScale = Vector3.one * scale;

            viewSlot
                .SetGoodSprite(goodType)
                .SetActive(true);
            viewSlot.transform
                .DOMove(start + Random.insideUnitCircle * radiusScale, 1.2f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    viewSlot.transform
                        .DOMove(end, 1f)
                        .SetEase(Ease.InQuart)
                        .OnComplete(() =>
                        {
                            callback?.Invoke();
                            viewSlot.SetActive(false);
                        });
                });
        }
    }
}
