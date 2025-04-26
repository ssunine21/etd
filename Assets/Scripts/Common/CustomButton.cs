using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Manager;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.Common
{
    public class CustomButton : Button
    {
        public UnityAction onBindPointerUp;
        public UnityAction<int> onBindContinuousClick;
        
        private bool _isPlayingDownAnimation;
        private bool _isPointerDown;
        private float _pointerDownTime;
        
        private RectTransform _rectTransform;
        
        public float UpScale
        {
            get => upScale;
            set => upScale = value;
        }
        public float DownScale
        {
            get => downScale;
            set => downScale = value;
        }

        private Vector3 originScale;

        [SerializeField] private float upScale; 
        [SerializeField] private float downScale;
        [SerializeField] private bool isContinuousClick;
        
        private Vector3 _originSize;

        protected override void Awake()
        {
            base.Awake();
            _rectTransform = GetComponent<RectTransform>();
            _originSize = _rectTransform.localScale;
            originScale = _rectTransform.localScale;
        }

        private float _clickDelay;
        private int continuousClickCount;
        
        private void Update()
        {
            if(isContinuousClick)
            {
                if (_isPointerDown)
                {
                    if (_pointerDownTime > 0.8f)
                    {
                        if(_clickDelay <= 0)
                        {
                            onBindContinuousClick?.Invoke(continuousClickCount++);
                            _clickDelay = 0.05f;
                        }

                        _clickDelay -= Time.unscaledDeltaTime;
                    }

                    _pointerDownTime += Time.unscaledDeltaTime;
                }
                else
                {
                    _pointerDownTime = 0;
                    continuousClickCount = 0;
                }
            }
        }

        protected override void OnDisable()
        {
            _isPointerDown = false;
            base.OnDisable();
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            _isPointerDown = true;
            _isPlayingDownAnimation = true;
            
            _rectTransform
                .DOScale(_originSize * DownScale, 0.05f)
                .OnComplete(() => _isPlayingDownAnimation = false);
            
            AudioManager.Instance.PlaySound(AudioClipType.Tic);
            DataController.Instance.setting.onBindInitPowerSaveTime?.Invoke();
            base.OnPointerDown(eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            _isPointerDown = false;
            onBindPointerUp?.Invoke();
            OnPointerUpAnimation().Forget();
        }

        private async UniTaskVoid OnPointerUpAnimation()
        {
            await UniTask.WaitUntil(() => !_isPlayingDownAnimation);
            _rectTransform
                .DOScale(_originSize * UpScale, 0.05f)
                .OnComplete(() =>
                {
                    _rectTransform.DOScale(originScale, 0.05f);
                });
        }
    }
}
