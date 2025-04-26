using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasPowerSaving : ViewCanvas
    {
        public Transform RotateIcon => rotateIcon;
        public BatteryInfo BatteryInfo => batteryInfo;
        public SlideToUnLock SlideToUnlock => slideToUnlock;

        [SerializeField] private SlideToUnLock slideToUnlock;
        
        [Space][Space][Header("Battery")]
        [SerializeField] private BatteryInfo batteryInfo;
        [SerializeField] private Image batteryImage;
        [SerializeField] private TMP_Text batteryTMP;
        
        [Space][Space][Header("DateTime")]
        [SerializeField] private TMP_Text timeTMP;
        [SerializeField] private TMP_Text dateTMP;
        
        [Space][Space][Header("Stage")]
        [SerializeField] private TMP_Text stageLevelTMP;
        [SerializeField] private Transform rotateIcon;

        public ViewCanvasPowerSaving SetTimeText(string text)
        {
            timeTMP.text = text;
            return this;
        }

        public ViewCanvasPowerSaving SetDateText(string text)
        {
            dateTMP.text = text;
            return this;
        }

        public ViewCanvasPowerSaving SetCurrLevel(string text)
        {
            stageLevelTMP.text = text;
            return this;
        }

        public ViewCanvasPowerSaving SetBatteryText(string text)
        {
            batteryTMP.text = text;
            return this;
        }

        public ViewCanvasPowerSaving SetBatteryColor(Color color)
        {
            batteryImage.color = color;
            return this;
        }
    }
}