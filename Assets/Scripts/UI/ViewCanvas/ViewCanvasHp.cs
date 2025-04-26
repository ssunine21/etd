using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasHp : ViewCanvas
    {
        public Image HpFillImage => hpFillImage;
        
        [SerializeField] private Image hpFillImage;
    }
}