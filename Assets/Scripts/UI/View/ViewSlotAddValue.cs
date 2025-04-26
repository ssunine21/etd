using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotAddValue : ViewSlot<ViewSlotAddValue>
    {
        public RectTransform RectTransform
        {
            get
            {
                if (!_rectTransform) TryGetComponent(out _rectTransform);
                return _rectTransform;
            }
        }
        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!_canvasGroup) TryGetComponent(out _canvasGroup);
                return _canvasGroup;
            }
        }

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text valueTMP;
        [SerializeField] private Image background;

        public ViewSlotAddValue SetIcon(Sprite sprite)
        {
            icon.enabled = sprite;
            icon.sprite = sprite;
            return this;
        }
        
        public ViewSlotAddValue SetTitleText(string text)
        {
            titleTMP.text = text;
            return this;
        }
        
        public ViewSlotAddValue SetValueText(string text)
        {
            valueTMP.text = text;
            return this;
        }

        public ViewSlotAddValue SetActiveBackground(bool flag)
        {
            background.gameObject.SetActive(flag);
            return this;
        }
    }
}
