using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotVIP : ViewSlot<ViewSlotVIP>
    {
        public Button VIPPurchaseButton => vipPurchaseButton;

        private readonly List<ViewSlotVIPReward> _viewSLotVipRewards = new();

        [SerializeField] private ViewSlotVIPReward viewSlotVipRewardPrefab;
        [SerializeField] private Transform viewSlotVipRewardParent;
        
        [SerializeField] private TMP_Text vipTitleTMP;
        [SerializeField] private TMP_Text priceTMP;
        [SerializeField] private Button vipPurchaseButton;

        public ViewSlotVIP SetReward(int index, Sprite sprite, string desc)
        {
            var count = _viewSLotVipRewards.Count;
            for (var i = index; i <= count; ++i)
            {
                _viewSLotVipRewards.Add(Instantiate(viewSlotVipRewardPrefab, viewSlotVipRewardParent));
            }

            _viewSLotVipRewards[index]
                .SetRewardSprite(sprite)
                .SetRewardText(desc)
                .SetActive(true);
            return this;
        }
        
        public ViewSlotVIP SetTitle(string text)
        {
            if (vipTitleTMP != null)
                vipTitleTMP.text = text;
            return this;
        }

        public ViewSlotVIP SetPrice(string text)
        {
            if (priceTMP != null)
                priceTMP.text = text;
            return this;
        }
    }
}
