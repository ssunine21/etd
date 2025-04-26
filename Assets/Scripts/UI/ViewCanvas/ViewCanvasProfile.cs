using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasProfile : ViewCanvas
    {
        public Button EditNicknameButton => editNicknameButton;
        public Button ChangeNicknameButton => changeNicknameButton;
        public Button CopyUUIDButton => copyUUIDButton;
        public TMP_Text UuidTMP => uuidTMP;
        public ViewCanvasPopup EditNicknameViewCanvasPopup => editNicknameViewCanvasPopup;
        public TMP_InputField NicknameInputField => nicknameInputField;
        public ViewSlotCombat ViewSlotCombatPrefab => viewSlotCombatPrefab;
        public Transform CombatPowerSlotParent => combatPowerSlotParent;
        public SlideButton CombatPowerSlideButton => combatPowerSlideButton;
        

        
        [Space][Space] [Header("Nickname")] 
        [SerializeField] private TMP_Text nicknameTMP;
        [SerializeField] private Button editNicknameButton;
        [FormerlySerializedAs("editNicknameViewPopup")] [SerializeField] private ViewCanvasPopup editNicknameViewCanvasPopup;
        [SerializeField] private Button copyUUIDButton;
        [SerializeField] private TMP_Text uuidTMP;

        [Space] [Space] [Header("Combat")] 
        [SerializeField] private TMP_Text totalCombatPowerTMP;
        [SerializeField] private Transform combatPowerSlotParent;
        [FormerlySerializedAs("viewCombatSlotPrefab")] [SerializeField] private ViewSlotCombat viewSlotCombatPrefab;
        [SerializeField] private SlideButton combatPowerSlideButton;

        [Header("Edit Nickname")] 
        [SerializeField] private TMP_InputField nicknameInputField;
        [SerializeField] private Button changeNicknameButton;
        [SerializeField] private TMP_Text badParameterTMP;
        [SerializeField] private GameObject goFreeOneChance;
        [SerializeField] private GameObject goViewGood;

        public ViewCanvasProfile SetCombatPower(string text)
        {
            totalCombatPowerTMP.text = text;
            return this;
        }
        
        public ViewCanvasProfile SetNickname(string nickname)
        {
            nicknameTMP.text = nickname;
            return this;
        }

        public ViewCanvasProfile SetBadParameterText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                badParameterTMP.text = text;
            
            badParameterTMP.gameObject.SetActive(!string.IsNullOrEmpty(text));
            return this;
        }

        public ViewCanvasProfile SetNicknameChangeChance(bool usedNicknameChangeChance)
        {
            goFreeOneChance.SetActive(!usedNicknameChangeChance);
            goViewGood.SetActive(usedNicknameChangeChance);
            return this;
        }
    }
}