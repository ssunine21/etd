using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotTime : MonoBehaviour
    {
        [SerializeField] private TMP_Text timeTMP;

        public ViewSlotTime SetTimeText(string text)
        {
            timeTMP.text = text;
            return this;
        }
    }
}