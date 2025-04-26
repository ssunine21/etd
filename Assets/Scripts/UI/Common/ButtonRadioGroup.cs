using System;
using System.Collections.Generic;
using ETD.Scripts.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    public class ButtonRadioGroup
    {
        public int Count => _radioGroups?.Count ?? 0;
        public int SelectedIndex => _selectedIndex;

        public UnityAction<int> onBindSelected;
        
        private List<IActiveable> _radioGroups;
        private List<UnityAction> _callbacks;
        private int _selectedIndex;
        public void AddListener(IActiveable activeable, UnityAction callback = null)
        {
            _radioGroups ??= new List<IActiveable>();
            _callbacks ??= new List<UnityAction>();
            
            _radioGroups.Add(activeable);

            var button = activeable.Button;
            
            if (!button) return;
            var index = _radioGroups.Count - 1;
            button.onClick.AddListener(() => Select(index));
            _callbacks.Add(callback);
        }

        public void Select(int index)
        {
            for (var i = 0; i < _radioGroups.Count; ++i)
            {
                _radioGroups[i].Selected(i == index);
            }

            _selectedIndex = index;
            onBindSelected?.Invoke(index);

            if (index < 0)
                return;
            
            if(_callbacks[index] != null)
                _callbacks[index]?.Invoke();
        }
    }
}
