using ETD.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotPass : ViewSlot<ViewSlotPass>
    {
        public ViewGood NViewGood => nViewGood;
        public ViewGood PViewGood => pViewGood;
        public Button[] ElementalSelectedButtons => elementalSelectedButtons;
        
        [SerializeField] private ViewGood nViewGood;
        [SerializeField] private Image fillAmountImage;
        [SerializeField] private ViewGood pViewGood;
        [SerializeField] private TMP_Text levelTMP;
        [SerializeField] private Button[] elementalSelectedButtons;
        [SerializeField] private GameObject[] selectDescViews;

        public ViewSlotPass SetFillAmount(float amount)
        {
            fillAmountImage.fillAmount = amount;
            return this;
        }

        public ViewSlotPass SetLevel(int level)
        {
            return SetLevel(level.ToString());
        }

        public ViewSlotPass SetLevel(string text)
        {
            levelTMP.text = text;
            return this;
        }

        public ViewSlotPass SetActiveLockPanel(bool nflag, bool pFlag)
        {
            NViewGood.SetActiveLockPaenl(nflag);
            PViewGood.SetActiveLockPaenl(pFlag);
            return this;
        }

        public ViewSlotPass ShowReddot(bool nFlag, bool pFlag)
        {
            NViewGood.ReddotView.ShowReddot(nFlag);
            PViewGood.ReddotView.ShowReddot(pFlag);
            return this;
        }

        public ViewSlotPass SetActiveCheckPanel(bool flag, BattlePassType type)
        {
            if (type == BattlePassType.Normal)
                NViewGood.SetActiveCheckPanel(flag);
            else
                PViewGood.SetActiveCheckPanel(flag);
            return this;
        }

        public ViewSlotPass SetActiveElementalSelectedButton(bool flag)
        {
            foreach (var button in elementalSelectedButtons)
            {
                button.gameObject.SetActive(flag);
            }

            foreach (var selectDescView in selectDescViews)
            {
                selectDescView.SetActive(flag);
            }
            return this;
        }
    }
}