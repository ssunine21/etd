using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace ETD.Scripts.UI.Common
{
    public class ViewRotation : MonoBehaviour
    {
        [SerializeField] private bool onAwake = true;
        [SerializeField] private bool isLoop = true;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private bool isHarf;
        [SerializeField] private float duration = 1;
        [SerializeField] private float delay;
        [SerializeField] private Ease ease = Ease.OutCubic;

        private void Awake()
        {
            if(onAwake) StartRotate();
        }
        private void StartRotate()
        {
            var value = clockwise ? isHarf ? -180f : -360f : isHarf ? 180f : 360f;
            var endValue = new Vector3(0, 0, value);
            transform.DOLocalRotate(endValue, duration, RotateMode.FastBeyond360).SetEase(ease).SetDelay(delay).SetLoops(int.MaxValue, LoopType.Restart).SetUpdate(true);
        }
    }
}
