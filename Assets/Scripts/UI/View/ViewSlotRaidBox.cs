using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotRaidBox : MonoBehaviour
    {
        public ActiveButton RewardButton => rewardButton;
        
        [SerializeField] private ActiveButton rewardButton;
        [SerializeField] private TMP_Text rewardCountTMP;
        [SerializeField] private TMP_Text boxTypeTitleTMP;

        public ViewSlotRaidBox SetBoxTypeTitle(GradeType gradeType)
        {
            var localizedTextType = gradeType switch
            {
                GradeType.C => LocalizedTextType.Guild_NormalRaidBox,
                GradeType.B => LocalizedTextType.Guild_RareRaidBox,
                GradeType.A => LocalizedTextType.Guild_UniqueRaidBox,
                _ => LocalizedTextType.Guild_NormalRaidBox,
            };
            boxTypeTitleTMP.text = LocalizeManager.GetText(localizedTextType);
            return this;
        }
        
        public ViewSlotRaidBox SetRewardCount(int count)
        {
            rewardCountTMP.text = $"X{count}";
            return this;
        }
    }
}