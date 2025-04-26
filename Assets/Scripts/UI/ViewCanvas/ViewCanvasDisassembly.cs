using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasDisassembly : ViewCanvas
    {
        public DynamicValue MaterialDynamicValue => materialDynamicValue;
        public ScrollRect SlotScrollRect => slotScrollRect;
        public GridLayoutGroup SlotGridLayoutGroup => slotGridLayoutGroup;
        public ViewCanvasPopup SettingViewCanvasPopup => settingViewCanvasPopup;
        public Button OpenSettingButton => openSettingButton;
        public Button AutoAddButton => autoAddButton;
        public ActiveButton DisassembleButton => disassembleButton;
        public Button SaveAutoAddData => saveAutoAddData;
        
        public CheckBox[] TypeCheckBoxes => typeCheckBoxes;
        public CheckBox[] GradeCheckBoxes => gradeCheckBoxes;
        public Image GoodBackground => goodBackground;
        public Image ArrowImage => arrowImage;
        
        [Space] [Space] [Header("Goods")]
        [SerializeField] private DynamicValue materialDynamicValue;

        [SerializeField] private Image goodBackground;
        
        [Space][Space][Header("View Slot UI")]
        [SerializeField] private ScrollRect slotScrollRect;
        [SerializeField] private GridLayoutGroup slotGridLayoutGroup;
        [SerializeField] private Image arrowImage;
        
        [Space][Space][Header("Buttons")]
        [SerializeField] private Button openSettingButton;
        [SerializeField] private Button autoAddButton;
        [SerializeField] private ActiveButton disassembleButton;
        
        [FormerlySerializedAs("settingViewPopup")]
        [Space][Space][Header("Auto Add Setting Popup")]
        [SerializeField] private ViewCanvasPopup settingViewCanvasPopup;
        [SerializeField] private CheckBox[] typeCheckBoxes;
        [SerializeField] private CheckBox[] gradeCheckBoxes;
        [SerializeField] private Button saveAutoAddData;
    }
}