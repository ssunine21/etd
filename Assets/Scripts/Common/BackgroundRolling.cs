using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.Common
{
    public class BackgroundRolling : MonoBehaviour
    {
        [SerializeField] private RawImage[] images;
        [SerializeField] private float horizontalRollingSpeed;
        [SerializeField] private float verticalRollingSpeed;

        [SerializeField] private bool isHorizontal;
        [SerializeField] private bool isVertical;
        
        [SerializeField] private bool isLeftToRight;
        [SerializeField] private bool isTopToBottom;

        private Rect _backgroundScrollOffset = Rect.zero;
        private int _currViewIndex;
        private bool _isbackgroundImageNotNull;

        private void Start()
        {
            if(images.Length > 0)
            {
                _backgroundScrollOffset = images[0].uvRect;
            }
        }

        private void Update()
        {
            ScrollBackgroundImage();
        }

        public void SetRawImage(int nextIndex)
        {
            if (_currViewIndex == nextIndex) return;
            
            var duration = 0.7f;
            for (var i = 0; i < images.Length; ++i)
            {
                images[i].DOFade(i == nextIndex ? 1 : 0, duration);
            }

            _currViewIndex = nextIndex;
        }

        private void ScrollBackgroundImage()
        {
            if (isHorizontal)
            {
                if (isLeftToRight)
                {
                    _backgroundScrollOffset.x -= horizontalRollingSpeed * Time.unscaledDeltaTime;
                }
                else
                {
                    _backgroundScrollOffset.x += horizontalRollingSpeed * Time.unscaledDeltaTime;
                }
            }
            if(isVertical)
            {
                if (isTopToBottom)
                {
                    _backgroundScrollOffset.y += verticalRollingSpeed * Time.unscaledDeltaTime;
                }
                else
                {
                    _backgroundScrollOffset.y -= verticalRollingSpeed * Time.unscaledDeltaTime;
                }
            }

            foreach (var rawImage in images)
            {
                rawImage.uvRect = _backgroundScrollOffset;
            }
        }
    }
}
