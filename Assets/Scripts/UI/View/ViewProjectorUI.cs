using System;
using DG.Tweening;
using ETD.Scripts.Common;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewProjectorUI : MonoBehaviour
    {
        [SerializeField] private bool isOnAnimationAwake;
        [SerializeField] private Image[] _unitImages;

        private Sequence _sequence;
        private const float InitRotateSpeed = 2f;
        private const float RotateSpeed = 10f;

        private void OnEnable()
        {
            if (isOnAnimationAwake)
                PlayStartAnimation();
        }

        public ViewProjectorUI SetActiveUnit(int projectorIndex)
        {
            for (var i = 1; i < _unitImages.Length; ++i)
            {
                _unitImages[i].enabled = !DataController.Instance.upgrade.IsLockElemental(projectorIndex, i);
            }
            return this;
        }
        
        public ViewProjectorUI PlayStartAnimation()
        {
            if (_sequence == null)
            {
                _sequence = DOTween.Sequence().SetAutoKill(false)
                    .OnStart(() =>
                    {
                        _unitImages[(int)EquippedPositionType.Active].transform.localRotation = Quaternion.Euler(Vector3.zero);
                        _unitImages[(int)EquippedPositionType.Link].transform.localRotation = Quaternion.Euler(Vector3.zero);
                        _unitImages[(int)EquippedPositionType.Passive].transform.localRotation = Quaternion.Euler(Vector3.zero);
                    })
                    .Append(_unitImages[(int)EquippedPositionType.Active].transform
                        .DOLocalRotate(new Vector3(0, 0, -360), InitRotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutQuart))
                    .Insert(0.2f, _unitImages[(int)EquippedPositionType.Passive].transform
                        .DOLocalRotate(new Vector3(0, 0, -360), InitRotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutQuart))
                    .Insert(0.3f, _unitImages[(int)EquippedPositionType.Link].transform
                        .DOLocalRotate(new Vector3(0, 0, -360), InitRotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.OutQuart))
                    .Insert(1.6f, _unitImages[(int)EquippedPositionType.Active].transform
                        .DOLocalRotate(new Vector3(0, 0, -360), RotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(int.MaxValue))
                    .Join( _unitImages[(int)EquippedPositionType.Link].transform
                        .DOLocalRotate(new Vector3(0, 0, -360), RotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(int.MaxValue))
                    .Join( _unitImages[(int)EquippedPositionType.Passive].transform
                        .DOLocalRotate(new Vector3(0, 0, 360), RotateSpeed, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(int.MaxValue))
                    .SetUpdate(true);
            }
            else
            {
                _sequence.Restart();
            }
            return this;
        }

        public ViewProjectorUI ChangeColor(Color newColor)
        {
            foreach (var unit in _unitImages)
            {
                unit.DOColor(newColor, 0.5f);
            }
            return this;
        }
    }
}