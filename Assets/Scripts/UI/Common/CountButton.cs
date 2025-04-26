using System;
using ETD.Scripts.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    public class CountButton : MonoBehaviour
    {
        public int MaxCount { get; set; }
        public int CurrCount { get; set; }
        public UnityAction<int> OnCountChanged;
        
        [SerializeField] private Button minButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private TMP_Text countTMP;
        [SerializeField] private Button increaseminButton;
        [SerializeField] private Button maxButton;

        private void Start()
        {
            minButton.onClick.AddListener(() => SetCount(1));
            decreaseButton.onClick.AddListener(() => DecreaseCount());
            increaseminButton.onClick.AddListener(() => IncreaseCount());
            maxButton.onClick.AddListener(() => SetCount(MaxCount));
        }

        private void IncreaseCount(int count = 1)
        {
            SetCount(CurrCount + count);
        }
        private void DecreaseCount(int count = -1)
        {
            SetCount(CurrCount + count);
        }

        public void SetCount(int count)
        {
            CurrCount = Mathf.Clamp(count, 1, MaxCount);
            UpdateCountText();
            OnCountChanged?.Invoke(CurrCount);
        }

        private void UpdateCountText()
        {
            countTMP.text = CurrCount.ToString();
        }
    }
}
