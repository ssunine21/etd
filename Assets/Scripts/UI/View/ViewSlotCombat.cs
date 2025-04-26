using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotCombat : ViewSlot<ViewSlotCombat>
    {
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text descTMP;

        public ViewSlotCombat SetTitle(string text)
        {
            titleTMP.text = text;
            return this;
        }
        public ViewSlotCombat SetDesc(string text)
        {
            descTMP.text = text;
            return this;
        }
    }
}