using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotMissionReward : MonoBehaviour
    {
        public ViewGood[] ViewGoods => viewGoods;
        
        [SerializeField] private TMP_Text title;
        [SerializeField] private Image countGage;
        [SerializeField] private TMP_Text countTMP;
        [SerializeField] private ViewGood[] viewGoods;

        public ViewSlotMissionReward SetTitle(string text)
        {
            title.text = text;
            return this;
        }

        public ViewSlotMissionReward SetFillAmount(float curr, float max)
        {
            SetFillAmount(curr == 0 || max == 0 ? 0 : curr / max);
            return this;
        }

        public ViewSlotMissionReward SetFillAmount(float amount)
        {
            countGage.fillAmount = amount;
            return this;
        }

        public ViewSlotMissionReward SetCountText(int curr, int max)
        {
            SetCountText($"{curr}/{max}");
            return this;
        }

        public ViewSlotMissionReward SetCountText(string text)
        {
            countTMP.text = text;
            return this;
        }
    }
}