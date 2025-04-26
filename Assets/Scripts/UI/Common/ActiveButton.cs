using System;
using DG.Tweening;
using ETD.Scripts.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    public class ActiveButton : MonoBehaviour, IActiveable
    {
        public Button Button => _button == null ? GetComponent<Button>(): _button;
        public Button.ButtonClickedEvent OnClick => Button.onClick;
        
        public UnityAction onBindChanged;
        public UnityAction onBindSelected;
        public UnityAction onBindUnSelected;
        public bool IsSelected => _isSelected;
        public ReddotView ReddotView => reddotView;
        
        [SerializeField] private GameObject activeObject;
        [SerializeField] private GameObject inactiveObject;
        [SerializeField] private Button _button;

        [SerializeField] private TMP_Text activeObjectTMP;
        [SerializeField] private TMP_Text inactiveObjectTMP;

        [SerializeField] private bool IsDisableButtonWhenInactiveObject;
        [SerializeField] private ReddotView reddotView;

        private bool _isSelected;
        private CanvasGroup _activeCanvasGroup;
        private CanvasGroup _inactiveCanvasGroup;

        public void Selected(bool flag)
        {
            if (activeObject)
            {            
                if (activeObject.TryGetComponent(out _activeCanvasGroup))
                {
                    _activeCanvasGroup
                        .DOFade(flag ? 1 : 0, 0.1f)
                        .OnPlay(() => { if(flag) activeObject.SetActive(true);})
                        .OnComplete(() => { activeObject.SetActive(flag); });
                }
                else 
                    activeObject.SetActive(flag);
            }

            if (inactiveObject)
            {
                if (inactiveObject.TryGetComponent(out _inactiveCanvasGroup))
                {
                    _inactiveCanvasGroup
                        .DOFade(!flag ? 1 : 0, 0.1f)
                        .OnPlay(() => { if (!flag) inactiveObject.SetActive(true); })
                        .OnComplete(() => { inactiveObject.SetActive(!flag); });   
                }
                else 
                    inactiveObject.SetActive(!flag);
                
                if (IsDisableButtonWhenInactiveObject)
                    Button.enabled = flag;
            }

            if (flag)
                onBindSelected?.Invoke();
            else
                onBindUnSelected?.Invoke();
            
            _isSelected = flag;
            onBindChanged?.Invoke();
        }

        public ActiveButton SetButtonText(string text)
        {
            return SetActiveButtonText(text).SetInactiveButtonText(text);
        }

        public ActiveButton SetActiveButtonText(string text)
        {
            if (activeObjectTMP)
                activeObjectTMP.text = text;
            return this;
        }

        public ActiveButton SetInactiveButtonText(string text)
        {
            if (inactiveObjectTMP)
                inactiveObjectTMP.text = text;
            return this;
        }

        public ActiveButton OnOffView(bool isOn)
        {
            if(activeObject)
                activeObject.SetActive(isOn);
            if(inactiveObject)
                inactiveObject.SetActive(!isOn);
            return this;
        }

        public ActiveButton SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
    }
}