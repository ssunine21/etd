using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotElementalInfo : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text desc;

        public ViewSlotElementalInfo SetTitleText(string text)
        {
            title.text = text;
            return this;
        }
        
        public ViewSlotElementalInfo SetDescText(string text)
        {
            desc.text = text;
            return this;
        }
    }
}
