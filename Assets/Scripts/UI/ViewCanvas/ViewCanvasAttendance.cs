using ETD.Scripts.UI.View;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasAttendance : ViewCanvas
    {
        public ViewSlotAttendance[] ViewSlotAttendances => viewSlotAttendances;
        public Button ReceiveButton => receiveButton;
        public ViewSlotTime ViewSlotTime => viewSlotTime;
        
        [SerializeField] private ViewSlotAttendance[] viewSlotAttendances;
        [SerializeField] private TMP_Text remainTime;
        [SerializeField] private Button receiveButton;
        [SerializeField] private ViewSlotTime viewSlotTime;
    }
}