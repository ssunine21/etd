using ETD.Scripts.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace ETD.Scripts.UI.Common
{
    public class SlideToUnLock : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public UnityAction onBindDrag;
    
        private Vector2 _touchPosition;
    
        public void OnPointerDown(PointerEventData eventData)
        {
            _touchPosition = eventData.position;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            var distance = Vector2.Distance(_touchPosition, eventData.position);
            Utility.Log($"distance: {distance}");
            if(distance  > 250f)
                onBindDrag?.Invoke();
        }
    }
}