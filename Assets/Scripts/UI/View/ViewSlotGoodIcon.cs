using ETD.Scripts.Common;
using ETD.Scripts.UI.Controller;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Button))]
    public class ViewSlotGoodIcon : ViewSlot<ViewSlotGoodIcon>
    {
        public const string PrefabName = nameof(ViewSlotGoodIcon);
        public Transform ImageTr => GoodImage.transform;

        public Button Button
        {
            get
            {
                if (!_button) TryGetComponent(out _button);
                return _button;
            }
        }

        private Image GoodImage
        {
            get
            {
                if (!_goodImage) TryGetComponent(out _goodImage);
                return _goodImage;
            }
        } 

        private Image _goodImage;
        private Button _button;
        private GoodType _goodType = GoodType.None;
        private int _param0;
        private bool isOverrideButtonAction;

        public ViewSlotGoodIcon SetGoodSprite(GoodType goodType, int param0 = 0)
        {
            GoodImage.sprite = DataController.Instance.good.GetImage(goodType, param0);
            _goodType = goodType;
            _param0 = param0;
            return this;
        }

        public ViewSlotGoodIcon SetButton()
        {
            if (!isOverrideButtonAction)
            {
                Button.onClick.RemoveListener(ShowGoodInfo);
                Button.onClick.AddListener(ShowGoodInfo);
            }
            return this;
        }

        public ViewSlotGoodIcon OverrideListener(UnityAction action)
        {
            isOverrideButtonAction = true;
            Button.onClick.RemoveListener(ShowGoodInfo);
            Button.onClick.AddListener(action);
            return this;
        }

        private void ShowGoodInfo()
        {
            if (_goodType == GoodType.None) return;
            ControllerCanvas.Get<ControllerCanvasToastMessage>().ShowGoodInfo(_goodType, _param0);
        }
    }
}