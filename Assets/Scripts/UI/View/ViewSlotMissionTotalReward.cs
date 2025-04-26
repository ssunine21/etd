using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotMissionTotalReward : MonoBehaviour
    {
        public ViewGood ViewGood => viewGood;
        
        [SerializeField] private ViewGood viewGood;
        [SerializeField] private TMP_Text goalTMP;

        public ViewSlotMissionTotalReward SetGoalCountText(string text)
        {
            goalTMP.text = text;
            return this;
        }
    }
}