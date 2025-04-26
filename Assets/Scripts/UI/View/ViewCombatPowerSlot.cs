using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewCombatPowerSlot : MonoBehaviour
    {
        public Transform CombatPowerTr => combatPowerTr;
        public Transform CombatTextTr => combatTextTr;

        public Color PlusColor => plusColor;
        public Color MinusColor => minusColor;
        
        [SerializeField] private Color plusColor;
        [SerializeField] private Color minusColor;

        [SerializeField] private Transform combatPowerTr;
        [SerializeField] private Transform combatTextTr;
        
        [SerializeField] private TMP_Text currPowerTMP;
        [SerializeField] private TMP_Text conversionPowerTMP;
        [SerializeField] private RectTransform iconRectTr;

        private CanvasGroup _combatPowerTrCanvasGroup;
        private CanvasGroup _combatPowerTextTrCanvasGroup;
        private Image _iconImage;
        private Sequence _sequence;

        public ViewCombatPowerSlot ShowAnimation(UnityAction callback)
        {
            if (!_combatPowerTrCanvasGroup) combatPowerTr.TryGetComponent(out _combatPowerTrCanvasGroup);
            if (!_combatPowerTextTrCanvasGroup) combatTextTr.TryGetComponent(out _combatPowerTextTrCanvasGroup);
            if(!_iconImage) iconRectTr.TryGetComponent(out _iconImage);

            if (_sequence == null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false)
                    .OnPlay(() =>
                    {
                        combatPowerTr.localScale = Vector3.zero;
                        combatTextTr.localScale = Vector3.zero;
                        _combatPowerTrCanvasGroup.alpha = 0;
                        _combatPowerTextTrCanvasGroup.alpha = 0;
                        iconRectTr.anchoredPosition = new Vector2(-60, 60);
                        _iconImage.color = new Color(1, 1, 1, 0);
                    })
                    .Append(combatPowerTr.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuart))
                    .Join(_combatPowerTrCanvasGroup.DOFade(1, 0.3f))
                    .Insert(0.2f, combatTextTr.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutQuart))
                    .Join(_combatPowerTextTrCanvasGroup.DOFade(1, 0.3f))
                    .Join(iconRectTr.DOLocalMove(Vector3.zero, 0.3f).SetEase(Ease.InOutQuart))
                    .Join(_iconImage.DOFade(1, 0.3f))
                    .AppendInterval(0.3f)
                    .Append(combatPowerTr.DOScale(Vector3.zero, 0.3f))
                    .Join(_combatPowerTrCanvasGroup.DOFade(0, 0.3f))
                    .OnComplete(() => callback?.Invoke())
                    .SetUpdate(true);
            }
            else
            {
                if (_sequence.IsPlaying())
                    _sequence.Pause();
                _sequence.Restart();
            }
            
            return this;
        }
        public ViewCombatPowerSlot SetCurrPower(string text)
        {
            currPowerTMP.text = text;
            return this;
        }

        public ViewCombatPowerSlot SetConversionPowerTextAndColor(string text, Color color)
        {
            SetConversionPower(text);
            conversionPowerTMP.color = color;
            return this;
        }

        private ViewCombatPowerSlot SetConversionPower(string text)
        {
            conversionPowerTMP.text = text;
            return this;
        }
        
        public ViewCombatPowerSlot SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}
