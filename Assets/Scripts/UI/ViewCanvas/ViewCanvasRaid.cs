using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasRaid : ViewCanvas
    {
        public Button CloseButton => closeButton;
        public Transform LevelPanel => levelPanel;
        public ActiveButton LevelActiveButtonPrefab => levelActiveButtonPrefab;
        public ViewSlotRaidPlace ViewSlotRaidPlacePrefab => viewSlotRaidPlacePrefab;
        public Transform PlacePanel => placePanel;
        public Button RaidButton => raidButton;
        public Image MyPointOnImage => myPointOnImage;
        public Image MyPointOffImage => myPointOffImage;
        public Button RaidCancelButton => cancelButton;
        public ActiveButton[] TabActiveButtons => tabActiveButtons;
        public GameObject[] TabPanels => tabPanels;
        public Button OpenGuidePopupViewButton => openGuidePopupViewButton;
        public Image DarkDiaStateStateImage => darkDiaStateImage;
        public Button RefreshButton => refreshButton;
        public Transform DarkDiaValueImageTr => currValueTMP.transform;
        public ReddotView Reddot => reddot;
        public ViewCanvasPopup ViewRaidInfoPopup => viewRaidInfoPopup;

        [SerializeField] private ActiveButton[] tabActiveButtons;
        [SerializeField] private GameObject[] tabPanels;
        [SerializeField] private ActiveButton levelActiveButtonPrefab;
        [SerializeField] private Transform levelPanel;
        [SerializeField] private ViewSlotRaidPlace viewSlotRaidPlacePrefab;
        [SerializeField] private Transform placePanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button openGuidePopupViewButton;

        [SerializeField] private GameObject nextRaidTicketTimePanel;
        [SerializeField] private TMP_Text nextRaidTicketTimeTMP;
        [SerializeField] private TMP_Text currRaidTicketTMP;
        [SerializeField] private Button refreshButton;
        [SerializeField] private GameObject refreshButtonLockPanel;
        [SerializeField] private TMP_Text refreshTimeTMP;

        [Space] [Space] [Header("RaidInfoPopupView")] 
        [SerializeField] private ViewCanvasPopup viewRaidInfoPopup;
        [SerializeField] private ViewSlotTitle viewSlotTitleRaidInfo;
        [SerializeField] private TMP_Text stateTMP;
        [SerializeField] private TMP_Text userInfoTMP;
        [SerializeField] private TMP_Text valueTMP;
        [SerializeField] private TMP_Text durationTMP;
        [SerializeField] private TMP_Text enterTMP;
        [SerializeField] private Button raidButton;
        [SerializeField] private Image pointOffImage;

        [Space] [Space] [Header("My State")] 
        [SerializeField] private TMP_Text attackerUserNickname;
        [SerializeField] private TMP_Text myStateTMP;
        [SerializeField] private TMP_Text currValueTMP;
        [SerializeField] private TMP_Text currTimeTMP;
        [SerializeField] private TMP_Text protectedTMP;
        [SerializeField] private TMP_Text buttonTMP;
        [SerializeField] private Image darkDiaStateImage;
        [SerializeField] private Image fillAmount;
        [SerializeField] private Image myPointOnImage;
        [SerializeField] private Image myPointOffImage;
        [SerializeField] private Button cancelButton;
        [SerializeField] private ReddotView reddot;


        public ViewCanvasRaid SetTitleText(string text)
        {
            viewSlotTitleRaidInfo.SetText(text);
            return this;
        }

        public ViewCanvasRaid SetAttackerNicknameText(string text)
        {
            attackerUserNickname.text = text;
            attackerUserNickname.enabled = !string.IsNullOrEmpty(text);
            return this;
        }

        public ViewCanvasRaid SetStateText(string text)
        {
            stateTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetUserInfoText(string text)
        {
            userInfoTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetValueText(string text)
        {
            valueTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetEnterButtonText(string text)
        {
            enterTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetDurationText(string text)
        {
            durationTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetEmpty(bool isEmpty)
        {
            pointOffImage.enabled = !isEmpty;
            userInfoTMP.gameObject.SetActive(!isEmpty);
            durationTMP.gameObject.SetActive(isEmpty);
            return this;
        }

        public ViewCanvasRaid SetMyStateTMP(string text)
        {
            myStateTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetCurrValueTMP(string text)
        {
            currValueTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetCurrTimeTMP(string text)
        {
            currTimeTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetMyRaidCancelButtonTMP(string text)
        {
            buttonTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetCurrFillAmount(float value)
        {
            fillAmount.fillAmount = value;
            return this;
        }

        public ViewCanvasRaid SetNextRaidTicketTimeText(string text)
        {
            nextRaidTicketTimeTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetCurrRaidTicketText(string text)
        {
            currRaidTicketTMP.text = text;
            return this;
        }

        public ViewCanvasRaid SetActiveTicketTimePanel(bool flag)
        {
            nextRaidTicketTimePanel.SetActive(flag);
            return this;
        }
        

        public ViewCanvasRaid SetLockRefreshButton(bool flag)
        {
            refreshButtonLockPanel.SetActive(flag);
            refreshButton.enabled = !flag;
            return this;
        }

        public ViewCanvasRaid SetRefreshTimeText(string text)
        {
            refreshTimeTMP.text = text;
            return this;
        }
        
        public ViewCanvasRaid SetProtectedText(string text)
        {
            protectedTMP.text = text;
            return this;
        }
    }
}