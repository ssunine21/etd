using UnityEngine;

namespace ETD.Scripts.UI.Common
{
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdjuster : MonoBehaviour
    {
        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            ApplySafeArea();
        }

        private void ApplySafeArea()
        {
            //var safeArea = Screen.safeArea;

            // 현재 RectTransform의 Parent 캔버스 기준 변환
            // Vector2 anchorMin = safeArea.position;
            // Vector2 anchorMax = safeArea.position + safeArea.size;
            //
            // // Screen 크기로 나누어 0~1 사이의 비율로 변환
            // anchorMin.x /= Screen.width;
            // anchorMin.y = 0;
            // anchorMax.x /= Screen.width;
            // anchorMax.y /= Screen.height;
            //
            // // 현재 설정된 Anchors를 고려하여 적용
            // if (Mathf.Approximately(_rectTransform.anchorMin.y, _rectTransform.anchorMax.y))  
            // {
            //     _rectTransform.anchorMin = new Vector2(anchorMin.x, 0);
            //     _rectTransform.anchorMax = new Vector2(anchorMax.x, 1);
            // }
            // else
            // {
            //     _rectTransform.anchorMin = anchorMin;
            //     _rectTransform.anchorMax = anchorMax;
            // }
            
            var safeArea = Screen.safeArea;
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y = 0;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;
            
            //
            // var safeArea = Screen.safeArea;
            //
            // var anchorMin = new Vector2(safeArea.xMin / Screen.width, 0);
            // var anchorMax = new Vector2(safeArea.xMax / Screen.width, safeArea.yMax / Screen.height);
            //
            // _rectTransform.anchorMin = anchorMin;
            // _rectTransform.anchorMax = anchorMax;
        }
    }
}