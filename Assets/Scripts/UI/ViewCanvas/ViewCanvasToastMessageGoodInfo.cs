using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasToastMessage
    {
        public ViewCanvasPopup ViewPopupGoodInfo => viewPopupGoodInfo;
        public ViewGood ViewGoodInfo => viewGoodInfo;
        
        [Space] [Space] [Header("GoodInfo")] 
        [SerializeField] private ViewCanvasPopup viewPopupGoodInfo;
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text descTMP;
        [SerializeField] private TMP_Text sourceTMP;
        [SerializeField] private ViewGood viewGoodInfo;

        public ViewCanvasToastMessage SetGoodInfoTitle(string text)
        {
            titleTMP.text = text;
            return this;
        }
        
        public ViewCanvasToastMessage SetGoodInfoDescription(string text)
        {
            descTMP.text = text;
            return this;
        }
        
        public ViewCanvasToastMessage SetGoodInfoSource(string text)
        {
            sourceTMP.text = text;
            return this;
        }
    }
}