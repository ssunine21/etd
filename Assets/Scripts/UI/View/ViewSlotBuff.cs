using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotBuff : MonoBehaviour
    {
        public Button WatchAdsButton => watchAdsButton;    

        [SerializeField] private Transform iconTr;
        [SerializeField] private Transform iconShadowTr;
        [SerializeField] private TMP_Text contentTMP;
        [SerializeField] private TMP_Text remainTimeTMP;
        [SerializeField] private Button watchAdsButton;
        [SerializeField] private ViewLevel viewLevel;
        [SerializeField] private GameObject freeOnceGO;
        [SerializeField] private GameObject enableGO;
        [SerializeField] private GameObject disableGO;
        [SerializeField] private GameObject timeGO;
        [SerializeField] private GameObject blockGO;

        public ViewSlotBuff SetBlock(bool flag)
        {
            if(blockGO)
            {
                blockGO.SetActive(flag);
                watchAdsButton.enabled = !flag;
            }
            return this;
        }

        public ViewSlotBuff SetOn(bool flag)
        {
            enableGO.SetActive(flag);
            disableGO.SetActive(!flag);
            timeGO.SetActive(flag);

            return this;
        }

        public ViewSlotBuff IconAnimation()
        {
            iconTr.DORotate(new Vector3(0, 0, -360), 10f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(int.MaxValue)
                .SetUpdate(true);
            iconShadowTr.DORotate(new Vector3(0, 0, -360), 10f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(int.MaxValue)
                .SetUpdate(true);

            return this;
        }

        public ViewSlotBuff SetFreeOnceButton(bool flag)
        {
            if (freeOnceGO)
                freeOnceGO.SetActive(flag);
            return this;
        }
        
        public ViewSlotBuff SetLevel(int level)
        {
            return SetLevel($"{level}");
        }
        
        public ViewSlotBuff SetLevel(string text)
        {
            viewLevel.SetLevel(text);
            return this;
        }
        
        public ViewSlotBuff SetExp(int currExp, int maxExp)
        {
            
            viewLevel.SetExp(currExp, maxExp);
            return this;
        }
        
        public ViewSlotBuff SetExp(string text)
        {
            viewLevel.SetExp(text);
            return this;
        }

        public ViewSlotBuff SetContents(string text)
        {
            contentTMP.text = text;
            return this;
        }

        public ViewSlotBuff SetRemainTime(string text)
        {
            remainTimeTMP.text = text;
            return this;
        }
    }
}