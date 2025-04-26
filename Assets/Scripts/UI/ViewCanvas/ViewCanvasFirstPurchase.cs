using ETD.Scripts.UI.Common;
using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasFirstPurchase : ViewCanvas
    {
        public ActiveButton GetRewardActiveButton => getRewardActiveButton;

        [SerializeField] private Image[] iconImages;
        [SerializeField] private TMP_Text[] valuesTMP;
        [SerializeField] private ActiveButton getRewardActiveButton;
        [SerializeField] private GameObject enablePanel;
        [SerializeField] private GameObject disablePanel;

        public ViewCanvasFirstPurchase SetRewardSprite(Sprite sprite)
        {
            foreach (var icon in iconImages)
            {
                icon.sprite = sprite;
            }
            return this;
        }

        public ViewCanvasFirstPurchase SetValueText(string text)
        {
            foreach (var valueTMP in valuesTMP)
            {          
                valueTMP.text = text;
            }
            return this;
        }

        public ViewCanvasFirstPurchase SetEnable(bool flag)
        {
            enablePanel.SetActive(flag);
            disablePanel.SetActive(!flag);
            
            getRewardActiveButton.Selected(flag);
            return this;
        }
    }
}