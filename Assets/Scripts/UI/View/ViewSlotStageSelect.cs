using System.Collections.Generic;
using ETD.Scripts.Common;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotStageSelect : ViewSlot<ViewSlotStageSelect>
    {
        public int StageLevel { get; set; }
        
        [SerializeField] private TMP_Text stageLevelText;
        [SerializeField] private ViewGood[] viewGoods;
        [SerializeField] private Button goStageButton;
        
        [SerializeField] private TMP_Text selectedStageText;
        [SerializeField] private Image selectedStageImage;
        [SerializeField] private TMP_Text beforeReachingText;
        [SerializeField] private Image beforeReachingImage;
        
        public ViewSlotStageSelect SetStageLevel(int stageLevel)
        {
            StageLevel = stageLevel;
            return this;
        }
        
        public ViewSlotStageSelect SetStageLevelText(string stageLevel)
        {
            stageLevelText.text = stageLevel;
            return this;
        }
        
        public ViewSlotStageSelect SetGoodValue(GoodType[] goodTypes, double[] goodValues)
        {
            if (goodTypes.Length <= 0 || goodValues.Length <= 0) return this;

            var i = 0;
            for (; i < goodTypes.Length; ++i)
            {
                viewGoods[i].gameObject.SetActive(true);
                viewGoods[i]
                    .SetInit(goodTypes[i])
                    .SetValue(goodValues[i]);
            }

            for (; i < viewGoods.Length; ++i)
            {
                viewGoods[i].gameObject.SetActive(false);
            }

            return this;
        }

        public ViewSlotStageSelect AddGoToStageButtonAction(UnityAction action)
        {
            goStageButton.onClick.AddListener(action);
            return this;
        }
        
        public ViewSlotStageSelect SetSelectedSlot(bool isCurrStage, bool aboveMaxLevel)
        {
            selectedStageText.enabled = isCurrStage;
            selectedStageImage.enabled = isCurrStage;
            
            beforeReachingText.enabled = aboveMaxLevel;
            beforeReachingImage.enabled = aboveMaxLevel;
            
            goStageButton.gameObject.SetActive(!isCurrStage && !aboveMaxLevel);
            return this;
        }
    }
}
