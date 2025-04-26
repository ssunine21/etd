using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UserData.DataController;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewGood : ViewSlot<ViewGood>
    {
        public Button Button => viewSlotGoodIcon.Button;
        public ReddotView ReddotView => reddotView;
        public Transform ImageTr => viewSlotGoodIcon.ImageTr;
        public GoodType GoodType => goodType;
        public double GoodValue { get; private set; }

        [SerializeField] private GoodType goodType;
        [SerializeField] private bool showMyGoods;
        [SerializeField] private bool isRedText;
        [SerializeField] private bool isShowDescription;
        [SerializeField] private bool isInit = true;

        [Space]
        [SerializeField] private ViewSlotGoodIcon viewSlotGoodIcon;
        [SerializeField] private TMP_Text goodText;
        [SerializeField] private TMP_Text goodGradeText;

        [Space] [Space]
        [SerializeField] private GameObject goAds;
        [SerializeField] private GameObject lockPanel;
        [SerializeField] private GameObject lockPanelBackground;
        [SerializeField] private GameObject checkPanel;
        [SerializeField] private GameObject backgroundPanel;

        [FormerlySerializedAs("reddot")]
        [Space] [Space] 
        [SerializeField] private ReddotView reddotView;
        

        private void Awake()
        {
            MainTask().Forget();
        }

        private async UniTaskVoid MainTask()
        {
            await UniTask.WaitUntil(() => GameManager.Instance.LoadingComplete);
            if (isInit) SetInit(goodType);
            
            DataController.Instance.good.OnBindChangeGood += UpdateView;
            UpdateView(goodType);
        }

        public ViewGood SetInit(GoodType type, int param0 = 0)
        {
            SetGoodType(type, param0);
            UpdateView(type);
            return this;
        }

        private ViewGood SetGoodType(GoodType type, int param0 = 0)
        {
            if (type == GoodType.None) return this;
            
            goodType = type;
            if (viewSlotGoodIcon)
            {
                viewSlotGoodIcon.SetGoodSprite(type, param0).SetButton();
            }
            
            return this;
        }

        public ViewGood SetGrade(GoodType type, int param0)
        {
            if (!goodGradeText) return this;
            
            goodGradeText.enabled = type is GoodType.SummonElemental or GoodType.SummonRune;
            var gradeType = type == GoodType.SummonElemental
                ? DataController.Instance.elemental.GetBData(param0).grade
                : DataController.Instance.rune.GetBData(param0).grade;

            goodGradeText.enabled = true;
            var htmlColor = ColorUtility.ToHtmlStringRGBA(ResourcesManager.Instance.GetGradeColor(gradeType));
            return SetGradeText($"<color=#{htmlColor}>{gradeType.ToString()}</color>");
        }

        private ViewGood SetGradeText(string text)
        {
            if (goodGradeText)
                goodGradeText.text = text;
            return this;
        }

        public ViewGood SetValue(string text)
        {
            if (!goodText) return this;
            goodText.text = text;
            return this;
        }

        public ViewGood SetValue(double goodValue, int param0 = 0)
        {
            GoodValue = goodValue;
            SetValue(goodValue.ToGoodString(goodType, param0));
            UpdateView(goodType);
            return this;
        }

        public ViewGood SetActiveValueText(bool flag)
        {
            goodText.gameObject.SetActive(flag);
            return this;
        }

        public ViewGood SetActiveBackground(bool flag)
        {
            backgroundPanel.SetActive(flag);
            return this;
        }
        
        public ViewGood ShowAdsPanel(bool flag)
        {
            goAds.SetActive(flag);
            return this;
        }

        public ViewGood AddListener(UnityAction action)
        {
            viewSlotGoodIcon.OverrideListener(action);
            return this;
        }

        public ViewGood SetActiveLockPaenl(bool flag)
        {
            lockPanel.SetActive(flag);
            if (lockPanelBackground)
                lockPanelBackground.SetActive(flag);
            return this;
        }

        [CanBeNull]
        public ViewGood SetActiveCheckPanel(bool flag)
        {
            checkPanel.SetActive(flag);
            return this;
        }

        public ViewGood StopTwincle()
        {
            if (TryGetComponent<ViewTwincle>(out var viewTwincle))
            {
                if (viewTwincle.IsPlaying)
                    viewTwincle.Stop();
            }

            return this;
        }

        public ViewGood PlayTwincle()
        {
            if (TryGetComponent<ViewTwincle>(out var viewTwincle))
            {
                if (!viewTwincle.IsPlaying)
                    viewTwincle.Play();
            }

            return this;
        }

        private void UpdateView(GoodType updatedGoodType)
        {
            var textColor = ResourcesManager.UIColor[ColorType.SkyBlue];
            if (goodType != updatedGoodType) return;
            if (!showMyGoods)
            {
                if (isRedText)
                {
                    textColor = DataController.Instance.good.IsEnoughGood(goodType, GoodValue) ? ResourcesManager.UIColor[ColorType.SkyBlue] : ResourcesManager.UIColor[ColorType.Red];
                }
            }
            else
            {
                SetValue(DataController.Instance.good.GetValue(goodType).ToGoodString(goodType));
            }
            
            goodText.color = textColor;
        }
    }
}