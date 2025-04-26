using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasRune : ViewCanvas
    {
        public RectTransform MainViewRect => mainViewRectTr;
        public RectTransform SubViewRect => subViewRectTr;
        public GameObject ShadowPanel => shadowPanel;
        public CanvasGroup EquippingAnimCanvasGroup => equippingAnimCanvasGroup;
        public Transform RoundArrow => roundArrow;
        public Button CancelEquippingButton => cancelEquippingButton;
        
        public ViewProjectorUI ProjectorUI => projectorView;
        public GridLayoutGroup GridLayoutGroup => gridLayoutGroup;     
        public ActiveButton EquippedActiveButton => equippedActiveButton;
        public ViewSlotUI[] EquippedUnitSlots => equippedUnitSlots;
        public ActiveButton SortSlotActiveButton => slotSortActiveButton;
        public SlideButton SlideButton => slideButton;
        public TMP_Dropdown SortDropdown => sortDropdown;
        public TMP_Text CountText => runeCountText;
        public GameObject EmptyTextPanel => emptyText.gameObject;
        public GameObject LockPanel => lockPanel;
        public ViewSlotAttribute[] ViewEquippingAttrs => viewEquippingAttrs;
        public ActiveButton OpenEnhancementViewActiveButton => openEnhancementViewActiveButton;
        public ActiveButton OpenDisassemblyViewActiveButton => openDisassemblyViewActiveButton;
        public RectTransform EnhanceGuideArrowRectTransform => enhanceGuideArrowRectTransform;
        public RectTransform DisassembleGuideArrowRectTransform => disassembleGuideArrowRectTransform;
        public GameObject AttributeView => attributeView;
        public GameObject TagTextView => tagBoxView;

        [Space][Space][Header("TOP")]
        [SerializeField] private GameObject lockPanel;
        [SerializeField] private ViewProjectorUI projectorView;
        [SerializeField] private ViewSlotUI[] equippedUnitSlots;
        [SerializeField] private TMP_Text earnedTagTMP;
        [SerializeField] private SlideButton slideButton;

        [Space][Space][Header("MID")]
        [SerializeField] private ActiveButton equippedActiveButton;
        [SerializeField] private ActiveButton openEnhancementViewActiveButton;
        [SerializeField] private ActiveButton openDisassemblyViewActiveButton;
        [SerializeField] private ViewSlotAttribute[] viewEquippingAttrs;
        
        [Space][Space][Header("Bottom")]     
        [SerializeField] private TMP_Text emptyText;
        [SerializeField] private TMP_Text runeCountText;
        [SerializeField] private TMP_Dropdown sortDropdown;
        [SerializeField] private ActiveButton slotSortActiveButton;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;
        
        [Space][Space][Header("Equipping Animation")]
        [SerializeField] private RectTransform mainViewRectTr;
        [SerializeField] private RectTransform subViewRectTr;
        [SerializeField] private GameObject shadowPanel;
        [SerializeField] private CanvasGroup equippingAnimCanvasGroup;
        [SerializeField] private Transform roundArrow;
        [SerializeField] private Button cancelEquippingButton;
        
        [Space][Space][Header("Tutorial")]
        [SerializeField] private RectTransform enhanceGuideArrowRectTransform;
        [SerializeField] private RectTransform disassembleGuideArrowRectTransform;
        [SerializeField] private GameObject attributeView;
        [SerializeField] private GameObject tagBoxView;
        
        public ViewCanvasRune SetTagText(string text)
        {
            earnedTagTMP.text = text;
            earnedTagTMP.gameObject.SetActive(!string.IsNullOrEmpty(text));
            return this;
        }
    }
}