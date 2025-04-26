using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasNewPackage : ViewCanvas
    {
        public ViewSlotProduct ViewSlotProduct => viewSlotProduct;
        public Button PurchaseButton => purchaseButton;
        public Button NextButton => nextButton;
        public Button PrevButton => prevButton;
        public ViewPositionMark MarkPrefabs => markPrefabs;
        
        [SerializeField] private ViewSlotProduct viewSlotProduct;
       
        [SerializeField] private Button nextButton;
        [SerializeField] private Image nextArrow;
        [SerializeField] private Button prevButton;
        [SerializeField] private Image prevArrow;
        
        [SerializeField] private TMP_Text remainingTimeTMP;
        [SerializeField] private Button purchaseButton;
        [SerializeField] private ViewPositionMark markPrefabs;

        public ViewCanvasNewPackage SetRemainingTime(string text)
        {
            remainingTimeTMP.text = text;
            return this;
        }

        public ViewCanvasNewPackage SetEnableNextButton(bool flag)
        {
            nextButton.enabled = flag;
            nextArrow.color = new Color(1, 1, 1, flag ? 0.7f : 0.4f);
            nextArrow.enabled = flag;
            return this;
        }

        public ViewCanvasNewPackage SetEnablePrevButton(bool flag)
        {
            prevButton.enabled = flag;
            prevArrow.color = new Color(1, 1, 1, flag ? 0.7f : 0.4f);
            prevArrow.enabled = flag;
            return this;
        }
    }
}