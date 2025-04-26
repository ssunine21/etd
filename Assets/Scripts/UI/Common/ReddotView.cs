using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using ETD.Scripts.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    public class ReddotView : MonoBehaviour
    {
        public bool IsShow => reddot.enabled;
        public ReddotType BaseReddotType => baseReddotType;
        
        private static UnityAction<ReddotType> _onBindShowReddot;
        private static Dictionary<ReddotType, List<ReddotView>> _baseDictionary;

        private Reddot _reddot;
        
        [SerializeField] private ReddotType baseReddotType;
        [SerializeField] private List<ReddotType> childrenReddotTypes;
        [SerializeField] private Image reddot;
        [SerializeField] private bool enableOnAwake;
        
        private void Awake()
        {
            if (_reddot == null)
                Init();
        }

        private void Start()
        {
            reddot.transform.DOScale(0.88f, 1f).SetEase(Ease.OutQuart).SetLoops(int.MaxValue, LoopType.Yoyo).SetUpdate(true);
        }
        
        public void ShowReddot(bool show)
        {
            if(_reddot == null) Init();
            
            ShowReddotIcon(show);
            if (_reddot != null)
            {
                _reddot.IsOn = show;
                _reddot.OnBindShowReddot();
            }
        }

        public ReddotView RemoveChildren()
        {
            childrenReddotTypes = new List<ReddotType>();
            return this;
        }

        public ReddotView SetBaseReddotType(ReddotType reddotType)
        {
            baseReddotType = reddotType;
            return this;
        }

        public ReddotView AddChildrenReddotType(ReddotType reddotType)
        {
            childrenReddotTypes ??= new List<ReddotType>();
            childrenReddotTypes.Add(reddotType);
            return this;
        }

        public void ShowReddotIcon(bool show)
        {
             if (reddot)
                reddot.enabled = show;
        }

        private void Init()
        {
            _reddot = new Reddot(this, baseReddotType, childrenReddotTypes)
            {
                IsOn = enableOnAwake
            };
            reddot.enabled = enableOnAwake;
        }
    }
}