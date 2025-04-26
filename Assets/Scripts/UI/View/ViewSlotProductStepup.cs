using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotProductStepup : MonoBehaviour
    {
        public ProductType ProductType => productType;
        public ViewGood ViewGoodPrefab => viewGoodPrefab;
        public Transform ViewGoodParent => viewGoodParent;
        public ActiveButton PurchaseButton => purchaseButton;
        public SlideButton StepSlideButton => stepSlideButton;
        
        [SerializeField] private ProductType productType;
        [SerializeField] private ViewGood viewGoodPrefab;
        [SerializeField] private Transform viewGoodParent;
        [SerializeField] private ActiveButton purchaseButton;
        [SerializeField] private SlideButton stepSlideButton;
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text purchaseCountTMP;
        
        public ViewSlotProductStepup SetPriceText(string text)
        {
            purchaseButton.SetButtonText(text);
            return this;
        }
        
        public ViewSlotProductStepup SetPurchaseCount(string text)
        {
            purchaseCountTMP.text = text;
            return this;
        }

        public ViewSlotProductStepup SetTitleText(string text)
        {
            titleTMP.text = text;
            return this;
        }
    }
}