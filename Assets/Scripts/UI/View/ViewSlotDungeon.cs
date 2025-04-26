using ETD.Scripts.Common;
using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotDungeon : MonoBehaviour
    {
        public ViewGood TicketViewGood => ticketViewGood;
        public Button EnterButton => enterButton;
        public ContentUnlock ContentUnlock => contentUnlock;
        public RectTransform GuideArrowRectTransform => guideArrowRectTransform;

        [FormerlySerializedAs("dungeonType")] public StageType stageType;
        [SerializeField] private TMP_Text clearGoalTMP;
        [SerializeField] private ViewGood ticketViewGood;
        [SerializeField] private Button enterButton;
        [SerializeField] private ContentUnlock contentUnlock;
        [SerializeField] private RectTransform guideArrowRectTransform;
        
        public ViewSlotDungeon SetGoalText(string text)
        {
            clearGoalTMP.text = text;
            return this;
        }
    }
}