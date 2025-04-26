using ETD.Scripts.UI.View;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public partial class ViewCanvasDungeon : ViewCanvas
    {
        public ViewSlotDungeon[] ViewSlotDungeons => viewSlotDungeons;
        
        [SerializeField] private ViewSlotDungeon[] viewSlotDungeons;
    }
}