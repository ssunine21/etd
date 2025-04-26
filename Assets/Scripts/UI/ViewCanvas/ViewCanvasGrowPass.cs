using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasGrowPass : ViewCanvas
    {
        public ActiveButton[] PassListButtons => passListButtons;
        public ActiveButton LevelActiveButton => levelActiveButton;
        public Transform LevelParent => levelParent;
        public ViewSlotPass ViewSlotPassPrefab => viewSlotPassPrefab;
        public Transform ViewSlotPassParent => viewSlotPassParent;
        public Button CloseButton => closeButton;
        public ScrollRect GoalScrollRect => goalScrollRect;
        public ActiveButton PurchasePassButton => purchasePassButton;
        public ActiveButton ReceiveAllButton => receiveAllButton;
        public GameObject PrevLastReward => prevLastReward;
        public ViewGood PrevLastRewardViewGood => prevLastRewardViewGood;
        
        [Space] [Space] [Header("Level")]
        [SerializeField] private ActiveButton[] passListButtons;
        [SerializeField] private ActiveButton levelActiveButton;
        [SerializeField] private Transform levelParent;
        [SerializeField] private ActiveButton receiveAllButton;

        [Space] [Space] [Header("Goal")] 
        [SerializeField] private TMP_Text currGoalCountTMP;
        [SerializeField] private ViewSlotPass viewSlotPassPrefab;
        [SerializeField] private ScrollRect goalScrollRect;
        [SerializeField] private Transform viewSlotPassParent;
        [SerializeField] private GameObject prevLastReward;
        [SerializeField] private ViewGood prevLastRewardViewGood;
        [SerializeField] private TMP_Text prevLastRewardTMP;

        [Space] [Space] [Header("Buttom")]
        [SerializeField] private TMP_Text descTMP;
        [SerializeField] private Button closeButton;
        [SerializeField] private ActiveButton purchasePassButton;
        [SerializeField] private TMP_Text accountPurchase;
        [SerializeField] private TMP_Text multiplePriceTMP;

        public ViewCanvasGrowPass SetPrevLastRewardText(string text)
        {
            prevLastRewardTMP.text = text;
            return this;
        }

        public ViewCanvasGrowPass SetAccountPurchaseCount(string text)
        {
            accountPurchase.text = text;
            return this;
        }

        public ViewCanvasGrowPass SetDescText(string text)
        {
            descTMP.text = text;
            return this;
        }

        public ViewCanvasGrowPass SetCurrGoalCount(string text)
        {
            currGoalCountTMP.text = text;
            return this;
        }

        public ViewCanvasGrowPass SetPriceText(string text)
        {
            purchasePassButton.SetButtonText(text);
            return this;
        }
        
        public ViewCanvasGrowPass SetMultiplePrice(string text)
        {
            multiplePriceTMP.text = text;
            return this;
        }
    }
}