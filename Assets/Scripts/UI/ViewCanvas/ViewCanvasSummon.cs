using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasSummon : ViewCanvas
    {
        public Transform SlotParent => _slotParent;
        public Button CloseButton => _closeButton;

        public ViewGood SummonViewGood => summonViewGood;
        public ViewGood TopTicketViewGood => topTicketViewGood;

        public Button AllOpenButton => allOpenButton;
        public Button ExitButton => exitButton;
        public Button SummonButton => summonButton;
        public CheckBox EffectSkipCheckBox => effectSkipCheckBox;
        public GameObject ResummonPanel => resummonPanel;
        
        [SerializeField] private Transform _slotParent;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ViewGood topTicketViewGood;
        [SerializeField] private ViewGood summonViewGood;
        [SerializeField] private GameObject resummonPanel;
        
        [Space] [Space] [Header("Buttons")]
        [SerializeField] private GameObject allOpenButtonGameObject;
        [SerializeField] private GameObject summonAndExitButtonGameObject;
        [SerializeField] private Button exitButton;
        [SerializeField] private Button summonButton;
        [SerializeField] private Button allOpenButton;
        [SerializeField] private CheckBox effectSkipCheckBox;

        public ViewCanvasSummon ChangeAllOpenButtonToExitButton(bool flag)
        {
            allOpenButton.enabled = !flag;
            allOpenButtonGameObject.SetActive(!flag);

            summonButton.enabled = flag;
            summonAndExitButtonGameObject.SetActive(flag);
            return this;
        }
    }
}