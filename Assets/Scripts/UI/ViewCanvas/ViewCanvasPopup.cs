using ETD.Scripts.Common;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasPopup : ViewCanvas
    {
        public bool IsActive => gameObject.activeSelf;

        private void Awake()
        {
            Init();
            foreach (var closeButton in CloseButtons)
            {
                closeButton.onClick.AddListener(this.Close);
            }
        }

        public override void SetActive(bool flag)
        {
            gameObject.SetActive(flag);
        }
    }
}