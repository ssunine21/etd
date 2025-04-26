using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotVIPReward : MonoBehaviour
    {
        [SerializeField] private TMP_Text rewardTMP;
        [SerializeField] private Image rewardImage;
        

        public ViewSlotVIPReward SetRewardText(string text)
        {
            rewardTMP.text = text;
            return this;
        }

        public ViewSlotVIPReward SetRewardSprite(Sprite sprite)
        {
            rewardImage.sprite = sprite;
            return this;
        }

        public ViewSlotVIPReward SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}
