using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.View
{
    public class ViewPositionMark : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;

        public ViewPositionMark SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }

        public ViewPositionMark On(bool flag)
        {
            backgroundImage.color = flag ? Color.white : Color.black;
            return this;
        }
    }
}
