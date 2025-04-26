using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ETD.Scripts.UI.Common
{
    public class PreserveAspectRatio : MonoBehaviour
    {
        [SerializeField] private bool dynamicHorizontal;
        [SerializeField] private bool dynamicVertical;

        [SerializeField] private float ratio;

        private RectTransform _rectTr;

        private void OnEnable()
        {
            OnEnableTask().Forget();
        }

        private async UniTaskVoid OnEnableTask()
        {
            await UniTask.Yield();
            _rectTr ??= GetComponent<RectTransform>();
            
            var sizeDelta = _rectTr.sizeDelta;
            var width = sizeDelta.x;
            var height = sizeDelta.y;
            
            if (dynamicHorizontal)
            {
                _rectTr.sizeDelta = new Vector2(height * ratio, height);
            }
            else if (dynamicVertical)
            {
                _rectTr.sizeDelta = new Vector2(width, width * ratio);
            }
        } 
    }
}
