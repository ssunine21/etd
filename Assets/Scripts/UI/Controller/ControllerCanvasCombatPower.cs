using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ETD.Scripts.Common;
using ETD.Scripts.UI.View;
using ETD.Scripts.UI.ViewCanvas;
using ETD.Scripts.UserData.DataController;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ETD.Scripts.UI.Controller
{
    public class ControllerCanvasCombatPower : ControllerCanvas
    {
        private ViewCanvasCombatPower View => ViewCanvas as ViewCanvasCombatPower;
        private readonly Queue<ViewCombatPowerSlot> _totalCombatPowerSlots = new();

        private bool _isFistShow;
        
        public ControllerCanvasCombatPower(CancellationTokenSource cts) : base(cts, UI.ViewCanvas.ViewCanvas.Get<ViewCanvasCombatPower>())
        {
            DataController.Instance.player.OnBindChangeTotalCombat += ShowCombatPowerConversion;
        }
        
        private void ShowCombatPowerConversion(double conversionValue)
        {
            if (!_isFistShow)
            {
                _isFistShow = true;
                return;
            }
            
            var difference = conversionValue - DataController.Instance.player.CurrCombatPower;
            if (Math.Abs(difference) < 1) return;

            var slot = View.ViewCombatPowerSlotPrefab;
            
            var isPlus = difference > 0;
            var color = isPlus ? slot.PlusColor : slot.MinusColor;
            var diffMark = isPlus ? "+" : "-";
            var differenceText = diffMark + Math.Abs(difference).ToDamage();

            slot
                .SetActive(true)
                .SetCurrPower(conversionValue.ToDamage())
                .SetConversionPowerTextAndColor(differenceText, color);
            
            slot.ShowAnimation(() =>
            {
                slot.SetActive(false);
                _totalCombatPowerSlots.Enqueue(slot);
            });
            
            View.SetActive(true);
        }
    }
}