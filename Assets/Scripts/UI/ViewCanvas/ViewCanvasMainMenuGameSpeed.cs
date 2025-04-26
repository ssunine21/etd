using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasMainMenu
    {
        public Button OnButton => button;
        public Image ArrowImage0 => arrowImage0;
        public Image ArrowImage1 => arrowImage1;
        public RectTransform GameSpeedGuideArrowRectTransform => gameSpeedGuideArrowRectTransform;
        
        [Space][Space][Header("Game Speed")]
        [SerializeField] private Image arrowImage0;
        [SerializeField] private Image arrowImage1;
        [SerializeField] private Image stroke;
        [SerializeField] private TMP_Text remainTimeTMP;
        [SerializeField] private Button button;
        [SerializeField] private RectTransform gameSpeedGuideArrowRectTransform;

        public ViewCanvasMainMenu SetActiveGameSpeed(bool flag)
        {
            button.gameObject.SetActive(flag);
            return this;
        }

        public ViewCanvasMainMenu SetMaterial(Material material)
        {
            arrowImage0.material = material;
            arrowImage1.material = material;
            stroke.material = material;
            return this;
        }
        public ViewCanvasMainMenu SetGrayColor(Color color)
        {
            stroke.color = color;
            return this;
        }

        public ViewCanvasMainMenu SetActiveTimeTMP(bool flag)
        {
            remainTimeTMP.gameObject.SetActive(flag);
            return this;
        }

        public ViewCanvasMainMenu SetTimeText(string text)
        {
            remainTimeTMP.text = text;
            return this;
        }
    }
}
