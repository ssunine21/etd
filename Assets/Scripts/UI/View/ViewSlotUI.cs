using System.Collections;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UserData.DataController;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Gradient = ETD.Scripts.UI.Common.Gradient;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotUI : MonoBehaviour
    {
        public ViewLevel ViewLevel => viewLevel;
        public Button Button => _activeButton.Button;
        public Rune Rune { get; set; }
        public Elemental Elemental { get; set; }
        public bool IsLock => _lockPanel.activeInHierarchy;
        public bool IsEnableEquipText => equipTMP.gameObject.activeInHierarchy;
        public bool IsEnableEquipping => _loadingArrowTr.gameObject.activeInHierarchy;
        public bool IsEnableBackCard => cardBackground.activeInHierarchy;
        public ReddotView ReddotView => reddotView;
        public CanvasGroup CanvasGroup => canvasGroup;
        public Image EffectFront => effectFront;
        public Image EffectBackground => effectBackground;
        public GradeType GradeType => gradeType;
        
        public ActiveButton ActiveButton => _activeButton;
        
        [Header("Border")]
        [SerializeField] private GameObject _equipBorder;
        [SerializeField] private GameObject _gradeBorder;
        [SerializeField] private GameObject enhancementBorder;
        [SerializeField] private Image effectBackground;
        [SerializeField] private Image effectFront;
        
        [Header("Border Info")]
        [SerializeField] private TMP_Text _gradeText;
        [SerializeField] private TMP_Text _equipIndexText;
        [SerializeField] private TMP_Text enhancementLevelText;
        
        [Space][Space][Header("Object")]
        [SerializeField] protected Image _objectImage;
        [SerializeField] private Image cardBackImage;
        [SerializeField] private ViewLevel viewLevel;
        [SerializeField] private GameObject cardBackground;
        
        
        [Space][Space][Header("etc")]
        [SerializeField] protected GameObject _lockPanel;
        [SerializeField] private Transform _loadingArrowTr;     
        [SerializeField] private ReddotView reddotView;
        [SerializeField] private ActiveButton _activeButton;
        [SerializeField] private Gradient gradient;
        [SerializeField] private Image strokeImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject goEquipTMP;
        [SerializeField] private TMP_Text equipTMP;
        [SerializeField] private TMP_Text duplicationCountTMP;

        protected GradeType gradeType;

        private Coroutine _coArrowRoutine;
        private Graphic _graphic;

        public virtual ViewSlotUI SetLock(bool flag)
        {
            _lockPanel.SetActive(flag);
            if (flag)
            {
                SetGradientColor(gradient.Color1);
            }
            return this;
        }

        public ViewSlotUI EnabledDuplicationCount(bool flag)
        {
            if (duplicationCountTMP)
                duplicationCountTMP.enabled = flag;
            return this;
        }

        public ViewSlotUI SetDuplicationCount(int count)
        {
            return SetDuplicationCountText($"X{count.ToString()}");
        }

        private ViewSlotUI SetDuplicationCountText(string text)
        {
            duplicationCountTMP.text = text;
            return this;
        }

        public ViewSlotUI SetActiveEquippedTMP(bool flag)
        {
            goEquipTMP.SetActive(flag);
            return this;
        }

        public ViewSlotUI SetEquippedText(string text)
        {
            SetActiveEquippedTMP(true);
            equipTMP.text = text;
            return this;
        }

        public ViewSlotUI SetUnitSprite(Sprite sprite)
        {
            _objectImage.enabled = sprite != null;
            _objectImage.sprite = sprite;
            return this;
        }

        public ViewSlotUI SetUnitColor(Color color)
        {
            _objectImage.color = color;
            return this;
        }

        public ViewSlotUI SetActiveEffectFront(bool flag)
        {
            effectFront.enabled = flag;
            return this;
        }

        public ViewSlotUI SetEffectFrontColor(Color color)
        {
            effectFront.color = color;
            return this;
        }

        public ViewSlotUI SetActiveEffectBackground(bool flag)
        {
            effectBackground.enabled = flag;
            return this;
        }

        public ViewSlotUI SetEffectBackgroundColor(Color color)
        {
            effectBackground.color = color;
            return this;
        }
        
        public ViewSlotUI SetActiveEquipBorder(int index = -1)
        {
            if (_equipBorder == null) return this;
            
            _equipIndexText.text = (index + 1).ToString();
            _equipBorder.SetActive(index > -1);
            return this;
        }

        public ViewSlotUI SetCardBackSprite(Sprite sprite)
        {
            cardBackImage.sprite = sprite;
            return this;
        }

        public ViewSlotUI SetActiveBackCard(bool flag)
        {
            cardBackground.SetActive(flag);
            return this;
        }

        public ViewSlotUI SetActiveGradeBorder(bool flag)
        {
            _gradeBorder.SetActive(flag);
            return this;
        }

        protected ViewSlotUI SetGradeTextColor(Color color)
        {
            _gradeText.color = color;
            SetGradientColor(color);
            SetStrokeImageColor(color);
            return this;
        }

        public ViewSlotUI SetGradeText(GradeType type)
        {
            gradeType = type;
            _gradeText.text = type.ToString();
            
            SetActiveGradeBorder(true);
            SetGradeTextColor(ResourcesManager.GradeColor[type]);
            return this;
        }

        public ViewSlotUI SetActiveEnhancementBorder(bool flag)
        {
            enhancementBorder.SetActive(flag);
            return this;
        }

        public ViewSlotUI SetEnhancementLevel(int level)
        {
            enhancementLevelText.text = $"+{level}";
            return this;
        }

        public ViewSlotUI SetActiveLevelView(bool flag)
        {
            viewLevel.SetActive(flag);
            return this;
        }

        public ViewSlotUI SetInit()
        {
            Elemental = null;
            Rune = null;
            SetActiveEquipBorder(-1)
                .SetActiveGradeBorder(false)
                .SetUnitSprite(null)
                .SetActiveEnhancementBorder(false)
                .SetActiveLevelView(false)
                .SetGradientColor(ResourcesManager.Instance.gradeDefaultColor);

            return this;
        }

        public ViewSlotUI SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotUI SetEnableEquipping(bool flag)
        {
            if (_loadingArrowTr.gameObject.activeSelf == flag) return this;
            
            var canvasGroup = _loadingArrowTr.GetComponent<CanvasGroup>();
            if (flag)
            {
                canvasGroup.alpha = 0;
                _loadingArrowTr.gameObject.SetActive(true);
                canvasGroup.DOFade(1, 0.1f).OnComplete(() =>
                {
                    _coArrowRoutine = StartCoroutine(StartArrowRoutine());
                });
            }
            else
            {
                if (_coArrowRoutine != null)
                {
                    canvasGroup.DOFade(0, 0.1f).OnComplete(() =>
                    {
                        StopCoroutine(_coArrowRoutine);
                        _loadingArrowTr.gameObject.SetActive(false);
                    });
                }
            }
            return this;
        }

        public ViewSlotUI SetGradientColor(Color color)
        {
            if (gradient)
            {
                gradient.Color2 = color;
                if (gradient.TryGetComponent<Graphic>(out var graphic))
                {
                    graphic.SetVerticesDirty();
                }
            }
            return this;
        }

        public ViewSlotUI SetStrokeImageColor(Color color)
        {
            if (strokeImage)
            {
                strokeImage.color = new Color(color.r, color.g, color.b, 0.3f);
            }
            return this;
        }

        private readonly WaitForSecondsRealtime _wfs = new (1.2f);
        private IEnumerator StartArrowRoutine()
        {
            while (true)
            {
                var endValue = new Vector3(0, 0, -180);
                _loadingArrowTr.DORotate(endValue, 1f).SetEase(Ease.OutCubic)
                    .OnComplete(() =>
                    {
                        _loadingArrowTr.rotation = Quaternion.Euler(Vector3.zero);
                    });
                yield return _wfs;
            }
        }
    }
}