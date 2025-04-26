using System;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotLevelAbility : ViewSlot<ViewSlotLevelAbility>
    {
        [SerializeField] private TMP_Text levelTMP;
        [SerializeField] private TMP_Text abilityTitleTMP;
        [SerializeField] private TMP_Text abilityContentTMP;
        [SerializeField] private GameObject lockPanel;
        [SerializeField] private GameObject checkPanel;

        private void Awake()
        {
            SetActive(false);
        }

        public ViewSlotLevelAbility SetLevel(int level)
        {
            levelTMP.text = level.ToString();
            return this;
        }

        public ViewSlotLevelAbility SetAbilityTitleText(string text)
        {
            abilityTitleTMP.text = text;
            return this;
        }

        public ViewSlotLevelAbility SetAbilityContentText(string text)
        {
            abilityContentTMP.text = text;
            return this;
        }

        public ViewSlotLevelAbility SetLock(bool flag)
        {
            lockPanel.SetActive(flag);
            checkPanel.SetActive(!flag);
            return this;
        }
    }
}
