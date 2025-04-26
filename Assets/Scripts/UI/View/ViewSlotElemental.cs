using System;
using ETD.Scripts.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewSlotElemental : ViewSlotUI
    {
        [SerializeField] private GameObject _expPanel;
        [SerializeField] private GameObject _levelPanel;
        
        public override ViewSlotUI SetLock(bool flag)
        {
            _lockPanel.SetActive(flag);
            _expPanel.SetActive(!flag);
            _levelPanel.SetActive(!flag);
            
            SetGrayScale(flag);
            return base.SetLock(flag);
        }

        private ViewSlotElemental SetGrayScale(bool flag)
        {
            _objectImage.material = flag ? ResourcesManager.Instance.grayScaleMaterial : null;
            
            var gradeTextColor =
                flag ? ResourcesManager.Instance.grayScaleColor : ResourcesManager.Instance.GetGradeColor(gradeType);
            
            SetGradeTextColor(gradeTextColor);
            return this;
        }
    }
}