using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasGuild : ViewCanvas
    {
        public ActiveButton RefreshListButton => refreshListButton;
        public TMP_InputField SearchInputField => searchInputField;
        public SlideButton SlideButton => slideButton;
        public ActiveButton OpenMarkCorrectionButton => openMarkCorrectionButton;
        public TMP_InputField NameInputField => nameInputField;
        public TMP_InputField InfoInputField => infoInputField;
        public CheckBox AutoJoinCheckBox => autoJoinCheckBox;
        public CheckBox ApproveJoinCheckBox => approveJoinCheckBox;
        public Slider NeededStageSlider => neededStageSlider;
        public ViewGood ViewGoodForCreation => viewGoodForCreation;
        public ActiveButton CreationButton => creationButton;
        public GameObject RecommendView => recommendView;
        public GameObject CreationView => creationView;
        public ViewCanvasPopup MarkCorrectionViewCanvasPopup => markCorrectionViewCanvasPopup;
        public SlideButton MarkTypeSlideButton => markTypeSlideButton;
        public ViewSlotUI ViewSlotUIPrefab => viewSlotUIPrefab;
        public GridLayoutGroup ViewSlotUIParent => viewSlotUIParent;
        public Button ConfirmButton => confirmButton;
        public ViewSlotGuildInfo MarkInfo => markInfo;
        public ViewSlotGuildInfo GuildInfo => guildInfo;
        public GameObject EmptyGuildText => emptyGuildText;
        public ViewSlotGuildInfo ViewSlotGuildListPrefab => viewSlotGuildListPrefab;
        public ScrollRect GuildListScrollRect => guildListScrollRect;
        public ViewCanvasPopup GuildInfoViewCanvasPopup => guildInfoViewCanvasPopup;
        public ViewSlotGuildMember ViewSlotGuildMember => viewSlotGuildMember;
        public GridLayoutGroup GuildMemberParent => guildMemberParent;
        public ViewSlotGuildInfo SelectedGuildInfo => selectedGuildInfo;
        public Button JoinButton => joinButton;
    
        [Space] [Header("Recommend")]
        [SerializeField] private GameObject recommendView;
        [SerializeField] private ActiveButton refreshListButton;
        [SerializeField] private TMP_InputField searchInputField;
        [SerializeField] private SlideButton slideButton;
        [SerializeField] private ViewSlotGuildInfo viewSlotGuildListPrefab;
        [SerializeField] private ScrollRect guildListScrollRect;

        [Space] [Space] [Header("Creation")] 
        [SerializeField] private GameObject creationView;
        [SerializeField] private ViewSlotGuildInfo guildInfo;
        [SerializeField] private ActiveButton openMarkCorrectionButton;
        [SerializeField] private TMP_InputField nameInputField;
        [SerializeField] private TMP_InputField infoInputField;
        [SerializeField] private CheckBox autoJoinCheckBox;
        [SerializeField] private CheckBox approveJoinCheckBox;
        [SerializeField] private Slider neededStageSlider;
        [SerializeField] private TMP_Text neededStageTMP;
        [SerializeField] private ViewGood viewGoodForCreation;
        [SerializeField] private ActiveButton creationButton;
    
        [FormerlySerializedAs("markCorrectionViewPopup")]
        [Space] [Space] [Header("Mark")] 
        [SerializeField] private ViewCanvasPopup markCorrectionViewCanvasPopup;
        [SerializeField] private SlideButton markTypeSlideButton;
        [SerializeField] private ViewSlotUI viewSlotUIPrefab;
        [SerializeField] private GridLayoutGroup viewSlotUIParent;
        [SerializeField] private Button confirmButton;
        [SerializeField] private ViewSlotGuildInfo markInfo;
    
        [FormerlySerializedAs("guildInfoViewPopup")]
        [Space] [Space] [Header("GuildInfo")] 
        [SerializeField] private ViewCanvasPopup guildInfoViewCanvasPopup;
        [SerializeField] private ViewSlotGuildInfo selectedGuildInfo;
        [SerializeField] private ViewSlotGuildMember viewSlotGuildMember;
        [SerializeField] private GridLayoutGroup guildMemberParent;
        [SerializeField] private TMP_Text joinButtonTMP;
        [SerializeField] private Button joinButton;
    

        [Space] [Space] [Header("ETC")] 
        [SerializeField] private GameObject emptyGuildText;

        public ViewCanvasGuild SetNeededStageText(string text)
        {
            neededStageTMP.text = text;
            return this;
        }
        
        public ViewCanvasGuild SetJoinButtonText(string text)
        {
            joinButtonTMP.text = text;
            return this;
        }
    }
}