using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasElemental : ViewCanvas
    {
        public RectTransform MainViewRect => mainViewRectTr;
        public RectTransform SubViewRect => subViewRectTr;
        public GameObject ShadowPanel => shadowPanel;
        public CanvasGroup EquippingAnimCanvasGroup => equippingAnimCanvasGroup;
        public Transform RoundArrow => roundArrow;
        public Button CancelEquippingButton => cancelEquippingButton;
        
        public ViewProjectorUI ProjectorUI => _elementalView;
        public ViewLevel ViewLevel => _viewLevel;
        public ViewSlotUI[] EquippedUnitSlots => _equippedUnitSlots;
        public GridLayoutGroup GridLayoutGroup => _gridLayoutGroup;     
        public Image LevelUpEffectImage => levelUpEffectImage;
        public ActiveButton EquippedActiveButton => _equippedActiveButton;
        public ActiveButton OpenEnhancementViewActiveButton => openEnhancementViewActiveButton;
        public SlideButton SlideButton => slideButton;
        
        public ViewSlotAttribute[] ViewEquippingAttrs => _viewEquippingAttrs;
        public ViewSlotAttribute[] ViewPossessionAttrs => _viewPossessionAttrs;

        public ActiveButton[] ChangeContentsViewActiveButtons => _changeContentsViewActiveButtons;
        public ActiveButton SortSlotActiveButton => _slotSortActiveButton;
        public GameObject LockPanel => _lockPanel;
        public RectTransform GuideArrowRectTransform => guideArrowRectTransform;
        public GameObject TagTextView => tagBoxView;
        public Button ShowInfoButon => showInfoButton;
        public Button AllLevelUpButton => allLevelUpButton;
        
        [Space] 
        [Header("Panel")]
        [SerializeField] private GameObject _viewNotAcquired;
        [SerializeField] private GameObject _viewDetailLeft;
        [SerializeField] private Button showInfoButton;
        
        [Space][Space][Header("TOP")]
        [SerializeField] private GameObject _lockPanel;
        [SerializeField] private ViewProjectorUI _elementalView;
        [SerializeField] private ViewSlotUI[] _equippedUnitSlots;
        [SerializeField] private TMP_Text earnedTagTMP;
        [SerializeField] private SlideButton slideButton;
        
        
        [Space][Space][Header("MID")]
        [SerializeField] private ViewLevel _viewLevel;
        [SerializeField] private Image levelUpEffectImage;
        [SerializeField] private ActiveButton levelUpActiveButton;
        [SerializeField] private ActiveButton _equippedActiveButton;
        [SerializeField] private ActiveButton openEnhancementViewActiveButton;
        [SerializeField] private ViewSlotAttribute[] _viewEquippingAttrs;
        [SerializeField] private ViewSlotAttribute[] _viewPossessionAttrs;
        
        [Space][Space][Header("Bottom")]
        [SerializeField] private GridLayoutGroup _gridLayoutGroup;
        [SerializeField] private ActiveButton[] _changeContentsViewActiveButtons;
        [SerializeField] private ActiveButton _slotSortActiveButton;
        [SerializeField] private Button allLevelUpButton;
        
        [Space][Space][Header("Equipping Animation")]
        [SerializeField] private RectTransform mainViewRectTr;
        [SerializeField] private RectTransform subViewRectTr;
        [SerializeField] private GameObject shadowPanel;
        [SerializeField] private CanvasGroup equippingAnimCanvasGroup;
        [SerializeField] private Transform roundArrow;
        [SerializeField] private Button cancelEquippingButton;
        
        [Space][Space][Header("Tutorial")]
        [SerializeField] private RectTransform guideArrowRectTransform;
        [SerializeField] private GameObject tagBoxView;
        
        public void SetLock(bool flag)
        {
            //_viewNotAcquired.SetActive(flag);
            //_viewDetailLeft.SetActive(!flag);
        }

        public ViewCanvasElemental SetTagText(string text)
        {
            earnedTagTMP.text = text;
            earnedTagTMP.gameObject.SetActive(!string.IsNullOrEmpty(text));
            return this;
        }
    }
}