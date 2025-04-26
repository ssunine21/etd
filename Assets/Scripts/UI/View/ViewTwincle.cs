using System;
using System.Collections;
using System.Collections.Generic;
using ETD.Scripts.Common;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ETD.Scripts.UI.View
{
    public class ViewTwincle : MonoBehaviour
    {
        public bool IsPlaying => _isPlaying;
        private bool _isPlaying;
        
        [SerializeField] private bool onAwake = true;
        [SerializeField] private SpriteAnimation spriteAnimation;
        [SerializeField] private float delayTime;
        [SerializeField] private int count;
        [SerializeField] private RectTransform parentRectTr;

        private WaitForSecondsRealtime _wfs;
        private WaitForSecondsRealtime _randomWfs;
        private Queue<SpriteAnimation> _spriteAnimations;

        private void Awake()
        {
            _wfs ??= new WaitForSecondsRealtime(delayTime);
            _randomWfs ??= new WaitForSecondsRealtime(0f);
            _spriteAnimations ??= new Queue<SpriteAnimation>();
        }

        private void OnEnable()
        {
            if (onAwake)
                Play();
        }

        public void Play()
        {
            StartCoroutine(CoAnimation());
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        private IEnumerator CoAnimation()
        {
            if (_spriteAnimations == null) yield break;
            
            foreach (var animation1 in _spriteAnimations)
            {
                animation1.SetEnableImage(false);
            }

            _isPlaying = true;
            
            while (IsPlaying)
            {
                for (var i = 0; i < count; ++i)
                {
                    _randomWfs.waitTime = Random.Range(0.1f, 0.4f);

                    var sa = GetAnimation();
                    if (sa.TryGetComponent<RectTransform>(out var childRect))
                    {

                        var parentWidth = parentRectTr.rect.width - 30;
                        var parentHeight = parentRectTr.rect.height - 30;
                        
                        var childWidth = childRect.rect.width;
                        var childHeight = childRect.rect.height;

                        var randomX = Random.Range(-30f, parentWidth) - parentWidth / 2 + childWidth / 2;
                        var randomY = Random.Range(0f, parentHeight) - parentHeight / 2 + childHeight / 2;
                        
                        childRect.localPosition = new Vector2(randomX, randomY);
                    }
                    sa.StartAnimation(false, () =>
                    {
                        _spriteAnimations.Enqueue(sa);
                    });
                    yield return _randomWfs;
                }
                yield return _wfs;
            }
        }

        private SpriteAnimation GetAnimation()
        {
            if(_spriteAnimations.Count == 0)
            {
                var temp = Instantiate(spriteAnimation, transform);
                temp.gameObject.SetActive(true);
                _spriteAnimations.Enqueue(temp);
            }

            return _spriteAnimations.Dequeue();
        }
    }
}