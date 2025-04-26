using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    [RequireComponent(typeof(Button))]
    public class SwitchButton : MonoBehaviour
    {
        public UnityAction<bool> OnValueChanged;
        
        private Button _button;

        private Transform _bgEnabled;
        private Transform _bgDisabled;

        private Transform _handleEnabled;
        private Transform _handleDisabled;

        private bool _switchEnabled; 

        private void Awake()
        {
            _button = GetComponent<Button>();
            Init();
            _button.onClick.AddListener(Toggle);
        }

        private void Init()
        {
            _bgEnabled = transform.GetChild(0).GetChild(0).GetComponent<Transform>();
            _bgDisabled = transform.GetChild(0).GetChild(1).GetComponent<Transform>();
            _handleEnabled = transform.GetChild(1).GetChild(0).GetComponent<Transform>();
            _handleDisabled = transform.GetChild(1).GetChild(1).GetComponent<Transform>();

        }

        public void SetToggle(bool isOn)
        {
            Toggle(isOn);
        }
        
        public void Toggle()
        {
            Toggle(!_switchEnabled);
        }
        
        public void Toggle(bool flag)
        {
            _switchEnabled = flag;
            if (_bgDisabled == null) Init();
            
            try
            {
                if (_switchEnabled)
                {
                    _bgDisabled.gameObject.SetActive(false);
                    _bgEnabled.gameObject.SetActive(true);
                    _handleDisabled.gameObject.SetActive(false);
                    _handleEnabled.gameObject.SetActive(true);
                }
                else
                {
                    _bgEnabled.gameObject.SetActive(false);
                    _bgDisabled.gameObject.SetActive(true);
                    _handleEnabled.gameObject.SetActive(false);
                    _handleDisabled.gameObject.SetActive(true);
                }
            }
            catch (Exception e)
            {
                
            }

            OnValueChanged?.Invoke(_switchEnabled);
        }

        public bool IsToggled()
        {
            return _switchEnabled;
        }
    }
}