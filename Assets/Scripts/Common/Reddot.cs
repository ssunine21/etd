using System.Collections.Generic;
using System.Linq;
using ETD.Scripts.UI.Common;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.Common
{
    public class Reddot
    {
        public bool IsOn { get; set; }
        private ReddotType BaseReddotType { get; }
        private readonly List<ReddotType> _childrenReddotTypes = new();
        
        private static UnityAction<ReddotType> _onBindShowReddot;
        private static Dictionary<ReddotType, List<Reddot>> _baseDictionary;

        private readonly ReddotView _view;

        public Reddot(ReddotType baseType)
        {
            BaseReddotType = baseType;
            _baseDictionary ??= new Dictionary<ReddotType, List<Reddot>>();
            if (BaseReddotType != ReddotType.None)
            {
                _baseDictionary.TryAdd(BaseReddotType, new List<Reddot>());
                _baseDictionary[BaseReddotType].Add(this);
            }

            _onBindShowReddot += ShowRegisterReddot;
        }

        public Reddot(ReddotView view, ReddotType baseType, IEnumerable<ReddotType> childrenType)
        {
            BaseReddotType = baseType;
            _childrenReddotTypes = childrenType.ToList();
            
            _baseDictionary ??= new Dictionary<ReddotType, List<Reddot>>();
            if (BaseReddotType != ReddotType.None)
            {
                _baseDictionary.TryAdd(BaseReddotType, new List<Reddot>());
                _baseDictionary[BaseReddotType].Add(this);
            }

            _view = view;
            _onBindShowReddot += ShowRegisterReddot;
        }

        public void OnBindShowReddot()
        {
            _onBindShowReddot?.Invoke(BaseReddotType);
        }
        
        private void ShowRegisterReddot(ReddotType type)
        {
            if (BaseReddotType != ReddotType.None
                || type == ReddotType.None
                || _childrenReddotTypes.Count == 0) return;

            if (!_view) return;
            
            foreach (var childrenReddotType in _childrenReddotTypes)
            {
                if(_baseDictionary.TryGetValue(childrenReddotType, out var value))
                {
                    if (value.Any(reddot => reddot.IsOn))
                    {
                        _view.ShowReddotIcon(true);
                        return;
                    }
                }
            }       
            _view.ShowReddotIcon(false);
        }
    }
}
