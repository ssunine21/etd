using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    [RequireComponent(typeof(Toggle))]
    public class CheckBox : MonoBehaviour
    {
        public bool IsChecked => _toggle.isOn;
        public TMP_Text TMPText => tmpText;

        public Toggle Toggle
        {
            get
            {
                _toggle ??= GetComponent<Toggle>();
                return _toggle;
            }
        }

        [SerializeField] private TMP_Text tmpText;
        private Toggle _toggle;
    }
}