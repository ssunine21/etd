using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotProbability : ViewSlot<ViewSlotProbability>
    {
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text probabilityTMP;
        [SerializeField] private GameObject line;

        public ViewSlotProbability SetTitle(string text)
        {
            titleTMP.text = text;
            return this;
        }
        
        public ViewSlotProbability SetProbability(float value)
        {
            return SetProbability($"{value}%");
        }

        public ViewSlotProbability SetTitleColor(Color color)
        {
            titleTMP.color = color;
            return this;
        }

        public ViewSlotProbability SetValueColor(Color color)
        {
            probabilityTMP.color = color;
            return this;
        }
        
        public ViewSlotProbability SetProbability(string text)
        {
            probabilityTMP.text = text;
            return this;
        }
        
        public ViewSlotProbability SetActiveLine(bool flag)
        {
            line.SetActive(flag);
            return this;
        }
    }
}