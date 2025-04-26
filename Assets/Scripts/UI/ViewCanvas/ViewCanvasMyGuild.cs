using System;
using System.Collections.Generic;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMyGuild : ViewCanvas
    {
        public ViewSlotGuildInfo ViewSlotGuildInfo => viewSlotGuildInfo;
        public Button[] GuildExitButtons => guildExitButtons;
        public Button GuildManageButton => guildManageButton;
        public Button CloseButton => closeButton;
        public ActiveButton GiftBoxRewardButton => giftBoxRewardButton;
        public ActiveButton ShowDonationButton => showDonationButton;
        public ActiveButton DonationButton => donationButton;
        public ViewCanvasPopup DonationViewCanvasPopup => donationViewCanvasPopup;
        public ViewGood DonationViewGoodPrefab => donationViewGoodPrefab;
        public Transform DonationViewGoodParent => donationViewGoodParent;
        public ViewCanvasPopup ManageViewCanvasPopup => manageViewCanvasPopup;
        public ViewSlotGuildInfo ManageGuildInfo => manageGuildInfo;
        public TMP_InputField DescInputField => descInputField;
        public Slider GuildManageStageSlider => guildManageStageSlider;
        public CheckBox AutoJoinCheckBox => autoJoinCheckBox;
        public CheckBox ApproveJoinCheckBox => approveJoinCheckBox;
        public Button GuildInfoChangeButton => guildInfoChangeButton;
        public GameObject DonationButtonWrapGo => donationButtonWrapGo;
        public ViewGood DonationViewGood => donationViewGood;
        public SlideButton SlideButton => slideButton;
        public ViewCanvasPopup RankingViewCanvasPopup => rankingViewCanvasPopup;
        public ViewSlotGuildInfo MyRankingViewSlot => myRankingViewSlot;
        public Transform RankingSlotParent => rankingSlotParent;
        public ActiveButton ShowRankingButton => showRankingButton;
        public ViewCanvasPopup ApplicantViewCanvasPopup => applicantViewCanvasPopup;
        public ViewSlotGuildApplicant ViewSlotGuildApplicantPrefab => viewSlotGuildApplicantPrefab;
        public Transform ApplicantSlotParent => applicantSlotParent;
        public ActiveButton ShowApplicantButton => showApplicantButton;
        public GameObject EmptyApplicantListTMPGo => emptyApplicantListTMPGo;
        public Button GiftBoxDescButton => giftBoxDescButton;


        public List<Transform> MenuPanels
        {
            get
            {
                if(_menuPanels == null)
                {
                    _menuPanels = new List<Transform>();
                    for (var i = 0; i < menuPanelParent.childCount; i++)
                    {
                        _menuPanels.Add(menuPanelParent.GetChild(i));
                    }
                }
                return _menuPanels;
            }
        }

        private List<Transform> _menuPanels;
        
        [SerializeField] private ViewSlotGuildInfo viewSlotGuildInfo;
        [SerializeField] private Transform menuPanelParent;
        [SerializeField] private Button[] guildExitButtons;
        [SerializeField] private Button guildManageButton;
        [SerializeField] private Button closeButton;

        [Space] [Space] [Header("Info")] 
        [SerializeField] private TMP_Text giftBoxTitleTMP;
        [SerializeField] private TMP_Text giftBoxKeyPointTMP;
        [SerializeField] private ActiveButton giftBoxRewardButton;
        [SerializeField] private ActiveButton showDonationButton;
        [SerializeField] private ActiveButton donationButton;
        [SerializeField] private Image giftBoxPointImage;
        [SerializeField] private Image giftBoxExpImage;
        [SerializeField] private TMP_Text giftBoxExpTMP;
        [SerializeField] private Button giftBoxDescButton;

        [FormerlySerializedAs("donationViewPopup")]
        [Space] [Space] [Header("Donation")]
        [SerializeField] private ViewCanvasPopup donationViewCanvasPopup;
        [SerializeField] private ViewGood donationViewGoodPrefab;
        [SerializeField] private Transform donationViewGoodParent;
        [SerializeField] private GameObject donationButtonWrapGo;
        [SerializeField] private ViewGood donationViewGood;
        [SerializeField] private TMP_Text donationTimeTMP;
        
        [FormerlySerializedAs("manageViewPopup")]
        [Space] [Space] [Header("Manage")]
        [SerializeField] private ViewCanvasPopup manageViewCanvasPopup;
        [SerializeField] private ViewSlotGuildInfo manageGuildInfo;
        [SerializeField] private TMP_InputField descInputField;
        [SerializeField] private Slider guildManageStageSlider;
        [SerializeField] private CheckBox autoJoinCheckBox;
        [SerializeField] private CheckBox approveJoinCheckBox;
        [SerializeField] private Button guildInfoChangeButton;

        [FormerlySerializedAs("rankingViewPopup")]
        [Space] [Space] [Header("Ranking")]
        [SerializeField] private ViewCanvasPopup rankingViewCanvasPopup;
        [SerializeField] private ViewSlotGuildInfo myRankingViewSlot;
        [SerializeField] private Transform rankingSlotParent;
        [SerializeField] private ActiveButton showRankingButton;
        
        [FormerlySerializedAs("applicantViewPopup")]
        [Space] [Space] [Header("Applicant View")]
        [SerializeField] private ViewCanvasPopup applicantViewCanvasPopup;
        [SerializeField] private ViewSlotGuildApplicant viewSlotGuildApplicantPrefab;
        [SerializeField] private Transform applicantSlotParent;
        [SerializeField] private ActiveButton showApplicantButton;
        [SerializeField] private GameObject emptyApplicantListTMPGo;
        
        [Space] [Space] 
        [SerializeField] private SlideButton slideButton;

        public ViewCanvasMyGuild SetGiftBoxTitle(string text)
        {
            giftBoxTitleTMP.text = text;
            return this;
        }
        
        public ViewCanvasMyGuild SetGiftKeyPoint(int currPoint, int maxPoint, bool fillAmount = true)
        {
            giftBoxKeyPointTMP.text = $"{currPoint}/{maxPoint}";
            if (fillAmount) SetGiftBoxPointFillAmount(currPoint, maxPoint);
            return this;
        }
        
        public ViewCanvasMyGuild SetGiftBoxExp(int currPoint, int maxPoint, bool fillAmount = true)
        {
            giftBoxExpTMP.text = $"{currPoint}/{maxPoint}";
            if (fillAmount) SetGiftBoxExpFillAmount(currPoint, maxPoint);
            return this;
        }
        
        private ViewCanvasMyGuild SetGiftBoxExpFillAmount(int currPoint, int maxPoint)
        {
            try
            {
                giftBoxExpImage.fillAmount = (float)currPoint / maxPoint;
            }
            catch (Exception e)
            {
                giftBoxExpImage.fillAmount = 0;
            }
            return this;
        }
        
        private ViewCanvasMyGuild SetGiftBoxPointFillAmount(int currPoint, int maxPoint)
        {
            try
            {
                giftBoxPointImage.fillAmount = (float)currPoint / maxPoint;
            }
            catch (Exception e)
            {
                giftBoxPointImage.fillAmount = 0;
            }
            return this;
        }

        public ViewCanvasMyGuild SetDonationTime(string text)
        {
            donationTimeTMP.text = text;
            return this;
        }
    }
}