using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMainMenu : ViewCanvas
    {
        public ViewGood[] ViewGoods => viewGoods;

        public Button OpenInnerMenu => openInnerMenu;
        public Button ProfileButton => profileButton;
        
        public RectTransform InnerMenuWrapRectTr => innerMenuWrapRectTr;
        public Button[] InnerMenuButtons => innerMenuButtons;
        public Button CloseInnerMenu => closeInnerMenu;
        public Button OpenSetting => openSetting;
        public Button OpenPowerSaving => openPowerSaving;
        public Button OpenRanking => openRanking;
        public Button OpenMission => openMission;
        public Button OpenMail => openMail;
        public Button OpenFreeGifts => openFreeGifts;
        public Button OpenNewPackage => openNewPackage;
        public Button OpenVip => openVip;
        public Button OpenBuffs => openBuffs;
        public Button OpenAttendance => openAttendance;
        public Button OpenFirstPurchase => openFirstPurchase;
        public Button OpenPass => openPass;
        public Button OpenGrowPass => openGrowPass;
        public Button OpenTestPanel => openTestPanel;
        public Button TestGameSpeedButton => testGameSpeedButton;
        public Button OpenPresetMenu => openPresetMenu;
        public Button OpenLab => openLab;
        public Button OpenRaid => openRaid;
        public Button OpenGuild => openGuild;
        public RectTransform PresetMenuWrapRectTr => presetMenuWrapRectTr;
        public ActiveButton[] PresetButtons => presetButtons;
        public Image ChessImage => chessImage;
        public Image PresetCloseImage => presetCloseImage;

        [Space] [Space] [Header("Infos")]
        [SerializeField] private Button profileButton;
        [SerializeField] private TMP_Text nicknameTMP;
        [SerializeField] private TMP_Text combatPowerTMP;
        [SerializeField] private ViewGood[] viewGoods;
        
        [Space] [Space] [Header("Inner Menus")] 
        [SerializeField] private RectTransform innerMenuWrapRectTr;
        [SerializeField] private Button openInnerMenu;
        [SerializeField] private Button[] innerMenuButtons;
        [SerializeField] private Button closeInnerMenu;
        [SerializeField] private Button openPowerSaving;
        [SerializeField] private Button openSetting;
        [SerializeField] private Button openRanking;
        [SerializeField] private Button openMail;
        [SerializeField] private Button openTestPanel;
        [SerializeField] private Button testGameSpeedButton;

        [Space] [Space] [Header("Right Menus")]    
        [SerializeField] private Button openMission;
        [SerializeField] private Button openLab;
        [SerializeField] private Button openRaid;
        [SerializeField] private Button openGuild;
        
        [Space] [Space] [Header("Left Menus")] 
        [SerializeField] private Button openVip;
        [SerializeField] private Button openBuffs;
        [SerializeField] private Button openFreeGifts;
        [SerializeField] private Button openPass;
        [SerializeField] private Button openGrowPass;
        [SerializeField] private Button openAttendance;
        [SerializeField] private Button openFirstPurchase;
        [SerializeField] private Button openNewPackage;
        
        [Space] [Space] [Header("Preset Menus")] 
        [SerializeField] private RectTransform presetMenuWrapRectTr; 
        [SerializeField] private Button openPresetMenu;
        [SerializeField] private ActiveButton[] presetButtons;
        [SerializeField] private Image presetCloseImage;
        [SerializeField] private Image chessImage;
        
        [Space] [Space] [Header("etc")] 
        [SerializeField] private TMP_Text newPackageTimeTMP;
        [SerializeField] private TMP_Text vipLevelTMP;

        public ViewCanvasMainMenu SetVipLevelText(string text)
        {
            vipLevelTMP.text = text;
            return this;
        }
        
        public ViewCanvasMainMenu SetNickname(string nickname)
        {
            nicknameTMP.text = nickname;
            return this;
        }

        public ViewCanvasMainMenu SetCombatPowerString(string combatPower)
        {
            combatPowerTMP.text = combatPower;
            return this;
        }

        public ViewCanvasMainMenu SetInnerMenu(Button[] buttons)
        {
            innerMenuButtons = buttons;
            return this;
        }

        public ViewCanvasMainMenu SetNewPackageTime(string time)
        {
            newPackageTimeTMP.text = time;
            return this;
        }
    }
}