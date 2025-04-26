using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasTest : ViewCanvas
    {
        public Button StageInitButton => _stageInitButton;
        public Button StageClearButton => _stageClearButton;
        public Button ElementalInitButton => _elementalInitButton;
        public Button RuneInitButton => _runeInitButton;
        public Button GoodButton => _goodsButton;
        public Button GoodViewCloseButton => _goodViewCloseButton;
        public Button ShowTransientMessage => _showTransientMessage;
        public Button BackButton => _backButton;
        public Button UpgradeInitButton => _upgradeInitButton;
        public Button ResearchInitButton => _researchInitButton;
        public Button NextTimeButton => _nextTimeButton;
        public Button AllInitButton => allInitButton;
        public Button ShowInitPanelButton => _showInitPanel;
        public ScrollRect GoodScrollRect => _goodScrollRect;
        public TMP_InputField GoodInputField => _goodInputField;
        public Toggle EnemyInvincibilityToggle => _enemyInvincibilityToggle;
        public Toggle UnitInvincibilityToggle => _unitInvincibilityToggle;
        public Button EarnAllElemental => _earnAllElemental;
        public Button MoveQuestPanel => _moveQuestPanelButton;
        public Button MoveQuest => _moveQuestButton;
        public Button ReleaseGrowPass => _releaseGrowPass;
        public TMP_InputField QuestInputField => _questInputField;
        public GameObject QuestPanel => _moveQuestPanel;
        
        public GameObject GoodView => _goodsView;

        [SerializeField] private GameObject _etcPanel;
        [SerializeField] private GameObject _initPanel;
        [SerializeField] private GameObject _moveQuestPanel;

        [SerializeField] private Button _stageClearButton;
        [SerializeField] private Button _goodsButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _showTransientMessage;
        [SerializeField] private Button _showInitPanel;
        [SerializeField] private Button _earnAllElemental;
        [SerializeField] private Button _releaseGrowPass;
        
        [Space][Space][Header("Initialize")]
        [SerializeField] private Button _stageInitButton;
        [SerializeField] private Button _moveQuestPanelButton;
        [SerializeField] private Button _elementalInitButton;
        [SerializeField] private Button _runeInitButton;
        [SerializeField] private Button _upgradeInitButton;
        [SerializeField] private Button _researchInitButton;
        [SerializeField] private Button _nextTimeButton;
        [SerializeField] private Button allInitButton;
        [SerializeField] private Button _backButton;


        [SerializeField] private GameObject _goodsView;
        [SerializeField] private Button _goodViewCloseButton;
        [SerializeField] private ScrollRect _goodScrollRect;
        [SerializeField] private TMP_InputField _goodInputField;
        [SerializeField] private TMP_InputField _questInputField;
        [SerializeField] private Button _moveQuestButton;

        [Space] 
        [SerializeField] private Toggle _enemyInvincibilityToggle;
        [SerializeField] private Toggle _unitInvincibilityToggle;

        public void InitPanel()
        {
            _etcPanel.SetActive(true);
            _initPanel.SetActive(false);
        }

        public void ShowInitPanel()
        {
            _etcPanel.SetActive(false);
            _initPanel.SetActive(true);
        }
    }
}