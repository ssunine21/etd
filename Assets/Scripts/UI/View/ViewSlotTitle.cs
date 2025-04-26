using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotTitle : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text titleShadow;

        public ViewSlotTitle SetText(string text)
        {
            title.text = text;
            titleShadow.text = text;
            return this;
        }

        public ViewSlotTitle SetTextColor(Color color)
        {
            title.color = color;
            return this;
        }
    }
}