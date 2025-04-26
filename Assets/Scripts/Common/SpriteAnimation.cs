using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.Common
{
    public class SpriteAnimation : MonoBehaviour
    {
        [SerializeField] private Sprite[] sprites;
        [SerializeField] private Image image;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private float animationTime;
        [SerializeField] private float delay;
        [SerializeField] private int repeatCount = 1;
        
        private WaitForSecondsRealtime _wfs;
        private WaitForSecondsRealtime _delayWaitForSeconds;
        private bool _isimageNotNull;
        private bool _isspriteRendererNotNull;
        [SerializeField] private bool _isLoop;

        private Coroutine _coroutine;

        private void Start()
        {
            _isspriteRendererNotNull = spriteRenderer != null;
            _isimageNotNull = image != null;
            _wfs = new WaitForSecondsRealtime(animationTime);
            _delayWaitForSeconds = new WaitForSecondsRealtime(delay);
        }

        private void OnEnable()
        {
            StartAnimation();
        }

        public void StartAnimation(bool isLoop = true, UnityAction onShowedAnimation = null)
        {
            if (!isActiveAndEnabled) return;
            _isLoop = isLoop;
            if (_coroutine != null)
            {
                StopCoroutine(_coroutine);
                _coroutine = null;
            }

            _coroutine = StartCoroutine(Play(onShowedAnimation));
        }
         
        private IEnumerator Play(UnityAction onShowedAnimation)
        {
            var repeat = repeatCount;
            
            if (sprites.Length > 0)
                SetSprite(sprites[0]);
            
            while (true)
            {
                SetEnableImage(true);
                foreach (var sprite in sprites)
                {
                    SetSprite(sprite);
                    yield return _wfs;
                }

                repeat -= 1;
                if (repeat <= 0)
                {
                    SetEnableImage(false);

                    if (!_isLoop)
                    {
                        onShowedAnimation?.Invoke();
                        break;
                    }
                    
                    repeat = repeatCount;
                    yield return _delayWaitForSeconds;
                }
            }
        }

        public void SetEnableImage(bool flag)
        {
            if (_isimageNotNull)
                image.enabled = flag;
                    
            if(_isspriteRendererNotNull) 
                spriteRenderer.enabled = flag;
        }

        private void SetSprite(Sprite sprite)
        {
            if (_isimageNotNull)
                image.sprite = sprite;
                    
            if(_isspriteRendererNotNull) 
                spriteRenderer.sprite = sprite;
        }
    }
}
