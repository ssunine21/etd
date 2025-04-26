using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotDPS : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private ViewProjectorUI viewProjectorUI;
        [SerializeField] private Image fillAmountImage;
        [SerializeField] private TMP_Text valueTMP;

        private RectTransform _rectTransform;

        public ViewSlotDPS SetProjectorColor(Color color)
        {
            viewProjectorUI.ChangeColor(color);
            return this;
        }

        public ViewSlotDPS SetProjector(bool flag)
        {
            viewProjectorUI.gameObject.SetActive(flag);
            icon.enabled = !flag;
            return this;
        }

        public ViewSlotDPS SetIcon(Sprite sprite)
        {
            if (sprite)
                icon.sprite = sprite;
            return this;
        }

        public ViewSlotDPS SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotDPS SetFillAmount(float value)
        {
            value = Mathf.Clamp01(value);
            fillAmountImage.fillAmount = value;
            return this;
        }

        public ViewSlotDPS SetValueText(string text)
        {
            valueTMP.text = text;
            return this;
        }

        public ViewSlotDPS SetHeight(float height)
        {
            if (!_rectTransform)
            {
                _rectTransform = GetComponent<RectTransform>();
            }
            
            var size = _rectTransform.sizeDelta;
            size.y = height;
            _rectTransform.sizeDelta = size;
            return this;
        }
    }
}
