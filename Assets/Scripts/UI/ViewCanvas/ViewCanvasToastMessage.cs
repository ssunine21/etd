using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasToastMessage : ViewCanvas
    {
        public RectTransform TransientViewBackgroundRect => transientViewBackgroundRect;
        public TMP_Text TransientMessage => transientMessageText;
        public Transform TransientView => transientView;
        public ViewCanvasPopup ToastMessageBox => toastMessageBox;
        public Image FadeInOutBackground => fadeInOutBackground;
        public ActiveButton ToastBoxButton0 => toastBoxButton0;
        public ActiveButton ToastBoxButton1 => toastBoxButton1;
        public TMP_Text ToastMessageBoxDesc => toastMessageBoxDesc;

        public CanvasGroup LoadingPanel => loadingPanel;
        public Transform BigCircle => bigCircle;
        public Transform SmallCircle => smallCircle;
        
        public ViewCanvasPopup SimpleRewardViewCanvasPopup => simpleRewardViewCanvasPopup;
        public ViewGood ViewGoodPrefab => viewGoodPrefab;
        public Transform SimpleRewardViewGoodParent => simpleRewardViewGoodParent;
        
        [Header("TransientPopup")]
        [SerializeField] private Transform transientView;
        [SerializeField] private TMP_Text transientMessageText;
        [SerializeField] private RectTransform transientViewBackgroundRect;

        [Space] [Space] [Header("Message Box")] 
        [SerializeField] private ViewCanvasPopup toastMessageBox;
        [SerializeField] private GameObject titleGo;
        [SerializeField] private GameObject buttonsGo;
        [SerializeField] private TMP_Text toastMessageBoxTitle;
        [SerializeField] private TMP_Text toastMessageBoxDesc;
        [SerializeField] private ActiveButton toastBoxButton0;
        [SerializeField] private ActiveButton toastBoxButton1;

        [Space] [Space] [Header("Loading")] 
        [SerializeField] private CanvasGroup loadingPanel;
        [SerializeField] private Transform bigCircle;
        [SerializeField] private Transform smallCircle;


        [FormerlySerializedAs("simpleRewardViewPopup")]
        [Space] [Space] [Header("Reward")] 
        [SerializeField] private ViewCanvasPopup simpleRewardViewCanvasPopup;
        [SerializeField] private ViewGood viewGoodPrefab;
        [SerializeField] private Transform simpleRewardViewGoodParent;
        [SerializeField] private ViewSlotTitle viewSlotRewardTitle;
        [SerializeField] private TMP_Text closeSecondsTMP;
        
        [Space] [Space] 
        [SerializeField] private Image fadeInOutBackground;

        public ViewCanvasToastMessage SetCloseSecondsText(string text)
        {
            closeSecondsTMP.text = text;
            return this;
        }
        
        public ViewCanvasToastMessage SetSimpleRewardTitle(string title)
        {
            viewSlotRewardTitle.SetText(title);
            return this;
        }
        public ViewCanvasToastMessage SetToastBoxMessage(string title, string desc)
        {
            toastMessageBoxTitle.text = title;
            toastMessageBoxDesc.text = desc;
            return this;
        }

        public ViewCanvasToastMessage SetActiveTitleGo(bool flag)
        {
            titleGo.SetActive(flag);
            return this;
        }

        public ViewCanvasToastMessage SetActiveButtonsGo(bool flag)
        {
            buttonsGo.SetActive(flag);
            return this;
        }
    }
}