using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.Common
{
    public class SnapScrollView : MonoBehaviour
    {
        public bool IsSnapping { get; set; }
        
        public ScrollRect scrollRect;        // ScrollRect 컴포넌트
        public RectTransform content;       // Content 영역
        public float snapSpeed = 10f;       // 스냅 속도
        public float threshold = 0.1f;      // 속도 임계값
        private float[] _positions;          // 각 항목의 위치 비율
        private int _targetIndex = -1;       // 스냅될 목표 인덱스

        public void ScrollToTarget(int index)
        {
            if(_positions == null) Init();
            if (_positions != null)
                _targetIndex = index % _positions.Length;
        }
        
        private void Start()
        {
            Init();
        }

        private void Init()
        {
            // 자식 항목 수에 따라 위치 비율 계산
            var itemCount = content.childCount;
            _positions = new float[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                _positions[i] = i / (float)(itemCount - 1); // 비율로 계산
            }
        }

        private void Update()
        {
            // 드래그가 멈췄을 때 스냅 동작
            if (!Input.GetMouseButton(0) && scrollRect.velocity.magnitude < threshold)
            {
                IsSnapping = true;
                
                if (_targetIndex == -1)
                    _targetIndex = FindClosestIndex();

                // 목표 위치로 Content 이동
                var targetPos = _positions[_targetIndex];
                var newHorizontal = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, targetPos, Time.unscaledDeltaTime * snapSpeed);
                scrollRect.horizontalNormalizedPosition = newHorizontal;

                // 목표에 도달하면 스냅 종료
                if (Mathf.Abs(scrollRect.horizontalNormalizedPosition - targetPos) < 0.001f)
                {
                    _targetIndex = -1;
                }
            }
            else
            {
                IsSnapping = false;
            }
        }

        // 가장 가까운 인덱스를 찾는 함수
        private int FindClosestIndex()
        {
            var currentPos = scrollRect.horizontalNormalizedPosition;
            var closestDistance = Mathf.Infinity;
            var closestIndex = 0;

            for (var i = 0; i < _positions.Length; i++)
            {
                var distance = Mathf.Abs(_positions[i] - currentPos);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }
    }
}