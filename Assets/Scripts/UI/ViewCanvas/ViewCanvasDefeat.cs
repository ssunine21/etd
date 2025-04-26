using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasDefeat : ViewCanvas
    {
        public TMP_Text AutoConfirmText => autoConfirmText;
        public TMP_Text TapToCloseTMP => tapToCloseTMP;
        
        [SerializeField] private TMP_Text tapToCloseTMP;
        [SerializeField] private TMP_Text autoConfirmText;
    }
}