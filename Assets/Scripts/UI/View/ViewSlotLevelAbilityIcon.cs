using System;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotLevelAbilityIcon : ViewSlot<ViewSlotLevelAbilityIcon>
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image[] starImages;

        private void Awake()
        {
            SetActive(false);
        }

        public ViewSlotLevelAbilityIcon SetIcon(Sprite sprite)
        {
            iconImage.sprite = sprite;
            return this;
        }

        public ViewSlotLevelAbilityIcon SetActiveStar(int totalCount)
        {
            foreach (var starImage in starImages)
            {
                starImage.transform.parent.gameObject.SetActive(totalCount > 0);
                totalCount--;
            }

            return this;
        }
        
        public ViewSlotLevelAbilityIcon OnStar(int count)
        {
            foreach (var starImage in starImages)
            {
                starImage.enabled = count > 0;
                count--;
            }
            return this;
        }
    }
}
