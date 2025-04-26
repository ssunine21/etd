using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace ETD.Scripts.Common
{
    public class DynamicValue : MonoBehaviour
    {
        [SerializeField] private GoodType goodType;
        [SerializeField] private TMP_Text valueTMPText;

        private double _value;
        private double _prevValue;
        private Coroutine _coDynamicView;

        private void Awake()
        {
            if (valueTMPText == null)
                valueTMPText = GetComponent<TMP_Text>();
        }

        public DynamicValue SetGoodType(GoodType type)
        {
            goodType = type;
            return this;
        }

        public DynamicValue ShowValue(double value)
        {
            if (_coDynamicView != null)
                StopCoroutine(_coDynamicView);
                
            _coDynamicView = StartCoroutine(CoDynamicValue(value));
            return this;
        }

        private IEnumerator CoDynamicValue(double goalValue)
        {
            while (Math.Abs(_value - goalValue) > 0)
            {
                _value = Mathf.Lerp((float)_value, (float)goalValue, 20f * Time.unscaledDeltaTime);
                valueTMPText.text = (_value).ToGoodString(goodType);
                yield return null;
            }

            _value = goalValue;
            valueTMPText.text = _value.ToGoodString(goodType);
        }
    }
}
