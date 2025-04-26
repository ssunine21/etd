using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasShop : ViewCanvas
    {
        public ViewGood[] ViewGoods => viewGoods;
        public ViewSlotProductStepup[] ViewSlotProductStepups => viewSlotProductStepups;
        public ViewSlotProduct[] PackageViews => packageViews;
        public ViewSlotProduct[] SummonElementalViews => summonElementalViews;
        public ViewSlotProduct[] SummonRuneViews => summonRuneViews;
        public Button ElementalProbabilityButton => elementalProbabilityButton;
        public Button RuneProbabilityButton => runeProbabilityButton;
        public Button ProtectedDescButton => protectedDescButton;
        public ScrollRect ScrollRect => scrollRect;
        public ViewSlotVIP ViewSlotVip => viewSlotVip;
        public SlideButton SlideButton => slideButton;
        public GameObject[] Contents => contents;

        [SerializeField] private ViewGood[] viewGoods;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private SlideButton slideButton;
        [SerializeField] private GameObject[] contents;

        [Space] [Space] [Header("Stepup")] 
        [SerializeField] private ViewSlotProductStepup[] viewSlotProductStepups;
        
        [Space] [Space] [Header("Package")]
        [SerializeField] private ViewSlotProduct[] packageViews;
        
        [Space] [Space] [Header("Summon")]
        [SerializeField] private ViewSlotProduct[] summonElementalViews;
        [SerializeField] private ViewSlotProduct[] summonRuneViews;

        [Space] [Space] [Header("Probability Button")]
        [SerializeField] private Button elementalProbabilityButton;
        [SerializeField] private Button runeProbabilityButton;
        
        [Space] [Space] [Header("Protection")]
        [SerializeField] private Button protectedDescButton;
        [SerializeField] private GameObject protectionProductLockPanel;
        [SerializeField] private TMP_Text protectionProductLockPanelTMP;
        
        [Space] [Space] [Header("VIP")]
        [SerializeField] private ViewSlotVIP viewSlotVip;

        public void SetActiveProtectionProductLock(bool flag, string text = "")
        {
            protectionProductLockPanel.SetActive(flag);
            protectionProductLockPanelTMP.text = text;
        }
    }
}
    
    
    
    
    
