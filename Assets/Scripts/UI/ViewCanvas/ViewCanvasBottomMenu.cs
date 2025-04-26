using System.Collections.Generic;
using ETD.Scripts.UI.Common;
using UnityEngine;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvasBottomMenu : ViewCanvas
    {
        public ActiveButton[] BottomMenus => bottomMenus;
        
        [SerializeField] private ActiveButton[] bottomMenus;
    }
}