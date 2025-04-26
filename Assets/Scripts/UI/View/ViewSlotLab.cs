using ETD.Scripts.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotLab : MonoBehaviour
    {
        public bool IsSpecialLab => isSpecialLab;
        public bool IsEmptySlot => disablePanel.activeSelf && !lockPanel.activeSelf;
        public bool IsLockSlot => lockPanel.activeSelf;
        public Button ShowInfo => showInfoButton;
        public Image LoadingIcon => loadingIcon;
        public ReddotView Reddot => reddot;

        [SerializeField] private bool isSpecialLab;
        
        [Space] [Space] [Header("Panel")]
        [SerializeField] private GameObject lockPanel;
        [SerializeField] private GameObject disablePanel;
        [SerializeField] private GameObject enablePanel;

        [Space] [Space]
        [SerializeField] private TMP_Text labTitleTMP;
        [SerializeField] private TMP_Text titleTMP;
        [SerializeField] private TMP_Text timeTMP;
        [SerializeField] private TMP_Text infoTMP;
        [SerializeField] private Button showInfoButton;
        [SerializeField] private Image timeFillAmount;
        [SerializeField] private Image loadingIcon;
        [SerializeField] private ReddotView reddot;

        public ViewSlotLab SetLabTitle(string title)
        {
            if (labTitleTMP)
                labTitleTMP.text = title;
            return this;
        }
        
        public ViewSlotLab SetTitle(string title)
        {
            titleTMP.text = title;
            return this;
        }

        public ViewSlotLab SetTime(string time)
        {
            timeTMP.text = time;
            return this;
        }

        public ViewSlotLab SetEnable(bool flag)
        {
            enablePanel.SetActive(flag);
            disablePanel.SetActive(!flag);
            return this;
        }

        public ViewSlotLab SetLock(bool flag)
        {
            lockPanel.SetActive(flag);
            return this;
        }

        public ViewSlotLab SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewSlotLab SetFillAmount(float fillAmount)
        {
            timeFillAmount.fillAmount = fillAmount;
            return this;
        }

        public ViewSlotLab SetInfoText(string text)
        {
            if (infoTMP)
                infoTMP.text = text;
            return this;
        }
    }
}
