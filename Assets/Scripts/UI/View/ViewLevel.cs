using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewLevel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelTMP;
        [SerializeField] private TMP_Text _expTMP;
        [SerializeField] private Image _expFillAmount;
        
        public ViewLevel SetLevel(int level)
        {
            return SetLevel(level.ToString());
        }

        public ViewLevel SetLevel(string text)
        {
            _levelTMP.text = text;
            return this;
        }

        public ViewLevel SetExp(string exp)
        {
            _expTMP.text = exp;
            return this;
        }

        public ViewLevel SetExp(int currExp, int maxExp, bool isAutoFillAmount = true)
        {
            if (isAutoFillAmount)
            {
                var amount = currExp == 0 || maxExp == 0 ? 0 : currExp / (float)maxExp;
                SetFillAmount(amount);
            }

            return SetExp($"{currExp}/{maxExp}");
        }

        private ViewLevel SetFillAmount(float amount)
        {
            _expFillAmount.fillAmount = amount;
            return this;
        }

        public ViewLevel SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}