using ETD.Scripts.Common;
using ETD.Scripts.Manager;
using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotAttendance : MonoBehaviour
    {
        public ReddotView ReddotView => reddotView;
        
        [SerializeField] private TMP_Text dayTMP;
        [SerializeField] private Image good;
        [SerializeField] private TMP_Text valueTMP;
        [SerializeField] private GameObject earnPanel;
        [SerializeField] private ReddotView reddotView;

        public ViewSlotAttendance SetDay(int day)
        {
            return SetDay(LocalizeManager.GetText(LocalizedTextType.Days, day));
        }
        
        public ViewSlotAttendance SetDay(string day)
        {
            dayTMP.text = day;
            return this;
        }

        public ViewSlotAttendance SetIcon(Sprite sprite)
        {
            good.sprite = sprite;
            return this;
        }

        public ViewSlotAttendance SetGrade(string text)
        {
            valueTMP.text = text;
            return this;
        }

        public ViewSlotAttendance SetActiveEarnPanel(bool flag)
        {
            earnPanel.SetActive(flag);
            return this;
        }
    }
}