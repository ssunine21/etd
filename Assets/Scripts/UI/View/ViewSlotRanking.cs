using ETD.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotRanking : ViewSlot<ViewSlotRanking>
    {
        [SerializeField] private TMP_Text rankTMP;
        [SerializeField] private TMP_Text nicknameTMP;
        [SerializeField] private TMP_Text scoreTMP;
        [SerializeField] private Image rankImage;

        public ViewSlotRanking SetRankingImage(int rank)
        {
            rankImage.gameObject.SetActive(rank  < 3);
            var sprite = ResourcesManager.Instance.GetRankSprite(rank);
            if (sprite != null)
                SetRankSprite(sprite);
            return this;
        }

        public ViewSlotRanking SetRank(string text)
        {
            rankTMP.text = text;
            return this;
        }

        private ViewSlotRanking SetRankSprite(Sprite sprite)
        {
            rankImage.sprite = sprite;
            return this;
        }

        public ViewSlotRanking SetNickanme(string text)
        {
            nicknameTMP.text = text;
            return this;
        }

        public ViewSlotRanking SetScore(string text)
        {
            scoreTMP.text = text;
            return this;
        }
    }
}