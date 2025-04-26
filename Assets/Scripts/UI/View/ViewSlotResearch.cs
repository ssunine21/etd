using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotResearch : MonoBehaviour
    {
        public ViewGood ViewGood => viewGood;
        public Button ResearchButton => researchButton;
        public GameObject ResearchingPanel => researchingPanel;
        
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text valueTMP;
        [SerializeField] private TMP_Text descTMP;
        [SerializeField] private TMP_Text timeTMP;
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private Button researchButton;
        [SerializeField] private GameObject researchingPanel;

        [SerializeField] private Image[] dynamicBackground;
        [SerializeField] private Image[] dynamicStroke;
        
        public ViewSlotResearch SetTitle(string title)
        {
            if (titleTMP)
                titleTMP.text = title; 
            return this;
        }
        
        public ViewSlotResearch SetDescription(string desc)
        {
            if (descTMP)
                descTMP.text = desc; 
            return this;
        }

        public ViewSlotResearch SetValue(string valueText)
        {
            if (valueTMP)
                valueTMP.text = valueText;
            return this;
        }

        public ViewSlotResearch SetTimestamp(string timeText)
        {
            if (timeTMP)
                timeTMP.text = timeText;
            return this;
        }

        public ViewSlotResearch SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotResearch SetMaxLevel(bool flag)
        {
            // if (dynamicBackground != null && dynamicStroke != null)
            // {
            //     var backgroundColor = flag ? new Color(6 / 255f, 26 / 255f, 35 / 255f) : new Color(17 / 255f, 55 / 255f, 74 / 255f);
            //     var strokeColor = flag ? new Color(17 / 255f, 55 / 255f, 74 / 255f) : new Color(87 / 255f, 111 / 255f, 114 / 255f);
            //     foreach (var image in dynamicBackground) image.color = backgroundColor;
            //     foreach (var image in dynamicStroke) image.color = strokeColor;
            // }

            if (flag)
            {
                var backgroundColor = /*flag ?*/ new Color(6 / 255f, 26 / 255f, 35 / 255f);// : new Color(17 / 255f, 55 / 255f, 74 / 255f);
                var strokeColor = /*flag ?*/ new Color(17 / 255f, 55 / 255f, 74 / 255f);// : new Color(87 / 255f, 111 / 255f, 114 / 255f);
                foreach (var image in dynamicBackground) image.color = backgroundColor;
                foreach (var image in dynamicStroke) image.color = strokeColor;
            }

            return this;
        }
    }
}