using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using UnityEngine;
using DG.Tweening;

namespace ETD.Scripts.Manager
{
    public class TextManager : Singleton<TextManager>
    {
        private CancellationTokenSource _cts;
        private Queue<ViewDamageText> _damageTextQueue;

        private readonly Color _defaultDamageColor = Color.white;
        private readonly Color _criticalDamageColor = Color.red;

        public override void Init(CancellationTokenSource cts)
        {
            _cts = cts;
            _damageTextQueue = new Queue<ViewDamageText>();
            CreateTextView();
        }

        public void ShowDamage(double damage, Vector2 position, bool isCritical)
        {
            var viewText = GetTextView();
            if (!viewText) return;
            
            viewText.Text = damage.ToDamage();
            viewText.TMPText.color = isCritical ? _criticalDamageColor : _defaultDamageColor;

            position.x += UnityEngine.Random.Range(-0.15f, 0.15f);
            position.y += UnityEngine.Random.Range(-0.15f, 0.15f);

            viewText
                .SetActive(true)
                .SetPosition(position);

            viewText.transform.DOMoveY(position.y + 0.2f, 0.3f).OnComplete(() =>
            {
                viewText.TMPText.DOFade(0, 0.3f).OnComplete(() =>
                {
                    viewText.SetActive(false);
                    _damageTextQueue.Enqueue(viewText);
                });
            });
        }

        private ViewDamageText GetTextView()
        {
            return _damageTextQueue.Count > 0 ? _damageTextQueue.Dequeue() : null;
        }

        private void CreateTextView()
        {
            for (var i = 0; i < 200; ++i)
            {
                var text = Instantiate(ResourcesManager.Instance.viewDamageTextPrefab, transform);
                text.SetActive(false);
                
                _damageTextQueue.Enqueue(text);
            }
        }
    }
}