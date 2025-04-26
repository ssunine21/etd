using System.Collections.Generic;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasLab : ViewCanvas
    {
        public GridLayoutGroup[] ResearchLayoutGroups => researchLayoutGroups;
        public Button CloseButton => closeButton;
        public List<RectTransform> ResearchRectTransforms
        {
            get
            {
                if (_researchRectTransforms == null)
                {
                    _researchRectTransforms = new();
                    foreach (var layoutGroup in researchLayoutGroups)
                    {
                        if (layoutGroup.TryGetComponent<RectTransform>(out var component))
                        {
                            _researchRectTransforms.Add(component);
                        }
                    }
                }

                return _researchRectTransforms;
            }
        }

        public ViewSlotResearch ViewSlotResearchPrefab => viewSlotResearchPrefab;
        public GameObject[] ResearchGroup0 => researchGroup0;
        public GameObject[] ResearchGroup1 => researchGroup1;
        public GameObject[] ResearchGroup2 => researchGroup2;
        public GameObject[] LabTypePanels => labTypePanels;
        public SlideButton ResearchGourp => researchGourp;
        public SlideButton LabType => labType;
        public ViewCanvasPopup ResearchViewCanvasPopup => researchViewCanvasPopup;
        public ViewSlotResearch ViewSlotResearchPopupView => viewPopupResearch;
        public ViewSlotLab ViewSlotLabPrefab => viewSlotLabPrefab;
        public ViewSlotLab ViewSlotSpecialLab => viewSlotSpecialLab;
        public Transform ViewSlotLabParent => viewSlotLabParent;
        public GameObject BeforeResearchPaenl => beforeResearchPaenl;
        public GameObject AfterResearchPaenl => afterResearchPaenl;
        public GameObject TimePanelAtViewPopup => timePanelAtViewPopup;
        public Image TimeFillAmount => timeFillAmount;
        public TMP_Text TimeTMP => timeTMP;
        public Button ResearchCancel => researchCancel;
        public Button CompleteImmediately => completeImmediately;
        public Button CompleteImmediatelyByDia => completeImmediatelyByDia;
        public Button OpenSpecialLabPopup => openSpecialLabPopup;
        public Button PurchaseSpecialLab => purchaseSpecialLab;
        public ViewCanvasPopup SepcialLabCanvasPopup => sepcialLabCanvasPopup;
        public ViewGood DiaPerSecondViewGood => diaPerSecondViewGood;
        public GameObject TutorialLabPanel => tutorialLabPanel;
        public GameObject TutorialStoragePanel => tutorialStoragePanel;
        public GameObject TutorialSaveToStoragePanel => tutorialSaveToStoragePanel;
        public ViewGood[] ViewGoods => viewGoods;
        
        [SerializeField] private GridLayoutGroup[] researchLayoutGroups;
        [SerializeField] private GameObject[] researchGroup0;
        [SerializeField] private GameObject[] researchGroup1;
        [SerializeField] private GameObject[] researchGroup2; 
        [SerializeField] private GameObject[] labTypePanels;
        [SerializeField] private ViewGood[] viewGoods;

        [FormerlySerializedAs("sepcialLabPopup")]
        [Space] [Space] 
        [SerializeField] private ViewCanvasPopup sepcialLabCanvasPopup;
        [FormerlySerializedAs("researchViewPopup")] [SerializeField] private ViewCanvasPopup researchViewCanvasPopup;
        [SerializeField] private ViewSlotResearch viewPopupResearch;
        [SerializeField] private ViewSlotLab viewSlotSpecialLab;
        [SerializeField] private Transform viewSlotLabParent;
        
        [Space][Space]
        [SerializeField] private ViewSlotLab viewSlotLabPrefab;
        [SerializeField] private ViewSlotResearch viewSlotResearchPrefab;
        [SerializeField] private TMP_Text specialLabPriceTMP;

        [Space][Space][Header("ViewPopup")]
        [SerializeField] private GameObject beforeResearchPaenl;
        [SerializeField] private GameObject afterResearchPaenl;
        [SerializeField] private GameObject timePanelAtViewPopup;
        [SerializeField] private Image timeFillAmount;
        [SerializeField] private TMP_Text timeTMP;
        [SerializeField] private Button researchCancel;
        [SerializeField] private Button completeImmediatelyByDia;
        [SerializeField] private Button completeImmediately;
        [SerializeField] private ViewGood diaPerSecondViewGood;
        
        [Space][Space]
        [SerializeField] private SlideButton researchGourp;
        [SerializeField] private SlideButton labType;
        [SerializeField] private Button purchaseSpecialLab;
        [SerializeField] private Button openSpecialLabPopup;
        [SerializeField] private Button closeButton;

        [Space] [Space] [Header("Tutorial")]
        [SerializeField] private GameObject tutorialLabPanel;
        [SerializeField] private GameObject tutorialStoragePanel;
        [SerializeField] private GameObject tutorialSaveToStoragePanel;

        private List<RectTransform> _researchRectTransforms;

        public void SetSpecialLabPrice(string text)
        {
            if (specialLabPriceTMP)
                specialLabPriceTMP.text = text;
        }
    }
}