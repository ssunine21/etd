using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMainMenu
    {
        public RectTransform DpsPanel => dpsPanel;
        public Button DpsArrowButton => dpsArrowButton;
        public Button DpsSwitchButton => dpsSwitchButton;
        public Button DpsResetButton => dpsResetButton;
        public GameObject DpsTimeGo => dpsTimeGo;
        public ViewSlotDPS[] ViewSlotDps => viewSlotDps;

        [Space] [Space] 
        [Header("DPS")] 
        [SerializeField] private RectTransform dpsPanel;
        [SerializeField] private Button dpsArrowButton;
        [SerializeField] private TMP_Text dpsTimeTMP;
        [SerializeField] private GameObject dpsTimeGo;
        [SerializeField] private Button dpsSwitchButton;
        [SerializeField] private Button dpsResetButton;
        [SerializeField] private ViewSlotDPS[] viewSlotDps;
        

        public ViewCanvasMainMenu SetPresetTimeText(string text)
        {
            dpsTimeTMP.text = text;
            return this;
        }
    }
}