using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ETD.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.Manager
{
    public class LoadingManager : Singleton<LoadingManager>
    {
        public bool IsInit { get; set; }
        
        [SerializeField] private Sprite[] titleSprites;
        [SerializeField] private Sprite[] subTitleSprites;
        [SerializeField] private Image loadingImage;
        [SerializeField] private Image titleImage;
        [SerializeField] private Image subTitleImage;
        [SerializeField] private Image background;
        [SerializeField] private TMP_Text versionTMP;
        [SerializeField] private TMP_Text loadingCountTMP;

        private int _maxCount;
        private int _currCount;
        
        public override void Init(CancellationTokenSource cts)
        {
            Version();

            var languageNumber = GameManager.systemLanguageNumber;
            titleImage.sprite = titleSprites[languageNumber];
            subTitleImage.sprite = subTitleSprites[languageNumber];

            StartAnimation().Forget();
        }
        
        public void SetMaxCount(int value)
        {
            _maxCount = value;
            loadingImage.fillAmount = 0;
        }

        public async UniTask Loading()
        {
            try
            {
                if (loadingImage != null)
                {
                    _currCount += 1;
                    loadingImage.fillAmount = (float)_currCount / _maxCount;
                }

                if (loadingCountTMP) loadingCountTMP.text = _currCount.ToString();
            }
            catch (Exception e)
            {
                FirebaseManager.LogError(e);
            }

            await UniTask.Yield();
        }

        public void SetActive(bool flag)
        {
            gameObject.SetActive(flag);
        }

        private async UniTaskVoid StartAnimation()
        {
            var alphaZero = Color.white;
            alphaZero.a = 0;

            titleImage.color = alphaZero;
            subTitleImage.color = alphaZero;

            var imageSpeed = 1f;

            await UniTask.Delay(300);
            
            var sequence = DOTween.Sequence().SetAutoKill(false)
                .Append(titleImage.rectTransform.DOAnchorPosY(280f, imageSpeed).SetEase(Ease.OutBack))
                .Join(titleImage.DOFade(1f, imageSpeed))     
                .Insert(0.2f, subTitleImage.rectTransform.DOAnchorPosY(-20f, imageSpeed).SetEase(Ease.OutBack))
                .Join(subTitleImage.DOFade(1f, imageSpeed))
                .AppendInterval(1f)
                .Append(background.DOFade(0f, 1.2f).OnComplete(() => IsInit = true))
                .SetUpdate(true);
        }
        
        private void Version()
        {
            string status = string.Empty;
#if IS_TEST
            status = "TEST";
#elif IS_LIVE
            status = "LIVE";
#endif
            versionTMP.text = $"{status} {Application.version}";
        }
    }
}