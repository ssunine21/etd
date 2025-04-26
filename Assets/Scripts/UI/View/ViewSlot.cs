using ETD.Scripts.Common;
using UnityEngine;
using UnityEngine.Events;

namespace ETD.Scripts.UI.View
{
    public class ViewSlot<T> : MonoBehaviour
    {
        public UnityAction OnBindDisable;
        public bool IsInit { get; set; }

        public static string ViewName => typeof(T).Name;

        public ViewSlot<T> SetActive(bool flag)
        {
            gameObject.SetActive(flag);
            return this;
        }
        
        private void OnDisable()
        {          
            OnBindDisable?.Invoke();
        }
    }
}