using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasRelease : ViewCanvas
    {
        public Image Stroke0 => stroke0;
        public Image Stroke1 => stroke1;
        public Button Close => close;
        public Image LockIcon => lockIcon;
        public Image LightImage => lightImage;
        public TMP_Text ContentsTMP => contentTMP;
        public Image Icon => icon;
        public TMP_Text TabToCloseTMP => closeTMP;
        
        [SerializeField] private Image icon;
        [SerializeField] private Image lockIcon;
        [SerializeField] private Image stroke0;
        [SerializeField] private Image stroke1;
        [SerializeField] private Image lightImage;

        [SerializeField] private TMP_Text closeTMP;
        [SerializeField] private TMP_Text contentTMP;
        [SerializeField] private Button close;

        [Space] [Space] 
        [SerializeField] private Sprite lockSprite;
        [SerializeField] private Sprite unLockSprite;

        public ViewCanvasRelease ShowIconSprite(bool isShow)
        {
            lockIcon.enabled = !isShow;
            icon.enabled = isShow;
            return this;
        }
        
        public ViewCanvasRelease SetLockIconSprite(bool isLock)
        {
            lockIcon.sprite = isLock ? lockSprite : unLockSprite;
            return this;
        }

        public ViewCanvasRelease SetContentsText(string text)
        {
            contentTMP.text = text;
            return this;
        }
    }
}