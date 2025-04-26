using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotRaidPlace : MonoBehaviour
    {
        public Button EnterButton => enterButton;
        public Image BackgroundImage => backgroundImage;
        
        [SerializeField] private TMP_Text stateTMP;
        [SerializeField] private TMP_Text middleFirstTMP;
        [SerializeField] private TMP_Text middleSecondTMP;
        [SerializeField] private TMP_Text enterTMP;

        [SerializeField] private Image pointOffImage;
        [SerializeField] private Button enterButton;
        [SerializeField] private Image backgroundImage;

        public ViewSlotRaidPlace SetStateText(string text)
        {
            stateTMP.text = text;
            return this;
        }

        public ViewSlotRaidPlace SetMiddleFirstText(string text)
        {
            middleFirstTMP.text = text;
            return this;
        }

        public ViewSlotRaidPlace SetMiddleSecondText(string text)
        {
            middleSecondTMP.text = text;
            return this;
        }

        public ViewSlotRaidPlace SetButtonText(string text)
        {
            enterTMP.text = text;
            return this;
        }

        public ViewSlotRaidPlace SetPointOn(bool isOn)
        {
            pointOffImage.enabled = !isOn;
            return this;
        }

        public ViewSlotRaidPlace SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}