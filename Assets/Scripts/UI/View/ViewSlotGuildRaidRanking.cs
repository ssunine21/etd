using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotGuildRaidRanking : MonoBehaviour
    {
        [SerializeField] private Image markImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private TMP_Text rankTMP;
        [SerializeField] private TMP_Text nicknameTMP;
        [SerializeField] private TMP_Text levelTMP;

        public ViewSlotGuildRaidRanking SetRankingMark(int rank)
        {
            markImage.gameObject.SetActive(rank < 3);
            if(rank < 3)
            {
                var rankSprite = ResourcesManager.Instance.GetRankSprite(rank);
                if (rankSprite != null)
                    markImage.sprite = rankSprite;
            }
            return this;
        }

        public ViewSlotGuildRaidRanking SetBackgroundColor(int rank)
        {
            var gradeType = rank switch
            {
                0 => GradeType.SS,
                1 => GradeType.S,
                _ => GradeType.C
            };
            var color = ResourcesManager.Instance.GetGradeColor(gradeType);
            backgroundImage.color = color;
            return this;
        }

        public ViewSlotGuildRaidRanking SetRanking(int rank)
        {
            rankTMP.text = (rank + 1).ToString();
            return this;
        }

        public ViewSlotGuildRaidRanking SetRanking(string rank)
        {
            rankTMP.text = rank;
            return this;
        }

        public ViewSlotGuildRaidRanking SetNickname(string text)
        {
            nicknameTMP.text = text;
            return this;
        }

        public ViewSlotGuildRaidRanking SetLevelAndTotalDamage(int level, double totalDamage)
        {
            levelTMP.text = $"{LocalizeManager.GetText(LocalizedTextType.Lv, level)}<size=75%> ({totalDamage.ToDamage()})</size>";
            return this;
        }

        public ViewSlotGuildRaidRanking SetLevelAndTotalDamage(string text)
        {
            levelTMP.text = text;
            return this;
        }

        public ViewSlotGuildRaidRanking SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}